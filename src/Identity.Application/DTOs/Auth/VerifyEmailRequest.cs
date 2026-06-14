namespace Identity.Application.DTOs.Auth
{
    public class VerifyEmailRequest
    {
        public required string Token { get; set; }
    }
}
