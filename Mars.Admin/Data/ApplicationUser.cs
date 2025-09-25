using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Mars.Admin.Data;

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{
    [Required(ErrorMessage = "Email address is required")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    public override string? Email { get; set; }

    public int? UserRoleId { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
    
    // User alert system fields
    public bool IsNewUser { get; set; } = false;
    public bool NeedsAttention { get; set; } = false;
    public DateTime? LastLoginAt { get; set; }
    public DateTime? AttentionCreatedAt { get; set; }
    public DateTime? AttentionIgnoredAt { get; set; }
    public string? AttentionIgnoredBy { get; set; }
    
    // Navigation properties
    public UserRole? UserRole { get; set; }
    public ICollection<UserWebsiteAccess> UserWebsiteAccesses { get; set; } = new List<UserWebsiteAccess>();
    public ICollection<IPSafeListing> IPSafeListings { get; set; } = new List<IPSafeListing>();
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}

