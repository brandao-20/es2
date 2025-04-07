using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;

namespace WebApp.Services
{
    public class AuthService
    {
        public string UserName { get; set; } = "";
        public string Role { get; set; } = "";
        public string Token { get; set; } = "";

        // Armazena o ID do utilizador com login efetuado
        public int UserId { get; set; } = 0;

        public bool IsLoggedIn => !string.IsNullOrEmpty(Token);
        public bool IsAdmin => Role.Equals("ADMIN", StringComparison.OrdinalIgnoreCase);
        public bool IsManager => Role.Equals("USER_MANAGER", StringComparison.OrdinalIgnoreCase);
        public bool IsUser => Role.Equals("USER", StringComparison.OrdinalIgnoreCase);

        public void Clear()
        {
            UserName = "";
            Role = "";
            Token = "";
            UserId = 0;
        }

        /// Valida o token verificando a data de expiração (claim "exp").
        /// Se o token estiver expirado, limpa os dados e retorna false.
        public bool ValidateToken()
        {
            if (string.IsNullOrEmpty(Token))
                return false;
            
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(Token);

                // O claim "exp" é um número que representa o UnixTime
                var expClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Exp)?.Value;
                if (expClaim != null)
                {
                    if (long.TryParse(expClaim, out long exp))
                    {
                        var expDate = DateTimeOffset.FromUnixTimeSeconds(exp).UtcDateTime;
                        if (expDate < DateTime.UtcNow)
                        {
                            // Token expirado
                            Clear();
                            return false;
                        }
                    }
                    else
                    {
                        // Se não conseguir converter, o token é considerado inválido
                        Clear();
                        return false;
                    }
                }
                return true;
            }
            catch (Exception)
            {
                Clear();
                return false;
            }
        }
    }
}
