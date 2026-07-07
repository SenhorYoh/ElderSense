using ElderSense.Data;
using ElderSense.Data.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

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
        /// Construtor que recebe as dependências injetadas pelo sistema
        /// </summary>
        public AuthController(ApplicationDbContext context,
           UserManager<Utilizador> userManager,
           SignInManager<Utilizador> signInManager,
           IConfiguration config)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _config = config;
        }

        /// <summary>
        /// Valida as credenciais do utilizador e devolve um token JWT em caso de sucesso
        /// </summary>
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] ApiLoginModel login)
        {
            var user = await _userManager.FindByEmailAsync(login.Username);
            if (user == null) return Unauthorized();

            var result = await _signInManager.CheckPasswordSignInAsync(user, login.Password, false);
            if (!result.Succeeded) return Unauthorized();

            var token = GenerateJwtToken(login.Username);

            return Ok(new { token });
        }

        /// <summary>
        /// Gera um token JWT assinado, válido durante 2 horas, com o nome de utilizador como claim
        /// </summary>
        private string GenerateJwtToken(string username)
        {
            var claims = new[] {
                new Claim(ClaimTypes.Name, username)
            };

            var jwtKey = _config["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey))
            {
                throw new Exception("A chave Jwt:Key não foi configurada.");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(2),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
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