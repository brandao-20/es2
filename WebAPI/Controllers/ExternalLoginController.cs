using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebAPI.Entities;
using WebAPI.Repositories;
using WebAPI.Services;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExternalLoginController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IUtilizadorRepository _utilizadorRepository;
        private readonly ITipoUtilizadorRepository _tipoUtilizadorRepository;
        private readonly IRoleService _roleService;

        public ExternalLoginController(
            IConfiguration configuration,
            IUtilizadorRepository utilizadorRepository,
            ITipoUtilizadorRepository tipoUtilizadorRepository,
            IRoleService roleService)
        {
            _configuration = configuration;
            _utilizadorRepository = utilizadorRepository;
            _tipoUtilizadorRepository = tipoUtilizadorRepository;
            _roleService = roleService;
        }

        [HttpGet("Google")]
        public IActionResult GoogleLogin()
        {
            var authProperties = new AuthenticationProperties
            {
                RedirectUri = Url.Action("GoogleCallback")
            };

            return Challenge(authProperties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet("GoogleCallback")]
        public async Task<IActionResult> GoogleCallback()
        {
            var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
            if (!result.Succeeded)
            {
                Console.WriteLine("[ERROR] Falha na autenticação com Google.");
                return Redirect("http://localhost:5116/login?error=GoogleLoginFailed");
            }

            var googleId = result.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(googleId))
            {
                Console.WriteLine("[ERROR] Google ID não encontrado na resposta da autenticação.");
                return Redirect("http://localhost:5116/login?error=GoogleIdMissing");
            }

            var givenName = result.Principal.FindFirst("given_name")?.Value;
            var familyName = result.Principal.FindFirst("family_name")?.Value;
            var fullName = (givenName + " " + familyName).Trim();
            if (string.IsNullOrWhiteSpace(fullName))
            {
                fullName = result.Principal.Identity?.Name ?? "GoogleUser";
            }
            var email = result.Principal.FindFirst(ClaimTypes.Email)?.Value ?? "noemail@example.com";

            var user = (await _utilizadorRepository.FindAsync(u => u.GoogleId == googleId)).FirstOrDefault();
            if (user == null)
            {
                // Novos utilizadores via Google devem ser USER por padrão
                var tipo = await _tipoUtilizadorRepository.FindAsync(t => t.Tipo == "USER")
                    .ContinueWith(t => t.Result.FirstOrDefault());
                if (tipo == null)
                {
                    tipo = new TipoUtilizador { Tipo = "USER" };
                    await _tipoUtilizadorRepository.AddAsync(tipo);
                }

                user = new Utilizador
                {
                    Username = fullName,
                    Email = email,
                    GoogleId = googleId,
                    TipoUtilizadorId = tipo.TipoUtilizadorId,
                    Password = "",
                    DataCriacao = DateTime.UtcNow
                };
                await _utilizadorRepository.AddAsync(user);
            }
            else
            {
                user.Username = fullName;
                user.Email = email;
                await _utilizadorRepository.UpdateAsync(user);
            }

            // Carregar o TipoUtilizador do utilizador existente
            if (user.TipoUtilizador == null)
            {
                var tipo = user.TipoUtilizadorId.HasValue
                    ? await _tipoUtilizadorRepository.GetByIdAsync(user.TipoUtilizadorId.Value)
                    : null;
                user.TipoUtilizador = tipo ?? new TipoUtilizador { Tipo = "USER" };
            }

            var token = GenerateJwtForGoogleUser(user);
            Console.WriteLine($"[DEBUG] Token gerado para utilizador {user.Username}: {token}");
            return Redirect($"http://localhost:5116/?googleToken={token}");
        }

        private string GenerateJwtForGoogleUser(Utilizador user)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            string jwtKey = jwtSettings["Key"] ?? throw new Exception("JWT Key não configurada.");
            string issuer = jwtSettings["Issuer"] ?? "http://localhost:5000";
            string audience = jwtSettings["Audience"] ?? "http://localhost:5000";

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            var role = _roleService.NormalizeRole(user.TipoUtilizador?.Tipo ?? "USER");
            Console.WriteLine($"[DEBUG] Papel atribuído ao utilizador {user.Username}: {role}");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, role),
                new Claim("utilizadorId", user.UtilizadorId.ToString())
            };

            // Log dos claims adicionados
            foreach (var claim in claims)
            {
                Console.WriteLine($"[DEBUG] Claim adicionado: {claim.Type} = {claim.Value}");
            }

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
