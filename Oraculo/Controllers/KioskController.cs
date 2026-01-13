using Microsoft.AspNetCore.Mvc;
using Oraculo.Data.Repositories;
using Oraculo.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Oraculo.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class KioskController : ControllerBase
    {
        private readonly IKioskRepository _iKioskRepository;

        public KioskController(IKioskRepository iIKioskRepository)
        {
            _iKioskRepository = iIKioskRepository;
        }

        // GET api/Kiosk/Stock/{environment}/{whsCode}/{priceList}
        [HttpGet("Stock/{environment}/{whsCode}/{priceList}")]
        public async Task<IActionResult> GetKioskStockComplete(
            int environment,
            string whsCode,
            string priceList
        )
        {
            try
            {
                var stock = await _iKioskRepository
                    .GetKioskStockComplete(environment, whsCode, priceList);

                var response = new Response<List<KioskStockComplete>>
                {
                    Code = 200,
                    Description = "Consulta exitosa",
                    Data = stock
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
