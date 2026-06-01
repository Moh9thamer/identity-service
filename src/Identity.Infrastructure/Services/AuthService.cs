using Identity.Application.DTOs;
using Identity.Application.Interfaces;
using Identity.Domain.Entities;
using Identity.Domain.Enums;
using Identity.Infrastructure.Authentication;
using Identity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Identity.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _dbContext;
        private readonly ITokenService _tokenService;
        private readonly JwtSettings _jwtSettings;
        public AuthService(AppDbContext dbContext, ITokenService tokenService, IOptions<JwtSettings> jwtSettings)
        {
            _dbContext = dbContext;
            _tokenService = tokenService;
            _jwtSettings = jwtSettings.Value!;
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
           
            var user = await ValidateUser(request);

            var token = _tokenService.GenerateToken(user);

            return new LoginResponse
            {
                AccessToken = token,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes)
            };
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

        private async Task<User> ValidateUser(LoginRequest loginRequest)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == loginRequest.Email.ToLower());
            if (user == null) throw new UnauthorizedAccessException();

            var isPasswordValid = BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.PasswordHash);
            if (!isPasswordValid) throw new UnauthorizedAccessException();

            return user;
        }
    }
}   
