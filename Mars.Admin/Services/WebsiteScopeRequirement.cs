using Microsoft.AspNetCore.Authorization;

namespace Mars.Admin.Services;

public class WebsiteScopeRequirement : IAuthorizationRequirement
{
    public int WebsiteId { get; }

    public WebsiteScopeRequirement(int websiteId)
    {
        WebsiteId = websiteId;
    }
}

public class WebsiteScopeHandler : AuthorizationHandler<WebsiteScopeRequirement>
{
    private readonly IUserScope _userScope;

    public WebsiteScopeHandler(IUserScope userScope)
    {
        _userScope = userScope;
    }

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, WebsiteScopeRequirement requirement)
    {
        if (_userScope.AllowedWebsiteIds.Contains(requirement.WebsiteId))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

