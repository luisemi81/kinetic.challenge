
using Kinetic.Inventory.API.Data;
using Kinetic.Inventory.API.Engines;
using Kinetic.Inventory.API.Engines.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Kinetic.Inventory.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddDbContext<InventoryDbContext>(options =>
                options.UseSqlite(builder.Configuration.GetConnectionString("InventoryDatabase")));

            builder.Services.AddScoped<IProductEngine, ProductEngine>();
            //creo el singleton del publisher para conectarme una vez
            builder.Services.AddSingleton<IPublisherEngine>(provider =>
            {
                var config = provider.GetRequiredService<IConfiguration>();
                var logger = provider.GetRequiredService<ILogger<PublisherEngine>>();
                return PublisherEngine.CreateAsync(config, logger).GetAwaiter().GetResult();
            });

           
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }            

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
