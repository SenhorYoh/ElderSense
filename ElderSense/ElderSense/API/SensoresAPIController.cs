using Microsoft.AspNetCore.Mvc;
using System;

namespace ElderSense.Controllers
{
    //diz ao ASP.NET que este ficheiro é uma API para máquinas
    [ApiController]
    //A rota (URL) para aceder a esta API será: https://localhost:xxxx/api/SensoresApi
    [Route("api/[controller]")]
    public class SensoresApiController : ControllerBase
    {
        // O método POST (Para RECEBER os dados do hardware fictício)
        [HttpPost("leitura")]
        public IActionResult ReceberLeituraDoHardware([FromBody] DadosSensor Dto)
        {
            // Se o hardware enviar dados vazios, a API devolve um erro 400 (Bad Request)
            if (Dto == null)
            {
                return BadRequest(new { mensagem = "Nenhum dado recebido do sensor." });
            }

            // AQUI ENTRARIA O CÓDIGO DA BASE DE DADOS:
            // _context.DadosMonitorizacao.Add(novoDado);
            // _context.SaveChanges();

            // Para já, vamos apenas imprimir no terminal para ver a magia a acontecer:
            Console.WriteLine($"[ALERTA API] Recebido do Sensor {Dto.MacAddress}: {Dto.Bpm} BPM, {Dto.Temperatura}ºC");

            // A API responde ao sensor com um 200 (OK) a dizer que guardou tudo com sucesso
            return Ok(new { mensagem = "Dados gravados na base de dados com sucesso!", dadosRecebidos = Dto });
        }
    }

    // 📦 DTO (Data Transfer Object) - O formato do "pacote" que esperamos receber do sensor
    public class DadosSensor
    {
        public string MacAddress { get; set; } = string.Empty; // O ID único da pulseira
        public int Bpm { get; set; }                           // Batimentos cardíacos
        public double Temperatura { get; set; }                // Temperatura corporal
    }
}