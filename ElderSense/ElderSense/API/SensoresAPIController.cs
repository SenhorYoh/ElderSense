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
        /// <summary>
        /// Contexto da base de dados, usado para guardar e consultar as leituras dos sensores
        /// </summary>
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Construtor que recebe o contexto da base de dados injetado pelo sistema
        /// </summary>
        public SensoresApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Recebe uma leitura enviada pelo hardware e guarda-a na tabela DadosMonitorizacao
        /// </summary>
        [HttpPost("leitura")]
        public async Task<IActionResult> ReceberLeituraDoHardware([FromBody] DadosSensor Dto)
        {
            if (Dto == null) return BadRequest("Nenhum dado recebido.");

            // Impede que o hardware envie leituras sem dizer de quem são
            if (string.IsNullOrEmpty(Dto.IdosoId))
            {
                return BadRequest("O ID do idoso é obrigatório.");
            }

            // Mapeia os dados recebidos para a entidade DadosMonitorizacao
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

        /// <summary>
        /// Devolve o histórico de leituras de um idoso específico, mais recentes primeiro
        /// </summary>
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

        /// <summary>
        /// Verifica se o sensor está ativo/online com base na última leitura recebida.
        /// O hardware usa esta rota para confirmar se o sensor existe e está ativo
        /// </summary>
        [HttpGet("estado/{sensorId}")]
        public async Task<IActionResult> VerificarEstadoSensor(int sensorId)
        {
            // Vai à tabela buscar a leitura mais recente deste sensor específico
            var ultimaLeitura = await _context.DadosMonitorizacao
                                              .Where(d => d.FKSensor == sensorId)
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

        /// <summary>
        /// Rota que verifica se a API está online
        /// </summary>
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
        /// <summary>
        /// Rota que atualiza o tipo e/ou o valor de uma leitura existente
        /// </summary>
        [HttpPut("leitura/{id}")]
        public async Task<IActionResult> AtualizarLeitura(int id, [FromBody] AtualizarDadosSensor Dto)
        {
            if (Dto == null) return BadRequest("Nenhum dado recebido.");

            if (string.IsNullOrEmpty(Dto.Tipo) && string.IsNullOrEmpty(Dto.Valor))
                return BadRequest("É necessário indicar o Tipo e/ou o Valor a atualizar.");

            // procura a leitura na base de dados
            var leitura = await _context.DadosMonitorizacao.FindAsync(id);
            if (leitura == null)
                return NotFound(new { mensagem = "Leitura não encontrada." });

            // atualiza apenas os campos permitidos (as FKs não são alteráveis via API)
            if (!string.IsNullOrEmpty(Dto.Tipo)) leitura.Tipo = Dto.Tipo;
            if (!string.IsNullOrEmpty(Dto.Valor)) leitura.Valor = Dto.Valor;

            await _context.SaveChangesAsync();

            return Ok(new { mensagem = "Leitura atualizada com sucesso.", leitura.Id, leitura.Tipo, leitura.Valor });
        }

        /// <summary>
        /// Rota que elimina uma leitura, cortando primeiro as ligações M:N com os alertas
        /// </summary>
        [HttpDelete("leitura/{id}")]
        public async Task<IActionResult> EliminarLeitura(int id)
        {
            // procura a leitura incluindo as ligações aos alertas, para poder limpar a junção M:N
            var leitura = await _context.DadosMonitorizacao
                .Include(d => d.ListadeAlertas)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (leitura == null)
                return NotFound(new { mensagem = "Leitura não encontrada." });

            // corta as ligações na tabela intermédia antes de apagar (o lado dos dados está em Restrict)
            leitura.ListadeAlertas.Clear();

            _context.DadosMonitorizacao.Remove(leitura);
            await _context.SaveChangesAsync();

            return Ok(new { mensagem = "Leitura eliminada com sucesso.", id });
        }
        /// <summary>
        /// Arquiva um sensor (soft delete): marca-o como arquivado e desativado,
        /// mas preserva todas as leituras dele como histórico consultável
        /// </summary>
        [HttpPut("arquivar/{id}")]
        public async Task<IActionResult> ArquivarSensor(int id)
        {
            var sensor = await _context.Sensores.FindAsync(id);
            if (sensor == null)
                return NotFound(new { mensagem = "Sensor não encontrado." });

            if (sensor.Arquivado)
                return BadRequest(new { mensagem = "Este sensor já se encontra arquivado." });

            // soft delete: não apaga o sensor nem as leituras, apenas o marca como arquivado e inativo
            sensor.Arquivado = true;
            sensor.Estado = false;

            await _context.SaveChangesAsync();

            return Ok(new { mensagem = "Sensor arquivado com sucesso. As leituras foram preservadas como histórico.", sensor.Id });
        }

        /// <summary>
        /// Reativa um sensor arquivado: retira-o do arquivo mas mantém-no desligado.
        /// O utilizador terá de o ativar manualmente depois, se quiser
        /// </summary>
        [HttpPut("desarquivar/{id}")]
        public async Task<IActionResult> DesarquivarSensor(int id)
        {
            var sensor = await _context.Sensores.FindAsync(id);
            if (sensor == null)
                return NotFound(new { mensagem = "Sensor não encontrado." });

            if (!sensor.Arquivado)
                return BadRequest(new { mensagem = "Este sensor não está arquivado." });

            // retira do arquivo mas deixa desligado (o utilizador ativa manualmente depois)
            sensor.Arquivado = false;
            sensor.Estado = false;

            await _context.SaveChangesAsync();

            return Ok(new { mensagem = "Sensor desarquivado com sucesso. Ative-o manualmente quando pretender voltar a recolher dados.", sensor.Id });
        }

        /// <summary>
        /// Devolve o histórico de leituras de um sensor específico, mesmo que arquivado
        /// </summary>
        [HttpGet("historico-sensor/{sensorId}")]
        public async Task<IActionResult> ObterHistoricoDoSensor(int sensorId)
        {
            var sensor = await _context.Sensores.FindAsync(sensorId);
            if (sensor == null)
                return NotFound(new { mensagem = "Sensor não encontrado." });

            var historico = await _context.DadosMonitorizacao
                                          .Where(d => d.FKSensor == sensorId)
                                          .OrderByDescending(d => d.DataHora)
                                          .ToListAsync();

            return Ok(new
            {
                sensorId = sensor.Id,
                localizacao = sensor.Localizacao,
                arquivado = sensor.Arquivado,
                totalLeituras = historico.Count,
                historico
            });
        }
    }

    /// <summary>
    /// DTO que representa os dados enviados pelo hardware numa leitura de sensor
    /// </summary>
    public class DadosSensor
    {
        /// <summary>
        /// Identificador do idoso a quem a leitura pertence
        /// </summary>
        public string IdosoId { get; set; } = string.Empty;

        /// <summary>
        /// Identificador do sensor que gerou a leitura
        /// </summary>
        public int SensorId { get; set; }

        /// <summary>
        /// Tipo de dado recolhido, ex: movimento, temperatura, bpm
        /// </summary>
        public string Tipo { get; set; } = string.Empty;

        /// <summary>
        /// Valor associado ao tipo de dado
        /// </summary>
        public string Valor { get; set; } = string.Empty;
    }
    /// <summary>
    /// DTO que representa os campos alteráveis de uma leitura via API
    /// </summary>
    public class AtualizarDadosSensor
    {
        /// <summary>
        /// Novo tipo de dado (opcional)
        /// </summary>
        public string Tipo { get; set; } = string.Empty;

        /// <summary>
        /// Novo valor associado (opcional)
        /// </summary>
        public string Valor { get; set; } = string.Empty;
    }
}