using System.ComponentModel.DataAnnotations;

namespace Mars.Admin.Data;

public class UserRole
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    public bool IsSuperAdmin { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public string? CreatedByUserId { get; set; }
    
    public DateTime? ModifiedAt { get; set; }
    
    public string? ModifiedByUserId { get; set; }
    
    // Navigation properties
    public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
    public ICollection<UserRolePermission> UserRolePermissions { get; set; } = new List<UserRolePermission>();
}

