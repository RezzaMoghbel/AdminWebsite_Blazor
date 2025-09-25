using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Mars.Admin.Data;

public class SeedDataService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<SeedDataService> _logger;

    public SeedDataService(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ILogger<SeedDataService> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        try
        {
            await SeedRolesAsync();
            await SeedPermissionsAsync();
            await SeedRolePermissionsAsync();
            await UpdateExistingRolePermissionsAsync(); // Update existing roles with new permissions
            await SeedWebsitesAsync();
            await UpdateExistingWebsiteNamesAsync(); // Update existing website names
            await SeedUsersAsync();
            await SeedUserWebsiteAccessAsync();
            await SeedIPSafeListingsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during database seeding. Application will continue but some data may not be seeded properly.");
            // Don't rethrow to prevent application startup failure
        }
    }

    private async Task SeedRolesAsync()
    {
        var roles = new[]
        {
            new UserRole { Name = "SuperAdmin", Description = "Full system access", IsSuperAdmin = true, IsActive = true },
            new UserRole { Name = "Developer", Description = "Developer access", IsSuperAdmin = false, IsActive = true },
            new UserRole { Name = "Manager", Description = "Management access", IsSuperAdmin = false, IsActive = true },
            new UserRole { Name = "Customer Service", Description = "Customer service access", IsSuperAdmin = false, IsActive = true },
            new UserRole { Name = "Account", Description = "Account management access", IsSuperAdmin = false, IsActive = true },
            new UserRole { Name = "Viewer", Description = "Read-only access", IsSuperAdmin = false, IsActive = true },
            new UserRole { Name = "Page Editor", Description = "Page editing access", IsSuperAdmin = false, IsActive = true }
        };

        foreach (var role in roles)
        {
            if (!await _context.UserRoles.AnyAsync(r => r.Name == role.Name))
            {
                _context.UserRoles.Add(role);
                _logger.LogInformation("Added role: {RoleName}", role.Name);
            }
        }

        await _context.SaveChangesAsync();
    }

    private async Task SeedPermissionsAsync()
    {
        var permissions = new[]
        {
            new Permission { Name = "User.Create", Description = "Create users", IsActive = true },
            new Permission { Name = "User.Read", Description = "Read users", IsActive = true },
            new Permission { Name = "User.Update", Description = "Update users", IsActive = true },
            new Permission { Name = "User.Delete", Description = "Delete users", IsActive = true },
            new Permission { Name = "Role.Create", Description = "Create roles", IsActive = true },
            new Permission { Name = "Role.Read", Description = "Read roles", IsActive = true },
            new Permission { Name = "Role.Update", Description = "Update roles", IsActive = true },
            new Permission { Name = "Role.Delete", Description = "Delete roles", IsActive = true },
            new Permission { Name = "Permission.Create", Description = "Create permissions", IsActive = true },
            new Permission { Name = "Permission.Read", Description = "Read permissions", IsActive = true },
            new Permission { Name = "Permission.Update", Description = "Update permissions", IsActive = true },
            new Permission { Name = "Permission.Delete", Description = "Delete permissions", IsActive = true },
            new Permission { Name = "Website.Create", Description = "Create websites", IsActive = true },
            new Permission { Name = "Website.Read", Description = "Read websites", IsActive = true },
            new Permission { Name = "Website.Update", Description = "Update websites", IsActive = true },
            new Permission { Name = "Website.Delete", Description = "Delete websites", IsActive = true },
            new Permission { Name = "Quotes.Read", Description = "Read quotes", IsActive = true },
            new Permission { Name = "Policies.Read", Description = "Read policies", IsActive = true },
            new Permission { Name = "IPSafeListing.Create", Description = "Create IP safe listings", IsActive = true },
            new Permission { Name = "IPSafeListing.Read", Description = "Read IP safe listings", IsActive = true },
            new Permission { Name = "IPSafeListing.Update", Description = "Update IP safe listings", IsActive = true },
            new Permission { Name = "IPSafeListing.Delete", Description = "Delete IP safe listings", IsActive = true }
        };

        foreach (var permission in permissions)
        {
            if (!await _context.Permissions.AnyAsync(p => p.Name == permission.Name))
            {
                _context.Permissions.Add(permission);
                _logger.LogInformation("Added permission: {PermissionName}", permission.Name);
            }
        }

        await _context.SaveChangesAsync();
    }

    private async Task SeedRolePermissionsAsync()
    {
        var rolePermissions = new Dictionary<string, string[]>
        {
            ["SuperAdmin"] = new[] { "User.Create", "User.Read", "User.Update", "User.Delete", "Role.Create", "Role.Read", "Role.Update", "Role.Delete", "Permission.Create", "Permission.Read", "Permission.Update", "Permission.Delete", "Website.Create", "Website.Read", "Website.Update", "Website.Delete", "Quotes.Read", "Policies.Read", "IPSafeListing.Create", "IPSafeListing.Read", "IPSafeListing.Update", "IPSafeListing.Delete" },
            ["Developer"] = new[] { "User.Create", "User.Read", "User.Update", "User.Delete", "Role.Create", "Role.Read", "Role.Update", "Role.Delete", "Permission.Create", "Permission.Read", "Permission.Update", "Permission.Delete", "Website.Create", "Website.Read", "Website.Update", "Website.Delete", "Quotes.Read", "Policies.Read", "IPSafeListing.Create", "IPSafeListing.Read", "IPSafeListing.Update", "IPSafeListing.Delete" },
            ["Manager"] = new[] { "User.Read", "Role.Read", "Permission.Read", "Website.Read", "Quotes.Read", "Policies.Read", "IPSafeListing.Read" },
            ["Customer Service"] = new[] { "User.Read", "Website.Read", "Quotes.Read", "Policies.Read" },
            ["Account"] = new[] { "User.Read", "Role.Read", "Permission.Read", "Website.Read", "Quotes.Read", "Policies.Read", "IPSafeListing.Read" },
            ["Viewer"] = new[] { "User.Read", "Role.Read", "Permission.Read", "Website.Read", "Quotes.Read", "Policies.Read" },
            ["Page Editor"] = new string[0] // No permissions from this set
        };

        var roles = await _context.UserRoles.ToListAsync();
        var permissions = await _context.Permissions.ToListAsync();

        foreach (var rolePermission in rolePermissions)
        {
            var role = roles.FirstOrDefault(r => r.Name == rolePermission.Key);
            if (role == null) continue;

            foreach (var permissionName in rolePermission.Value)
            {
                var permission = permissions.FirstOrDefault(p => p.Name == permissionName);
                if (permission == null) continue;

                var existingRolePermission = await _context.UserRolePermissions
                    .FirstOrDefaultAsync(rp => rp.UserRoleId == role.Id && rp.PermissionId == permission.Id);

                if (existingRolePermission == null)
                {
                    _context.UserRolePermissions.Add(new UserRolePermission
                    {
                        UserRoleId = role.Id,
                        PermissionId = permission.Id,
                        IsGranted = true,
                        IsActive = true
                    });
                    _logger.LogInformation("Added role permission: {RoleName} -> {PermissionName}", role.Name, permission.Name);
                }
            }
        }

        await _context.SaveChangesAsync();
    }

    private async Task SeedWebsitesAsync()
    {
        var websites = new[]
        {
            new Website { Code = "ID", Name = "InsureDaily", Url = "https://www.InsureDaily.co.uk", IsActive = true },
            new Website { Code = "ILD", Name = "InsureLearnerDriver", Url = "https://www.InsureLearnerDriver.co.uk", IsActive = true },
            new Website { Code = "SI", Name = "SafelyInsured", Url = "https://www.SafelyInsured.co.uk", IsActive = true }
        };

        foreach (var website in websites)
        {
            if (!await _context.Websites.AnyAsync(w => w.Code == website.Code))
            {
                _context.Websites.Add(website);
                _logger.LogInformation("Added website: {WebsiteCode} - {WebsiteName}", website.Code, website.Name);
            }
        }

        await _context.SaveChangesAsync();
    }

    private async Task UpdateExistingWebsiteNamesAsync()
    {
        try
        {
            var websites = await _context.Websites.ToListAsync();
            bool hasChanges = false;

            foreach (var website in websites)
            {
                bool updated = false;

                // Update website names based on code
                switch (website.Code)
                {
                    case "ID":
                        if (website.Name != "InsureDaily" || website.Url != "https://www.InsureDaily.co.uk")
                        {
                            website.Name = "InsureDaily";
                            website.Url = "https://www.InsureDaily.co.uk";
                            website.ModifiedAt = DateTime.UtcNow;
                            website.ModifiedByUserId = "System";
                            updated = true;
                            _logger.LogInformation("Updated website ID: {OldName} -> {NewName}", website.Name, "InsureDaily");
                        }
                        break;

                    case "ILD":
                        if (website.Name != "InsureLearnerDriver" || website.Url != "https://www.InsureLearnerDriver.co.uk")
                        {
                            website.Name = "InsureLearnerDriver";
                            website.Url = "https://www.InsureLearnerDriver.co.uk";
                            website.ModifiedAt = DateTime.UtcNow;
                            website.ModifiedByUserId = "System";
                            updated = true;
                            _logger.LogInformation("Updated website ILD: {OldName} -> {NewName}", website.Name, "InsureLearnerDriver");
                        }
                        break;

                    case "SI":
                        if (website.Name != "SafelyInsured" || website.Url != "https://www.SafelyInsured.co.uk")
                        {
                            website.Name = "SafelyInsured";
                            website.Url = "https://www.SafelyInsured.co.uk";
                            website.ModifiedAt = DateTime.UtcNow;
                            website.ModifiedByUserId = "System";
                            updated = true;
                            _logger.LogInformation("Updated website SI: {OldName} -> {NewName}", website.Name, "SafelyInsured");
                        }
                        break;
                }

                if (updated)
                {
                    hasChanges = true;
                }
            }

            if (hasChanges)
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Updated existing website names successfully");
            }
            else
            {
                _logger.LogInformation("No website name updates needed");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating existing website names");
            // Don't rethrow to prevent application startup failure
        }
    }

    private async Task SeedUsersAsync()
    {
        var superAdminRole = await _context.UserRoles.FirstOrDefaultAsync(r => r.Name == "SuperAdmin");
        if (superAdminRole == null) return;

        var superAdminUser = new ApplicationUser
        {
            UserName = "superadmin@safelyinsured.co.uk",
            Email = "superadmin@safelyinsured.co.uk",
            EmailConfirmed = true,
            UserRoleId = superAdminRole.Id,
            IsActive = true,
            IsDeleted = false
        };

        var existingSuperAdmin = await _userManager.FindByNameAsync(superAdminUser.UserName);
        if (existingSuperAdmin == null)
        {
            var result = await _userManager.CreateAsync(superAdminUser, "12345Si!");
            if (result.Succeeded)
            {
                _logger.LogInformation("Created super admin user: {Email}", superAdminUser.Email);
            }
            else
            {
                _logger.LogError("Failed to create super admin user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
        else
        {
            // Ensure existing superadmin has the correct role assignment
            if (existingSuperAdmin.UserRoleId != superAdminRole.Id)
            {
                existingSuperAdmin.UserRoleId = superAdminRole.Id;
                await _userManager.UpdateAsync(existingSuperAdmin);
                _logger.LogInformation("Updated super admin user role assignment: {Email}", existingSuperAdmin.Email);
            }
        }

        // Create test users for each role
        var testUsers = new[]
        {
            new { Role = "Developer", Email = "developer@safelyinsured.co.uk", Password = "12345Si!" },
            new { Role = "Manager", Email = "manager@safelyinsured.co.uk", Password = "12345Si!" },
            new { Role = "Customer Service", Email = "support@safelyinsured.co.uk", Password = "12345Si!" },
            new { Role = "Account", Email = "account@safelyinsured.co.uk", Password = "12345Si!" },
            new { Role = "Viewer", Email = "viewer@safelyinsured.co.uk", Password = "12345Si!" },
            new { Role = "Page Editor", Email = "editor@safelyinsured.co.uk", Password = "12345Si!" }
        };

        foreach (var testUser in testUsers)
        {
            var role = await _context.UserRoles.FirstOrDefaultAsync(r => r.Name == testUser.Role);
            if (role == null) continue;

            var user = new ApplicationUser
            {
                UserName = testUser.Email,
                Email = testUser.Email,
                EmailConfirmed = true,
                UserRoleId = role.Id,
                IsActive = true,
                IsDeleted = false
            };

            var existingUser = await _userManager.FindByNameAsync(user.UserName);
            if (existingUser == null)
            {
                var result = await _userManager.CreateAsync(user, testUser.Password);
                if (result.Succeeded)
                {
                    _logger.LogInformation("Created test user: {Email} with role {Role}", user.Email, testUser.Role);
                }
                else
                {
                    _logger.LogError("Failed to create test user {Email}: {Errors}", user.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
            else
            {
                // Ensure existing user has the correct role assignment
                if (existingUser.UserRoleId != role.Id)
                {
                    existingUser.UserRoleId = role.Id;
                    await _userManager.UpdateAsync(existingUser);
                    _logger.LogInformation("Updated user role assignment: {Email} with role {Role}", existingUser.Email, testUser.Role);
                }
            }
        }
    }

    private async Task SeedIPSafeListingsAsync()
    {
        var officeIPs = new[]
        {
            new IPSafeListing { IPAddress = "127.0.0.1", UserId = null, Label = "Local Development", IsActive = true, CreatedAt = DateTime.UtcNow, CreatedByUserId = "system" },
            new IPSafeListing { IPAddress = "::1", UserId = null, Label = "Local Development IPv6", IsActive = true, CreatedAt = DateTime.UtcNow, CreatedByUserId = "system" },
            new IPSafeListing { IPAddress = "192.168.0.0/16", UserId = null, Label = "Private Network Range", IsActive = true, CreatedAt = DateTime.UtcNow, CreatedByUserId = "system" },
            new IPSafeListing { IPAddress = "10.0.0.0/8", UserId = null, Label = "Private Network Range", IsActive = true, CreatedAt = DateTime.UtcNow, CreatedByUserId = "system" },
            new IPSafeListing { IPAddress = "172.16.0.0/12", UserId = null, Label = "Private Network Range", IsActive = true, CreatedAt = DateTime.UtcNow, CreatedByUserId = "system" }
        };

        foreach (var ip in officeIPs)
        {
            if (!await _context.IPSafeListings.AnyAsync(i => i.IPAddress == ip.IPAddress && i.UserId == null))
            {
                _context.IPSafeListings.Add(ip);
                _logger.LogInformation("Added office IP safe listing: {IPAddress}", ip.IPAddress);
            }
        }

        // Add individual IP for superadmin
        var superadmin = await _userManager.FindByEmailAsync("superadmin@safelyinsured.co.uk");
        if (superadmin != null)
        {
            var individualIP = new IPSafeListing 
            { 
                IPAddress = "127.0.0.2", 
                Label = "SuperAdmin Individual IP", 
                UserId = superadmin.Id,
                IsActive = true, 
                CreatedAt = DateTime.UtcNow, 
                CreatedByUserId = "system" 
            };

            if (!await _context.IPSafeListings.AnyAsync(i => i.IPAddress == individualIP.IPAddress && i.UserId == individualIP.UserId))
            {
                _context.IPSafeListings.Add(individualIP);
                _logger.LogInformation("Added individual IP safe listing for superadmin: {IPAddress}", individualIP.IPAddress);
            }
        }

        await _context.SaveChangesAsync();
    }

    private async Task SeedUserWebsiteAccessAsync()
    {
        try
        {
            // Get all users and all websites
            var users = await _userManager.Users.ToListAsync();
            var websites = await _context.Websites.ToListAsync();

            // Clean up any corrupted UserWebsiteAccess records with NULL WebsiteId
            var corruptedAccesses = await _context.UserWebsiteAccesses
                .Where(ua => ua.WebsiteId == 0 || !_context.Websites.Any(w => w.Id == ua.WebsiteId))
                .ToListAsync();
            
            if (corruptedAccesses.Any())
            {
                _logger.LogWarning("Found {Count} corrupted UserWebsiteAccess records, removing them", corruptedAccesses.Count);
                _context.UserWebsiteAccesses.RemoveRange(corruptedAccesses);
                await _context.SaveChangesAsync();
            }

            foreach (var user in users)
            {
                foreach (var website in websites)
                {
                    var existingAccess = await _context.UserWebsiteAccesses
                        .FirstOrDefaultAsync(ua => ua.UserId == user.Id && ua.WebsiteId == website.Id);

                    if (existingAccess == null)
                    {
                        _context.UserWebsiteAccesses.Add(new UserWebsiteAccess
                        {
                            UserId = user.Id,
                            WebsiteId = website.Id,
                            IsGranted = true,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow,
                            CreatedByUserId = "system"
                        });
                        _logger.LogInformation("Added website access: {UserEmail} -> {WebsiteCode}", user.Email, website.Code);
                    }
                }
            }

            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding user website access");
            // Don't rethrow to prevent application startup failure
        }
    }

    private async Task UpdateExistingRolePermissionsAsync()
    {
        try
        {
            var roles = await _context.UserRoles.ToListAsync();
            var permissions = await _context.Permissions.ToListAsync();

            // Define the new permissions that should be added to existing roles
            var rolePermissionUpdates = new Dictionary<string, string[]>
            {
                ["SuperAdmin"] = new[] { "Role.Create", "Role.Read", "Role.Update", "Role.Delete", "Permission.Create", "Permission.Read", "Permission.Update", "Permission.Delete", "Website.Create", "Website.Read", "Website.Update", "Website.Delete" },
                ["Developer"] = new[] { "Role.Create", "Role.Read", "Role.Update", "Role.Delete", "Permission.Create", "Permission.Read", "Permission.Update", "Permission.Delete", "Website.Create", "Website.Read", "Website.Update", "Website.Delete" },
                ["Manager"] = new[] { "User.Read", "Role.Read", "Permission.Read", "Website.Read", "IPSafeListing.Read" },
                ["Customer Service"] = new[] { "User.Read", "Website.Read" },
                ["Account"] = new[] { "User.Read", "Role.Read", "Permission.Read", "Website.Read", "IPSafeListing.Read" },
                ["Viewer"] = new[] { "User.Read", "Role.Read", "Permission.Read", "Website.Read" }
            };

            foreach (var roleUpdate in rolePermissionUpdates)
            {
                var role = roles.FirstOrDefault(r => r.Name == roleUpdate.Key);
                if (role == null) continue;

                foreach (var permissionName in roleUpdate.Value)
                {
                    var permission = permissions.FirstOrDefault(p => p.Name == permissionName);
                    if (permission == null) continue;

                    // Check if this role-permission combination already exists
                    var existingRolePermission = await _context.UserRolePermissions
                        .FirstOrDefaultAsync(urp => urp.UserRoleId == role.Id && urp.PermissionId == permission.Id);

                    if (existingRolePermission == null)
                    {
                        // Create new role permission
                        var newRolePermission = new UserRolePermission
                        {
                            UserRoleId = role.Id,
                            PermissionId = permission.Id,
                            IsGranted = true,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow,
                            CreatedByUserId = "System"
                        };

                        _context.UserRolePermissions.Add(newRolePermission);
                        _logger.LogInformation("Added permission {Permission} to role {Role}", permissionName, role.Name);
                    }
                    else if (!existingRolePermission.IsActive || !existingRolePermission.IsGranted)
                    {
                        // Reactivate existing permission
                        existingRolePermission.IsActive = true;
                        existingRolePermission.IsGranted = true;
                        existingRolePermission.ModifiedAt = DateTime.UtcNow;
                        existingRolePermission.ModifiedByUserId = "System";
                        _logger.LogInformation("Reactivated permission {Permission} for role {Role}", permissionName, role.Name);
                    }
                }
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Updated existing role permissions successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating existing role permissions");
            // Don't rethrow to prevent application startup failure
        }
    }
}
