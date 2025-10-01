using Microsoft.EntityFrameworkCore;
using Mars.Admin.Data;
using System.Net;

namespace Mars.Admin.Middleware;

public class IpSafeListingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<IpSafeListingMiddleware> _logger;

    public IpSafeListingMiddleware(RequestDelegate next, ILogger<IpSafeListingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ApplicationDbContext dbContext)
    {
        try
        {
            var clientIP = GetClientIPAddress(context);
            if (string.IsNullOrEmpty(clientIP))
            {
                _logger.LogWarning("Could not determine client IP address for request {RequestPath} from {UserAgent}", 
                    context.Request.Path, context.Request.Headers["User-Agent"].FirstOrDefault());
                await RedirectToSafelyInsuredAsync(context, dbContext);
                return;
            }

            _logger.LogDebug("Checking IP safe listing for: {ClientIP} accessing {RequestPath}", clientIP, context.Request.Path);

            // A. First check if the client IP is in our safe listing table at all
            // This includes both office IPs (UserId = null) and individual IPs (UserId = assigned)
            var isIPInSafeListing = await IsIPInSafeListingAsync(dbContext, clientIP);
            if (!isIPInSafeListing)
            {
                _logger.LogWarning("IP {ClientIP} is not in safe listing table - denying access to {RequestPath} from {UserAgent}", 
                    clientIP, context.Request.Path, context.Request.Headers["User-Agent"].FirstOrDefault());
                await RedirectToSafelyInsuredAsync(context, dbContext);
                return;
            }

            // If user is authenticated, check if user is still active and not deleted
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var userId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    // Check if user is still active and not deleted
                    var user = await dbContext.Users.FindAsync(userId);
                    if (user == null || !user.IsActive || user.IsDeleted)
                    {
                        _logger.LogWarning("Authenticated user {UserId} is inactive or deleted, redirecting to Safely Insured from IP {ClientIP}", 
                            userId, clientIP);
                        await RedirectToSafelyInsuredAsync(context, dbContext);
                        return;
                    }
                }
            }

            // If we reach here, user is not authenticated but IP is in safe listing
            // Allow access to login page - IP validation will happen during authentication
            _logger.LogInformation("IP {ClientIP} is in safe listing - allowing access to {RequestPath}", clientIP, context.Request.Path);
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in IP safe listing middleware");
            await RedirectToSafelyInsuredAsync(context, dbContext);
        }
    }

    private string GetClientIPAddress(HttpContext context)
    {
        // Check for forwarded headers first (for load balancers/proxies)
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            var ips = forwardedFor.Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (ips.Length > 0)
            {
                var ip = ips[0].Trim();
                if (IPAddress.TryParse(ip, out _))
                {
                    return ip;
                }
            }
        }

        var realIP = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIP) && IPAddress.TryParse(realIP, out _))
        {
            return realIP;
        }

        // Fall back to connection remote IP
        var remoteIP = context.Connection.RemoteIpAddress;
        if (remoteIP != null)
        {
            if (remoteIP.IsIPv4MappedToIPv6)
            {
                remoteIP = remoteIP.MapToIPv4();
            }
            return remoteIP.ToString();
        }

        return string.Empty;
    }

    /// <summary>
    /// A. Check if the client IP is in our safe listing table at all (both office and individual IPs)
    /// This is the first check - if IP is not in safe listing, deny access completely
    /// </summary>
    private async Task<bool> IsIPInSafeListingAsync(ApplicationDbContext dbContext, string clientIP)
    {
        var now = DateTime.UtcNow;
        var allSafeIPs = await dbContext.IPSafeListings
            .Where(i => i.IsActive 
                        && (i.ExpiryDate == null || i.ExpiryDate > now))
            .ToListAsync();

        return allSafeIPs.Any(ip => IsIPInRange(clientIP, ip.IPAddress));
    }


    /// <summary>
    /// Check if a client IP matches an IP range from our safe listing
    /// Supports both exact IP matches and CIDR notation (e.g., 192.168.1.0/24)
    /// </summary>
    private bool IsIPInRange(string clientIP, string ipRange)
    {
        try
        {
            // If it's an exact match (e.g., 192.168.1.100 == 192.168.1.100)
            if (clientIP == ipRange)
                return true;

            // If it's CIDR notation (e.g., 192.168.1.0/24 covers 192.168.1.1-254)
            if (ipRange.Contains('/'))
            {
                var parts = ipRange.Split('/');
                if (parts.Length == 2 && IPAddress.TryParse(parts[0], out var networkIP) && int.TryParse(parts[1], out var prefixLength))
                {
                    var clientIPAddr = IPAddress.Parse(clientIP);
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

    /// <summary>
    /// Check if a client IP falls within a CIDR network range
    /// Example: 192.168.1.50 is in range 192.168.1.0/24
    /// </summary>
    private bool IsIPInCIDRRange(IPAddress clientIP, IPAddress networkIP, int prefixLength)
    {
        try
        {
            // Convert IPs to byte arrays for bitwise comparison
            var clientBytes = clientIP.GetAddressBytes();
            var networkBytes = networkIP.GetAddressBytes();

            // Ensure both IPs are the same version (IPv4 or IPv6)
            if (clientBytes.Length != networkBytes.Length)
                return false;

            // Calculate the number of bytes to check based on prefix length
            var bytesToCheck = prefixLength / 8;
            var bitsToCheck = prefixLength % 8;

            // Check full bytes (e.g., for /24, check first 3 bytes)
            for (int i = 0; i < bytesToCheck; i++)
            {
                if (clientBytes[i] != networkBytes[i])
                    return false;
            }

            // Check remaining bits in the partial byte (e.g., for /25, check first bit of 4th byte)
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

    private async Task RedirectToSafelyInsuredAsync(HttpContext context, ApplicationDbContext dbContext)
    {
        // Log the unauthorized access attempt before redirecting
        await LogUnauthorizedAccessAsync(context, dbContext);
        
        context.Response.Redirect("https://www.InsureDaily.co.uk");
    }

    private async Task LogUnauthorizedAccessAsync(HttpContext context, ApplicationDbContext dbContext)
    {
        try
        {
            var clientIP = GetClientIPAddress(context);
            _logger.LogInformation("LogUnauthorizedAccessAsync called for IP: {ClientIP}", clientIP ?? "NULL");
            
            if (string.IsNullOrEmpty(clientIP))
            {
                _logger.LogWarning("Could not determine client IP address for logging");
                return;
            }

            var userAgent = context.Request.Headers["User-Agent"].FirstOrDefault();
            var requestPath = context.Request.Path.Value;
            var referer = context.Request.Headers["Referer"].FirstOrDefault();

            // Check if this IP already exists in the audit log
            var existingLog = await dbContext.AuditLogsIPSafelistings
                .FirstOrDefaultAsync(log => log.IPAddress == clientIP && log.IsActive);

            if (existingLog != null)
            {
                // Update existing record
                existingLog.AccessAttempts++;
                existingLog.LastAttemptAt = DateTime.UtcNow;
                existingLog.UpdatedAt = DateTime.UtcNow;
                existingLog.UpdatedByUserId = "System";
                
                // Update additional context if available
                if (!string.IsNullOrEmpty(userAgent) && existingLog.UserAgent != userAgent)
                    existingLog.UserAgent = userAgent;
                if (!string.IsNullOrEmpty(requestPath) && existingLog.RequestPath != requestPath)
                    existingLog.RequestPath = requestPath;
                if (!string.IsNullOrEmpty(referer) && existingLog.Referer != referer)
                    existingLog.Referer = referer;

                _logger.LogInformation("Updated unauthorized access log for IP {ClientIP} - Attempt #{AttemptCount}", 
                    clientIP, existingLog.AccessAttempts);
            }
            else
            {
                // Create new record
                var newLog = new AuditLogsIPSafelisting
                {
                    IPAddress = clientIP,
                    UserAgent = userAgent,
                    RequestPath = requestPath,
                    Referer = referer,
                    AccessAttempts = 1,
                    FirstAttemptAt = DateTime.UtcNow,
                    LastAttemptAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = "System",
                    IsActive = true
                };

                dbContext.AuditLogsIPSafelistings.Add(newLog);
                _logger.LogInformation("Created new unauthorized access log for IP {ClientIP}", clientIP);
            }

            await dbContext.SaveChangesAsync();
            _logger.LogInformation("Successfully saved unauthorized access log for IP: {ClientIP}", clientIP);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging unauthorized access attempt for IP {ClientIP}", 
                GetClientIPAddress(context));
            // Don't rethrow - we don't want logging errors to break the redirect
        }
    }
}
