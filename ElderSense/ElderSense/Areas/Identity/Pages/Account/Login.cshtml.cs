// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using ElderSense.Data.Model;

namespace ElderSense.Areas.Identity.Pages.Account
{
    /// <summary>
    /// Página de login, feita em dois passos: primeiro o email, depois a password
    /// (a password só é pedida se o utilizador já existir no sistema)
    /// </summary>
    public class LoginModel : PageModel
    {
        /// <summary>
        /// Indica se o campo de password deve ser mostrado no formulário (segundo passo do login)
        /// </summary>
        public bool ShowPassword { get; set; } = false;

        /// <summary>
        /// Gestor de utilizadores do Identity
        /// </summary>
        private readonly UserManager<Utilizador> _userManager;

        /// <summary>
        /// Gestor de autenticação do Identity, usado para validar as credenciais e iniciar sessão
        /// </summary>
        private readonly SignInManager<Utilizador> _signInManager;

        /// <summary>
        /// Logger usado para registar tentativas de autenticação
        /// </summary>
        private readonly ILogger<LoginModel> _logger;

        /// <summary>
        /// Construtor que recebe as dependências injetadas pelo sistema
        /// </summary>
        public LoginModel(
        UserManager<Utilizador> userManager,
        SignInManager<Utilizador> signInManager,
        ILogger<LoginModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        /// <summary>
        /// Dados do formulário de login submetidos pelo utilizador
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        /// Lista de esquemas de autenticação externos disponíveis (ex: Google)
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        /// URL para onde o utilizador deve ser redirecionado após o login
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        /// Mensagem de erro a mostrar, persistida entre redirecionamentos
        /// </summary>
        [TempData]
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Campos do formulário de login
        /// </summary>
        public class InputModel
        {
            /// <summary>
            /// Email do utilizador
            /// </summary>
            [Required(ErrorMessage = "O email é obrigatório.")]
            [EmailAddress(ErrorMessage = "Formato de email inválido.")]
            public string Email { get; set; }

            /// <summary>
            /// Password do utilizador, opcional no primeiro passo (só email)
            /// </summary>
            [Required(ErrorMessage = "A password é obrigatória.")]
            [DataType(DataType.Password)]
            public string? Password { get; set; }

            /// <summary>
            /// Indica se a sessão deve permanecer iniciada entre visitas
            /// </summary>
            [Display(Name = "Lembrar-me")]
            public bool RememberMe { get; set; }
        }

        /// <summary>
        /// Carrega a página de login, limpando qualquer cookie de autenticação externa pendente
        /// </summary>
        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            // Remove o cookie de autenticação externa existente para garantir um processo de login limpo
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        /// <summary>
        /// Processa o login em dois passos: se o email não tiver conta, redireciona para o registo;
        /// se a password ainda não foi submetida, mostra o campo; caso contrário, valida as credenciais
        /// </summary>
        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            // Recarregar sempre os logins externos para o botão do Google não desaparecer
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            // 1. Verificar se o email foi preenchido (validação básica manual)
            if (string.IsNullOrEmpty(Input.Email))
            {
                ModelState.AddModelError("Input.Email", "O email é obrigatório.");
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(Input.Email);

            // 2. Se o utilizador NÃO existe -> Redireciona para o Registo
            if (user == null)
            {
                return RedirectToPage("Register", new { email = Input.Email });
            }

            // 3. Se o utilizador existe mas ainda não enviou a password
            if (string.IsNullOrEmpty(Input.Password))
            {
                ShowPassword = true;

                ModelState.Clear();
                return Page();
            }

            // 4. já temos Email e Password. Validamos o estado geral.
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(
                    Input.Email,
                    Input.Password,
                    Input.RememberMe,
                    lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Utilizador autenticado.");
                    return LocalRedirect(returnUrl);
                }

                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                }

                if (result.IsLockedOut)
                {
                    _logger.LogWarning("Conta bloqueada.");
                    return RedirectToPage("./Lockout");
                }
                else
                {
                    // Se a password estiver errada, mantém-se o campo visível
                    ShowPassword = true;
                    ModelState.AddModelError(string.Empty, "Password incorreta.");
                    return Page();
                }
            }

            // Se algo falhou, volta a mostrar a página (garantindo que ShowPassword é mantido se necessário)
            return Page();
        }
    }
}