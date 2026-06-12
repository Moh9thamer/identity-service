namespace Identity.Application.DTOs.Auth
{
    public class LogoutRequest
    {
        public required string RefreshToken { get; set; }
    }
}
