using JobTracker.Api.Data;
using JobTracker.Api.Dtos;
using JobTracker.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace JobTracker.Api.Services;

/// <summary>
/// All queries are scoped by userId, so one user can never read or modify another user's rows.
/// </summary>
public class ApplicationService(AppDbContext db)
{
    public async Task<List<ApplicationResponse>> GetAllAsync(int userId, ApplicationStatus? status)
    {
        var query = db.JobApplications.AsNoTracking().Where(a => a.UserId == userId);
        if (status is not null)
            query = query.Where(a => a.Status == status);

        return await query
            .OrderByDescending(a => a.AppliedDate).ThenByDescending(a => a.Id)
            .Select(a => ToResponse(a))
            .ToListAsync();
    }

    public async Task<ApplicationResponse?> GetByIdAsync(int userId, int id)
    {
        var entity = await db.JobApplications.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);
        return entity is null ? null : ToResponse(entity);
    }

    public async Task<ApplicationResponse> CreateAsync(int userId, ApplicationRequest request)
    {
        var entity = new JobApplication { UserId = userId };
        Apply(entity, request);
        db.JobApplications.Add(entity);
        await db.SaveChangesAsync();
        return ToResponse(entity);
    }

    public async Task<ApplicationResponse?> UpdateAsync(int userId, int id, ApplicationRequest request)
    {
        var entity = await db.JobApplications.FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);
        if (entity is null) return null;

        Apply(entity, request);
        await db.SaveChangesAsync();
        return ToResponse(entity);
    }

    public async Task<bool> DeleteAsync(int userId, int id)
    {
        var entity = await db.JobApplications.FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);
        if (entity is null) return false;

        db.JobApplications.Remove(entity);
        await db.SaveChangesAsync();
        return true;
    }

    private static void Apply(JobApplication entity, ApplicationRequest request)
    {
        entity.Company = request.Company.Trim();
        entity.Role = request.Role.Trim();
        entity.Status = request.Status;
        entity.AppliedDate = request.AppliedDate;
        entity.Notes = request.Notes;
    }

    private static ApplicationResponse ToResponse(JobApplication a) =>
        new(a.Id, a.Company, a.Role, a.Status, a.AppliedDate, a.Notes);
}
