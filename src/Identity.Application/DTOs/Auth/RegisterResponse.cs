using Identity.Domain.Enums;

namespace Identity.Application.DTOs.Auth
{
    public class RegisterResponse
    {
        public Guid UserId { get; set; }
        public required string Email { get; set; }
    }
}
