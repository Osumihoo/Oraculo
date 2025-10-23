using Microsoft.AspNetCore.Mvc;
using Oraculo.Data.Repositories;
using Oraculo.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Oraculo.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class R2QInvoicesController : ControllerBase
    {
        private readonly IR2QInvoicesRepository _invoicesRepository;

        public R2QInvoicesController(IR2QInvoicesRepository invoicesRepository)
        {
            _invoicesRepository = invoicesRepository;
        }
        // GET: api/<PurchaseInvoicesController>/purchase/0
        [HttpGet("Purchase/{environment}/{fecha}")]
        public async Task<IActionResult> GetPurchaseInvoices(int environment, DateTime fecha)
        {
            try
            {
                var invoices = await _invoicesRepository.GetPurchaseInvoices(environment, fecha);

                var response = new Response<List<R2QPurchaseInvoices>>
                {
                    Code = 200,
                    Description = "Consulta exitosa",
                    Data = invoices
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                var error = new Response<string>
                {
                    Code = 500,
                    Description = "Error al obtener los datos: " + ex.Message,
                    Data = null
                };

                return StatusCode(500, error);
            }
        }

        // GET: api/invoices/sales/0
        [HttpGet("Sales/{environment}/{fecha}")]
        public async Task<IActionResult> GetSalesInvoices(int environment, DateTime fecha)
        {
            try
            {
                var sales = await _invoicesRepository.GetSalesInvoices(environment, fecha);

                var response = new Response<List<R2QSalesInvoices>>
                {
                    Code = 200,
                    Description = "Consulta exitosa",
                    Data = sales
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                var error = new Response<string>
                {
                    Code = 500,
                    Description = "Error al obtener los datos: " + ex.Message,
                    Data = null
                };

                return StatusCode(500, error);
            }
        }
    }
}
