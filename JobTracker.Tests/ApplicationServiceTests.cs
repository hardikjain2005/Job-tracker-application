using JobTracker.Api.Dtos;
using JobTracker.Api.Models;
using JobTracker.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace JobTracker.Tests;

public class ApplicationServiceTests : IDisposable
{
    private readonly TestDb _testDb = new();

    public void Dispose() => _testDb.Dispose();

    private static ApplicationRequest Request(
        string company = "Acme", ApplicationStatus status = ApplicationStatus.Applied) =>
        new(company, "Developer", status, new DateOnly(2026, 9, 20), null);

    [Fact]
    public async Task Create_SavesApplicationForUser()
    {
        var userId = _testDb.AddUser("a@test.com");
        var service = new ApplicationService(_testDb.NewContext());

        var created = await service.CreateAsync(userId, Request("Acme"));

        Assert.True(created.Id > 0);
        // Read through a different context to prove it was persisted, not just tracked.
        var saved = await _testDb.NewContext().JobApplications.SingleAsync();
        Assert.Equal("Acme", saved.Company);
        Assert.Equal(userId, saved.UserId);
        Assert.Equal(ApplicationStatus.Applied, saved.Status);
    }

    [Fact]
    public async Task GetAll_FilterByStatus_ReturnsOnlyMatchingRows()
    {
        var userId = _testDb.AddUser("a@test.com");
        var service = new ApplicationService(_testDb.NewContext());
        await service.CreateAsync(userId, Request("Acme", ApplicationStatus.Interview));
        await service.CreateAsync(userId, Request("Beta", ApplicationStatus.Applied));
        await service.CreateAsync(userId, Request("Gamma", ApplicationStatus.Interview));

        var interviews = await service.GetAllAsync(userId, ApplicationStatus.Interview);
        var all = await service.GetAllAsync(userId, null);

        Assert.Equal(2, interviews.Count);
        Assert.All(interviews, a => Assert.Equal(ApplicationStatus.Interview, a.Status));
        Assert.Equal(3, all.Count);
    }

    [Fact]
    public async Task User_CannotSeeOrChangeAnotherUsersApplications()
    {
        var alice = _testDb.AddUser("alice@test.com");
        var bob = _testDb.AddUser("bob@test.com");
        var service = new ApplicationService(_testDb.NewContext());
        var aliceApp = await service.CreateAsync(alice, Request("AliceCo"));

        Assert.Empty(await service.GetAllAsync(bob, null));
        Assert.Null(await service.GetByIdAsync(bob, aliceApp.Id));
        Assert.Null(await service.UpdateAsync(bob, aliceApp.Id, Request("Hacked")));
        Assert.False(await service.DeleteAsync(bob, aliceApp.Id));

        // Alice's record is untouched.
        var stillThere = await service.GetByIdAsync(alice, aliceApp.Id);
        Assert.Equal("AliceCo", stillThere!.Company);
    }

    [Fact]
    public async Task Delete_RemovesTheRecord()
    {
        var userId = _testDb.AddUser("a@test.com");
        var service = new ApplicationService(_testDb.NewContext());
        var created = await service.CreateAsync(userId, Request());

        Assert.True(await service.DeleteAsync(userId, created.Id));

        Assert.Empty(await _testDb.NewContext().JobApplications.ToListAsync());
        Assert.False(await service.DeleteAsync(userId, created.Id)); // already gone
    }

    [Fact]
    public async Task Update_ChangesFieldsOfOwnedApplication()
    {
        var userId = _testDb.AddUser("a@test.com");
        var service = new ApplicationService(_testDb.NewContext());
        var created = await service.CreateAsync(userId, Request("Acme", ApplicationStatus.Applied));

        var updated = await service.UpdateAsync(userId, created.Id, Request("Acme Inc", ApplicationStatus.Offer));

        Assert.Equal("Acme Inc", updated!.Company);
        var saved = await _testDb.NewContext().JobApplications.SingleAsync();
        Assert.Equal(ApplicationStatus.Offer, saved.Status);
    }
}
