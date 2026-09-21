using System.ComponentModel.DataAnnotations;
using JobTracker.Api.Models;

namespace JobTracker.Api.Dtos;

public record ApplicationRequest(
    [Required, StringLength(200)] string Company,
    [Required, StringLength(200)] string Role,
    ApplicationStatus Status,
    DateOnly AppliedDate,
    [StringLength(2000)] string? Notes);

public record ApplicationResponse(
    int Id,
    string Company,
    string Role,
    ApplicationStatus Status,
    DateOnly AppliedDate,
    string? Notes);
