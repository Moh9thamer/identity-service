using Identity.Domain.Enums;

namespace Identity.Application.DTOs.Users
{
    public class UpdateRoleRequest
    {
        public required UserRole Role { get; set; }
    }
}
