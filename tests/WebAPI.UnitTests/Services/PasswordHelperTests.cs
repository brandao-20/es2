using FluentAssertions;
using NUnit.Framework;
using WebAPI.Helpers;

namespace WebAPI.UnitTests.Services
{
    [TestFixture]
    public class PasswordHelperTests
    {
        [Test]
        public void HashPassword_ThenVerifyPassword_ReturnsTrue()
        {
            var senha = "SenhaMuitoSegura123!";
            var hash = PasswordHelper.HashPassword(senha);
            hash.Should().Contain(":");
            PasswordHelper.VerifyPassword(senha, hash).Should().BeTrue();
            PasswordHelper.VerifyPassword("errada", hash).Should().BeFalse();
        }

        [Test]
        public void VerifyPassword_WithMalformedHash_ReturnsFalse()
        {
            PasswordHelper.VerifyPassword("qualquer", "malformed_hash").Should().BeFalse();
        }
    }
}
