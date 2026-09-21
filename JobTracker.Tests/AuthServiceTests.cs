using JobTracker.Api.Dtos;
using JobTracker.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace JobTracker.Tests;

public class AuthServiceTests : IDisposable
{
    private readonly TestDb _testDb = new();

    public void Dispose() => _testDb.Dispose();

    private AuthService NewService()
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:Key"] = new string('k', 48),
            ["Jwt:Issuer"] = "test-issuer",
            ["Jwt:Audience"] = "test-audience",
            ["Jwt:ExpiryMinutes"] = "5"
        }).Build();
        return new AuthService(_testDb.NewContext(), config);
    }

    [Fact]
    public async Task Register_StoresBCryptHash_NotThePlainPassword()
    {
        Assert.True(await NewService().RegisterAsync(new RegisterRequest("Me@Test.com", "password123")));

        var user = await _testDb.NewContext().Users.SingleAsync();
        Assert.Equal("me@test.com", user.Email); // normalized
        Assert.NotEqual("password123", user.PasswordHash);
        Assert.True(BCrypt.Net.BCrypt.Verify("password123", user.PasswordHash));
    }

    [Fact]
    public async Task Register_DuplicateEmail_IsRejected()
    {
        var service = NewService();
        await service.RegisterAsync(new RegisterRequest("me@test.com", "password123"));

        Assert.False(await service.RegisterAsync(new RegisterRequest("ME@test.com", "password456")));
    }

    [Fact]
    public async Task Login_CorrectPassword_ReturnsToken()
    {
        var service = NewService();
        await service.RegisterAsync(new RegisterRequest("me@test.com", "password123"));

        var result = await service.LoginAsync(new LoginRequest("me@test.com", "password123"));

        Assert.NotNull(result);
        Assert.Equal(3, result.Token.Split('.').Length); // header.payload.signature
    }

    [Theory]
    [InlineData("me@test.com", "wrong-password")]
    [InlineData("nobody@test.com", "password123")]
    public async Task Login_WrongCredentials_ReturnsNull(string email, string password)
    {
        var service = NewService();
        await service.RegisterAsync(new RegisterRequest("me@test.com", "password123"));

        Assert.Null(await service.LoginAsync(new LoginRequest(email, password)));
    }
}
