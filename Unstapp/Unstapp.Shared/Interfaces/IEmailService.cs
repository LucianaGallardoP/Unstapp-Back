namespace Unstapp.Shared.Interfaces
{
    public interface IEmailService
    {
        /// <summary>
        /// Despacha el correo de verificación de primer inicio de sesión,
        /// con el enlace (ya ensamblado) hacia la vista de creación de contraseña.
        /// </summary>
        Task SendFirstLoginEmailAsync(string toEmail, string fullName, string confirmationLink);
    }
}
