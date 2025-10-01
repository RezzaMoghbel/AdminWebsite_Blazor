using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Mars.Admin.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    // DbSets for role-based access control
    public new DbSet<UserRole> UserRoles { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<UserRolePermission> UserRolePermissions { get; set; }
    public DbSet<Website> Websites { get; set; }
    public DbSet<UserWebsiteAccess> UserWebsiteAccesses { get; set; }
    public DbSet<IPSafeListing> IPSafeListings { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<AuditLogsIPSafelisting> AuditLogsIPSafelistings { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure UserRole
        builder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        // Configure Permission
        builder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        // Configure UserRolePermission (composite key)
        builder.Entity<UserRolePermission>(entity =>
        {
            entity.HasKey(e => new { e.UserRoleId, e.PermissionId });
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            
            entity.HasOne(e => e.UserRole)
                .WithMany(e => e.UserRolePermissions)
                .HasForeignKey(e => e.UserRoleId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.Permission)
                .WithMany(e => e.UserRolePermissions)
                .HasForeignKey(e => e.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Website
        builder.Entity<Website>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Url).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        // Configure UserWebsiteAccess (composite key)
        builder.Entity<UserWebsiteAccess>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.WebsiteId });
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            
            entity.HasOne(e => e.User)
                .WithMany(e => e.UserWebsiteAccesses)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.Website)
                .WithMany(e => e.UserWebsiteAccesses)
                .HasForeignKey(e => e.WebsiteId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure IPSafeListing
        builder.Entity<IPSafeListing>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.IPAddress).IsRequired().HasMaxLength(45);
            entity.Property(e => e.Label).HasMaxLength(200);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            
            entity.HasOne(e => e.User)
                .WithMany(e => e.IPSafeListings)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure AuditLog
        builder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EntityName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.EntityKey).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Action).IsRequired().HasMaxLength(50);
            entity.Property(e => e.PerformedAt).HasDefaultValueSql("GETUTCDATE()");
            
            entity.HasOne(e => e.PerformedByUser)
                .WithMany(e => e.AuditLogs)
                .HasForeignKey(e => e.PerformedByUserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure AuditLogsIPSafelisting
        builder.Entity<AuditLogsIPSafelisting>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.IPAddress);
            entity.Property(e => e.IPAddress).IsRequired().HasMaxLength(45);
            entity.Property(e => e.UserAgent).HasMaxLength(200);
            entity.Property(e => e.RequestPath).HasMaxLength(500);
            entity.Property(e => e.Referer).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.FirstAttemptAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.LastAttemptAt).HasDefaultValueSql("GETUTCDATE()");
        });

        // Configure ApplicationUser UserRole relationship
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.HasOne(e => e.UserRole)
                .WithMany(e => e.Users)
                .HasForeignKey(e => e.UserRoleId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}