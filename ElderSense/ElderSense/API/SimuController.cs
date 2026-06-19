using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using ElderSense.Data.Model; 

namespace ElderSense.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SimuladorController : ControllerBase
    {
        // Se já tiveres o teu ApplicationDbContext ou o contexto da BD criado, 
        // injeta-o aqui no construtor para poderes gravar os dados falsos.

        public SimuladorController()
        {
        }

        [HttpPost("gerar-dados-teste")]
        public async Task<IActionResult> GerarDadosFalsos()
        {
            // O código para gerar os batimentos e alertas vai ficar aqui dentro!
            return Ok(new { mensagem = "Simulador pronto a funcionar!" });
        }
    }
}