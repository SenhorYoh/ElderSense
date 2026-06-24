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
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<SimuWorker> _logger;

        public SimuWorker(IServiceProvider serviceProvider, ILogger<SimuWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

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

                        // Chama o teu método que cria o dado e apaga os antigos além do limite
                        await simulador.InjetarDadosDeTesteAsync();
                    }

                    _logger.LogInformation("Dados injetados com sucesso. Próximo envio em 1 minuto.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ocorreu um erro ao correr o simulador de sensores.");
                }

                // Envia os dados de 1 em 1 minuto 
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }
}