using ElderSense.Data;
using ElderSense.Data.Model;
using ElderSense.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ElderSense.Services
{
    /// <summary>
    /// Classe que envia dados falsos para a tabela DadosMonitorizacao.
    /// Simula o funcionamento da monitorização do idoso no sistema, enviando dados de sua rotina
    /// </summary>
    public class SimuController
    {
        /// <summary>
        /// Contexto da base de dados, usado para criar e remover registos de simulação
        /// </summary>
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Contexto do Hub de SignalR, usado para notificar os clientes ligados em tempo real
        /// </summary>
        private readonly IHubContext<AlertaHub> _hubContext;

        /// <summary>
        /// Construtor que recebe as dependências injetadas pelo sistema
        /// </summary>
        public SimuController(ApplicationDbContext context, IHubContext<AlertaHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        /// <summary>
        /// Executa um ciclo completo de simulação: gera leituras para os sensores ativos
        /// (Beacon com 40% de probabilidade de passagem; Pulseira com temperatura e bpm,
        /// criando Alerta quando o bpm sai do intervalo saudável) (Fase 1),
        /// remove os registos mais antigos para manter no máximo 10 por sensor (Fase 2)
        /// e limita os Alertas aos mais recentes, limpando primeiro as ligações M:N (Fase 2b)
        /// </summary>
        public async Task InjetarDadosDeTesteAsync()
        {
            var sensoresAtivos = await _context.Sensores
                                   .Where(s => s.Estado == true && !s.Arquivado)    
                                   .ToListAsync();

            if (!sensoresAtivos.Any()) return;

            var random = new Random();

            // FASE 1: criar e gravar os dados novos
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
                    string donoPulseira = sensor.FKIdoso ?? sensor.FKUtilizador;

                    var registoTemperatura = new DadosMonitorizacao
                    {
                        DataHora = DateTime.Now,
                        FKSensor = sensor.Id,
                        FKUtilizador = donoPulseira,
                        Tipo = "Temperatura Corporal",
                        Valor = (random.Next(350, 380) / 10.0).ToString("0.0") + " ºC"
                    };
                    _context.DadosMonitorizacao.Add(registoTemperatura);

                    int bpm = random.Next(110, 121);
                    var registoBpm = new DadosMonitorizacao
                    {
                        DataHora = DateTime.Now,
                        FKSensor = sensor.Id,
                        FKUtilizador = donoPulseira,
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

            // FASE 2: limpar o lixo (garante no máximo 10 registos por sensor)
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
                    //corta as ligações antes de apagar
                    foreach (var dado in dadosAntigos)
                    {
                        // Isto apaga apenas a ligação na tabela intermédia, libertando os dados
                        dado.ListadeAlertas.Clear();
                    }

                    // apaga os dados com permissão do SQL Server
                    _context.DadosMonitorizacao.RemoveRange(dadosAntigos);
                }
            }

            // FASE 2b: limitar os Alertas (mantém apenas os mais recentes)
            var limiteDeAlertas = 30;
            var alertasAntigos = await _context.Alertas
                .Include(a => a.ListadeDados)
                .OrderByDescending(a => a.DataHora)
                .Skip(limiteDeAlertas)
                .ToListAsync();

            if (alertasAntigos.Any())
            {
                // corta as ligações M:N na tabela intermédia antes de apagar (mesmo padrão da limpeza dos dados)
                foreach (var alerta in alertasAntigos)
                {
                    alerta.ListadeDados.Clear();
                }

                // apaga os alertas antigos com permissão do SQL Server
                _context.Alertas.RemoveRange(alertasAntigos);
            }

            //Executa a limpeza final
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Cria um Alerta na base de dados, associa-o ao registo de dados que o despoletou
        /// (relacionamento M:N Alerta-DadosMonitorizacao), vai buscar o nome do idoso associado,
        /// e notifica em tempo real os clientes ligados via SignalR (mensagem + nome do idoso)
        /// </summary>
        private async Task CriarAlertaAsync(string idCuidador, string mensagem, DadosMonitorizacao dadoOrigem)
        {
            string idDoIdoso = dadoOrigem.FKUtilizador;

            var idosoExiste = await _context.Utilizadores.AnyAsync(u => u.Id == idDoIdoso);

            if (!idosoExiste)
            {
                // O idoso não existe na base de dados (foi apagado ou o ID está errado).
                // Abortamos a operação silenciosamente para não rebentar o servidor com o Erro 547.
                return;
            }

            var novoAlerta = new Alerta
            {
                DataHora = DateTime.Now,
                Mensagem = mensagem,
                FKIdoso = idDoIdoso,
                FKUtilizador = idCuidador
            };

            // associa o alerta ao registo de dados que o causou (preenche a tabela junction M:N)
            novoAlerta.ListadeDados.Add(dadoOrigem);

            _context.Alertas.Add(novoAlerta);
            await _context.SaveChangesAsync();

            // vai buscar o nome do idoso para incluir na notificação
            var idoso = await _context.Utilizadores.FindAsync(idDoIdoso);
            var nomeIdoso = idoso?.Nome ?? "Desconhecido";

            // Notifica todos os clientes ligados ao Hub que há um alerta novo
            await _hubContext.Clients.All.SendAsync("NovoAlerta", mensagem, nomeIdoso);
        }
    }
}