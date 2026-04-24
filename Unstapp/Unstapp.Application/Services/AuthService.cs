using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Unstapp.Application.DTOs;
using Unstapp.Application.Interfaces;
using Unstapp.Infrastructure.Entities;
using Unstapp.Infrastructure.Interfaces;

namespace Unstapp.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IJwtService _jwtService;
        private readonly IConfiguration _config;

        public AuthService(
            IUserRepository userRepository,
            IMapper mapper,
            IJwtService jwtService,
            IConfiguration config)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _jwtService = jwtService;
            _config = config;
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
    }
}
