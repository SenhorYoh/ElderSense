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
        public async Task<IActionResult> Login([FromBody] ApiLoginModel login) 
        {
            
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

    
    public class ApiLoginModel
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}