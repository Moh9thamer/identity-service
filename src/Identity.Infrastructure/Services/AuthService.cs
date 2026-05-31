using Identity.Application.DTOs;
using Identity.Application.Interfaces;
using Identity.Domain.Enums;
using Identity.Domain.Entities;
using Identity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _dbContext;
        public AuthService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
        {

            var normalizedEmail = request.Email.ToLower();

            var isEmailTaken = await IsEmailTaken(normalizedEmail);

            if(isEmailTaken)
            {
                throw new Exception("Email is already taken.");
            };

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new User
            {
                Email = normalizedEmail,
                PasswordHash = hashedPassword,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Role = UserRole.User
            };

           var registeredUser = await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            return new RegisterResponse
            {
                UserId = registeredUser.Entity.Id,
                Email = registeredUser.Entity.Email
            };
        }

        private async Task<bool> IsEmailTaken(string email)
        {
            return await _dbContext.Users.AnyAsync(u => u.Email == email);
        }
    }
}   
