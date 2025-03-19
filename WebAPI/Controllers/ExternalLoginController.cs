using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExternalLoginController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public ExternalLoginController(IConfiguration configuration)
        {
            _configuration = configuration;
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

            // Tenta obter "given_name" e "family_name"
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

            // Gera token
            var token = GenerateJwtForGoogleUser(email, fullName);

            // Redireciona para a porta do Blazor
            // Assim, o Blazor lerá ?googleToken= e decodificará o JWT
            return Redirect($"http://localhost:5116/?googleToken={token}");
        }

        private string GenerateJwtForGoogleUser(string email, string name)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            string jwtKey = jwtSettings["Key"] ?? throw new Exception("JWT Key não configurada.");
            string issuer = jwtSettings["Issuer"] ?? "http://localhost:5000";
            string audience = jwtSettings["Audience"] ?? "http://localhost:5000";

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, name),
                new Claim(ClaimTypes.Email, email)
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
    }
}
