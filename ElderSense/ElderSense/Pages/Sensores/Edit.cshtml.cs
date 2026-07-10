using ElderSense.Data;
using ElderSense.Data.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
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
        /// Construtor que recebe o contexto da base de dados injetado pelo sistema
        /// </summary>
        public EditModel(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Sensor a editar, vindo do formulário
        /// </summary>
        [BindProperty]
        public Sensor Sensor { get; set; } = new();

        /// <summary>
        /// Carrega o sensor selecionado para edição
        /// </summary>
        public async Task<IActionResult> OnGetAsync(int id)
        {
            // vai buscar o sensor pelo id
            var sensor = await _context.Sensores.FindAsync(id);

            // se não existir, redireciona para a lista
            if (sensor == null) return NotFound();

            Sensor = sensor;
            return Page();
        }

        /// <summary>
        /// Processa a atualização do sensor na base de dados
        /// </summary>
        public async Task<IActionResult> OnPostAsync()
        {
            // remove a validação da navigation property pois não vem do formulário
            ModelState.Remove("Sensor.Utilizador");

            if (!ModelState.IsValid) return Page();

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
    }
}