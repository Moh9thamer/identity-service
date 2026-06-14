using Identity.Application.Interfaces;

namespace Identity.Infrastructure.Services
{
    public class ConsoleEmailService : IEmailService
    {
        public Task SendPasswordResetEmailAsync(string email, string token)
        {
            Console.WriteLine($"Reset password email sent to {email} with token: {token}");
            return Task.CompletedTask;
        }

        public Task SendVerificationEmailAsync(string email, string token)
        {
            Console.WriteLine($"Verification email sent to {email} with token: {token}");
            return Task.CompletedTask;
        }
    }
}
