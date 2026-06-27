using ElderSense.Data;
using ElderSense.Data.Model;
using ElderSense.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace ElderSense.Pages
{

    /// <summary>
    /// Página de criação/associação de idoso a conta do Cuidador.
    /// O cuidador deve ter pelo menos 1 idoso associado para adicionar sensores,
    /// senão tiver, deve associar um idoso
    /// </summary>
    /// 

    //apenas cuidadores podem aceder a esta página
    [Authorize(Roles = "Cuidador")]
    public class AdicionarIdosoModel : PageModel
    {
        private readonly UserManager<Utilizador> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailSender;

        public AdicionarIdosoModel(UserManager<Utilizador> userManager, ApplicationDbContext context, IEmailSender emailSender)
        {
            _userManager = userManager;
            _context = context;
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        // de onde veio o utilizador, para saber para onde voltar depois de criar o idoso
        [BindProperty(SupportsGet = true)]
        public string? ReturnUrl { get; set; }

        // classe auxiliar só para apanhar os dados deste formulário
        public class InputModel
        {
            [Required(ErrorMessage = "O nome do idoso é obrigatório.")]
            [Display(Name = "Nome do Idoso")]
            [StringLength(50)]
            public string Nome { get; set; } = string.Empty;

            [Required(ErrorMessage = "O email é obrigatório.")]
            [EmailAddress(ErrorMessage = "Email inválido.")]
            [Display(Name = "Email do Idoso")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "A Palavra-Passe é obrigatória.")]
            [StringLength(50, ErrorMessage = "A {0} deve ter entre {2} e {1} caracteres.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Palavra-Passe")]
            public string Password { get; set; } = string.Empty;

            [DataType(DataType.Password)]
            [Display(Name = "Confirmar Palavra-Passe")]
            [Compare("Password", ErrorMessage = "As palavra-passes não coincidem.")]
            public string ConfirmPassword { get; set; } = string.Empty;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Verifica  se o E-mail já existe na base de dados
            var emailJaExiste = await _userManager.FindByEmailAsync(Input.Email);
            if (emailJaExiste != null)
            {
                // Mostra o erro a vermelho e cancela a operação
                ModelState.AddModelError(string.Empty, "Erro: Este email já se encontra registado. O idoso já tem uma conta ou o email está em uso.");
                return Page();
            }

            // Vai buscar o Cuidador logado e a sua lista
            var userId = _userManager.GetUserId(User);
            var cuidador = await _context.Set<Utilizador>()
                .Include(u => u.ListadeIdosos)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (cuidador == null) return NotFound();


            // Prepara o novo perfil de Idoso
            var novoIdoso = new Utilizador
            {
                UserName = Input.Email,
                Email = Input.Email,
                EmailConfirmed = true,
                Nome = Input.Nome,
                Tipo = TipoUtilizador.Idoso
            };

            //cria a conta com a password fornecida
            var result = await _userManager.CreateAsync(novoIdoso, Input.Password);

            if (result.Succeeded)
            {
                //atribui a role ao idoso
                await _userManager.AddToRoleAsync(novoIdoso, "Idoso");

                // Liga o Idoso recém-criado ao Cuidador
                cuidador.ListadeIdosos.Add(novoIdoso);
                await _context.SaveChangesAsync();

                //Envio de email informativo ao idoso. Não é preciso fazer a verificação,
                //pois como a conta do Cuidador é válida, supomos que a conta do idoso também é
                var nomeCuidador = cuidador.Nome;

                string assuntoEmail = "ElderSense - Nova Associação de Cuidador";
                string corpoEmail = $@"
                <h3>Bem-vindo ao ElderSense!</h3>
                <p>Olá, <strong>{Input.Nome}</strong>.</p>
                <p>Informamos que a sua conta de monitorização foi criada com sucesso e associada ao Cuidador <strong>{nomeCuidador}</strong>.</p>
                <p>A partir deste momento, os seus dispositivos de teleassistência já se encontram ativos no sistema.</p>
                <br>
                <small>Esta é uma mensagem automática informativa.</small>";

                // Dispara o email em segundo plano
                await _emailSender.SendEmailAsync(Input.Email, assuntoEmail, corpoEmail);

                TempData["MensagemSucesso"] = "Perfil de idoso associado com sucesso! O idoso recebeu um email informativo.";

                // Volta para onde o cuidador veio; se não vier de lado nenhum, assume Sensores/Create
                // (comportamento original, para o fluxo de bloqueio ao criar sensores)
                if (!string.IsNullOrEmpty(ReturnUrl))
                {
                    return LocalRedirect(ReturnUrl);
                }
                return RedirectToPage("/Sensores/Create");
            }

            // Se der erro a criar (ex: o Identity reclama de algo), mostra no ecrã
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return Page();
        }
    }
}