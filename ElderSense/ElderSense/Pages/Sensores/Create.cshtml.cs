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
    [Authorize(Roles = "Cuidador")]
    public class CreateModel : PageModel
    {
        /// <summary>
        /// Contexto da base de dados
        /// </summary>
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Gestor de utilizadores do Identity, usado para identificar o cuidador autenticado
        /// </summary>
        private readonly UserManager<Utilizador> _userManager;

        /// <summary>
        /// Construtor que recebe as dependências injetadas pelo sistema
        /// </summary>
        public CreateModel(ApplicationDbContext context, UserManager<Utilizador> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// Sensor submetido pelo formulário de criação
        /// </summary>
        [BindProperty]
        public Sensor Sensor { get; set; } = new();

        /// <summary>
        /// Lista para alimentar o dropdown de Idosos no HTML
        /// </summary>
        public SelectList ListaIdosos { get; set; } = default!;

        /// <summary>
        /// Indica se a criação deve ser bloqueada por o cuidador não ter idosos associados
        /// </summary>
        public bool BloqueiaInsercao { get; set; } = false;

        /// <summary>
        /// Carrega a página de criação, preenchendo o dropdown de idosos associados ao cuidador
        /// </summary>
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
                ListaIdosos = new SelectList(cuidador.ListadeIdosos, "Id", "Nome");
            }
            else
            {
                // Se não tiver idosos, cria uma lista vazia para não dar erro no ecrã
                ListaIdosos = new SelectList(new List<Utilizador>(), "Id", "UserName");
                BloqueiaInsercao = true;
            }
        }

        /// <summary>
        /// Processa a criação do sensor: valida que o cuidador tem idosos associados,
        /// valida as regras específicas do tipo de sensor (Pulseira/Beacon) e grava na base de dados
        /// </summary>
        public async Task<IActionResult> OnPostAsync()
        {
            // 1. Vai buscar o utilizador atualmente logado no sistema
            var user = await _userManager.GetUserAsync(User);

            // Segurança extra: se a sessão expirou ou o user é nulo, manda para o Login
            if (user == null) { return Challenge(); }

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

            // Validação da Pulseira: garante que está associada ao pulso de alguém
            if (Sensor.Tipo == TipoSensor.Pulseira && string.IsNullOrEmpty(Sensor.FKIdoso))
            {
                ModelState.AddModelError("Sensor.FKIdoso", "Uma pulseira tem de estar associada a um idoso.");
            }

            // Validação da localização do beacon inserido pelo utilizador
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
                ListaIdosos = new SelectList(cuidador.ListadeIdosos ?? new List<Utilizador>(), "Id", "Nome");

                return Page();
            }

            // 4. Grava na Base de Dados
            _context.Sensores.Add(Sensor);
            await _context.SaveChangesAsync();

            return RedirectToPage("Index");
        }
    }
}