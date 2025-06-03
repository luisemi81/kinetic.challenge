using Kinetic.Notification.Service.Data;
using Kinetic.Common.DTO;
using Kinetic.Notification.Service.Engine.Interface;
using Kinetic.Notification.Service.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Kinetic.Notification.Service.Engine
{
    public class NotificationMessageEngine: INotificationMessageEngine, IDisposable
    {
        private readonly NotificationDbContext _dbContext;
        private readonly ILogger<NotificationMessageEngine> _logger;

        public NotificationMessageEngine(
            NotificationDbContext dbContext,
            ILogger<NotificationMessageEngine> logger)
        {
            _dbContext = dbContext;
            _dbContext.Database.EnsureCreated();
            _logger = logger;
        }

        public async Task<IEnumerable<NotificationMessage>> GetAllNotificationMessagesAsync()
        {
            try
            {
                return await _dbContext.NotificationMessages
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo notificaciones");
                throw;
            }
        }

        public async Task<NotificationMessage> CreateNotificationMessageAsync(ProductMessage productMessage)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var notificationMessage = new NotificationMessage
                {
                    EventType = productMessage.EventType,
                    ProductId = productMessage.ProductId,
                    ReceivedAt = DateTime.UtcNow,
                    Payload = JsonSerializer.Serialize(productMessage),
                    Status = "Processed"
                };

                _dbContext.NotificationMessages.Add(notificationMessage);
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return notificationMessage;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error al crear Notificatio Message");
                throw;
            }
        }

        public void Dispose()
        {
            _dbContext?.Dispose();
        }

        
    }
}
