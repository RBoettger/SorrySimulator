using Microsoft.AspNetCore.Mvc;
using ExcusesService.Helpers;
using System.Threading.Tasks;

namespace ExcusesService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExcuseController : ControllerBase
    {
        private readonly ExcuseGeneratorClient _generator;

        public ExcuseController(ExcuseGeneratorClient generator)
        {
            _generator = generator;
        }

        [HttpPost("generate")]
        public async Task<IActionResult> Generate([FromBody] GenerateRequest req)
        {
            var result = await _generator.GerarDesculpaAsync(req.Nome, req.Motivo);
            return Ok(result);
        }

        public class GenerateRequest
        {
            public string Nome { get; set; }
            public string Motivo { get; set; }
        }
    }
}
