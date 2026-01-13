using Oraculo.Models;

namespace Oraculo.Data.Repositories
{
    public interface IKioskRepository
    {
        Task<List<KioskStockComplete>> GetKioskStockComplete(int environment, string whsCode, string priceList);
    }
}
