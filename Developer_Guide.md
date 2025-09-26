# Mars Admin System - Developer Guide

## Overview

This guide provides comprehensive instructions for developers on how to implement permissions, authorization, and create new pages in the Mars Admin System. The system uses ASP.NET Core Identity with custom claims-based authorization.

## Architecture Overview

### Core Components

- **ASP.NET Core Identity**: User management and authentication
- **Custom Claims**: Permission and website access control
- **Dynamic Authorization**: Runtime permission checking
- **Role-Based Access Control (RBAC)**: Hierarchical permission system

### Key Services

- `IUserScope`: User context and permission checking
- `UserAlertService`: User attention and alert management
- `CustomClaimsPrincipalFactory`: Claims generation during login
- `DynamicAuthorizationPolicyProvider`: Runtime policy creation

## Permission System Implementation

### 1. Permission Structure

#### Database Model

```csharp
public class Permission
{
    public int Id { get; set; }
    public string Name { get; set; }           // e.g., "CreateUser", "EditRole"
    public string Description { get; set; }   // Human-readable description
    public bool IsActive { get; set; }         // Enable/disable permission
}
```

#### Role-Permission Relationship

```csharp
public class UserRolePermission
{
    public int UserRoleId { get; set; }
    public int PermissionId { get; set; }
    public UserRole UserRole { get; set; }
    public Permission Permission { get; set; }
}
```

### 2. Claims-Based Authorization

#### Claims Structure

The system uses three types of claims:

- **Role Claims**: `ClaimTypes.Role` (e.g., "SuperAdmin", "Developer")
- **Permission Claims**: `"perm"` (e.g., "CreateUser", "EditRole")
- **Website Claims**: `"site"` (e.g., "WebsiteId1", "WebsiteId2")

#### Claims Generation (CustomClaimsPrincipalFactory.cs)

```csharp
public override async Task<ClaimsPrincipal> CreateAsync(ApplicationUser user)
{
    var principal = await base.CreateAsync(user);

    // Add role claims
    var roles = await UserManager.GetRolesAsync(user);
    foreach (var role in roles)
    {
        ((ClaimsIdentity)principal.Identity!).AddClaim(new Claim(ClaimTypes.Role, role));
    }

    // Add permission claims
    var userRole = await _context.UserRoles
        .Include(ur => ur.UserRolePermissions)
        .ThenInclude(urp => urp.Permission)
        .FirstOrDefaultAsync(ur => ur.Id == user.UserRoleId);

    if (userRole != null)
    {
        foreach (var permission in userRole.UserRolePermissions.Select(urp => urp.Permission))
        {
            ((ClaimsIdentity)principal.Identity!).AddClaim(new Claim("perm", permission.Name));
        }
    }

    // Add website access claims
    var websiteAccesses = await _context.UserWebsiteAccesses
        .Where(uwa => uwa.UserId == user.Id && uwa.IsActive && uwa.IsGranted)
        .ToListAsync();

    foreach (var access in websiteAccesses)
    {
        ((ClaimsIdentity)principal.Identity!).AddClaim(new Claim("site", access.WebsiteId.ToString()));
    }

    return principal;
}
```

## Creating New Pages with Authorization

### 1. Basic Page Structure

#### Step 1: Create the Razor Page

```razor
@page "/admin/newfeature"
@using Mars.Admin.Services
@using Mars.Admin.Components.Shared
@attribute [Authorize]
@inject IUserScope UserScope
@inject NavigationManager NavigationManager

<PageTitle>New Feature</PageTitle>

@if (!UserScope.HasPermission("ViewNewFeature"))
{
    <div class="alert alert-danger">
        <h4>Access Denied</h4>
        <p>You don't have permission to access this feature.</p>
    </div>
    return;
}

<div class="container-fluid">
    <div class="row">
        <div class="col-12">
            <div class="card">
                <div class="card-header d-flex justify-content-between align-items-center">
                    <h3 class="mb-0">New Feature Management</h3>
                    @if (UserScope.HasPermission("CreateNewFeature"))
                    {
                        <button class="btn btn-primary" @onclick="CreateNew">
                            <i class="bi bi-plus-circle"></i> Create New
                        </button>
                    }
                </div>
                <div class="card-body">
                    <!-- Your content here -->
                </div>
            </div>
        </div>
    </div>
</div>

@code {
    protected override async Task OnInitializedAsync()
    {
        // Check if user has required permissions
        if (!UserScope.HasPermission("ViewNewFeature"))
        {
            NavigationManager.NavigateTo("/access-denied");
            return;
        }

        await LoadData();
    }

    private async Task LoadData()
    {
        // Load your data here
    }

    private async Task CreateNew()
    {
        if (!UserScope.HasPermission("CreateNewFeature"))
        {
            // Handle unauthorized access
            return;
        }

        // Create new item logic
    }
}
```

