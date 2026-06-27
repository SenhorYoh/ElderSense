// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Text;
using System.Threading.Tasks;
using ElderSense.Data.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace ElderSense.Areas.Identity.Pages.Account
{
    /// <summary>
    /// Página mostrada após o registo, a confirmar que o email foi enviado
    /// </summary>
    [AllowAnonymous]
    public class RegisterConfirmationModel : PageModel
    {
        /// <summary>
        /// Gestor de utilizadores do Identity
        /// </summary>
        private readonly UserManager<Utilizador> _userManager;

        /// <summary>
        /// Serviço de envio de emails
        /// </summary>
        private readonly IEmailSender _sender;

        /// <summary>
        /// Construtor que recebe as dependências injetadas pelo sistema
        /// </summary>
        public RegisterConfirmationModel(UserManager<Utilizador> userManager, IEmailSender sender)
        {
            _userManager = userManager;
            _sender = sender;
        }

        /// <summary>
        /// Esta API suporta a infraestrutura padrão de UI do ASP.NET Core Identity e não foi
        /// pensada para ser usada diretamente no código. Esta API pode mudar ou ser removida em futuras versões.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Esta API suporta a infraestrutura padrão de UI do ASP.NET Core Identity e não foi
        /// pensada para ser usada diretamente no código. Esta API pode mudar ou ser removida em futuras versões.
        /// </summary>
        public bool DisplayConfirmAccountLink { get; set; }

        /// <summary>
        /// Esta API suporta a infraestrutura padrão de UI do ASP.NET Core Identity e não foi
        /// pensada para ser usada diretamente no código. Esta API pode mudar ou ser removida em futuras versões.
        /// </summary>
        public string EmailConfirmationUrl { get; set; }

        /// <summary>
        /// Carrega a página de confirmação de registo para o email indicado
        /// </summary>
        public async Task<IActionResult> OnGetAsync(string email, string returnUrl = null)
        {
            if (email == null)
            {
                return RedirectToPage("/Index");
            }
            returnUrl = returnUrl ?? Url.Content("~/");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return NotFound($"Não foi possível carregar o utilizador com o email '{email}'.");
            }

            Email = email;

            // DisplayConfirmAccountLink é declarado como falso para que exija uma conta confirmada
            // e impede o login imediato
            DisplayConfirmAccountLink = false;
            if (DisplayConfirmAccountLink)
            {
                var userId = await _userManager.GetUserIdAsync(user);
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                EmailConfirmationUrl = Url.Page(
                    "/Account/ConfirmEmail",
                    pageHandler: null,
                    values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                    protocol: Request.Scheme);
            }

            return Page();
        }
    }
}