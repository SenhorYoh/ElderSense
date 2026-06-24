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

            // ==========================================
            // FASE 1: CRIAR E GRAVAR OS DADOS NOVOS
            // ==========================================
            foreach (var sensor in sensoresAtivos)
            {
                if (sensor.Tipo == TipoSensor.Beacon)
                {
                    bool detetado = random.Next(0, 100) < 40;
                    if (!detetado) continue;

                    //escolhe um idoso associado ao cuidador
                    string idDonoDado = sensor.FKUtilizador; // Por defeito, fica o Cuidador caso não haja idosos

                    var cuidador = await _context.Set<Utilizador>()
                                 .Include(u => u.ListadeIdosos)
                                 .FirstOrDefaultAsync(u => u.Id == sensor.FKUtilizador);

                    // Se o cuidador existir e tiver idosos na lista, faz o sorteio
                    if (cuidador != null && cuidador.ListadeIdosos.Any())
                    {
                        var listaIdosos = cuidador.ListadeIdosos.ToList();
                        idDonoDado = listaIdosos[random.Next(listaIdosos.Count)].Id; // Sorteia um e guarda o ID
                    }

                    var registoPassagem = new DadosMonitorizacao
                    {
                        DataHora = DateTime.Now,
                        FKSensor = sensor.Id,
                        FKUtilizador = idDonoDado,
                        Tipo = "Passagem",
                        Valor = "Detetado"
                    };
                    _context.DadosMonitorizacao.Add(registoPassagem);
                }
                else if (sensor.Tipo == TipoSensor.Pulseira)
                {
                    // Para a pulseira, usamos o idoso que está associado a ela de forma fixa
                    // (Se por algum motivo falhar, usa o do Cuidador)
                    string DonoPulseira = sensor.FKIdoso ?? sensor.FKUtilizador;

                    var registoTemperatura = new DadosMonitorizacao
                    {
                        DataHora = DateTime.Now,
                        FKSensor = sensor.Id,
                        FKUtilizador = DonoPulseira,
                        Tipo = "Temperatura Corporal",
                        Valor = (random.Next(350, 380) / 10.0).ToString("0.0") + " ºC"
                    };
                    _context.DadosMonitorizacao.Add(registoTemperatura);

                    int bpm = random.Next(40, 121);
                    var registoBpm = new DadosMonitorizacao
                    {
                        DataHora = DateTime.Now,
                        FKSensor = sensor.Id,
                        FKUtilizador = DonoPulseira,
                        Tipo = "Frequência Cardíaca",
                        Valor = bpm.ToString() + " bpm"
                    };
                    _context.DadosMonitorizacao.Add(registoBpm);

                    if (bpm < 50 || bpm > 100)
                    {
                        await CriarAlertaAsync(sensor.FKUtilizador, $"Frequência cardíaca fora do normal: {bpm} bpm", registoBpm);
                    }
                }
            }

            //Grava os dados novos na base de dados para a contagem ficar certa!
            await _context.SaveChangesAsync();


            // ==========================================
            // FASE 2: LIMPAR O LIXO (Garante 10 por sensor)
            // ==========================================
            foreach (var sensor in sensoresAtivos)
            {
                var limiteDeRegistos = 10;
                var dadosAntigos = await _context.DadosMonitorizacao
                .Include(d => d.ListadeAlertas)
                .Where(d => d.FKSensor == sensor.Id)
                .OrderByDescending(d => d.DataHora)
                .Skip(limiteDeRegistos)
                .ToListAsync();


                if (dadosAntigos.Any())
                {
                    //CORTAR AS LIGAÇÕES ANTES DE APAGAR
                    foreach (var dado in dadosAntigos)
                    {
                        // Isto apaga apenas a ligação na tabela intermédia, libertando os dados
                        dado.ListadeAlertas.Clear();
                    }

                    // apaga os dados com permissão do SQL Server
                    _context.DadosMonitorizacao.RemoveRange(dadosAntigos);
                }
            }

            //Executa a limpeza final
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