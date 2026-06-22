using Identity.Application.DTOs.Auth;
using Identity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace Identity.Api.Tests.Auth
{
    public class AuthControllerTests : IClassFixture<ApiFactory>, IAsyncLifetime
    {
        private readonly HttpClient _client;
        private readonly ApiFactory _factory;

        public AuthControllerTests(ApiFactory factory)
        {
            _client = factory.CreateClient();
            _factory = factory;
        }

        public async Task InitializeAsync()
        {
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await dbContext.Users.ExecuteDeleteAsync();
            await dbContext.RefreshTokens.ExecuteDeleteAsync();
        }
        public Task DisposeAsync() => Task.CompletedTask;


        [Fact]
        public async Task Register_WithValidData_Returns201()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Email = "test@example.com",
                Password = "password123",
                FirstName = "Test",
                LastName = "User"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/register", request);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task Register_WithDuplicateEmail_Returns409()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Email = "duplicate@example.com",
                Password = "password123",
                FirstName = "Test",
                LastName = "User"
            };

            // Act
            await _client.PostAsJsonAsync("/api/auth/register", request);
            var response = await _client.PostAsJsonAsync("/api/auth/register", request);

            // Assert
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Fact]
        public async Task Register_WithInvalidEmail_Returns400()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Email = "not-an-email",
                Password = "password123",
                FirstName = "Test",
                LastName = "User"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/register", request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Login_WithWrongPassword_Returns401()
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                Email = "login-test@example.com",
                Password = "password123",
                FirstName = "Test",
                LastName = "User"
            };
            await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

            var loginRequest = new LoginRequest
            {
                Email = "login-test@example.com",
                Password = "wrongpassword"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]

        public async Task Login_WithUnverifiedEmail_Returns409()
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                Email = "login-test-unverfied@test.com",
                Password = "password123",
                FirstName = "Test",
                LastName = "User"
            };

            await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

            var loginRequest = new LoginRequest
            {
                Email = "login-test-unverfied@test.com",
                Password = "password123"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

            // Assert
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Fact]
        public async Task Login_WithValidCredentials_Returns200()
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                Email = "login-test-verfied@test.com",
                Password = "password123",
                FirstName = "Test",
                LastName = "User"
            };

            await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

            // validate the email
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var user = await dbContext.Users.FirstAsync(u => u.Email == "login-test-verfied@test.com");
            user.EmailVerified = true;
            await dbContext.SaveChangesAsync();

            var loginRequest = new LoginRequest
            {
                Email = "login-test-verfied@test.com",
                Password = "password123"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        }
    }
}
