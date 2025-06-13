using FluentAssertions;
using NUnit.Framework;
using WebApp.Services;

namespace WebAPI.UnitTests.Services
{
    [TestFixture]
    public class AuthService_ClearTests
    {
        [Test]
        public void Clear_Removes_All_State()
        {
            var svc = new AuthService
            {
                UserName = "A", Role = "ADMIN", Token = "x", UserId = 99
            };

            svc.Clear();

            svc.UserName.Should().BeEmpty();
            svc.Role.Should().BeEmpty();
            svc.Token.Should().BeEmpty();
            svc.UserId.Should().Be(0);
        }
    }
}
