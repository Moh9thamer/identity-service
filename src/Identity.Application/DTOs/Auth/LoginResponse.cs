namespace Identity.Application.DTOs.Auth
{
    public class LoginResponse
    {
        public required string AccessToken { get; set; }
        public DateTime ExpiresAt { get; set; }
        public required string RefreshToken { get; set; }
    }
}
