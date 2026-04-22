using Microsoft.EntityFrameworkCore;
using Unstapp.Application.DTOs;
using Unstapp.Application.Interfaces;
using Unstapp.Infrastructure.Entities;
using Unstapp.Infrastructure.Interfaces;

namespace Unstapp.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;

        public AuthService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto dto)
        {
            var user = await _userRepository.GetByDniAsync(dto.Dni);

            if(user == null) return null;

            bool isValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.Password);

            if(!isValid) return null;

            return new LoginResponseDto
            {
                UserId = user.UserId,
                FullName = $"{user.Name} {user.LastName}",
                Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList()
            };
        }

        public async Task RegisterAsync(RegisterRequestDto dto)
        {
            string hashedPass = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var user = new User
            {
                Name = dto.Name,
                LastName = dto.LastName,
                Email = dto.Mail,
                Password = hashedPass,
                DNI = dto.DNI,
                PhoneNumber = dto.PhoneNumber,
                FirstTime = true
            };

            await _userRepository.AddAsync(user);
        }
    }
}
