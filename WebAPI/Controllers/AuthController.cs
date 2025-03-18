using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WebAPI.Context;
using WebAPI.Entities;
using WebAPI.Helpers;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _context.Utilizadores
                .Include(u => u.TipoUtilizador)
                .FirstOrDefaultAsync(u => u.Username == request.Username);

            if (user == null || !PasswordHelper.VerifyPassword(request.Password, user.Password))
            {
                return Unauthorized("Credenciais inválidas");
            }

            var token = GenerateJwtToken(user);
            return Ok(new { token, role = user.TipoUtilizador?.Tipo });
        }

        private string GenerateJwtToken(Utilizador user)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            string key = jwtSettings["Key"] ?? "chave_default_muito_segura_aqui";
            string issuer = jwtSettings["Issuer"] ?? "https://seusite.com";
            string audience = jwtSettings["Audience"] ?? "https://seusite.com";

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim("utilizadorId", user.UtilizadorId.ToString()),
                new Claim(ClaimTypes.Role, user.TipoUtilizador?.Tipo ?? "User"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer,
                audience,
                claims,
                expires: DateTime.Now.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
