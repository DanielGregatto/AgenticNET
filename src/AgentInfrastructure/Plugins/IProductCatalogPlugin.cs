using System.Threading;
using System.Threading.Tasks;

namespace AgentInfrastructure.Plugins
{
    public interface IProductCatalogPlugin
    {
        Task<string> ListProducts(CancellationToken cancellationToken = default);
        Task<string> GetProductById(string id, CancellationToken cancellationToken = default);
        Task<string> SearchProducts(string keyword, CancellationToken cancellationToken = default);
    }
}
