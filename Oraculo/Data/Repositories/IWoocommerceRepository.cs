using Oraculo.Models;

namespace Oraculo.Data.Repositories
{
    public interface IWoocommerceRepository
    {
        Task<List<WoocommerceStockComplete>> GetStockComplete(int environment);
        Task<List<WoocommercePriceUpdate>> GetPriceUpdates(int environment);
        Task<List<WoocommerceStockComplete>> GetAllStockByBranch(int environment, string branch);
        Task<WoocommerceStockComplete> GetAllStockByBranchByProduct(int environment, string branch, string item);
    }
}
