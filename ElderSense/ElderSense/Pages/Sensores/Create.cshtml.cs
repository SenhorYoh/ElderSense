using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
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

        // Lista para alimentar o dropdown de Idosos no HTML
        public SelectList ListaIdosos { get; set; } = default!;

        public bool BloqueiaInsercao { get; set; } = false;
        public async Task OnGetAsync()
        {
            var userId = _userManager.GetUserId(User);

            // Vai buscar o cuidador e a sua lista de idosos
            var cuidador = await _context.Set<Utilizador>()
                .Include(u => u.ListadeIdosos)
                .FirstOrDefaultAsync(u => u.Id == userId);

            // Se o cuidador tiver idosos na sua rede, preenche o dropdown
            if (cuidador != null && cuidador.ListadeIdosos.Any())
            {
                // NOTA: Usa "UserName" ou o nome da propriedade que guarda o Nome do utilizador
                ListaIdosos = new SelectList(cuidador.ListadeIdosos, "Id", "UserName");
            }
            else
            {
                // Se não tiver idosos, cria uma lista vazia para não dar erro no ecrã
                ListaIdosos = new SelectList(new List<Utilizador>(), "Id", "UserName");
                BloqueiaInsercao = true;
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // 1. Vai buscar o utilizador atualmente logado no sistema
            var user = await _userManager.GetUserAsync(User);

            // Segurança extra: se a sessão expirou ou o user é nulo, manda para o Login
            if (user == null) {return Challenge();}

            // Impede que gravem sensores forçando o código HTML
            var cuidador = await _context.Set<Utilizador>().Include(u => u.ListadeIdosos).FirstOrDefaultAsync(u => u.Id == user.Id);
            if (cuidador == null || !cuidador.ListadeIdosos.Any())
            {
                BloqueiaInsercao = true;
                ModelState.AddModelError(string.Empty, "O ElderSense requer pelo menos um idoso associado para registar hardware.");
                return Page();
            }

            // 2. Associa o sensor ao ID do utilizador e limpa a navegação
            Sensor.FKUtilizador = user.Id;
            Sensor.Utilizador = null!;
            Sensor.IdosoAssociado = null!; // Limpa lixo do novo relacionamento

            // 3. Remove as validações automáticas do ModelState
            ModelState.Remove("Sensor.Utilizador");
            ModelState.Remove("Sensor.FKUtilizador");
            ModelState.Remove("Sensor.IdosoAssociado");

            /// <summary>
            /// Validação da Pulseira: Garante que está associada ao pulso de alguém
            /// </summary>
            if (Sensor.Tipo == TipoSensor.Pulseira && string.IsNullOrEmpty(Sensor.FKIdoso))
            {
                ModelState.AddModelError("Sensor.FKIdoso", "Uma pulseira tem de estar associada a um idoso.");
            }

            /// <summary>
            /// Validação da localização do beacon inserido pelo utilizador
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

                ListaIdosos = new SelectList(cuidador.ListadeIdosos ?? new List<Utilizador>(), "Id", "UserName");

                return Page();
            }

            // 4. Grava na Base de Dados
            _context.Sensores.Add(Sensor);
            await _context.SaveChangesAsync();

            return RedirectToPage("Index");
        }
    }
}