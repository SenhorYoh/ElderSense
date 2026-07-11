using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ElderSense.Data.Model;

namespace ElderSense.Services
{
    /// <summary>
    /// Classe que gera a token JWT, ou identidade digital, dos utilizadores que se registem.
    /// É essencial para a gestão de permissões nas páginas do website.
    /// </summary>
    public class TokenService
    {
        /// <summary>
        /// Configuração da aplicação, usada para obter as definições do JWT
        /// </summary>
        private readonly IConfiguration _config;

        /// <summary>
        /// Construtor que recebe a configuração injetada pelo sistema
        /// </summary>
        public TokenService(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Gera um token JWT assinado para o utilizador, com as claims de identidade e permissões
        /// </summary>
        public string GenerateToken(Utilizador user)
        {
            var jwtSettings = _config.GetSection("Jwt");
            var jwtKey = jwtSettings["Key"];
            if (string.IsNullOrEmpty(jwtKey))
            {
                throw new InvalidOperationException("A chave Jwt:Key não foi configurada.");
            }
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // As claims carregam o ID único do utilizador, o email, um ID único para o token e o tipo de utilizador
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("role", user.Tipo.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(Convert.ToDouble(jwtSettings["ExpireHours"] ?? "2")),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}