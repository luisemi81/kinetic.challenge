using Kinetic.Inventory.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Kinetic.Inventory.API.Data
{
    public class InventoryDbContext: DbContext
    {
        public DbSet<Product> Products { get; set; }
        
        public InventoryDbContext(DbContextOptions<InventoryDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configuraciones del modelo
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Id)
                      .ValueGeneratedOnAdd(); // autoincremental

                entity.Property(p => p.Name)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(p => p.Price)
                      .HasColumnType("decimal(18,2)");
            });
        }
    }
}
