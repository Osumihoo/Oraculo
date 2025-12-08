using Microsoft.AspNetCore.Mvc;
using Oraculo.Data.Repositories;
using Oraculo.Models;
using System.Text.RegularExpressions;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Oraculo.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ITController : ControllerBase
    {
        private readonly IITRepository _iTRepository;

        public ITController(IITRepository iTRepository)
        {
            _iTRepository = iTRepository;
        }

        // GET api/Stock/{environment}/{whsCode}
        [HttpGet("Stock/{environment}/{whsCode}")]
        public async Task<IActionResult> GetStockByWh(int environment, string whsCode)
        {
            try
            {
                var stock = await _iTRepository.GetStockByWh(environment, whsCode);

                var response = new Response<List<ITStockByWh>>
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
