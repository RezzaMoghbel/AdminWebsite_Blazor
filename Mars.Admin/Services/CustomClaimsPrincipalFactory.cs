using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Mars.Admin.Data;
using System.Security.Claims;

namespace Mars.Admin.Services;

public class CustomClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser>
{
    private readonly ApplicationDbContext _context;

    public CustomClaimsPrincipalFactory(
        UserManager<ApplicationUser> userManager,
        IOptions<IdentityOptions> optionsAccessor,
        ApplicationDbContext context)
        : base(userManager, optionsAccessor)
    {
        _context = context;
    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        // Check if user is active and not deleted
        if (!user.IsActive || user.IsDeleted)
        {
            Console.WriteLine($"Login blocked for user: {user.Email} - IsActive: {user.IsActive}, IsDeleted: {user.IsDeleted}");
            // Return an empty identity with no claims to prevent login
            return new ClaimsIdentity();
        }

        var identity = await base.GenerateClaimsAsync(user);

        // Add logging to debug claims generation
        Console.WriteLine($"Generating claims for user: {user.Email}, UserRoleId: {user.UserRoleId}");

        // Add role-based permissions
        if (user.UserRoleId.HasValue)
        {
            var userRole = await _context.UserRoles
                .Include(r => r.UserRolePermissions)
                .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(r => r.Id == user.UserRoleId.Value);

            if (userRole != null)
            {
                Console.WriteLine($"Found user role: {userRole.Name}, IsSuperAdmin: {userRole.IsSuperAdmin}");
                
                // Add role claim
                identity.AddClaim(new Claim(ClaimTypes.Role, userRole.Name));

                // If super admin, add full access claim
                if (userRole.IsSuperAdmin)
                {
                    identity.AddClaim(new Claim("perm", "*"));
                    Console.WriteLine("Added FullAccess claim (*)");
                }
                else
                {
                    // Add individual permissions
                    var grantedPermissions = userRole.UserRolePermissions
                        .Where(rp => rp.IsGranted && rp.IsActive && rp.Permission.IsActive)
                        .Select(rp => rp.Permission.Name);

                    foreach (var permission in grantedPermissions)
                    {
                        identity.AddClaim(new Claim("perm", permission));
                        Console.WriteLine($"Added permission claim: {permission}");
                    }
                }
            }
            else
            {
                Console.WriteLine("No user role found for UserRoleId: " + user.UserRoleId.Value);
            }
        }

        // Add website scope claims
        var allowedWebsites = await _context.UserWebsiteAccesses
            .Where(ua => ua.UserId == user.Id && ua.IsGranted && ua.IsActive)
            .Select(ua => ua.WebsiteId.ToString())
            .ToListAsync();

        foreach (var websiteId in allowedWebsites)
        {
            identity.AddClaim(new Claim("site", websiteId));
        }

        return identity;
    }
}
