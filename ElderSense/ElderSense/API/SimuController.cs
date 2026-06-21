using ElderSense.Data;
using ElderSense.Data.Model;
using ElderSense.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ElderSense.Services
{

    /// <summary>/// 
    /// Classe que envia dados falsos para a tabela DadosMonitorizacao.
    /// Simula o funcionamento da monitorização do idoso no sistema, enviando dados de sua rotina
    /// </summary>
    public class SimuController
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<AlertaHub> _hubContext;

        public SimuController(ApplicationDbContext context, IHubContext<AlertaHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task InjetarDadosDeTesteAsync()
        {
            var sensoresAtivos = await _context.Sensores
                                               .Where(s => s.Estado == true)
                                               .ToListAsync();

            if (!sensoresAtivos.Any()) return;

            var random = new Random();

            foreach (var sensor in sensoresAtivos)
            {
                // 1. CRIAR O DADO NOVO ASSOCIADO AO SENSOR EXISTENTE
                var novoRegisto = new DadosMonitorizacao
                {
                    DataHora = DateTime.Now,
                    FKSensor = sensor.Id,
                    FKUtilizador = sensor.FKUtilizador,
                };

                // 2. Decide que tipo de dado gerar com base no hardware físico simulado
                if (sensor.Localizacao.Contains("Porta"))
                {
                    novoRegisto.Tipo = "Abertura";

                    novoRegisto.Valor = random.Next(0, 2) == 0 ? "Aberta" : "Fechada";
                }
                else if (sensor.Localizacao.Contains("Cama") || sensor.Localizacao.Contains("Quarto"))
                {
                    novoRegisto.Tipo = "Movimento";
                    novoRegisto.Valor = "Detetado";
                }
                else if (sensor.Localizacao.Contains("Pulseira"))
                {
                    novoRegisto.Tipo = "Frequência Cardíaca";
                    // Gera um ritmo cardíaco, ocasionalmente fora do normal para testar os alertas
                    int bpm = random.Next(40, 121);
                    novoRegisto.Valor = bpm.ToString() + " bpm";

                    // 2.1 Verifica se o valor é anómalo (fora do intervalo normal 50-100 bpm)
                    if (bpm < 50 || bpm > 100)
                    {
                        await CriarAlertaAsync(sensor.FKUtilizador, $"Frequência cardíaca fora do normal: {bpm} bpm", novoRegisto);
                    }
                }
                else
                {
                    // Um valor padrão (ex: temperatura ambiente) caso a localização não seja nenhuma das de cima
                    novoRegisto.Tipo = "Temperatura";
                    novoRegisto.Valor = random.Next(18, 26).ToString() + " ºC";
                }

                // 3. Adiciona o registo formatado à base de dados
                _context.DadosMonitorizacao.Add(novoRegisto);


                // Vai buscar todos os dados deste sensor, ordenados do mais recente para o mais antigo.
                // O .Skip(50) ignora os 50 mais recentes e seleciona todos os que sobrarem (o "lixo" antigo).
                var limiteDeRegistos = 50;
                var dadosAntigos = await _context.DadosMonitorizacao
                                                 .Where(d => d.FKSensor == sensor.Id)
                                                 .OrderByDescending(d => d.DataHora)
                                                 .Skip(limiteDeRegistos)
                                                 .ToListAsync();

                // Se encontrou dados antigos além do limite, apaga-os da BD
                if (dadosAntigos.Any())
                {
                    _context.DadosMonitorizacao.RemoveRange(dadosAntigos);
                }
            }

            // 3. Executa a inserção dos novos e a limpeza dos antigos tudo de uma vez
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Cria um Alerta na base de dados, associa-o ao registo de dados que o despoletou
        /// (relacionamento M:N Alerta-DadosMonitorizacao), e notifica em tempo real os clientes
        /// ligados via SignalR
        /// </summary>
        private async Task CriarAlertaAsync(string fkUtilizador, string mensagem, DadosMonitorizacao dadoOrigem)
        {
            var novoAlerta = new Alerta
            {
                DataHora = DateTime.Now,
                Mensagem = mensagem,
                FKUtilizador = fkUtilizador
            };

            // associa o alerta ao registo de dados que o causou (preenche a tabela junction M:N)
            novoAlerta.ListadeDados.Add(dadoOrigem);

            _context.Alertas.Add(novoAlerta);
            await _context.SaveChangesAsync();

            // Notifica todos os clientes ligados ao Hub que há um alerta novo
            await _hubContext.Clients.All.SendAsync("NovoAlerta", mensagem);
        }
    }
}