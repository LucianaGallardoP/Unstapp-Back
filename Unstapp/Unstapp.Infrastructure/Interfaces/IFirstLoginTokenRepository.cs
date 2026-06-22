using Unstapp.Infrastructure.Entities;

namespace Unstapp.Infrastructure.Interfaces
{
    public interface IFirstLoginTokenRepository
    {
        Task AddAsync(FirstLoginToken token);

        /// <summary>
        /// Invalida (marca como usados) los tokens previos no utilizados del usuario,
        /// de modo que solo el último enlace enviado sea válido.
        /// </summary>
        Task InvalidatePreviousAsync(int userId);

        /// <summary>
        /// Recupera un token vigente (no usado y no expirado) por su hash, incluyendo el usuario asociado.
        /// </summary>
        Task<FirstLoginToken?> GetActiveByHashAsync(string tokenHash);

        Task SaveChangesAsync();
    }
}
