using Identity.Domain.Entities;

namespace Identity.Application.Interfaces
{
    public interface ITokenService
    {
        public string GenerateToken(User user);
        
        public string GenerateRefreshToken();
    }
}
