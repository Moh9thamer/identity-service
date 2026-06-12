namespace Identity.Application.DTOs.Auth
{
    public class RefreshTokenResponse
    {
        public required string AccessToken { get; set; }
        public DateTime ExpiresAt { get; set; }
        public required string RefreshToken { get; set; }
    }
}
