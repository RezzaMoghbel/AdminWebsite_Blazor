using System.ComponentModel.DataAnnotations;

namespace Mars.Admin.Data;

public class IPSafeListing
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(45)] // IPv6 max length
    public string IPAddress { get; set; } = string.Empty;
    
    public string? UserId { get; set; } // Nullable - if null = Office IP, if assigned = Individual IP
    
    [MaxLength(200)]
    public string? Label { get; set; }
    
    public DateTime? ExpiryDate { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public string? CreatedByUserId { get; set; }
    
    public DateTime? ModifiedAt { get; set; }
    
    public string? ModifiedByUserId { get; set; }
    
    // Navigation properties
    public ApplicationUser? User { get; set; }
}
