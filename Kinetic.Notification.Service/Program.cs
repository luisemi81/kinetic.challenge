using Kinetic.Notification.Service.Data;
using Kinetic.Notification.Service.Engine;
using Kinetic.Notification.Service.Engine.Interface;
using Microsoft.EntityFrameworkCore;

namespace Kinetic.Notification.Service
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);
            builder.Services.AddHostedService<Worker>();
            builder.Services.AddScoped<INotificationMessageEngine, NotificationMessageEngine>();
            builder.Services.AddDbContext<NotificationDbContext>(options =>
                options.UseSqlite(builder.Configuration.GetConnectionString("NotificationDatabase")));
            var host = builder.Build();
            host.Run();
        }
    }
}