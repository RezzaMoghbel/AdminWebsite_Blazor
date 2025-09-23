using Microsoft.AspNetCore.Authorization;

namespace Mars.Admin.Services;

public class IpAllowedRequirement : IAuthorizationRequirement
{
    // This is a no-op requirement since IP checking is handled by middleware
    // It's included for symmetry and future extensibility
}

