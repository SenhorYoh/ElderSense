using ElderSense.Data;
using ElderSense.Data.Model; // Adicionado para reconhecer o teu 'Utilizador'
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
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Utilizador> _userManager; // CORREÇÃO: Utilizador em vez de IdentityUser
        private readonly SignInManager<Utilizador> _signInManager; // CORREÇÃO: Utilizador em vez de IdentityUser
        private readonly IConfiguration _config;

        public AuthController(ApplicationDbContext context,
           UserManager<Utilizador> userManager, // CORREÇÃO
           SignInManager<Utilizador> signInManager, // CORREÇÃO
           IConfiguration config)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _config = config;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] ApiLoginModel login) // Mudado o nome para evitar conflitos
        {
            // O teu professor usou login.Username, mas como o Identity usa o Email para o login, procuramos por Email:
            var user = await _userManager.FindByEmailAsync(login.Username);
            if (user == null) return Unauthorized();

            var result = await _signInManager.CheckPasswordSignInAsync(user, login.Password, false);
            if (!result.Succeeded) return Unauthorized();

            var token = GenerateJwtToken(login.Username);

            return Ok(new { token });
        }

        private string GenerateJwtToken(string username)
        {
            var claims = new[] {
                new Claim(ClaimTypes.Name, username)
            };

            // Certifica-te de que tens "Jwt:Key" configurado no appsettings.json ou nos Secrets!
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

    // Criamos a classe aqui em baixo para o controlador saber o que receber no JSON do Postman/Frontend
    public class ApiLoginModel
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}