using Microsoft.AspNetCore.Mvc;
using Oraculo.Data.Repositories;
using System;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Oraculo.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class RRHHController : ControllerBase
    {
        private readonly IRRHHRepository _RRHHrepository;

        public RRHHController(IRRHHRepository RRHHrepository)
        {
            _RRHHrepository = RRHHrepository;
        }

        [HttpGet("VariousDebtors/{environment}")]
        public async Task<IActionResult> Get(int environment)
        {
            var result = await _RRHHrepository.GetVariousDebtorsAsync(environment);
            return Ok(new
            {
                code = 200,
                description = "Consulta exitosa",
                data = result
            });
        }

        [HttpGet("TimeClock/{environment}")]
        public async Task<IActionResult> GetTimeClock(int environment)
        {
            var result = await _RRHHrepository.GetRRHHTimeClock(environment);

            return Ok(new
            {
                code = 200,
                description = "Consulta exitosa",
                data = result
            });
        }
    }
}
