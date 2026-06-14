using Identity.Domain.Enums;

namespace Identity.Application.DTOs.Auth
{
    public class RegisterResponse
    {
        public Guid UserId { get; set; }
        public required string Email { get; set; }
        public string Message { get; set; } = "Registration successful. Please check your email to verify your account.";
    }
}