### 2. Permission Checking Methods

#### Using IUserScope Service

```csharp
// Check single permission
if (UserScope.HasPermission("CreateUser"))
{
    // User has permission
}

// Check multiple permissions (ALL required)
if (UserScope.HasPermission("EditUser") && UserScope.HasPermission("ViewUsers"))
{
    // User has both permissions
}

// Check website access
if (UserScope.HasWebsiteAccess(websiteId))
{
    // User has access to specific website
}

// Check if user is SuperAdmin
if (UserScope.IsSuperAdmin)
{
    // User is SuperAdmin (has wildcard access)
}
```

#### Direct Claims Checking

```csharp
// Check permission claim
if (User.HasClaim("perm", "CreateUser"))
{
    // User has permission
}

// Check role claim
if (User.IsInRole("SuperAdmin"))
{
    // User is SuperAdmin
}

// Check website access claim
if (User.HasClaim("site", websiteId.ToString()))
{
    // User has access to website
}
```

### 3. Authorization Attributes

#### Page-Level Authorization

```razor
@page "/admin/users"
@attribute [Authorize]  // Requires authentication
@attribute [Authorize(Roles = "SuperAdmin")]  // Requires specific role
```

#### Method-Level Authorization

```csharp
[Authorize(Policy = "PermissionPolicy")]
public async Task<IActionResult> CreateUser()
{
    // Method requires permission policy
}

[Authorize(Roles = "SuperAdmin,Developer")]
public async Task<IActionResult> AdminOnlyMethod()
{
    // Method requires specific roles
}
```

### 4. Dynamic Authorization Policies

#### Policy Registration (Program.cs)

```csharp
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("PermissionPolicy", policy =>
        policy.Requirements.Add(new PermissionRequirement()));

    options.AddPolicy("WebsiteAccessPolicy", policy =>
        policy.Requirements.Add(new WebsiteScopeRequirement()));
});
```

#### Custom Authorization Handler

```csharp
public class DynamicPermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IUserScope _userScope;

    public DynamicPermissionAuthorizationHandler(IUserScope userScope)
    {
        _userScope = userScope;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (_userScope.HasPermission(requirement.Permission))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
```

## Complete Example: User Management Page

### 1. Page Implementation

