using ElderSense.Data;
using ElderSense.Data.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ElderSense.Pages.Sensores
{
    /// <summary>
    /// Página de edição dos sensores. Apenas um utilizador logado e do tipo Cuidador pode editar
    /// </summary>
    [Authorize(Roles = "Cuidador")]
    public class EditModel : PageModel
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
        public EditModel(ApplicationDbContext context, UserManager<Utilizador> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// Sensor a editar, vindo do formulário
        /// </summary>
        [BindProperty]
        public Sensor Sensor { get; set; } = new();

        /// <summary>
        /// Lista de idosos do cuidador, para preencher o dropdown de associação
        /// </summary>
        public SelectList ListaIdosos { get; set; } = new(new List<Utilizador>());

        /// <summary>
        /// Carrega o sensor selecionado para edição e a lista de idosos disponíveis
        /// </summary>
        public async Task<IActionResult> OnGetAsync(int id)
        {
            // vai buscar o sensor pelo id
            var sensor = await _context.Sensores.FindAsync(id);

            // se não existir, redireciona para a lista
            if (sensor == null) return NotFound();

            Sensor = sensor;

            await CarregarIdososAsync();

            return Page();
        }

        /// <summary>
        /// Processa a atualização do sensor na base de dados
        /// </summary>
        public async Task<IActionResult> OnPostAsync()
        {
            // remove a validação das navigation properties pois não vêm do formulário
            ModelState.Remove("Sensor.Utilizador");
            ModelState.Remove("Sensor.IdosoAssociado");

            // Validação da Pulseira: garante que continua associada a um idoso
            if (Sensor.Tipo == TipoSensor.Pulseira && string.IsNullOrEmpty(Sensor.FKIdoso))
            {
                ModelState.AddModelError("Sensor.FKIdoso", "Uma pulseira tem de estar associada a um idoso.");
            }

            if (!ModelState.IsValid)
            {
                await CarregarIdososAsync();
                return Page();
            }

            // regra: um sensor arquivado tem de estar sempre desligado.
            // Não pode ser ativado enquanto não for reativado (tirado do arquivo).
            if (Sensor.Arquivado)
            {
                Sensor.Estado = false;
            }

            // atualiza o sensor na BD
            _context.Sensores.Update(Sensor);
            await _context.SaveChangesAsync();

            return RedirectToPage("Index");
        }

        /// <summary>
        /// Carrega os idosos associados ao cuidador autenticado para o dropdown
        /// </summary>
        private async Task CarregarIdososAsync()
        {
            var userId = _userManager.GetUserId(User);

            var cuidador = await _context.Utilizadores
                .Include(u => u.ListadeIdosos)
                .FirstOrDefaultAsync(u => u.Id == userId);

            var idosos = cuidador?.ListadeIdosos.ToList() ?? new List<Utilizador>();

            ListaIdosos = new SelectList(idosos, "Id", "Nome");
        }
    }
}