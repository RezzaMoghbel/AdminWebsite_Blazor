using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Mars.Admin.Data;

namespace Mars.Admin.Services;

public class DynamicPermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IUserScope _userScope;
    private readonly ILogger<DynamicPermissionAuthorizationHandler> _logger;

    public DynamicPermissionAuthorizationHandler(
        IUserScope userScope, 
        ILogger<DynamicPermissionAuthorizationHandler> logger)
    {
        _userScope = userScope;
        _logger = logger;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, 
        PermissionRequirement requirement)
    {
        try
        {
            _logger.LogInformation("Checking permission {Permission} for user {UserId}", requirement.PermissionName, _userScope.UserId ?? "null");

            // Use the existing UserScope service which already handles permission checking
            if (_userScope.HasPermission(requirement.PermissionName))
            {
                _logger.LogDebug("User {UserId} has permission {Permission}", _userScope.UserId, requirement.PermissionName);
                context.Succeed(requirement);
            }
            else
            {
                _logger.LogDebug("User {UserId} does not have permission {Permission}", _userScope.UserId, requirement.PermissionName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking permission {Permission} for user", requirement.PermissionName);
        }

        return Task.CompletedTask;
    }
}

public class PermissionRequirement : IAuthorizationRequirement
{
    public string PermissionName { get; }

    public PermissionRequirement(string permissionName)
    {
        PermissionName = permissionName;
    }
}
