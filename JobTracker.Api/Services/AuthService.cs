using System.Security.Claims;
using System.Text;
using JobTracker.Api.Data;
using JobTracker.Api.Dtos;
using JobTracker.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace JobTracker.Api.Services;

public class AuthService(AppDbContext db, IConfiguration config)
{
    /// <returns>false when the email is already registered.</returns>
    public async Task<bool> RegisterAsync(RegisterRequest request)
    {
        var email = Normalize(request.Email);
        if (await db.Users.AnyAsync(u => u.Email == email))
            return false;

        db.Users.Add(new User
        {
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        });
        await db.SaveChangesAsync();
        return true;
    }

    /// <returns>null when the email or password is wrong (deliberately not saying which).</returns>
    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        var email = Normalize(request.Email);
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return null;

        return new LoginResponse(CreateToken(user), user.Email);
    }

    private string CreateToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email)
            ]),
            Issuer = config["Jwt:Issuer"],
            Audience = config["Jwt:Audience"],
            Expires = DateTime.UtcNow.AddMinutes(config.GetValue("Jwt:ExpiryMinutes", 60)),
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        };
        return new JsonWebTokenHandler().CreateToken(descriptor);
    }

    private static string Normalize(string email) => email.Trim().ToLowerInvariant();
}
