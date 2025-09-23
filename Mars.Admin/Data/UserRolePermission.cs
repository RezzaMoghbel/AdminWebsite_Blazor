using System.ComponentModel.DataAnnotations;

namespace Mars.Admin.Data;

public class UserRolePermission
{
    public int UserRoleId { get; set; }
    public int PermissionId { get; set; }
    
    public bool IsGranted { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public string? CreatedByUserId { get; set; }
    
    public DateTime? ModifiedAt { get; set; }
    
    public string? ModifiedByUserId { get; set; }
    
    // Navigation properties
    public UserRole UserRole { get; set; } = null!;
    public Permission Permission { get; set; } = null!;
}

