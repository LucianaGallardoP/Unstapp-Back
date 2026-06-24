using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Unstapp.Application.DTOs;
using Unstapp.Application.Interfaces;
using Unstapp.Infrastructure.Entities;
using Unstapp.Infrastructure.Interfaces;
using Unstapp.Shared.DTOs.Common;
using Unstapp.Shared.Interfaces;

namespace Unstapp.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IJwtService _jwtService;
        private readonly IConfiguration _config;
        private readonly IFirstLoginTokenRepository _firstLoginTokenRepository;
        private readonly IEmailService _emailService;

        private const int FirstLoginTokenTtlMinutes = 10;

        public AuthService(
            IUserRepository userRepository,
            IMapper mapper,
            IJwtService jwtService,
            IConfiguration config,
            IFirstLoginTokenRepository firstLoginTokenRepository,
            IEmailService emailService)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _jwtService = jwtService;
            _config = config;
            _firstLoginTokenRepository = firstLoginTokenRepository;
            _emailService = emailService;
        }

        public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto dto)
        {

            var dni = dto.DNI.Trim();

            var user = await _userRepository.GetByDniAsync(dni);

            if(user == null) return null;

            bool isValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.Password);

            if(!isValid) return null;

            var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();

            var token = _jwtService.GenerateToken(user.UserId, user.DNI, roles);

            var expiresInMinutes = int.Parse(_config["Jwt:ExpiresInMinutes"] ?? "60");

            return new LoginResponseDto
            {
                UserId = user.UserId,
                FullName = $"{user.Name} {user.LastName}",
                Roles = roles,
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddMinutes(expiresInMinutes),
            };
        }

        public async Task RegisterAsync(RegisterRequestDto dto)
        {
            var existingUser = await _userRepository.GetByDniAsync(dto.DNI);

            if(existingUser != null) throw new Exception("Ya existe un usuario con ese DNI");

            var user = _mapper.Map<User>(dto);

            user.Password = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            await _userRepository.AddAsync(user);
        }

        public async Task<ServiceResult<MessageResponseDto>> VerifyFirstTimeAsync(VerifyFirstTimeRequestDto dto)
        {
            var dni = dto.DNI.Trim();

            var user = await _userRepository.GetByDniAsync(dni);

            // Caso 3 (TC-BE-43): el DNI no está empadronado en la institución.
            if (user == null)
            {
                return ServiceResult<MessageResponseDto>.Fail(
                    StatusCodes.Status404NotFound,
                    "STUDENT_NOT_FOUND",
                    "Alumno no encontrado.");
            }

            // Caso 2 (TC-BE-42): el primer ingreso ya fue completado previamente.
            if (!user.FirstTime)
            {
                return ServiceResult<MessageResponseDto>.Fail(
                    StatusCodes.Status409Conflict,
                    "ALREADY_REGISTERED",
                    "Este DNI ya se encuentra registrado.");
            }

            // Caso 1 (TC-BE-41 / TC-BE-44 / TC-BE-45 / TC-BE-46): generar token y disparar el correo.
            var rawToken = GenerateRawToken();
            var tokenHash = HashToken(rawToken);

            await _firstLoginTokenRepository.InvalidatePreviousAsync(user.UserId);
            await _firstLoginTokenRepository.AddAsync(new FirstLoginToken
            {
                UserId = user.UserId,
                TokenHash = tokenHash,
                ExpiresAt = DateTime.UtcNow.AddMinutes(FirstLoginTokenTtlMinutes),
                CreatedAt = DateTime.UtcNow,
                Used = false
            });

            var confirmationLink = BuildConfirmationLink(rawToken);

            await _emailService.SendFirstLoginEmailAsync(
                user.Email,
                $"{user.Name} {user.LastName}",
                confirmationLink);

            return ServiceResult<MessageResponseDto>.Ok(new MessageResponseDto
            {
                Message = "Te enviamos un enlace de registro al correo asociado a tu DNI."
            });
        }

        public async Task<ServiceResult<MessageResponseDto>> SetInitialPasswordAsync(SetInitialPasswordRequestDto dto)
        {
            var tokenHash = HashToken(dto.Token);

            // TC-BE-48: token inexistente, alterado o expirado.
            var token = await _firstLoginTokenRepository.GetActiveByHashAsync(tokenHash);

            if (token == null)
            {
                return ServiceResult<MessageResponseDto>.Fail(
                    StatusCodes.Status401Unauthorized,
                    "INVALID_OR_EXPIRED_TOKEN",
                    "El enlace no es válido o ha expirado.");
            }

            // Defensa extra: si entre el envío y el uso el alumno ya completó el primer ingreso.
            if (!token.User.FirstTime)
            {
                token.Used = true;
                await _firstLoginTokenRepository.SaveChangesAsync();

                return ServiceResult<MessageResponseDto>.Fail(
                    StatusCodes.Status409Conflict,
                    "ALREADY_REGISTERED",
                    "Este DNI ya se encuentra registrado.");
            }

            // TC-BE-47: hashear la contraseña, persistir y marcar el token como usado.
            token.User.Password = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            token.User.FirstTime = false;
            token.Used = true;

            await _firstLoginTokenRepository.SaveChangesAsync();

            return ServiceResult<MessageResponseDto>.Ok(new MessageResponseDto
            {
                Message = "Tu cuenta fue activada correctamente."
            });
        }

        private static string GenerateRawToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(32);

            // Base64 url-safe para poder viajar como query param sin escapado adicional.
            return Convert.ToBase64String(bytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .TrimEnd('=');
        }

        private static string HashToken(string rawToken)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
            return Convert.ToHexString(bytes);
        }

        private string BuildConfirmationLink(string rawToken)
        {
            var baseUrl = (_config["Frontend:BaseUrl"] ?? string.Empty).TrimEnd('/');
            var path = _config["Frontend:CreatePasswordPath"] ?? "/crear-clave";

            if (!path.StartsWith("/"))
            {
                path = "/" + path;
            }

            return $"{baseUrl}{path}?token={Uri.EscapeDataString(rawToken)}";
        }
    }
}
