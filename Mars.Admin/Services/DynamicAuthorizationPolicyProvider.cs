using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Mars.Admin.Services;

public class DynamicAuthorizationPolicyProvider : IAuthorizationPolicyProvider
{
    private readonly DefaultAuthorizationPolicyProvider _fallbackPolicyProvider;
    private readonly AuthorizationOptions _options;

    public DynamicAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options)
    {
        _fallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
        _options = options.Value;
    }

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        // Check if it's a permission-based policy
        if (IsPermissionPolicy(policyName))
        {
            var policy = new AuthorizationPolicyBuilder()
                .AddRequirements(new PermissionRequirement(policyName))
                .Build();

            return Task.FromResult<AuthorizationPolicy?>(policy);
        }

        // Fall back to default policy provider
        return _fallbackPolicyProvider.GetPolicyAsync(policyName);
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
    {
        return _fallbackPolicyProvider.GetDefaultPolicyAsync();
    }

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
    {
        return _fallbackPolicyProvider.GetFallbackPolicyAsync();
    }

    private static bool IsPermissionPolicy(string policyName)
    {
        // Check if the policy name looks like a permission (contains a dot)
        // Examples: User.Read, IPSafeListing.Create, Quotes.Read, etc.
        return policyName.Contains('.') && policyName.Length > 3;
    }
}
