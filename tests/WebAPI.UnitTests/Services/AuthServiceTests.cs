using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using FluentAssertions;
using NUnit.Framework;
using WebApp.Services;

namespace WebAPI.UnitTests.Services
{
    [TestFixture]
    public class AuthServiceTests
    {
        private AuthService _auth;

        [SetUp]
        public void SetUp() => _auth = new AuthService();

        private string BuildToken(DateTimeOffset exp)
        {
            var handler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(JwtRegisteredClaimNames.Exp, exp.ToUnixTimeSeconds().ToString())
                }),
                Expires = exp.UtcDateTime, // Define explicitamente a expiração
                IssuedAt = DateTime.UtcNow,
                NotBefore = DateTime.UtcNow.AddMinutes(-10) // Garante que o token é válido antes da expiração
            };
            var token = handler.CreateToken(tokenDescriptor);
            return handler.WriteToken(token);
        }

        [Test]
        public void ValidateToken_ShouldReturnFalse_IfTokenEmpty()
        {
            _auth.Token = "";
            _auth.ValidateToken().Should().BeFalse();
        }

        [Test]
        public void ValidateToken_ShouldReturnFalse_IfExpired()
        {
            _auth.Token = BuildToken(DateTimeOffset.UtcNow.AddMinutes(-5));
            _auth.ValidateToken().Should().BeFalse();
        }

        [Test]
        public void ValidateToken_ShouldReturnTrue_IfValidFuture()
        {
            _auth.Token = BuildToken(DateTimeOffset.UtcNow.AddMinutes(10));
            _auth.ValidateToken().Should().BeTrue();
        }
    }
}
