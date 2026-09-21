using JobTracker.Api.Data;
using JobTracker.Api.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace JobTracker.Tests;

/// <summary>
/// A throwaway SQLite in-memory database. The database lives as long as the open connection,
/// so each test gets its own isolated, real-SQL database that vanishes on Dispose.
/// </summary>
public sealed class TestDb : IDisposable
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    public TestDb()
    {
        _connection.Open();
        using var db = NewContext();
        db.Database.EnsureCreated();
    }

    /// <summary>A fresh DbContext on the same database, so tests also prove data was really persisted.</summary>
    public AppDbContext NewContext() =>
        new(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options);

    public int AddUser(string email)
    {
        using var db = NewContext();
        var user = new User { Email = email, PasswordHash = "x" };
        db.Users.Add(user);
        db.SaveChanges();
        return user.Id;
    }

    public void Dispose() => _connection.Dispose();
}
