namespace Unstapp.Shared.Helpers
{
    public static class RoleHelper
    {
        public static bool IsAdmin(List<string> roles)
        {
            return roles.Any(role =>
                role.Equals("Admin", StringComparison.OrdinalIgnoreCase) ||
                role.Equals("Administrador", StringComparison.OrdinalIgnoreCase) ||
                role.Equals("Administracion", StringComparison.OrdinalIgnoreCase) ||
                role.Equals("Administrativo", StringComparison.OrdinalIgnoreCase));
        }
    }
}
