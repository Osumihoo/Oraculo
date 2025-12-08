using Oraculo.Models;

namespace Oraculo.Data.Repositories
{
    public interface IITRepository
    {
        Task<List<ITStockByWh>> GetStockByWh(int environment, string whsCode);
    }
}
