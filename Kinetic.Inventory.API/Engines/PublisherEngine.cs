using Kinetic.Common.DTO;
using Kinetic.Inventory.API.Engines.Interfaces;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;

namespace Kinetic.Inventory.API.Engines
{
    public class PublisherEngine : IPublisherEngine, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IChannel _channel;
        private readonly string _exchangeName = "inventory_exchange";
        private readonly AsyncCircuitBreakerPolicy _circuitBreakerPolicy;
        private readonly ILogger<PublisherEngine> _logger;

        public PublisherEngine(IConnection connection,
        IChannel channel,
        AsyncCircuitBreakerPolicy circuitBreakerPolicy,
        ILogger<PublisherEngine> logger)
        {
            _connection = connection;
            _channel = channel;
            _circuitBreakerPolicy = circuitBreakerPolicy;
            _logger = logger;

            _channel.ExchangeDeclareAsync(_exchangeName, ExchangeType.Direct, false, false);
        }

        public static async Task<PublisherEngine> CreateAsync(IConfiguration configuration, ILogger<PublisherEngine> logger)
        {
            var factory = new ConnectionFactory()
            { 
                HostName = configuration["RabbitMQ:HostName"],
                UserName = configuration["RabbitMQ:UserName"],
                Password = configuration["RabbitMQ:Password"],
                Port = 5672
            };
            //Usamos Polly para implementar circuitbreaker
            var circuitBreakerPolicy = Policy
                .Handle<Exception>()
                .CircuitBreakerAsync(
                    exceptionsAllowedBeforeBreaking: 3, //cuantos retries antes de abrir el circuito
                    durationOfBreak: TimeSpan.FromSeconds(30), //tiempo hasta volver a cerrar
                    onBreak: (ex, breakDelay) =>
                    {
                        logger.LogWarning($"No se aceptan mas operaciones por {breakDelay.TotalSeconds}. Error: {ex.Message}");
                    },
                    onReset: () =>
                    {
                        logger.LogInformation("Se aceptan operaciones otra vez");
                    });

            try
            {
                //conectamos utilizando circuitbreaker, si falla intentará hasta 3 veces
                var connection = await circuitBreakerPolicy.ExecuteAsync(
                    () => factory.CreateConnectionAsync());

                //creamos utilizando circuitbreaker, si falla intentará hasta 3 veces
                var channel = await circuitBreakerPolicy.ExecuteAsync(
                    () => connection.CreateChannelAsync());

                return new PublisherEngine(connection, channel, circuitBreakerPolicy, logger);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error en PublisherEngine: " + ex.Message);
                throw;
            }
        }

        public async Task PublishProductMessageAsync(ProductMessage productMessage)
        {
            try
            {
                //ejecuto utilizanod el circuitbreaker
                await _circuitBreakerPolicy.ExecuteAsync(async () =>
                {
                    var message = JsonSerializer.Serialize(productMessage);
                    var body = new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(message));

                    //envía el mensaje a rabbit
                    await _channel.BasicPublishAsync(exchange: _exchangeName, routingKey: productMessage.EventType, body: body);

                    _logger.LogInformation($"Se envió mensaje para producto ID: {productMessage.ProductId}");
                });
            }
            catch (BrokenCircuitException)
            {
                _logger.LogError("Mensaje no fue ennviado porque el circuite se abrió");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "No se pudo enviar el mensaje");
                throw;
            }
        }

        public void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }
    }
}
