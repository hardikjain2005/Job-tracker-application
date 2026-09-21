using System.ComponentModel.DataAnnotations;

namespace JobTracker.Api.Dtos;

public record RegisterRequest(
    [Required, EmailAddress, StringLength(256)] string Email,
    [Required, StringLength(100, MinimumLength = 8)] string Password);

public record LoginRequest(
    [Required] string Email,
    [Required] string Password);

public record LoginResponse(string Token, string Email);
