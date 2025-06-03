using Kinetic.Common.DTO;
using Kinetic.Common.Enum;
using Kinetic.Notification.Service.Engine.Interface;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;
using Polly;
using Polly.CircuitBreaker;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using static System.Formats.Asn1.AsnWriter;

namespace Kinetic.Notification.Service
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private IConfiguration _configuration;
        private IConnection _connection;
        private IChannel _channel;
        private readonly string _exchangeName = "inventory_exchange";
        private readonly IServiceProvider _serviceProvider;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly AsyncCircuitBreakerPolicy _circuitBreakerPolicy;

        public Worker(IServiceProvider serviceProvider, ILogger<Worker> logger,
            IConfiguration configuration, IServiceScopeFactory scopeFactory)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _configuration = configuration;
            _scopeFactory = scopeFactory;
            _circuitBreakerPolicy = Policy
            .Handle<Exception>()
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking: 2,
                durationOfBreak: TimeSpan.FromSeconds(10),
                onBreak: (ex, timespan) => _logger.LogWarning("Circuito abierto por {0} porque pasó {1}", timespan.TotalSeconds, ex.Message),
                onReset: () => _logger.LogInformation("Circuito cerrado, se aceptan acciones"),
                onHalfOpen: () => _logger.LogInformation("Circuito medio abierto, intentando nuevamente")
            );
        }

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (!(_connection != null && _connection.IsOpen && _channel != null && _channel.IsOpen))
                    {
                        var factory = new ConnectionFactory()
                        {
                            HostName = _configuration["RabbitMQ:HostName"],
                            UserName = _configuration["RabbitMQ:UserName"],
                            Password = _configuration["RabbitMQ:Password"],
                        };

                        await _circuitBreakerPolicy.ExecuteAsync(async () =>
                        {
                            _connection = await factory.CreateConnectionAsync();
                            _channel = await _connection.CreateChannelAsync();
                            await _channel.ExchangeDeclareAsync(_exchangeName, ExchangeType.Direct, false, false);
                        });

                        if (_channel != null && _channel.IsOpen)
                        {
                            var consumer = new AsyncEventingBasicConsumer(_channel);
                            consumer.ReceivedAsync += async (model, ea) =>
                            {
                                using (var scope = _scopeFactory.CreateScope())
                                {
                                    try
                                    {
                                        //ejecuto utilizanod el circuitbreaker
                                        await _circuitBreakerPolicy.ExecuteAsync(async () =>
                                        {
                                            var notificationEngine = scope.ServiceProvider.GetRequiredService<INotificationMessageEngine>();
                                            byte[] body = ea.Body.ToArray();
                                            var message = Encoding.UTF8.GetString(body);
                                            var productMessage = JsonSerializer.Deserialize<ProductMessage>(message);
                                            if (productMessage != null)
                                            {
                                                _logger.LogInformation($"Procesando mensaje: {productMessage.ProductId}");

                                                await notificationEngine.CreateNotificationMessageAsync(productMessage);
                                            }
                                        });
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.LogError(ex, "Error processing message");

                                    }
                                }
                            };


                            foreach (var queue in Enum.GetValues(typeof(EnumQueue)))
                            {
                                await _channel.BasicConsumeAsync(queue.ToString(), true, consumer);
                            }
                        }
                    }
                }
                catch (BrokenCircuitException ex)
                {
                    _logger.LogWarning("Circuito abierto. Esperando antes de volver a conectar: {0}", ex.Message);
                    await Task.Delay(5000, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al conectar o crear canal, va a hacerlo 2 veces ya q esta configurado de esa forma, lugo se abre ell circuito por 20 seg");
                    await Task.Delay(3000, stoppingToken);
                }
            }
        }
    }
}
