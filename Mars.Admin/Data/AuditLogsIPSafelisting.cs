using System.ComponentModel.DataAnnotations;

namespace Mars.Admin.Data;

public class AuditLogsIPSafelisting
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(45)] // IPv6 max length
    public string IPAddress { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string? UserAgent { get; set; }
    
    [MaxLength(500)]
    public string? RequestPath { get; set; }
    
    [MaxLength(100)]
    public string? Referer { get; set; }
    
    public int AccessAttempts { get; set; } = 1;
    
    public DateTime FirstAttemptAt { get; set; } = DateTime.UtcNow;
    
    public DateTime LastAttemptAt { get; set; } = DateTime.UtcNow;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    [MaxLength(100)]
    public string? CreatedByUserId { get; set; }
    
    [MaxLength(100)]
    public string? UpdatedByUserId { get; set; }
    
    public bool IsActive { get; set; } = true;
}
