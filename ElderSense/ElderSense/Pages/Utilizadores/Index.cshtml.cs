using ElderSense.Data;
using ElderSense.Data.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ElderSense.Pages.Utilizadores
{
    /// <summary>
    /// Página de visualização dos idosos associados ao cuidador autenticado
    /// </summary>
    [Authorize]
    public class IndexModel : PageModel
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
        public IndexModel(ApplicationDbContext context, UserManager<Utilizador> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// Lista dos idosos associados ao cuidador autenticado
        /// </summary>
        public IList<Utilizador> Utilizadores { get; set; } = [];

        /// <summary>
        /// Número total de idosos associados ao cuidador
        /// </summary>
        public int TotalIdosos => Utilizadores.Count;

        /// <summary>
        /// Carrega a lista de idosos da rede do cuidador autenticado
        /// </summary>
        public async Task OnGetAsync()
        {
            var userId = _userManager.GetUserId(User);

            // vai buscar o cuidador autenticado, já com a lista de idosos associados
            var cuidador = await _context.Utilizadores
                .Include(u => u.ListadeIdosos)
                .FirstOrDefaultAsync(u => u.Id == userId);

            // se o cuidador existir, mostra só os idosos da sua rede
            Utilizadores = cuidador?.ListadeIdosos.ToList() ?? [];
        }
    }
}