using Identity.Domain.Enums;

namespace Identity.Application.DTOs.Users
{
    public class UserSummaryResponse
    {
        public Guid Id { get; set; }
        public required string Email { get; set; }  
        public required string Name { get; set; }
        public required UserRole Role { get; set; }
        public bool EmailVerified { get; set; }
    }
}
