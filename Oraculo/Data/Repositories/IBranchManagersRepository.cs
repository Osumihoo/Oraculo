using Oraculo.Models;

namespace Oraculo.Data.Repositories
{
    public interface IBranchManagersRepository
    {
        Task<List<Dictionary<string, object>>> GetStockResupply(int environment, string grupo, string sucursal);
        Task<List<Dictionary<string, object>>> GetStockResupplyWOCV(int environment, string sucursal);
        Task<List<Dictionary<string, object>>> GetStockZeroResupply(int environment, string family);
        Task<List<Dictionary<string, object>>> GetLast30DaysByFamily(int environment, string family);
        Task<List<BranchStockResupplyJuan>> GetBranchStockResupply(int environment);
        Task<List<PriceList>> GetPriceList(int environment);
        Task<List<CostList>> GetCostList(int environment);
    }
}