```razor
@page "/admin/users"
@using Mars.Admin.Services
@using Mars.Admin.Data
@attribute [Authorize]
@inject IUserScope UserScope
@inject ApplicationDbContext Context
@inject UserManager<ApplicationUser> UserManager
@inject NavigationManager NavigationManager

<PageTitle>User Management</PageTitle>

@if (!UserScope.HasPermission("ViewUsers"))
{
    <div class="alert alert-danger">
        <h4>Access Denied</h4>
        <p>You don't have permission to view users.</p>
    </div>
    return;
}

<div class="container-fluid">
    <div class="row">
        <div class="col-12">
            <div class="card">
                <div class="card-header d-flex justify-content-between align-items-center">
                    <h3 class="mb-0">User Management</h3>
                    <div>
                        @if (UserScope.HasPermission("CreateUser"))
                        {
                            <button class="btn btn-primary me-2" @onclick="CreateUser">
                                <i class="bi bi-plus-circle"></i> Create User
                            </button>
                        }
                        @if (UserScope.HasPermission("ExportUsers"))
                        {
                            <button class="btn btn-success" @onclick="ExportUsers">
                                <i class="bi bi-download"></i> Export
                            </button>
                        }
                    </div>
                </div>
                <div class="card-body">
                    <div class="table-responsive">
                        <table class="table table-striped">
                            <thead>
                                <tr>
                                    <th>Email</th>
                                    <th>Role</th>
                                    <th>Status</th>
                                    <th>Last Login</th>
                                    <th>Actions</th>
                                </tr>
                            </thead>
                            <tbody>
                                @foreach (var user in users)
                                {
                                    <tr>
                                        <td>@user.Email</td>
                                        <td>@(user.UserRole?.Name ?? "No Role")</td>
                                        <td>
                                            @if (user.IsActive)
                                            {
                                                <span class="badge bg-success">Active</span>
                                            }
                                            else
                                            {
                                                <span class="badge bg-danger">Inactive</span>
                                            }
                                        </td>
                                        <td>@(user.LastLoginAt?.ToString("yyyy-MM-dd HH:mm") ?? "Never")</td>
                                        <td>
                                            <div class="btn-group" role="group">
                                                @if (UserScope.HasPermission("EditUser"))
                                                {
                                                    <button class="btn btn-sm btn-outline-primary" @onclick="() => EditUser(user.Id)">
                                                        <i class="bi bi-pencil"></i>
                                                    </button>
                                                }
                                                @if (UserScope.HasPermission("DeleteUser"))
                                                {
                                                    <button class="btn btn-sm btn-outline-danger" @onclick="() => DeleteUser(user.Id)">
                                                        <i class="bi bi-trash"></i>
                                                    </button>
                                                }
                                            </div>
                                        </td>
                                    </tr>
                                }
                            </tbody>
                        </table>
                    </div>
                </div>
            </div>
        </div>
    </div>
</div>

@code {
    private List<ApplicationUser> users = new();

    protected override async Task OnInitializedAsync()
    {
        // Verify permission before loading data
        if (!UserScope.HasPermission("ViewUsers"))
        {
            NavigationManager.NavigateTo("/access-denied");
            return;
        }

        await LoadUsers();
    }

    private async Task LoadUsers()
    {
        users = await Context.Users
            .Include(u => u.UserRole)
            .Where(u => !u.IsDeleted)
            .ToListAsync();
    }

    private async Task CreateUser()
    {
        if (!UserScope.HasPermission("CreateUser"))
        {
            // Handle unauthorized access
            return;
        }

        // Navigate to create user page
        NavigationManager.NavigateTo("/admin/users/create");
    }

    private async Task EditUser(string userId)
    {
        if (!UserScope.HasPermission("EditUser"))
        {
            return;
        }

        // Navigate to edit user page
        NavigationManager.NavigateTo($"/admin/users/edit/{userId}");
    }

    private async Task DeleteUser(string userId)
    {
        if (!UserScope.HasPermission("DeleteUser"))
        {
            return;
        }

        // Show confirmation dialog
        // Implement delete logic
    }

    private async Task ExportUsers()
    {
        if (!UserScope.HasPermission("ExportUsers"))
        {
            return;
        }

        // Implement export logic
    }
}
```

### 2. Permission Requirements

To use this page, users need these permissions:

- `ViewUsers`: Required to see the page
- `CreateUser`: Required to create new users
- `EditUser`: Required to edit existing users
- `DeleteUser`: Required to delete users
- `ExportUsers`: Required to export user data

## Adding New Permissions

### 1. Database Migration

```csharp
// In SeedDataService.cs
private async Task SeedPermissionsAsync()
{
    var permissions = new[]
    {
        // Existing permissions...
        new Permission { Name = "ViewNewFeature", Description = "View new feature", IsActive = true },
        new Permission { Name = "CreateNewFeature", Description = "Create new feature items", IsActive = true },
        new Permission { Name = "EditNewFeature", Description = "Edit new feature items", IsActive = true },
        new Permission { Name = "DeleteNewFeature", Description = "Delete new feature items", IsActive = true },
    };

    foreach (var permission in permissions)
    {
        if (!await _context.Permissions.AnyAsync(p => p.Name == permission.Name))
        {
            _context.Permissions.Add(permission);
        }
    }
}
```

### 2. Role Assignment

```csharp
// Assign permissions to roles
private async Task SeedRolePermissionsAsync()
{
    // SuperAdmin gets all permissions
    var superAdminRole = await _context.UserRoles.FirstAsync(r => r.Name == "SuperAdmin");
    var allPermissions = await _context.Permissions.ToListAsync();

    foreach (var permission in allPermissions)
    {
        if (!await _context.UserRolePermissions.AnyAsync(urp =>
            urp.UserRoleId == superAdminRole.Id && urp.PermissionId == permission.Id))
        {
            _context.UserRolePermissions.Add(new UserRolePermission
            {
                UserRoleId = superAdminRole.Id,
                PermissionId = permission.Id
            });
        }
    }
}
```

## Website-Specific Authorization

### 1. Website Access Checking

```csharp
// Check if user has access to specific website
if (UserScope.HasWebsiteAccess(websiteId))
{
    // User can access this website
}

// Check multiple websites
var accessibleWebsites = UserScope.GetAccessibleWebsites();
if (accessibleWebsites.Contains(websiteId))
{
    // User has access
}
```

### 2. Website-Scoped Data Loading

