using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ElderSense.Data;
using ElderSense.Data.Model;

namespace ElderSense.Pages.Sensores
{
    /// <summary>
    /// Página de criação dos sensores. Apenas um utilizador logado e do tipo Cuidador pode criar
    /// </summary>
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Utilizador> _userManager;

        public CreateModel(ApplicationDbContext context, UserManager<Utilizador> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public Sensor Sensor { get; set; } = new();

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // 1. Vai buscar o utilizador atualmente logado no sistema
            var user = await _userManager.GetUserAsync(User);

            // Segurança extra: se a sessão expirou ou o user é nulo, manda para o Login
            if (user == null)
            {
                return Challenge();
            }

            // 2. Associa o sensor ao ID do utilizador (Garante que na classe 'Sensor' a propriedade se chama 'FKUtilizador')
            Sensor.FKUtilizador = user.Id;
            Sensor.Utilizador = null!; //limpa qualquer lixo invisivel 

            // 3. Remove as validações automáticas do ModelState que costumam bloquear o formulário
            ModelState.Remove("Sensor.Utilizador");
            ModelState.Remove("Sensor.FKUtilizador"); // Remove esta também, já que a preenchemos via código e não via HTML

            /// <summary>
            /// Validação da localização do beacon inserido pelo utilizador
            /// O utilizador deve especificar o local com 'quarto', 'cozinha' etc
            /// para que o sistema envie os dados corretos associados a estes comôdos
            /// Exemplo de local correto 'Quarto2', 'quarto da amália'
            /// Exemplo de local incorreto 'banana', 'torneira'
            /// </summary>
            if (Sensor.Tipo == TipoSensor.Beacon)
            {
                if (string.IsNullOrWhiteSpace(Sensor.Localizacao))
                {
                    ModelState.AddModelError("Sensor.Localizacao", "Por favor, especifique a localização do Beacon.");
                }
                else
                {
                    var palavrasPermitidas = new[] { "cozinha", "sala", "quarto", "casa de banho", "entrada", "garagem" };
                    var textoInserido = Sensor.Localizacao.ToLower();

                    bool localizacaoValida = palavrasPermitidas.Any(palavra => textoInserido.Contains(palavra));

                    if (!localizacaoValida)
                    {
                        ModelState.AddModelError("Sensor.Localizacao", "A localização tem de conter uma divisão válida (ex: Quarto, Sala, Cozinha, Garagem, etc.).");
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // 4. Grava na Base de Dados
            _context.Sensores.Add(Sensor);
            await _context.SaveChangesAsync();

            return RedirectToPage("Index");
        }
    }
}