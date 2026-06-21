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
                // 1. Decide que tipo(s) de dado gerar com base no hardware físico do sensor
                if (sensor.Tipo == TipoSensor.Beacon)
                {
                    // Um Beacon só sabe registar quando deteta a passagem da pulseira/tag por perto.
                    // Não regista "ausência" - simplesmente não gera registo nenhum se não houver passagem.
                    bool detetado = random.Next(0, 100) < 40; // 40% de hipótese de deteção neste ciclo

                    if (!detetado)
                    {
                        continue; // não passou por aqui neste ciclo, não há nada a registar
                    }

                    var registoPassagem = new DadosMonitorizacao
                    {
                        DataHora = DateTime.Now,
                        FKSensor = sensor.Id,
                        FKUtilizador = sensor.FKUtilizador,
                        Tipo = "Passagem",
                        Valor = "Detetado"
                    };

                    _context.DadosMonitorizacao.Add(registoPassagem);
                }
                else if (sensor.Tipo == TipoSensor.Pulseira)
                {
                    // A pulseira está no corpo do idoso, por isso gera dois sinais vitais em simultâneo:
                    // temperatura corporal e frequência cardíaca

                    // 2.1 Temperatura corporal
                    var registoTemperatura = new DadosMonitorizacao
                    {
                        DataHora = DateTime.Now,
                        FKSensor = sensor.Id,
                        FKUtilizador = sensor.FKUtilizador,
                        Tipo = "Temperatura Corporal",
                        Valor = (random.Next(350, 380) / 10.0).ToString("0.0") + " ºC"
                    };
                    _context.DadosMonitorizacao.Add(registoTemperatura);

                    // 2.2 Frequência cardíaca, ocasionalmente fora do normal para testar os alertas
                    int bpm = random.Next(40, 121);
                    var registoBpm = new DadosMonitorizacao
                    {
                        DataHora = DateTime.Now,
                        FKSensor = sensor.Id,
                        FKUtilizador = sensor.FKUtilizador,
                        Tipo = "Frequência Cardíaca",
                        Valor = bpm.ToString() + " bpm"
                    };
                    _context.DadosMonitorizacao.Add(registoBpm);

                    // Verifica se o valor é anómalo (fora do intervalo normal 50-100 bpm)
                    if (bpm < 50 || bpm > 100)
                    {
                        await CriarAlertaAsync(sensor.FKUtilizador, $"Frequência cardíaca fora do normal: {bpm} bpm", registoBpm);
                    }
                }

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

            // Executa a inserção dos novos e a limpeza dos antigos tudo de uma vez
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Cria um Alerta na base de dados, associa-o ao registo de dados que o despoletou
        /// (relacionamento M:N Alerta-DadosMonitorizacao), vai buscar o nome do idoso associado,
        /// e notifica em tempo real os clientes ligados via SignalR (mensagem + nome do idoso)
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

            // vai buscar o nome do idoso para incluir na notificação
            var idoso = await _context.Utilizadores.FindAsync(fkUtilizador);
            var nomeIdoso = idoso?.Nome ?? "Desconhecido";

            // Notifica todos os clientes ligados ao Hub que há um alerta novo
            await _hubContext.Clients.All.SendAsync("NovoAlerta", mensagem, nomeIdoso);
        }
    }
}