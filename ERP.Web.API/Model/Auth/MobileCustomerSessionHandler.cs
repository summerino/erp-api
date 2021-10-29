using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ERP.Entity;

namespace ERP.Web.API.Model.Auth
{
    public class MobileCustomerSessionRequirement : IAuthorizationRequirement
    {
        public MobileCustomerSessionRequirement()
        {
        }
    }

    public class MobileCustomerSessionHandler : AuthorizationHandler<MobileCustomerSessionRequirement>
    {
        private readonly TenantContext _tenantCtx;
        private readonly IClaimService _claim;
        private readonly JwtConfig _jwtConfig;

        public MobileCustomerSessionHandler(TenantContext tenantCtx, IClaimService claim, IOptionsMonitor<JwtConfig> optionsMonitor)
        {
            _tenantCtx = tenantCtx;
            _claim = claim;
            _jwtConfig = optionsMonitor.CurrentValue;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, MobileCustomerSessionRequirement requirement)
        {
            var currentUserId = _claim.CatalogUserId;

            if (!string.IsNullOrWhiteSpace(currentUserId))
            {
                if (
                    _tenantCtx.Customers
                        .Any(x => x.IsActive && x.IsMobileLoggedIn && x.MobileSignIn &&
                                  x.CatalogUserId.ToString() == currentUserId &&
                                  x.MobileTokenId == _claim.KeyToken &&
                                  EF.Functions.DateDiffMonth(x.MobileLastLogin, DateTime.Now) < _jwtConfig.TimeInMinute))
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
}
