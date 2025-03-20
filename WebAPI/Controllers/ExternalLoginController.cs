using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebAPI.Context;
using WebAPI.Entities;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExternalLoginController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;

        public ExternalLoginController(IConfiguration configuration, AppDbContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        // Inicia o fluxo de login com Google, sem usar returnUrl
        [HttpGet("Google")]
        public IActionResult GoogleLogin()
        {
            var authProperties = new AuthenticationProperties
            {
                // Callback fixo
                RedirectUri = Url.Action("GoogleCallback")
            };

            return Challenge(authProperties, GoogleDefaults.AuthenticationScheme);
        }

        // Recebe o retorno do Google e redireciona para Blazor (http://localhost:5116) com ?googleToken=...
        [HttpGet("GoogleCallback")]
        public async Task<IActionResult> GoogleCallback()
        {
            var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
            if (!result.Succeeded)
            {
                // Se quiser, redirecione para alguma tela de erro no Blazor
                return Redirect("http://localhost:5116/login?error=GoogleLoginFailed");
            }

            // O "sub" do Google (ID único do usuário)
            var googleId = result.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Tenta obter given_name e family_name
            var givenName = result.Principal.FindFirst("given_name")?.Value;
            var familyName = result.Principal.FindFirst("family_name")?.Value;

            // Se "given_name" e "family_name" estiverem vazios, tentar Identity?.Name
            var fullName = (givenName + " " + familyName).Trim();
            if (string.IsNullOrWhiteSpace(fullName))
            {
                fullName = result.Principal.Identity?.Name ?? "GoogleUser";
            }

            // Pega o email
            var email = result.Principal.FindFirst(ClaimTypes.Email)?.Value ?? "noemail@example.com";

            // Se quiser associar o GoogleId no banco
            var user = await _context.Utilizadores.FirstOrDefaultAsync(u => u.GoogleId == googleId);
            if (user == null)
            {
                // Se não existe, cria. Ex: role = "USER"
                // Precisamos de um TipoUtilizador. Ex: "USER" -> "User"
                var tipo = await _context.TipoUtilizadors.FirstOrDefaultAsync(t => t.Tipo == "USER");
                if (tipo == null)
                {
                    tipo = new TipoUtilizador { Tipo = "USER" };
                    _context.TipoUtilizadors.Add(tipo);
                    await _context.SaveChangesAsync();
                }

                user = new Utilizador
                {
                    Username = fullName,
                    Email = email,
                    GoogleId = googleId,
                    TipoUtilizadorId = tipo.TipoUtilizadorId,
                    Password = "", // se quiser
                    DataCriacao = DateTime.UtcNow
                };
                _context.Utilizadores.Add(user);
                await _context.SaveChangesAsync();
            }
            else
            {
                // Se já existe, podemos atualizar o nome/email, se desejado
                user.Username = fullName;
                user.Email = email;
                await _context.SaveChangesAsync();
            }

            // Agora gera o token JWT
            var token = GenerateJwtForGoogleUser(user);

            // Redireciona para a porta do Blazor
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

            // Normaliza role do DB
            var role = NormalizeRoleFromDb(user.TipoUtilizador?.Tipo ?? "USER");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, role),
                new Claim("utilizadorId", user.UtilizadorId.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // Mapeia "ADMIN" -> "Admin", "USER_MANAGER" -> "UserManager", etc.
        private string NormalizeRoleFromDb(string roleDb)
        {
            switch (roleDb.ToUpper())
            {
                case "ADMIN":
                    return "Admin";
                case "USER_MANAGER":
                    return "UserManager";
                default:
                    return "User";
            }
        }
    }
}
