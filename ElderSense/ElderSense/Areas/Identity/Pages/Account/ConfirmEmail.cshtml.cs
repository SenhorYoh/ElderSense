// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ElderSense.Data.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace ElderSense.Areas.Identity.Pages.Account
{
    /// <summary>
    /// Página de confirmação de email, acedida através do link enviado por email no registo
    /// </summary>
    public class ConfirmEmailModel : PageModel
    {
        /// <summary>
        /// Gestor de utilizadores do Identity
        /// </summary>
        private readonly UserManager<Utilizador> _userManager;

        /// <summary>
        /// Gestor de autenticação do Identity, usado para iniciar sessão automaticamente após a confirmação
        /// </summary>
        private readonly SignInManager<Utilizador> _signInManager;

        /// <summary>
        /// Construtor que recebe as dependências injetadas pelo sistema
        /// </summary>
        public ConfirmEmailModel(UserManager<Utilizador> userManager, SignInManager<Utilizador> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        /// <summary>
        /// Esta API suporta a infraestrutura padrão de UI do ASP.NET Core Identity e não foi
        /// pensada para ser usada diretamente no código. Esta API pode mudar ou ser removida em futuras versões.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

        /// <summary>
        /// Valida o código de confirmação recebido por email e, se for válido,
        /// confirma a conta do utilizador e inicia sessão automaticamente
        /// </summary>
        public async Task<IActionResult> OnGetAsync(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return RedirectToPage("/Index");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"Não foi possível carregar o utilizador com o ID '{userId}'.");
            }

            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            var result = await _userManager.ConfirmEmailAsync(user, code);
            StatusMessage = result.Succeeded ? "Obrigado por confirmares o email!" : "Erro ao confirmar email, tente novamente";

            if (result.Succeeded)
            {
                // Faz o login automático
                await _signInManager.SignInAsync(user, isPersistent: false);

                // Redireciona para a página inicial
                return RedirectToPage("/Index");
            }
            return Page();
        }
    }
}