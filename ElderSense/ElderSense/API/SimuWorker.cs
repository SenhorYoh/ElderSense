using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ElderSense.Services
{
    /// <summary>
    /// Classe que trabalha em conjunto com SimuController.cs,
    /// enviando os dados de 1 em 1 minuto para o DadosMonitorizacao
    /// </summary>
    public class SimuWorker : BackgroundService
    {
        /// <summary>
        /// Fornecedor de serviços usado para criar um scope e resolver dependências com âmbito (scoped)
        /// </summary>
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Logger usado para registar o estado e os erros do simulador
        /// </summary>
        private readonly ILogger<SimuWorker> _logger;

        /// <summary>
        /// Construtor que recebe as dependências injetadas pelo sistema
        /// </summary>
        public SimuWorker(IServiceProvider serviceProvider, ILogger<SimuWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        /// <summary>
        /// Ciclo principal do serviço em segundo plano: cria um scope temporário,
        /// invoca o SimuController para injetar dados de teste, e repete periodicamente
        /// enquanto a aplicação estiver ativa
        /// </summary>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("O Worker do Simulador do ElderSense foi iniciado.");

            // O ciclo corre continuamente enquanto o site estiver ativo
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Simulador a injetar dados na Base de Dados...");

                    // Como os BackgroundServices vivem fora do ciclo normal das páginas,
                    // precisamos de criar um "Scope" temporário para usar o ApplicationDbContext com segurança
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var simulador = scope.ServiceProvider.GetRequiredService<SimuController>();

                        // Chama o método que cria os dados novos e apaga os antigos além do limite
                        await simulador.InjetarDadosDeTesteAsync();
                    }

                    _logger.LogInformation("Dados injetados com sucesso. Próximo envio em 1 minuto.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ocorreu um erro ao correr o simulador de sensores.");
                }

                // Envia os dados de 1 em 1 minuto
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}