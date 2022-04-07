using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ERP.Entity;

namespace ERP.Web.API.Model.Auth;

public class UserSessionRequirement : IAuthorizationRequirement
{
    public UserSessionRequirement()
    {
    }
}

public class UserSessionHandler : AuthorizationHandler<UserSessionRequirement>
{
    private readonly TenantContext _tenantCtx;
    private readonly IClaimService _claim;
    private readonly JwtConfig _jwtConfig;

    public UserSessionHandler(TenantContext tenantCtx, IClaimService claim, IOptionsMonitor<JwtConfig> optionsMonitor)
    {
        _tenantCtx = tenantCtx;
        _claim = claim;
        _jwtConfig = optionsMonitor.CurrentValue;
    }

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, UserSessionRequirement requirement)
    {
        var currentUserId = _claim.CatalogUserId;

        if (!string.IsNullOrWhiteSpace(currentUserId))
        {
            if (
                _tenantCtx.Users
                .Any(x => x.IsActive && x.IsLoggedIn &&
                          x.CatalogUserId.ToString() == currentUserId &&
                          x.TokenId == _claim.KeyToken &&
                          EF.Functions.DateDiffMonth(x.LastLogin, DateTime.Now) < _jwtConfig.ExpiresInMinute))
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail();
            }
        }
        else
        {
            context.Fail();
        }

        return Task.CompletedTask;
    }
}