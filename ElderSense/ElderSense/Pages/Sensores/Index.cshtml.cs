using ElderSense.Data;
using ElderSense.Data.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ElderSense.Pages.Sensores
{
    /// <summary>
    /// Página de visualização dos sensores. Os dois tipos de utilizadores podem visualizar
    /// </summary>
    [Authorize]
    public class IndexModel : PageModel
    {
        /// <summary>
        /// Contexto da base de dados
        /// </summary>
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Construtor que recebe o contexto da base de dados injetado pelo sistema
        /// </summary>
        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lista de sensores a mostrar na página
        /// </summary>
        public IList<Sensor> Sensores { get; set; } = [];

        /// <summary>
        /// Número total de sensores registados
        /// </summary>
        public int TotalSensores => Sensores.Count;

        /// <summary>
        /// Número de sensores atualmente ativos
        /// </summary>
        public int SensoresAtivos => Sensores.Count(s => s.Estado);

        /// <summary>
        /// Número de sensores atualmente inativos
        /// </summary>
        public int SensoresInativos => Sensores.Count(s => !s.Estado);

        /// <summary>
        /// Carrega a lista de sensores, incluindo o utilizador responsável associado
        /// </summary>
        public async Task OnGetAsync()
        {
            // busca todos os sensores incluindo o utilizador associado
            Sensores = await _context.Sensores
                .Include(s => s.Utilizador)
                .ToListAsync();
        }
    }
}