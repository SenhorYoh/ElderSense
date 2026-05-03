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
    public class LoginModel : PageModel
    {
        
        // Flag para saber se mostramos a password
        public bool ShowPassword { get; set; } = false;

        private readonly UserManager<Utilizador> _userManager;
        private readonly SignInManager<Utilizador> _signInManager;
        private readonly ILogger<LoginModel> _logger;


        public LoginModel(
        UserManager<Utilizador> userManager,
        SignInManager<Utilizador> signInManager,
        ILogger<LoginModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }


        public IList<AuthenticationScheme> ExternalLogins { get; set; }

   
        public string ReturnUrl { get; set; }

 
        [TempData]
        public string ErrorMessage { get; set; }

    
        public class InputModel
        {

            [Required]
            [EmailAddress]
            public string Email { get; set; }

   
            [Required]
            [DataType(DataType.Password)]
            public string? Password { get; set; } //opcional no primeiro passo

    
            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

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

                // LIMPEZA CRUCIAL: Remove o erro de "Password Required" do ModelState 
                // para que a página não mostre mensagens de erro vermelhas no primeiro passo.
                ModelState.Clear();
                return Page();
            }

            // 4. Se chegou aqui, já temos Email e Password. Validamos o estado geral.
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
                    // Se a password estiver errada, mantemos o campo visível
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
