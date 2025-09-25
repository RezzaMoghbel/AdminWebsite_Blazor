using Microsoft.EntityFrameworkCore;
using Mars.Admin.Data;

namespace Mars.Admin.Services;

public class UserAlertService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UserAlertService> _logger;

    public UserAlertService(ApplicationDbContext context, ILogger<UserAlertService> logger)
    {
        _context = context;
        _logger = logger;
    }

    // Constants for time thresholds
    public static class Thresholds
    {
        public const int NewUserGracePeriod = 7;        // days
        public const int InactiveThreshold = 30;        // days
        public const int AttentionTimeout = 90;         // days
    }

    // Predefined attention reasons
    public static class AttentionReasons
    {
        // New user reasons
        public const string NewUserMissingBoth = "New user - no role and website assigned";
        public const string NewUserMissingRole = "New user - no role assigned";
        public const string NewUserMissingWebsite = "New user - no website assigned";
        
        // Existing user reasons
        public const string MissingRole = "No role assigned";
        public const string MissingWebsite = "No website assigned";
        public const string MissingBoth = "No role and website assigned";
        
        // Other reasons
        public const string Inactive = "User inactive for 30+ days";
        public const string AdminIgnored = "Admin acknowledged";
    }

    /// <summary>
    /// Sets a user as new (called during registration)
    /// </summary>
    public async Task SetUserAsNewAsync(string userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user != null)
        {
            user.IsNewUser = true;
            user.NeedsAttention = true;
            user.AttentionCreatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            _logger.LogInformation("Set user {UserId} as new user", userId);
        }
    }

    /// <summary>
    /// Updates user's last login time and checks for auto-reset conditions
    /// </summary>
    public async Task UpdateLastLoginAsync(string userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user != null)
        {
            user.LastLoginAt = DateTime.UtcNow;
            
            // Auto-reset flags if user has both role and website access
            await CheckAndResetFlagsAsync(user);
            
            await _context.SaveChangesAsync();
            _logger.LogInformation("Updated last login for user {UserId}", userId);
        }
    }

    /// <summary>
    /// Checks if user needs attention and sets flags accordingly
    /// </summary>
    public async Task CheckUserNeedsAttentionAsync(string userId)
    {
        var user = await _context.Users
            .Include(u => u.UserRole)
            .Include(u => u.UserWebsiteAccesses)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) return;

        var hasRole = user.UserRoleId.HasValue && user.UserRole != null;
        var hasWebsiteAccess = user.UserWebsiteAccesses.Any(uwa => uwa.IsActive && uwa.IsGranted);
        var isInactive = user.LastLoginAt.HasValue && 
                        user.LastLoginAt.Value < DateTime.UtcNow.AddDays(-Thresholds.InactiveThreshold);

        // Reset flags if user has both role and website access
        if (hasRole && hasWebsiteAccess)
        {
            await CheckAndResetFlagsAsync(user);
        }
        else
        {
            // Set attention flag if missing assignments (only if not already set)
            if (!user.NeedsAttention)
            {
                user.NeedsAttention = true;
                user.AttentionCreatedAt = DateTime.UtcNow;
                _logger.LogInformation("Set attention flag for user {UserId} - Missing role: {MissingRole}, Missing website: {MissingWebsite}", 
                    userId, !hasRole, !hasWebsiteAccess);
            }
        }

        // Set inactive flag if user hasn't logged in for 30+ days
        if (isInactive && !user.NeedsAttention)
        {
            user.NeedsAttention = true;
            user.AttentionCreatedAt = DateTime.UtcNow;
            _logger.LogInformation("Set attention flag for user {UserId} - Inactive for 30+ days", userId);
        }

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Checks and resets flags if user has both role and website access
    /// </summary>
    private async Task CheckAndResetFlagsAsync(ApplicationUser user)
    {
        // Ensure we have the latest data from database
        var freshUser = await _context.Users
            .Include(u => u.UserRole)
            .Include(u => u.UserWebsiteAccesses)
            .FirstOrDefaultAsync(u => u.Id == user.Id);

        if (freshUser == null) return;

        var hasRole = freshUser.UserRoleId.HasValue && freshUser.UserRole != null;
        var hasWebsiteAccess = freshUser.UserWebsiteAccesses.Any(uwa => uwa.IsActive && uwa.IsGranted);

        Console.WriteLine($"CheckAndResetFlagsAsync for {freshUser.Email}: hasRole={hasRole}, hasWebsiteAccess={hasWebsiteAccess}, IsNewUser={freshUser.IsNewUser}, NeedsAttention={freshUser.NeedsAttention}");

        if (hasRole && hasWebsiteAccess)
        {
            if (freshUser.IsNewUser || freshUser.NeedsAttention)
            {
                freshUser.IsNewUser = false;
                freshUser.NeedsAttention = false;
                freshUser.AttentionCreatedAt = null;
                freshUser.AttentionIgnoredAt = null;
                freshUser.AttentionIgnoredBy = null;
                
                await _context.SaveChangesAsync();
                _logger.LogInformation("Auto-reset flags for user {UserId} - Has role and website access", freshUser.Id);
                Console.WriteLine($"Auto-reset flags for user {freshUser.Email}");
            }
        }
    }

    /// <summary>
    /// Forces a check and reset of all user flags - useful for cleanup
    /// </summary>
    public async Task ForceCheckAllUsersAsync()
    {
        var allUsers = await _context.Users
            .Include(u => u.UserRole)
            .Include(u => u.UserWebsiteAccesses)
            .Where(u => u.IsActive && !u.IsDeleted)
            .ToListAsync();

        foreach (var user in allUsers)
        {
            await CheckAndResetFlagsAsync(user);
        }

        _logger.LogInformation("Force check completed for {UserCount} users", allUsers.Count);
    }

    /// <summary>
    /// Gets users that need attention for the dashboard
    /// </summary>
    public async Task<UserAlertSummary> GetUserAlertSummaryAsync()
    {
        var now = DateTime.UtcNow;
        var thirtyDaysAgo = now.AddDays(-Thresholds.InactiveThreshold);

        var attentionNeededUsers = await _context.Users
            .Where(u => u.IsActive && !u.IsDeleted && u.NeedsAttention && u.AttentionIgnoredAt == null)
            .Include(u => u.UserRole)
            .Include(u => u.UserWebsiteAccesses)
            .ToListAsync();

        var attentionNeeded = attentionNeededUsers.Select(u => new UserAlertInfo
        {
            Id = u.Id,
            Email = u.Email!,
            CreatedAt = u.AttentionCreatedAt ?? DateTime.MinValue,
            Reason = DetermineAttentionReason(u.IsNewUser, u.UserRoleId.HasValue && u.UserRole != null, u.UserWebsiteAccesses.Any(uwa => uwa.IsActive && uwa.IsGranted))
        }).ToList();

        var inactiveUsers = await _context.Users
            .Where(u => u.IsActive && !u.IsDeleted && 
                       u.LastLoginAt.HasValue && u.LastLoginAt < thirtyDaysAgo &&
                       u.AttentionIgnoredAt == null)
            .Select(u => new UserAlertInfo
            {
                Id = u.Id,
                Email = u.Email!,
                CreatedAt = u.LastLoginAt!.Value,
                Reason = AttentionReasons.Inactive
            })
            .ToListAsync();

        var ignoredAlerts = await _context.Users
            .Where(u => u.IsActive && !u.IsDeleted && u.AttentionIgnoredAt != null)
            .Select(u => new UserAlertInfo
            {
                Id = u.Id,
                Email = u.Email!,
                CreatedAt = u.AttentionIgnoredAt!.Value,
                Reason = AttentionReasons.AdminIgnored
            })
            .ToListAsync();

        return new UserAlertSummary
        {
            AttentionNeeded = attentionNeeded,
            InactiveUsers = inactiveUsers,
            IgnoredAlerts = ignoredAlerts
        };
    }

    /// <summary>
    /// Ignores an alert for a specific user
    /// </summary>
    public async Task IgnoreAlertAsync(string userId, string ignoredBy)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user != null)
        {
            user.AttentionIgnoredAt = DateTime.UtcNow;
            user.AttentionIgnoredBy = ignoredBy;
            await _context.SaveChangesAsync();
            _logger.LogInformation("Alert ignored for user {UserId} by {IgnoredBy}", userId, ignoredBy);
        }
    }

    /// <summary>
    /// Un-ignores an alert for a specific user
    /// </summary>
    public async Task UnignoreAlertAsync(string userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user != null)
        {
            user.AttentionIgnoredAt = null;
            user.AttentionIgnoredBy = null;
            await _context.SaveChangesAsync();
            _logger.LogInformation("Alert un-ignored for user {UserId}", userId);
        }
    }

    /// <summary>
    /// Determines the specific attention reason based on user status
    /// </summary>
    private static string DetermineAttentionReason(bool isNewUser, bool hasRole, bool hasWebsiteAccess)
    {
        // Debug logging to understand what's happening
        Console.WriteLine($"DetermineAttentionReason: isNewUser={isNewUser}, hasRole={hasRole}, hasWebsiteAccess={hasWebsiteAccess}");

        if (isNewUser)
        {
            if (!hasRole && !hasWebsiteAccess)
            {
                Console.WriteLine("Returning: NewUserMissingBoth");
                return AttentionReasons.NewUserMissingBoth;
            }
            else if (!hasRole && hasWebsiteAccess)
            {
                Console.WriteLine("Returning: NewUserMissingRole");
                return AttentionReasons.NewUserMissingRole;
            }
            else if (hasRole && !hasWebsiteAccess)
            {
                Console.WriteLine("Returning: NewUserMissingWebsite");
                return AttentionReasons.NewUserMissingWebsite;
            }
            else
            {
                Console.WriteLine("New user has both role and website - should not be in alerts");
                return "New user complete";
            }
        }
        else
        {
            if (!hasRole && !hasWebsiteAccess)
            {
                Console.WriteLine("Returning: MissingBoth");
                return AttentionReasons.MissingBoth;
            }
            else if (!hasRole && hasWebsiteAccess)
            {
                Console.WriteLine("Returning: MissingRole");
                return AttentionReasons.MissingRole;
            }
            else if (hasRole && !hasWebsiteAccess)
            {
                Console.WriteLine("Returning: MissingWebsite");
                return AttentionReasons.MissingWebsite;
            }
            else
            {
                Console.WriteLine("Existing user has both role and website - should not be in alerts");
                return "User complete";
            }
        }
    }
}

// Data transfer objects for the alert system
public class UserAlertSummary
{
    public List<UserAlertInfo> AttentionNeeded { get; set; } = new();
    public List<UserAlertInfo> InactiveUsers { get; set; } = new();
    public List<UserAlertInfo> IgnoredAlerts { get; set; } = new();
}

public class UserAlertInfo
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string Reason { get; set; } = string.Empty;
    public int DaysSince => (int)(DateTime.UtcNow - CreatedAt).TotalDays;
}
