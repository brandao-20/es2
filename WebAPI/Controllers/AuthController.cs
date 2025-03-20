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
            // Retornamos também a role normalizada
            return Ok(new { token, role = NormalizeRole(user.TipoUtilizador?.Tipo ?? "USER") });
        }

        private string GenerateJwtToken(Utilizador user)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            string key = jwtSettings["Key"] 
                         ?? "cf7fe7d90327ce76c4f697bfb31f1e1fe11cd98c484af55e9fe5b9e9fe10d35d09ff4ea95c763a91d4fbae68b348c8b2f32b29dd57f349d42b23aa3749cbac8adf59b35f9093a54b28c92d1b17f8a06fd65a7aa6a6331507d4366656823c40d50d43c597bdfd659098e3ddddfe75bcd923f4a47399001d1c5ab17bda70c69defc8e0a463030bb75f7d0610cff50aea4ffbbf64b101a6481cac42b8dca368ee368dadbe7f9ac88db6dc5476aefa8c0d5c67f0a18d0483eb3b056e93eb4dc51384f2d64abbe5fa74432545d0bd31cdc173c2f85fc2019bb154418c5cd59bb1400419d57557ac14a3284a9e40977975545efc2338eb4ba810ac4e0b7b32c7c49688";
            string issuer = jwtSettings["Issuer"] ?? "http://localhost:5000";
            string audience = jwtSettings["Audience"] ?? "http://localhost:5000";

            // Normalizar a role que vem do banco (por ex. "ADMIN" -> "Admin", "USER_MANAGER" -> "UserManager", etc.)
            var roleNormalized = NormalizeRole(user.TipoUtilizador?.Tipo ?? "USER");

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim("utilizadorId", user.UtilizadorId.ToString()),
                // Agora, ClaimTypes.Role terá "Admin" ou "UserManager" ou "User"
                new Claim(ClaimTypes.Role, roleNormalized),
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

        // Método auxiliar para mapear os valores do banco para as roles do [Authorize]
        private string NormalizeRole(string roleDb)
        {
            switch (roleDb.ToUpper())
            {
                case "ADMIN":
                    return "Admin";
                case "USER_MANAGER":
                    return "UserManager";
                case "USER":
                default:
                    return "User";
            }
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
