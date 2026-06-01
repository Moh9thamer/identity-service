using Identity.Application.Interfaces;
using Identity.Infrastructure.Authentication;
using Identity.Infrastructure.Persistence;
using Identity.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPersistenceServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "Connection string 'DefaultConnection' not found in configuration. " +
                    "Ensure it's in appsettings.json or User Secrets.");

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(connectionString);

                if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")))
                {
                    var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                    if (env == "Development")
                    {
                        options.EnableSensitiveDataLogging();
                        options.LogTo(Console.WriteLine);
                    }
                }
            });

            services.Configure<JwtSettings>(configuration.GetSection("Jwt"));

            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ITokenService, TokenService>();

            return services;
        }

        public static async Task ApplyMigrationsAsync(
           this IServiceProvider services)
        {
            using var scope = services.CreateScope();

            var dbContext = scope.ServiceProvider
                .GetRequiredService<AppDbContext>();

            await dbContext.Database.MigrateAsync();
        }
    }
}