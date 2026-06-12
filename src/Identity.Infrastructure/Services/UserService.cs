using Identity.Application.DTOs.Users;
using Identity.Application.Interfaces;
using Identity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _dbContext;

        public UserService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<UserResponse> GetUserAsync(Guid userId)
        {
            return await _dbContext.Users
                .Where(u => u.Id == userId)
                .Select(u => new UserResponse
                {
                    Email = u.Email,
                    Name = $"{u.FirstName} {u.LastName}"
                })
                .FirstOrDefaultAsync() ?? throw new Exception("User not found.");
        }
    }
}
