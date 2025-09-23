using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Mars.Admin.Data;

namespace Mars.Admin.Services;

public class CustomSignInManager : SignInManager<ApplicationUser>
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CustomSignInManager(
        UserManager<ApplicationUser> userManager,
        IHttpContextAccessor contextAccessor,
        IUserClaimsPrincipalFactory<ApplicationUser> claimsFactory,
        IOptions<IdentityOptions> optionsAccessor,
        ILogger<SignInManager<ApplicationUser>> logger,
        IAuthenticationSchemeProvider schemes,
        IUserConfirmation<ApplicationUser> confirmation,
        ApplicationDbContext context)
        : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
    {
        _context = context;
        _httpContextAccessor = contextAccessor;
    }

    public override async Task<SignInResult> PasswordSignInAsync(string userName, string password, bool isPersistent, bool lockoutOnFailure)
    {
        // First check if user exists and is active/not deleted
        // Try to find by email first (since login form uses email), then by username
        var user = await UserManager.FindByEmailAsync(userName) ?? await UserManager.FindByNameAsync(userName);
        if (user == null)
        {
            return SignInResult.Failed;
        }

        // Check if user is active and not deleted
        if (!user.IsActive || user.IsDeleted)
        {
            Logger.LogWarning("Login attempt blocked for inactive/deleted user: {UserName}", userName);
            return SignInResult.NotAllowed;
        }

        // Check IP validation before proceeding with sign-in
        var ipValidationResult = await ValidateIPForUserAsync(user);
        if (!ipValidationResult.IsValid)
        {
            Logger.LogWarning("Login attempt blocked for user {UserName} from IP {ClientIP}: {Reason}", 
                userName, ipValidationResult.ClientIP, ipValidationResult.Reason);
            return SignInResult.NotAllowed;
        }

        // Proceed with normal sign-in
        return await base.PasswordSignInAsync(userName, password, isPersistent, lockoutOnFailure);
    }

    public override async Task<SignInResult> PasswordSignInAsync(ApplicationUser user, string password, bool isPersistent, bool lockoutOnFailure)
    {
        // Check if user is active and not deleted
        if (!user.IsActive || user.IsDeleted)
        {
            Logger.LogWarning("Login attempt blocked for inactive/deleted user: {UserName}", user.UserName);
            return SignInResult.NotAllowed;
        }

        // Check IP validation before proceeding with sign-in
        var ipValidationResult = await ValidateIPForUserAsync(user);
        if (!ipValidationResult.IsValid)
        {
            Logger.LogWarning("Login attempt blocked for user {UserName} from IP {ClientIP}: {Reason}", 
                user.UserName, ipValidationResult.ClientIP, ipValidationResult.Reason);
            return SignInResult.NotAllowed;
        }

        // Proceed with normal sign-in
        return await base.PasswordSignInAsync(user, password, isPersistent, lockoutOnFailure);
    }

    private async Task<(bool IsValid, string ClientIP, string Reason)> ValidateIPForUserAsync(ApplicationUser user)
    {
        try
        {
            var clientIP = GetClientIPAddress();
            Logger.LogInformation("Validating IP for user {UserName} (ID: {UserId}) from IP {ClientIP}", user.UserName, user.Id, clientIP);
            
            if (string.IsNullOrEmpty(clientIP))
            {
                return (false, "unknown", "Could not determine client IP address");
            }

            var now = DateTime.UtcNow;
            
            // Get all active IPs from safe listing
            var allSafeIPs = await _context.IPSafeListings
                .Where(i => i.IsActive && (i.ExpiryDate == null || i.ExpiryDate > now))
                .ToListAsync();

            Logger.LogInformation("Found {Count} active IPs in safe listing", allSafeIPs.Count);
            foreach (var ip in allSafeIPs)
            {
                Logger.LogInformation("IP: {IPAddress}, UserId: {UserId}, Label: {Label}", ip.IPAddress, ip.UserId ?? "null", ip.Label ?? "null");
            }

            // A. Check if IP is an Office IP (UserId = null) - if yes, allow any user to login
            var officeIPs = allSafeIPs.Where(i => i.UserId == null).ToList();
            Logger.LogInformation("Found {Count} office IPs", officeIPs.Count);
            var isOfficeIP = officeIPs.Any(ip => IsIPInRange(clientIP, ip.IPAddress));
            if (isOfficeIP)
            {
                Logger.LogInformation("IP {ClientIP} is office IP - allowing login for user {UserName}", clientIP, user.UserName);
                return (true, clientIP, "Office IP access granted");
            }

            // B. If not office IP, then it must be an individual IP - check if it's assigned to this user
            var individualIPs = allSafeIPs.Where(i => i.UserId != null).ToList();
            Logger.LogInformation("Found {Count} individual IPs", individualIPs.Count);
            
            // Find ALL IPs that match the client IP (not just the first one)
            var matchingIPs = individualIPs.Where(ip => IsIPInRange(clientIP, ip.IPAddress)).ToList();
            Logger.LogInformation("Found {Count} individual IPs matching client IP {ClientIP}", matchingIPs.Count, clientIP);
            
            if (matchingIPs.Any())
            {
                // Check if ANY of the matching IPs belong to this user
                var userMatchingIP = matchingIPs.FirstOrDefault(ip => ip.UserId == user.Id);
                
                if (userMatchingIP != null)
                {
                    Logger.LogInformation("IP {ClientIP} is individual IP for user {UserName} - allowing login", clientIP, user.UserName);
                    return (true, clientIP, "Individual IP access granted");
                }
                else
                {
                    // Log all users who have this IP for debugging
                    var assignedUserIds = string.Join(", ", matchingIPs.Select(ip => ip.UserId));
                    Logger.LogWarning("IP {ClientIP} is individual IP for users {AssignedUserIds}, but user {LoginUserId} is trying to login - denying login", 
                        clientIP, assignedUserIds, user.Id);
                    return (false, clientIP, $"IP is assigned to other users (IDs: {assignedUserIds})");
                }
            }

            // If we reach here, IP is not in safe listing at all (shouldn't happen due to middleware)
            Logger.LogWarning("IP {ClientIP} is not in safe listing - denying login for user {UserName}", clientIP, user.UserName);
            return (false, clientIP, "IP is not in safe listing");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error validating IP for user {UserName}", user.UserName);
            return (false, "error", "IP validation error occurred");
        }
    }

    private string GetClientIPAddress()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null) return string.Empty;

        // Check for forwarded IP first (for reverse proxy scenarios)
        var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            var ip = forwardedFor.Split(',')[0].Trim();
            if (!string.IsNullOrEmpty(ip))
                return ip;
        }

        // Check for real IP header
        var realIP = httpContext.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIP))
            return realIP;

        // Fall back to connection remote IP
        return httpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
    }

    private bool IsIPInRange(string clientIP, string ipRange)
    {
        try
        {
            // If it's an exact match
            if (clientIP == ipRange)
                return true;

            // If it's CIDR notation
            if (ipRange.Contains('/'))
            {
                var parts = ipRange.Split('/');
                if (parts.Length == 2 && System.Net.IPAddress.TryParse(parts[0], out var networkIP) && int.TryParse(parts[1], out var prefixLength))
                {
                    var clientIPAddr = System.Net.IPAddress.Parse(clientIP);
                    return IsIPInCIDRRange(clientIPAddr, networkIP, prefixLength);
                }
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    private bool IsIPInCIDRRange(System.Net.IPAddress clientIP, System.Net.IPAddress networkIP, int prefixLength)
    {
        try
        {
            var clientBytes = clientIP.GetAddressBytes();
            var networkBytes = networkIP.GetAddressBytes();

            if (clientBytes.Length != networkBytes.Length)
                return false;

            var bytesToCheck = prefixLength / 8;
            var bitsToCheck = prefixLength % 8;

            for (int i = 0; i < bytesToCheck; i++)
            {
                if (clientBytes[i] != networkBytes[i])
                    return false;
            }

            if (bitsToCheck > 0 && bytesToCheck < clientBytes.Length)
            {
                var mask = (byte)(0xFF << (8 - bitsToCheck));
                if ((clientBytes[bytesToCheck] & mask) != (networkBytes[bytesToCheck] & mask))
                    return false;
            }

            return true;
        }
        catch
        {
            return false;
        }
    }
}
