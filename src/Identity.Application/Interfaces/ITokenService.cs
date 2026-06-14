using Identity.Domain.Entities;

namespace Identity.Application.Interfaces
{
    public interface ITokenService
    {
        public string GenerateAccessToken(User user);
        
        public string GenerateRandomToken();
    }
}
