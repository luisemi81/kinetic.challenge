using Kinetic.Common.DTO;

namespace Kinetic.Inventory.API.Engines.Interfaces
{
    public interface IPublisherEngine
    {
        Task PublishProductMessageAsync(ProductMessage productMessage);
    }
}
