namespace JobTracker.Api.Models;

public class JobApplication
{
    public int Id { get; set; }
    public string Company { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public ApplicationStatus Status { get; set; } = ApplicationStatus.Applied;
    public DateOnly AppliedDate { get; set; }
    public string? Notes { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;
}
