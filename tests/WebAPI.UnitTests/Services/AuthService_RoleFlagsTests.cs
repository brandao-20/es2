using FluentAssertions;
using NUnit.Framework;
using WebApp.Services;

namespace WebAPI.UnitTests.Services
{
    [TestFixture]
    public class AuthService_RoleFlagsTests
    {
        [TestCase("ADMIN",      true,  false, false)]
        [TestCase("USERMANAGER",false, true,  false)]
        [TestCase("USER",       false, false, true)]
        public void RoleFlags_Work(string role,bool isAdmin,bool isManager,bool isUser)
        {
            var svc = new AuthService { Role = role };
            svc.IsAdmin.Should().Be(isAdmin);
            svc.IsManager.Should().Be(isManager);
            svc.IsUser.Should().Be(isUser);
        }
    }
}
