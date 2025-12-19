using Microsoft.EntityFrameworkCore;
using RenessansAPI.Domain.Entities.Auth;
using RenessansAPI.Domain.Entities.News.AboutCamps;
using RenessansAPI.Domain.Entities.News.CampPossiblities;
using RenessansAPI.Domain.Entities.News.CoursesEvents;
using RenessansAPI.Domain.Entities.News.OverallImages;
using RenessansAPI.Domain.Entities.News.Tidings;
using RenessansAPI.Domain.Entities.Users;

namespace RenessansAPI.DataAccess.AppDBContexts;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    //Auth
    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<Role> Roles { get; set; }
    public virtual DbSet<Permission> Permissions { get; set; }
    public virtual DbSet<Token> Tokens { get; set; }

    //News
    public virtual DbSet<AbtCamp> AbtCamps { get; set; }
    public virtual DbSet<CourseEvent> CourseEvents { get; set; }
    public virtual DbSet<CourseEventApplication> CourseEventApplication { get; set; }
    public virtual DbSet<Tiding> Tidings { get; set; }
    public virtual DbSet<Possibilities> Possibilities { get; set; }
    public virtual DbSet<Images> Images { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // 🔗 One-to-many relationship between CourseEvent and Applications
        builder.Entity<CourseEventApplication>()
            .HasOne(a => a.CourseEvent)
            .WithMany(e => e.Applications)
            .HasForeignKey(a => a.CourseEventId)
            .OnDelete(DeleteBehavior.Cascade);

        // ⚡ Indexes for performance
        builder.Entity<CourseEventApplication>()
            .HasIndex(a => a.CourseEventId);

        builder.Entity<CourseEventApplication>()
            .HasIndex(a => a.PhoneNumber);

        // (Optional) Unique constraints, default values, etc.
        // builder.Entity<User>().HasIndex(u => u.Email).IsUnique();
    }
}
