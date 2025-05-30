using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using WebAPI.Entities;
using WebAPI.Helpers;
using WebAPI.Repositories;
using WebAPI.Services;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUtilizadorRepository _utilizadorRepository;
        private readonly IRoleService _roleService;
        private readonly IConfiguration _configuration;

        public AuthController(
            IUtilizadorRepository utilizadorRepository,
            IRoleService roleService,
            IConfiguration configuration)
        {
            _utilizadorRepository = utilizadorRepository;
            _roleService = roleService;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<LoginResponse>>> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username))
                return BadRequest(new ApiResponse<LoginResponse>
                {
                    Success = false,
                    Message = "O nome de utilizador é obrigatório.",
                    ErrorCode = "INVALID_USERNAME",
                    StatusCode = 400,
                    Data = null
                });

            if (string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new ApiResponse<LoginResponse>
                {
                    Success = false,
                    Message = "A senha é obrigatória.",
                    ErrorCode = "INVALID_PASSWORD",
                    StatusCode = 400,
                    Data = null
                });

            Expression<Func<Utilizador, bool>> predicate = u => u.Username == request.Username;
            var users = await _utilizadorRepository.FindWithDetailsAsync(predicate);
            var user = users.FirstOrDefault();

            if (user == null || !PasswordHelper.VerifyPassword(request.Password, user.Password))
                return Unauthorized(new ApiResponse<LoginResponse>
                {
                    Success = false,
                    Message = "Credenciais inválidas.",
                    ErrorCode = "INVALID_CREDENTIALS",
                    StatusCode = 401,
                    Data = null
                });

            var token = GenerateJwtToken(user);
            var role = _roleService.NormalizeRole(user.TipoUtilizador?.Tipo ?? "USER");
            var response = new LoginResponse { Token = token, Role = role };
            return Ok(new ApiResponse<LoginResponse>
            {
                Success = true,
                Message = "Login realizado com sucesso.",
                StatusCode = 200,
                Data = response
            });
        }

        private string GenerateJwtToken(Utilizador user)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            string key = jwtSettings["Key"]
                         ?? "cf7fe7d90327ce76c4f697bfb31f1e1fe11cd98c484af55e9fe5b9e9fe10d35d09ff4ea95c763a91d4fbae68b348c8b2f32b29dd57f349d42b23aa3749cbac8adf59b35f9093a54b28c92d1b17f8a06fd65a7aa6a6331507d4366656823c40d50d43c597bdfd659098e3ddddfe75bcd923f4a47399001d1c5ab17bda70c69defc8e0a463030bb75f7d0610cff50aea4ffbbf64b101a6481cac42b8dca368ee368dadbe7f9ac88db6dc5476aefa8c0d5c67f0a18d0483eb3b056e93eb4dc51384f2d64abbe5fa74432545d0bd31cdc173c2f85fc2019bb154418c5cd59bb1400419d57557ac14a3284a9e40977975545efc2338eb4ba810ac4e0b7b32c7c49688";
            string issuer = jwtSettings["Issuer"] ?? "http://localhost:5000";
            string audience = jwtSettings["Audience"] ?? "http://localhost:5000";

            var roleNormalized = _roleService.NormalizeRole(user.TipoUtilizador?.Tipo ?? "USER");
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim("utilizadorId", user.UtilizadorId.ToString()),
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
    }

    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
