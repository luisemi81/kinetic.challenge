using Kinetic.Common.DTO;
using Kinetic.Common.Enum;
using Kinetic.Inventory.API.Engines;
using Kinetic.Inventory.API.Engines.Interfaces;
using Kinetic.Inventory.API.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Kinetic.Inventory.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductEngine _productEngine;
        private readonly IPublisherEngine _publisherEngine;

        public ProductsController(IProductEngine productEngine, IPublisherEngine publisherEngine)
        {
            _productEngine = productEngine;
            _publisherEngine = publisherEngine;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProducts()
        {
            var products = await _productEngine.GetAllProductsAsync();
            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            var product = await _productEngine.GetProductByIdAsync(id);
            if (product == null) return NotFound();
            return Ok(product);
        }

        [HttpPost ("BatchProductCreate")]
        public async Task<IActionResult> BatchProductCreate([FromBody] int count)
        {
            for (int i = 0; i < count; i++)
            {
                var createdProduct = await _productEngine.CreateProductAsync(new Product()
                {
                    Name = $"Product {i + 1}",
                    Description = $"Description {i + 1}",
                    Category = "Product",
                    Price = i + 1,
                    Stock = i + 2
                });
                await _publisherEngine.PublishProductMessageAsync(new ProductMessage
                {
                    ProductId = createdProduct.Id,
                    EventType = "create",
                    Timestamp = DateTime.UtcNow,
                    Payload = JsonSerializer.Serialize(createdProduct)
                });
            }


            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] Product product)
        {
            var createdProduct = await _productEngine.CreateProductAsync(product);

            await _publisherEngine.PublishProductMessageAsync(new ProductMessage
            {
                ProductId = createdProduct.Id,
                EventType = EnumQueue.create.ToString(),
                Timestamp = DateTime.UtcNow,
                Payload = JsonSerializer.Serialize(createdProduct)
            });

            return Ok(createdProduct);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] Product product)
        {
            if (id != product.Id) return BadRequest();

            var updatedProduct = await _productEngine.UpdateProductAsync(product);

            // Publicar evento
            await _publisherEngine.PublishProductMessageAsync(new ProductMessage
            {
                ProductId = updatedProduct.Id,
                EventType = EnumQueue.update.ToString(),
                Timestamp = DateTime.UtcNow,
                Payload = JsonSerializer.Serialize(updatedProduct)
            });

            return Ok(updatedProduct);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _productEngine.GetProductByIdAsync(id);
            if (product == null) return NotFound();

            await _productEngine.DeleteProductAsync(id);

            // Publicar evento
            await _publisherEngine.PublishProductMessageAsync(new ProductMessage
            {
                ProductId = id,
                EventType = EnumQueue.delete.ToString(),
                Timestamp = DateTime.UtcNow,
                Payload = JsonSerializer.Serialize(product)
            });

            return NoContent();
        }
    }
}
