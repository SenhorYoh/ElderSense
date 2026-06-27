// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using ElderSense.Data.Model;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;

namespace ElderSense.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<Utilizador> _signInManager;
        private readonly UserManager<Utilizador> _userManager;
        private readonly IUserStore<Utilizador> _userStore;
        private readonly IUserEmailStore<Utilizador> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;

        public RegisterModel(
            UserManager<Utilizador> userManager,
            IUserStore<Utilizador> userStore,
            SignInManager<Utilizador> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {

            /// <summary>
            /// Campos que o utilizador vê na criação da conta
            /// </summary>
            [Required(ErrorMessage = "O(a) {0} é obrigatório(a)")]
            [StringLength(50)]
            [Display(Name = "Nome")]
            public string Nome { get; set; }

            [Required(ErrorMessage = "O(a) {0} é obrigatório(a)")]
            [Display(Name = "Tipo de Utilizador")]
            public TipoUtilizador Tipo { get; set; }

            [Required(ErrorMessage = "O(a) {0} é obrigatório(a)")]
            [Display(Name = "Data de nascimento")]
            [DataType(DataType.Date)]
            [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]
            public DateOnly DataNascimento { get; set; }

            [Display(Name = "Telefone (Opcional)")]
            public string? Telefone { get; set; }

            [Required(ErrorMessage = "O(a) {0} é obrigatório(a)")]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required(ErrorMessage = "O(a) {0} é obrigatório(a)")]
            [StringLength(100, ErrorMessage = "A {0} deve ter pelo menos {2} caracteres", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [Required(ErrorMessage = "O(a) {0} é obrigatório(a)")]
            [DataType(DataType.Password)]
            [Display(Name = "Confirmar password")]
            [Compare("Password", ErrorMessage = "As passwords não coincidem")]
            public string ConfirmPassword { get; set; }
        }


        public async Task OnGetAsync(string returnUrl = null, string email = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            //se o utilizador não existe, busca o email do login para o registo
            if (!string.IsNullOrEmpty(email))
            {
                Input = new InputModel
                {
                    Email = email
                };
            }
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                // Calcula a idade e garante que o cuidador é maior de 18 anos
                var hoje = DateOnly.FromDateTime(DateTime.Today);
                var idade = hoje.Year - Input.DataNascimento.Year;
                if (Input.DataNascimento > hoje.AddYears(-idade))
                {
                    idade--;
                }

                if (idade < 18)
                {
                    ModelState.AddModelError("Input.DataNascimento", "Tem de ter pelo menos 18 anos para criar uma conta.");
                    return Page();
                }

                var user = CreateUser();

                user.Nome = Input.Nome; // Mapeia o nome do formulário para a classe
                user.Tipo = TipoUtilizador.Cuidador;         // Mapeia o tipo (Idoso/Cuidador)
                user.Telefone = Input.Telefone; // Mapeia o telefone
                user.DataNascimento = Input.DataNascimento; // Mapeia a data de nascimento

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    //o utilizador é do tipo cuidador por padrão
                    await _userManager.AddToRoleAsync(user, "Cuidador");

                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    string assunto = "ElderSense - Confirme a sua conta 🚀";

                    string corpoEmail = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #eee; border-radius: 10px;'>
                        <h2 style='color: #0d6efd; text-align: center;'>Bem-vindo à ElderSense!</h2>
                        <p>Olá,</p>
                        <p>Obrigado por se registar na nossa plataforma de monitorização. Para começar a utilizar a sua conta em segurança, por favor confirme o seu endereço de email clicando no botão abaixo:</p>
        
                        <div style='text-align: center; margin: 30px 0;'>
                            <a href='{HtmlEncoder.Default.Encode(callbackUrl)}' 
                               style='background-color: #0d6efd; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; font-weight: bold; display: inline-block;'>
                               Confirmar Conta
                            </a>
                        </div>
        
                        <p style='font-size: 12px; color: #666;'>Se o botão não funcionar, copie e cole o seguinte link no seu navegador:</p>
                        <p style='font-size: 12px; color: #0d6efd; word-break: break-all;'>{callbackUrl}</p>
                        <hr style='border: 0; border-top: 1px solid #eee; margin-top: 30px;' />
                        <p style='font-size: 11px; color: #999; text-align: center;'>Este é un email automático da ElderSense. Por favor, não responda a esta mensagem.</p>
                    </div>";

                    // 3. Envia o email com o teu assunto e corpo personalizados
                    await _emailSender.SendEmailAsync(Input.Email, assunto, corpoEmail);

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        private Utilizador CreateUser()
        {
            try
            {
                return Activator.CreateInstance<Utilizador>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(Utilizador)}'.");
            }
        }

        private IUserEmailStore<Utilizador> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<Utilizador>)_userStore;
        }
    }
}