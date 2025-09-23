using System.ComponentModel.DataAnnotations;

namespace Mars.Admin.Data;

public class AuditLog
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string EntityName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string EntityKey { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string Action { get; set; } = string.Empty; // Create, Update, Delete
    
    public string? BeforeJson { get; set; }
    
    public string? AfterJson { get; set; }
    
    public DateTime PerformedAt { get; set; } = DateTime.UtcNow;
    
    public string? PerformedByUserId { get; set; }
    
    // Navigation properties
    public ApplicationUser? PerformedByUser { get; set; }
}

