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


        public static bool IsProffesor(List<string> roles)
        {
            return roles.Any(role =>
                role.Equals("Docente", StringComparison.OrdinalIgnoreCase) ||
                role.Equals("Profesor", StringComparison.OrdinalIgnoreCase));
        }
    }
}