```csharp
private async Task LoadWebsiteData(int websiteId)
{
    // Verify website access
    if (!UserScope.HasWebsiteAccess(websiteId))
    {
        // Handle unauthorized access
        return;
    }

    // Load data for specific website
    var data = await Context.SomeTable
        .Where(x => x.WebsiteId == websiteId)
        .ToListAsync();
}
```

## Error Handling & Security

### 1. Access Denied Handling

```csharp
// Redirect to access denied page
if (!UserScope.HasPermission("RequiredPermission"))
{
    NavigationManager.NavigateTo("/access-denied");
    return;
}

// Show inline error message
@if (!UserScope.HasPermission("RequiredPermission"))
{
    <div class="alert alert-danger">
        <h4>Access Denied</h4>
        <p>You don't have permission to perform this action.</p>
    </div>
    return;
}
```

### 2. Secure Data Loading

```csharp
// Always check permissions before loading sensitive data
private async Task LoadSensitiveData()
{
    if (!UserScope.HasPermission("ViewSensitiveData"))
    {
        throw new UnauthorizedAccessException("Insufficient permissions");
    }

    // Load data
}
```

### 3. Audit Logging

```csharp
// Log permission checks for audit purposes
private async Task LogPermissionCheck(string permission, bool granted)
{
    await _context.AuditLogs.AddAsync(new AuditLog
    {
        UserId = UserScope.UserId,
        Action = $"Permission Check: {permission}",
        Result = granted ? "Granted" : "Denied",
        Timestamp = DateTime.UtcNow
    });

    await _context.SaveChangesAsync();
}
```

## Testing Authorization

### 1. Unit Tests

```csharp
[Test]
public async Task UserWithPermission_ShouldHaveAccess()
{
    // Arrange
    var user = CreateTestUser();
    user.Claims.Add(new Claim("perm", "CreateUser"));
    var userScope = new UserScope(user);

    // Act
    var hasPermission = userScope.HasPermission("CreateUser");

    // Assert
    Assert.IsTrue(hasPermission);
}

[Test]
public async Task UserWithoutPermission_ShouldNotHaveAccess()
{
    // Arrange
    var user = CreateTestUser();
    var userScope = new UserScope(user);

    // Act
    var hasPermission = userScope.HasPermission("CreateUser");

    // Assert
    Assert.IsFalse(hasPermission);
}
```

### 2. Integration Tests

```csharp
[Test]
public async Task AuthorizedUser_CanAccessProtectedPage()
{
    // Arrange
    var client = CreateAuthenticatedClient("SuperAdmin");

    // Act
    var response = await client.GetAsync("/admin/users");

    // Assert
    Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
}
```

## Best Practices

### 1. Permission Naming Convention

- Use descriptive names: `CreateUser`, `EditRole`, `DeleteWebsite`
- Follow consistent patterns: `[Action][Resource]`
- Use PascalCase for permission names

### 2. Security Guidelines

- **Always check permissions** before showing UI elements
- **Verify permissions** before executing actions
- **Use least privilege** principle
- **Log permission checks** for audit purposes
- **Test authorization** thoroughly

### 3. Performance Considerations

- **Cache permission checks** when possible
- **Use efficient queries** for permission validation
- **Minimize database calls** in permission checking
- **Consider using claims** for frequently checked permissions

### 4. Maintenance

- **Document all permissions** and their purposes
- **Regular permission audits** to remove unused permissions
- **Version control** permission changes
- **Test permission changes** thoroughly

## Troubleshooting

### Common Issues

#### 1. "Access Denied" Errors

- Check if user has the required permission
- Verify permission is assigned to user's role
- Ensure permission is active in database
- Check website access if applicable

#### 2. Permissions Not Working

- Verify claims are generated correctly
- Check if permission exists in database
- Ensure role-permission relationship exists
- Clear browser cache and re-login

#### 3. Performance Issues

- Optimize permission checking queries
- Consider caching frequently checked permissions
- Use efficient LINQ queries
- Monitor database performance

### Debug Tools

```csharp
// Debug user claims
var claims = User.Claims.ToList();
foreach (var claim in claims)
{
    Console.WriteLine($"{claim.Type}: {claim.Value}");
}

// Debug user scope
Console.WriteLine($"IsSuperAdmin: {UserScope.IsSuperAdmin}");
Console.WriteLine($"HasPermission('CreateUser'): {UserScope.HasPermission("CreateUser")}");
Console.WriteLine($"Accessible Websites: {string.Join(", ", UserScope.GetAccessibleWebsites())}");
```

---

_This guide should be updated whenever new authorization patterns or permissions are added to the system. Always test authorization thoroughly before deploying to production._
