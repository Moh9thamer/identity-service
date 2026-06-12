using Identity.Application.DTOs.Auth;

namespace Identity.Application.Interfaces
{
    public interface IAuthService
    {
       Task<RegisterResponse> RegisterAsync(RegisterRequest request);
       Task<LoginResponse> LoginAsync(LoginRequest request);
       Task<RefreshTokenResponse> RefreshTokenAsync(RefreshTokenRequest request);
       Task LogoutAsync(LogoutRequest request);
    }
}
