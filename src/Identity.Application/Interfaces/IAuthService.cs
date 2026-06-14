using Identity.Application.DTOs.Auth;
using Identity.Application.DTOs.Users;

namespace Identity.Application.Interfaces
{
    public interface IAuthService
    {
       Task<RegisterResponse> RegisterAsync(RegisterRequest request);
       Task<LoginResponse> LoginAsync(LoginRequest request);
       Task<RefreshTokenResponse> RefreshTokenAsync(RefreshTokenRequest request);
       Task LogoutAsync(LogoutRequest request);
       Task VerifyEmailAsync(VerifyEmailRequest request);
       Task ResendVerificationEmailAsync(ResendVerificationEmailRequest request);

        Task ForgotPasswordAsync(ForgotPasswordRequest request);
        Task ResetPasswordAsync (ResetPasswordRequest request);
    }
}
