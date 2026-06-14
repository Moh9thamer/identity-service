using FluentValidation;
using Identity.Application.DTOs.Auth;
using Identity.Application.Exceptions;
using Identity.Application.Interfaces;
using Identity.Application.Validators.Auth;
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
        private readonly IValidator<ForgotPasswordRequest> _forgotPasswordValidator;
        private readonly IValidator<ResetPasswordRequest> _resetPasswordValidator;
        private readonly IEmailService _emailService;

        public AuthService(
            AppDbContext dbContext,
            ITokenService tokenService,
            IOptions<JwtSettings> jwtSettings,
            IValidator<RegisterRequest> registerValidator,
            IValidator<LoginRequest> loginValidator,
            IEmailService emailService,
            IValidator<ForgotPasswordRequest> forgotPasswordValidator,
            IValidator<ResetPasswordRequest> resetPasswordValidator)
        {
            _dbContext = dbContext;
            _tokenService = tokenService;
            _jwtSettings = jwtSettings.Value!;
            _registerValidator = registerValidator;
            _loginValidator = loginValidator;
            _emailService = emailService;
            _forgotPasswordValidator = forgotPasswordValidator;
            _resetPasswordValidator = resetPasswordValidator;
        }

        public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
        {
            await _registerValidator.ValidateAndThrowAsync(request);

            var normalizedEmail = request.Email.ToLower();

            var isEmailTaken = await IsEmailTaken(normalizedEmail);

            if (isEmailTaken) throw new ConflictException("Email is already taken.");

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new User
            {
                Email = normalizedEmail,
                PasswordHash = hashedPassword,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Role = UserRole.User,
                VerificationToken = _tokenService.GenerateRandomToken(),
                VerificationTokenExpiry = DateTime.UtcNow.AddHours(24)
            };

            var registeredUser = await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            await _emailService.SendVerificationEmailAsync(user.Email, user.VerificationToken!);

            return new RegisterResponse
            {
                UserId = registeredUser.Entity.Id,
                Email = registeredUser.Entity.Email,
            };
        }

        public async Task VerifyEmailAsync(VerifyEmailRequest request)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.VerificationToken == request.Token);

            if (user == null) throw new NotFoundException("Invalid verification token.");

            if (user.EmailVerified) throw new ConflictException("Email is already verified.");

            if (user.VerificationTokenExpiry < DateTime.UtcNow) throw new ConflictException("Verification token has expired");

            user.VerificationToken = null;
            user.VerificationTokenExpiry = null;
            user.EmailVerified = true;

            await _dbContext.SaveChangesAsync();
        }
        public async Task ResendVerificationEmailAsync(ResendVerificationEmailRequest request)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == request.Email.ToLower());

            if (user==null) throw new NotFoundException("User not found.");

            if (user.EmailVerified) throw new ConflictException("Email is already verified.");

            user.VerificationToken = _tokenService.GenerateRandomToken();
            user.VerificationTokenExpiry = DateTime.UtcNow.AddHours(24);

            await _dbContext.SaveChangesAsync();

            await _emailService.SendVerificationEmailAsync(user.Email, user.VerificationToken!);
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            await _loginValidator.ValidateAndThrowAsync(request);

            var user = await ValidateUser(request);

            if (!user.EmailVerified) throw new ConflictException("Verify your email first.");

            var token = _tokenService.GenerateAccessToken(user);

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
                AccessToken = _tokenService.GenerateAccessToken(user),
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

        public async Task ForgotPasswordAsync(ForgotPasswordRequest request)
        {
            await _forgotPasswordValidator.ValidateAndThrowAsync(request);

            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == request.Email.ToLower());

            // no throw  for security reasons, we don't want to reveal whether the email exists or not
            if (user == null) return;

            user.PasswordResetToken = _tokenService.GenerateRandomToken();
            user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);
            await _dbContext.SaveChangesAsync();

            await _emailService.SendPasswordResetEmailAsync(user.Email, user.PasswordResetToken!);
        }
        public async Task ResetPasswordAsync(ResetPasswordRequest request)
        {
           await _resetPasswordValidator.ValidateAndThrowAsync(request);

           var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.PasswordResetToken == request.Token);

            if(user == null) throw new NotFoundException("Invalid password reset token.");

            if (user.PasswordResetTokenExpiry < DateTime.UtcNow) throw new ConflictException("Reset token has expired.");

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

            user.PasswordHash = hashedPassword;
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiry = null;

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
                Token = _tokenService.GenerateRandomToken(),
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
                Token = _tokenService.GenerateRandomToken(),
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };
            await _dbContext.RefreshTokens.AddAsync(newRefreshToken);
            await _dbContext.SaveChangesAsync();
            return newRefreshToken;
        }

    }
}   
