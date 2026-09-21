using JobTracker.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace JobTracker.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<JobApplication> JobApplications => Set<JobApplication>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.Email).HasMaxLength(256).IsRequired();
        });

        modelBuilder.Entity<JobApplication>(e =>
        {
            // Store the enum as text ("Interview") so the DB stays readable.
            e.Property(a => a.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(a => a.Company).HasMaxLength(200).IsRequired();
            e.Property(a => a.Role).HasMaxLength(200).IsRequired();
            e.Property(a => a.Notes).HasMaxLength(2000);
            e.HasOne(a => a.User).WithMany(u => u.Applications)
                .HasForeignKey(a => a.UserId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
