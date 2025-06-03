using Kinetic.Notification.Service.Data;
using Kinetic.Common.DTO;
using Kinetic.Notification.Service.Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinetic.Notification.Service.Service
{
    public class NotificationService : INotificationService
    {
        private readonly NotificationDbContext _context;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            NotificationDbContext context,
            ILogger<NotificationService> logger)
        {
            _context = context;
            _logger = logger;
        }
        public Task ProcessInventoryNotificationAsync(ProductMessage notification)
        {
            throw new NotImplementedException();
        }
    }
}
