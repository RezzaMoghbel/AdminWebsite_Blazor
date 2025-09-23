using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Mars.Admin.Data;

namespace Mars.Admin.Services;

public interface IUserScope
{
    IReadOnlyCollection<int> AllowedWebsiteIds { get; }
    IReadOnlyCollection<WebsiteInfo> AllowedWebsites { get; }
    bool HasPermission(string permissionName);
    bool IsSuperAdmin { get; }
    string? UserId { get; }
}

public class WebsiteInfo
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Url { get; set; }
}

public class UserScope : IUserScope
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<UserScope> _logger;
    private readonly ApplicationDbContext _context;
    private IReadOnlyCollection<WebsiteInfo>? _cachedWebsites;

    public UserScope(IHttpContextAccessor httpContextAccessor, ILogger<UserScope> logger, ApplicationDbContext context)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
        _context = context;
    }

    public IReadOnlyCollection<int> AllowedWebsiteIds
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated != true)
                return Array.Empty<int>();

            var siteClaims = user.FindAll("site");
            var websiteIds = new List<int>();

            foreach (var claim in siteClaims)
            {
                if (int.TryParse(claim.Value, out var websiteId))
                {
                    websiteIds.Add(websiteId);
                }
            }

            return websiteIds.AsReadOnly();
        }
    }

    public IReadOnlyCollection<WebsiteInfo> AllowedWebsites
    {
        get
        {
            if (_cachedWebsites != null)
                return _cachedWebsites;

            var user = _httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated != true)
            {
                _cachedWebsites = Array.Empty<WebsiteInfo>();
                return _cachedWebsites;
            }

            var siteClaims = user.FindAll("site");
            var websiteIds = new List<int>();

            foreach (var claim in siteClaims)
            {
                if (int.TryParse(claim.Value, out var websiteId))
                {
                    websiteIds.Add(websiteId);
                }
            }

            if (websiteIds.Count == 0)
            {
                _cachedWebsites = Array.Empty<WebsiteInfo>();
                return _cachedWebsites;
            }

            try
            {
                var websites = _context.Websites
                    .Where(w => websiteIds.Contains(w.Id) && w.IsActive)
                    .Select(w => new WebsiteInfo
                    {
                        Id = w.Id,
                        Code = w.Code,
                        Name = w.Name,
                        Url = w.Url
                    })
                    .ToList()
                    .AsReadOnly();

                _cachedWebsites = websites;
                return _cachedWebsites;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading allowed websites for user");
                _cachedWebsites = Array.Empty<WebsiteInfo>();
                return _cachedWebsites;
            }
        }
    }

    public bool HasPermission(string permissionName)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated != true)
            return false;

        // Check for full access
        if (user.HasClaim("perm", "*"))
            return true;

        // Check for specific permission
        return user.HasClaim("perm", permissionName);
    }

    public bool IsSuperAdmin
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            return user?.HasClaim("perm", "*") == true;
        }
    }

    public string? UserId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            return user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}
