using Microsoft.AspNetCore.Mvc;
using ElderSense.Data;
using ElderSense.Data.Model;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System;

namespace ElderSense.Controllers
{

    /// <summary>
    /// Classe da API. Tem as rotas necessárias para a comunicação com o hardware
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class SensoresApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SensoresApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        //POST: Guarda dados de monitorizacao do idoso na tabela DadosMonitorizacao
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

           
            return Ok(new { mensagem = "Leitura guardada e associada ao idoso com sucesso" });
        }


        // GET: Devolve o histórico de leituras de um idoso específico
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

        // GET: Verifica se o sensor está ativo/online com base na última leitura
        //o hardware verifica se o sensor existe e está ativo 
        [HttpGet("estado/{sensorId}")]
        public async Task<IActionResult> VerificarEstadoSensor(int sensorId)
        {
            // Vai à tabela buscar a leitura mais recente deste sensor específico
            var ultimaLeitura = await _context.DadosMonitorizacao
                                              .Where(d => d.FKSensor == sensorId) // Confirma se o teu campo se chama exatamente 'SensorId'
                                              .OrderByDescending(d => d.DataHora)
                                              .FirstOrDefaultAsync();

            if (ultimaLeitura == null)
            {
                return Ok(new
                {
                    sensorId = sensorId,
                    estado = "Desconectado",
                    mensagem = "Este dispositivo nunca enviou dados para o servidor."
                });
            }

            var tempoPassado = DateTime.Now - ultimaLeitura.DataHora;
            double minutosAusente = tempoPassado.TotalMinutes;

            // Se comunicou há menos de 5 minutos, está ativo. Caso contrário, está offline.
            string estadoAtual = minutosAusente <= 5 ? "Online" : "Offline";

            //Devolve o relatório completo sobre o estado do hardware
            return Ok(new
            {
                sensorId = sensorId,
                estado = estadoAtual,
                minutosDesdeUltimaLeitura = Math.Round(minutosAusente, 1),
                ultimaComunicacao = ultimaLeitura.DataHora,
                ultimoTipoDado = ultimaLeitura.Tipo,
                ultimoValorDado = ultimaLeitura.Valor,
                pacienteId = ultimaLeitura.FKUtilizador
            });
        }

        //GET: Rota que verifica se a API está online
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok(new
            {
                status = "Online",
                mensagem = "Pong! A API do ElderSense está a escutar perfeitamente.",
                horaServidor = DateTime.Now
            });
        }



    }

    public class DadosSensor
    {
        public string IdosoId { get; set; } = string.Empty;

        public int SensorId { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string Valor { get; set; } = string.Empty;
    }
}