using Oraculo.Models;

namespace Oraculo.Data.Repositories
{
    public interface IR2QInvoicesRepository
    {
        Task<List<R2QPurchaseInvoices>> GetPurchaseInvoices(int environment, DateTime fecha);
        Task<List<R2QSalesInvoices>> GetSalesInvoices(int environment, DateTime fecha);
    }
}
