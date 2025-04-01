namespace WebAPI.Services
{
    public class RoleService : IRoleService
    {
        public string NormalizeRole(string roleDb)
        {
            if (string.IsNullOrEmpty(roleDb))
                return "User";

            return roleDb.ToUpper() switch
            {
                "ADMIN" => "Admin",
                "USER_MANAGER" => "UserManager",
                _ => "User"
            };
        }
    }
}
