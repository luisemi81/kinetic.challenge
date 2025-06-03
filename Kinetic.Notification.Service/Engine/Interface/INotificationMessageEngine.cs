using Kinetic.Common.DTO;
using Kinetic.Notification.Service.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinetic.Notification.Service.Engine.Interface
{
    public interface INotificationMessageEngine
    {
        Task<IEnumerable<NotificationMessage>> GetAllNotificationMessagesAsync();
        Task<NotificationMessage> CreateNotificationMessageAsync(ProductMessage productMessage);
    }
}
