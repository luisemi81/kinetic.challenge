using Kinetic.Inventory.API.Data;
using Kinetic.Inventory.API.Engines.Interfaces;
using Kinetic.Inventory.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Kinetic.Inventory.API.Engines
{
    public class ProductEngine : IProductEngine, IDisposable
    {
        private readonly InventoryDbContext _dbContext;
        private readonly ILogger<ProductEngine> _logger;

        public ProductEngine(
            InventoryDbContext dbContext,
            ILogger<ProductEngine> logger)
        {
            _dbContext = dbContext;
            _dbContext.Database.EnsureCreated();
            _logger = logger;
        }

        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            try
            {
                return await _dbContext.Products
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en: GetAllProductsAsync");
                throw;
            }
        }

        public async Task<Product> GetProductByIdAsync(int id)
        {
            try
            {
                return await _dbContext.Products
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error en: GetProductByIdAsync({id})");
                throw;
            }
        }

        public async Task<Product> CreateProductAsync(Product product)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                // Validación de precio
                if (product.Price < 0)
                    throw new ArgumentException("Precio no puede ser negativo");

                _dbContext.Products.Add(product);
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return product;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error en CreateProductAsync");
                throw;
            }
        }

        public async Task<Product> UpdateProductAsync(Product product)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var existingProduct = await _dbContext.Products
                    .FirstOrDefaultAsync(p => p.Id == product.Id);

                if (existingProduct == null)
                    throw new KeyNotFoundException($"No se encontró el producto {product.Id}");

                // Actualizar propiedades
                existingProduct.Name = product.Name;
                existingProduct.Description = product.Description;
                existingProduct.Price = product.Price;
                existingProduct.Stock = product.Stock;
                existingProduct.Category = product.Category;

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return existingProduct;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"Error en UpdateProductAsync {product.Id}");
                throw;
            }
        }

        public async Task DeleteProductAsync(int id)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var product = await _dbContext.Products
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (product == null)
                    throw new KeyNotFoundException($"No se encontró el producto {product.Id}");

                _dbContext.Products.Remove(product);
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"Error en DeleteProductAsync({id})");
                throw;
            }
        }

        
        public void Dispose()
        {
            _dbContext?.Dispose();
        }
    }
}
