using Identity.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;
using Microsoft.Extensions.Configuration;

namespace Identity.Api.Tests
{
    public class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private readonly MsSqlContainer _sqlContainer = new MsSqlBuilder().Build();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureAppConfiguration(config =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:SecretKey"] = "test-secret-key-for-integration-tests-minimum-32-chars"
                });
            });

            builder.ConfigureServices(services =>
            {
                // Replace SQL Server with test container
                var dbDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (dbDescriptor != null) services.Remove(dbDescriptor);

                services.AddDbContext<AppDbContext>(options =>
                    options.UseSqlServer(_sqlContainer.GetConnectionString()));

                // Replace Redis with in-memory cache
                var redisDescriptors = services.Where(d => d.ServiceType == typeof(IDistributedCache)).ToList();
                foreach (var descriptor in redisDescriptors)
                    services.Remove(descriptor);

                services.AddDistributedMemoryCache();
            });
        }

        public async Task InitializeAsync()
        {
            await _sqlContainer.StartAsync();
        }

        public new async Task DisposeAsync()
        {
            await _sqlContainer.DisposeAsync();
        }
    }
}
