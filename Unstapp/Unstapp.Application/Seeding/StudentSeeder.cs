using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Unstapp.Infrastructure.Data;
using Unstapp.Infrastructure.Entities;

namespace Unstapp.Application.Seeding
{
    /// <summary>
    /// Siembra alumnos "pre-empadronados" (simulando la base de datos de la universidad)
    /// para poder probar el flujo de primer login. Es idempotente: no duplica si el DNI ya existe.
    /// Se ejecuta solo cuando la config "SeedStudents" está en true.
    /// </summary>
    public static class StudentSeeder
    {
        private record SeedStudent(string Name, string LastName, string DNI, string Email, string PhoneNumber);

        private static readonly SeedStudent[] Students =
        {
            new("Lucía",    "Gómez",     "40111222", "lucia.gomez@alumnos.unstapp.edu",    "3511112201"),
            new("Mateo",    "Fernández", "40222333", "mateo.fernandez@alumnos.unstapp.edu","3511112202"),
            new("Sofía",    "Rodríguez", "40333444", "sofia.rodriguez@alumnos.unstapp.edu","3511112203"),
            new("Bautista", "López",     "40444555", "bautista.lopez@alumnos.unstapp.edu", "3511112204"),
            new("Valentina","Martínez",  "40555666", "valentina.martinez@alumnos.unstapp.edu","3511112205"),
            new("Thiago",   "Sánchez",   "40666777", "thiago.sanchez@alumnos.unstapp.edu", "3511112206"),
        };

        /// <summary>
        /// Siembra los alumnos de prueba. Si se pasa <paramref name="testStudentEmail"/>
        /// (configurable por la env var "Seed:TestStudentEmail") agrega además un alumno
        /// con ese correo REAL, para poder recibir el mail de verificación en staging.
        /// </summary>
        public static async Task SeedAsync(
            AppDbContext db,
            ILogger? logger = null,
            string? testStudentEmail = null,
            string? testStudentDni = null)
        {
            var toSeed = Students.ToList();

            if (!string.IsNullOrWhiteSpace(testStudentEmail))
            {
                var dni = string.IsNullOrWhiteSpace(testStudentDni) ? "40999000" : testStudentDni.Trim();
                toSeed.Add(new SeedStudent("Alumno", "Prueba", dni, testStudentEmail.Trim(), "3510009999"));
            }

            var inserted = 0;

            foreach (var s in toSeed)
            {
                // Idempotente por DNI o por email (evita duplicar el alumno de prueba si cambia el DNI).
                var exists = await db.Users.AnyAsync(u => u.DNI == s.DNI || u.Email == s.Email);
                if (exists)
                {
                    continue;
                }

                db.Users.Add(new User
                {
                    Name = s.Name,
                    LastName = s.LastName,
                    DNI = s.DNI,
                    Email = s.Email,
                    PhoneNumber = s.PhoneNumber,
                    // Placeholder: el alumno todavía no eligió su clave. Es un hash BCrypt válido
                    // de un valor aleatorio, de modo que cualquier intento de login previo al
                    // primer ingreso falle limpiamente (401) en vez de romper la verificación.
                    Password = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString("N")),
                    FirstTime = true
                });

                inserted++;
            }

            if (inserted > 0)
            {
                await db.SaveChangesAsync();
            }

            logger?.LogInformation(
                "[StudentSeeder] Alumnos de prueba: {Inserted} nuevos insertados, {Skipped} ya existentes.",
                inserted, toSeed.Count - inserted);
        }
    }
}
