using Identity.Application.DTOs.Users;

namespace Identity.Application.Interfaces
{
    public interface IUserService
    {
        Task<UserResponse> GetUserAsync(Guid userId);
    }
}
