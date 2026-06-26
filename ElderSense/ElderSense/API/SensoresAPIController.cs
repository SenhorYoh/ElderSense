using Microsoft.AspNetCore.Mvc;
using ElderSense.Data;
using ElderSense.Data.Model;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System;

namespace ElderSense.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SensoresApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SensoresApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("leitura")]
        public async Task<IActionResult> ReceberLeituraDoHardware([FromBody] DadosSensor Dto)
        {
            if (Dto == null) return BadRequest("Nenhum dado recebido.");



            // Impede que o hardware envie leituras sem dizer de quem são
            if (string.IsNullOrEmpty(Dto.IdosoId))
            {
                return BadRequest("O ID do idoso é obrigatório.");
            }

            // Mapeamento: Pacote do Postman -> Tabela da Base de Dados
            var novaLeitura = new DadosMonitorizacao
            {

                FKUtilizador = Dto.IdosoId,
                FKSensor = Dto.SensorId,
                Tipo = Dto.Tipo,
                Valor = Dto.Valor,
                DataHora = DateTime.Now
            };

            // Guarda na tabela e associa ao Idoso
            _context.DadosMonitorizacao.Add(novaLeitura);
            await _context.SaveChangesAsync();

            return Ok(new { mensagem = "Leitura guardada e associada ao idoso com sucesso!" });
        }


        //  Devolve o histórico de leituras de um idoso específico
        [HttpGet("historico/{idosoId}")]
        public async Task<IActionResult> ObterHistorico(string idosoId)
        {
            // Vai à tabela DadosMonitorizacao procurar tudo o que pertence a este idoso
            var historico = await _context.DadosMonitorizacao
                                          .Where(d => d.FKUtilizador == idosoId)
                                          .OrderByDescending(d => d.DataHora) // Mostra os mais recentes primeiro
                                          .ToListAsync();

            if (historico.Count == 0)
            {
                return NotFound(new { mensagem = "Nenhum dado encontrado para este idoso." });
            }

            return Ok(historico);
        }

        
    }

    // O formato do pacote JSON que a API vai aceitar
    public class DadosSensor
    {
        public string IdosoId { get; set; } = string.Empty; // O ASP.NET Identity usa strings (GUIDs) para os IDs

        public int SensorId { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string Valor { get; set; } = string.Empty;
    }
}