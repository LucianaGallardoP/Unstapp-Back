namespace Unstapp.Infrastructure.Entities
{
    public class FirstLoginToken
    {
        public int FirstLoginTokenId { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        /// <summary>
        /// Hash (SHA-256) del token. Nunca se guarda el token en texto plano:
        /// el valor crudo viaja únicamente en el enlace del correo.
        /// </summary>
        public string TokenHash { get; set; } = null!;

        public DateTime ExpiresAt { get; set; }
        public bool Used { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
