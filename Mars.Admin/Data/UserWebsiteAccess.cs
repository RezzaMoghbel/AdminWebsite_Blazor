using System.ComponentModel.DataAnnotations;

namespace Mars.Admin.Data;

public class UserWebsiteAccess
{
    public string UserId { get; set; } = string.Empty;
    public int WebsiteId { get; set; }
    
    public bool IsGranted { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public string? CreatedByUserId { get; set; }
    
    public DateTime? ModifiedAt { get; set; }
    
    public string? ModifiedByUserId { get; set; }
    
    // Navigation properties
    public ApplicationUser User { get; set; } = null!;
    public Website Website { get; set; } = null!;
}

