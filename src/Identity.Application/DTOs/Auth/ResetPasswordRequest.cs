namespace Identity.Application.DTOs.Auth
{
    public class ResetPasswordRequest
    {
        public required string Token {  get; set; }
        public required string Password { get; set; }
    }
}
