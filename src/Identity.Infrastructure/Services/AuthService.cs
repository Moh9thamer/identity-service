using FluentValidation;
using Identity.Application.DTOs.Auth;
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
        private readonly IValidator<RegisterRequest> _registerValidator;
        private readonly IValidator<LoginRequest> _loginValidator;

        public AuthService(
            AppDbContext dbContext,
            ITokenService tokenService,
            IOptions<JwtSettings> jwtSettings,
            IValidator<RegisterRequest> registerValidator,
            IValidator<LoginRequest> loginValidator)
        {
            _dbContext = dbContext;
            _tokenService = tokenService;
            _jwtSettings = jwtSettings.Value!;
            _registerValidator = registerValidator;
            _loginValidator = loginValidator;

        }

        public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
        {
            await _registerValidator.ValidateAndThrowAsync(request);

            var normalizedEmail = request.Email.ToLower();

            var isEmailTaken = await IsEmailTaken(normalizedEmail);

            if (isEmailTaken)
            {
                throw new Exception("Email is already taken.");
            }
            ;

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
        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            await _loginValidator.ValidateAndThrowAsync(request);

            var user = await ValidateUser(request);

            var token = _tokenService.GenerateToken(user);

            var refreshToken = await GenerateRefreshTokenAsync(user.Id);

            return new LoginResponse
            {
                AccessToken = token,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
                RefreshToken = refreshToken.Token
            };
        }

        public async Task<RefreshTokenResponse> RefreshTokenAsync(RefreshTokenRequest request)
        {

            var newRefreshToken = await RotateRefreshTokenAsync(request.RefreshToken);

            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == newRefreshToken.UserId);
            if (user == null) throw new UnauthorizedAccessException();

            return new RefreshTokenResponse
            {
                AccessToken = _tokenService.GenerateToken(user),
                ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
                RefreshToken = newRefreshToken.Token
            };

        }
        public async Task LogoutAsync(LogoutRequest request)
        {
            var token = await _dbContext.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken);
            if (token == null) return;
            token.RevokedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
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

        private async Task<RefreshToken> GenerateRefreshTokenAsync(Guid userId)
        {
            var refreshToken = new RefreshToken
            {
                UserId = userId,
                Token = _tokenService.GenerateRefreshToken(),
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };
            await _dbContext.RefreshTokens.AddAsync(refreshToken);
            await _dbContext.SaveChangesAsync();
            return refreshToken;
        }

        private async Task<RefreshToken> RotateRefreshTokenAsync(string refreshToken)
        {
            var existingToken = await _dbContext.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == refreshToken);
            if (existingToken == null || existingToken.ExpiresAt < DateTime.UtcNow || existingToken.RevokedAt != null)
            {
                throw new UnauthorizedAccessException("Invalid refresh token.");
            }
            existingToken.RevokedAt = DateTime.UtcNow;
            var newRefreshToken = new RefreshToken
            {
                UserId = existingToken.UserId,
                Token = _tokenService.GenerateRefreshToken(),
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };
            await _dbContext.RefreshTokens.AddAsync(newRefreshToken);
            await _dbContext.SaveChangesAsync();
            return newRefreshToken;
        }
    }
}   
