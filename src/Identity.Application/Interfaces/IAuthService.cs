using Identity.Application.DTOs;

namespace Identity.Application.Interfaces
{
    public interface IAuthService
    {
       Task<RegisterResponse> RegisterAsync(RegisterRequest request);
    }
}
