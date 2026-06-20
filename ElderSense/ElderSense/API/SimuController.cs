using ElderSense.Data;
using ElderSense.Data.Model;
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

        public SimuController(ApplicationDbContext context)
        {
            _context = context;
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
                    // Gera um ritmo cardíaco normal (ex: entre 60 e 90 bpm)
                    novoRegisto.Valor = random.Next(60, 91).ToString() + " bpm";
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
    }
}