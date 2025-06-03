using Kinetic.Common.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinetic.Notification.Service.Service.Interface
{
    public interface INotificationService
    {
        Task ProcessInventoryNotificationAsync(ProductMessage notification);
    }
}
