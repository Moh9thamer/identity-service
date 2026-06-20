using Identity.Application.DTOs.Users;
using Identity.Application.Exceptions;
using Identity.Application.Interfaces;
using Identity.Domain.Entities;
using Identity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Identity.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _dbContext;
        private readonly IDistributedCache _cache;

        public UserService(AppDbContext dbContext, IDistributedCache cache)
        {
            _dbContext = dbContext;
            _cache = cache;
        }

        public async Task<IEnumerable<UserSummaryResponse>> GetAllUsersAsync()
        {
            var users = await _dbContext.Users
                .Select(u => new UserSummaryResponse
                {
                    Id = u.Id,
                    Email = u.Email,
                    Name = $"{u.FirstName} {u.LastName}",
                    Role = u.Role,
                    EmailVerified = u.EmailVerified
                })
                .ToListAsync();

            return users;
        }
        

        public async Task<UserResponse> GetUserAsync(Guid userId)
        {
            var cached = await GetCachedUserAsync(userId);
            if (cached != null) return cached;

            var user = await _dbContext.Users
                .Where(u => u.Id == userId)
                .Select(u => new UserResponse
                {
                    Email = u.Email,
                    Name = $"{u.FirstName} {u.LastName}"
                })
                .FirstOrDefaultAsync() ?? throw new NotFoundException("User is not found");

            await SetCachedUserAsync(userId, user);

            return user;
        }

        public async Task UpdateRoleAsync(Guid id, UpdateRoleRequest request)
        {
           var user = await _dbContext.Users.FindAsync(id) ?? throw new NotFoundException("User is not found");

            user.Role = request.Role;

            await _dbContext.SaveChangesAsync();
        }
        private async Task SetCachedUserAsync(Guid userId, UserResponse user)
        {
            await _cache.SetStringAsync($"user:{userId}", JsonSerializer.Serialize(user), new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            });
        }
        private async Task<UserResponse?> GetCachedUserAsync(Guid userId)
        {
            var cacheKey = $"user:{userId}";

            var cached = await _cache.GetStringAsync(cacheKey);
            if (cached != null)
                return JsonSerializer.Deserialize<UserResponse>(cached)!;
            return default;
        }
    }
}
