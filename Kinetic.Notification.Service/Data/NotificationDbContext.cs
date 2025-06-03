using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;
using Kinetic.Notification.Service.Models;

namespace Kinetic.Notification.Service.Data
{
    public class NotificationDbContext : DbContext
    {
        public NotificationDbContext(DbContextOptions<NotificationDbContext> options) : base(options) { }

        public DbSet<NotificationMessage> NotificationMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<NotificationMessage>(entity =>
            {
                entity.HasIndex(n => n.ProductId);
                entity.HasIndex(n => n.ReceivedAt);
                entity.HasIndex(n => n.Status);

                entity.Property(n => n.Payload)
                    .HasColumnType("text");
            });
        }
    }
}
