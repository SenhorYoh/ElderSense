using ElderSense.Data;
using ElderSense.Data.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ElderSense.DTOs;

namespace ElderSense.API
{
    /// <summary>
    /// Classe da API responsável pela autenticação e geração de tokens JWT
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        /// <summary>
        /// Contexto da base de dados
        /// </summary>
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Gestor de utilizadores do Identity, usado para procurar o utilizador pelo email
        /// </summary>
        private readonly UserManager<Utilizador> _userManager;

        /// <summary>
        /// Gestor de autenticação do Identity, usado para validar a password
        /// </summary>
        private readonly SignInManager<Utilizador> _signInManager;

        /// <summary>
        /// Configuração da aplicação, usada para obter as definições do JWT
        /// </summary>
        private readonly IConfiguration _config;

        /// <summary>
        /// Serviço responsável por gerar os tokens JWT
        /// </summary>
        private readonly Services.TokenService _tokenService;

        /// <summary>
        /// Construtor que recebe as dependências injetadas pelo sistema
        /// </summary>
        public AuthController(ApplicationDbContext context,
           UserManager<Utilizador> userManager,
           SignInManager<Utilizador> signInManager,
           IConfiguration config,
           Services.TokenService tokenService)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _config = config;
            _tokenService = tokenService;
        }

        /// <summary>
        /// Valida as credenciais do utilizador e devolve um token JWT em caso de sucesso
        /// </summary>
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO login)
        {
            var user = await _userManager.FindByEmailAsync(login.Username);
            if (user == null) return Unauthorized();

            var result = await _signInManager.CheckPasswordSignInAsync(user, login.Password, false);
            if (!result.Succeeded) return Unauthorized();

            var token = _tokenService.GenerateToken(user);

            return Ok(new { token });
        }
    }

    /// <summary>
    /// DTO que representa as credenciais de login recebidas pela API
    /// </summary>
    public class ApiLoginModel
    {
        /// <summary>
        /// Email do utilizador (usado como nome de utilizador no login)
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Password do utilizador
        /// </summary>
        public string Password { get; set; } = string.Empty;
    }
}