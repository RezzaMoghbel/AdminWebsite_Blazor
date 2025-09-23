using System.ComponentModel.DataAnnotations;

namespace Mars.Admin.Data;

public class Website
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Url { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public string? CreatedByUserId { get; set; }
    
    public DateTime? ModifiedAt { get; set; }
    
    public string? ModifiedByUserId { get; set; }
    
    // Navigation properties
    public ICollection<UserWebsiteAccess> UserWebsiteAccesses { get; set; } = new List<UserWebsiteAccess>();
}

