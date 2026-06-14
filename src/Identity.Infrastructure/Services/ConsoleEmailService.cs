using Identity.Application.Interfaces;

namespace Identity.Infrastructure.Services
{
    public class ConsoleEmailService : IEmailService
    {
        public Task SendVerificationEmailAsync(string email, string token)
        {
            Console.WriteLine($"Verification email sent to {email} with token: {token}");
            return Task.CompletedTask;
        }
    }
}
