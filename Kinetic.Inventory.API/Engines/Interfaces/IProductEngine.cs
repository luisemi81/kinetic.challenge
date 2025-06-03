using Kinetic.Inventory.API.Models;

namespace Kinetic.Inventory.API.Engines.Interfaces
{
    public interface IProductEngine
    {
        Task<IEnumerable<Product>> GetAllProductsAsync();
        Task<Product> GetProductByIdAsync(int id);
        Task<Product> CreateProductAsync(Product product);
        Task<Product> UpdateProductAsync(Product product);
        Task DeleteProductAsync(int id);
    }
}
