using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using ERP_API.Domain.Entities;
using ERP_API.Domain.Services;

namespace ERP_API.Model.Auth
{
    public class UserSessionRequirement : IAuthorizationRequirement
    {
        public UserSessionRequirement()
        {
        }
    }

    public class UserSessionHandler : AuthorizationHandler<UserSessionRequirement>
    {
        private readonly CatalogContext _catalogCtx;
        private readonly TenantContext _tenantCtx;
        private readonly IClaimService _claim;

        public UserSessionHandler(CatalogContext catalogCtx, TenantContext tenantCtx, IClaimService claim)
        {
            _catalogCtx = catalogCtx;
            _tenantCtx = tenantCtx;
            _claim = claim;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, UserSessionRequirement requirement)
        {
            string currentUserId = _claim.CatalogUserId;
            if (currentUserId != null)
            {
                var loggedUser = _catalogCtx.Users.FirstOrDefault(x => x.Id.ToString() == currentUserId);
                if (loggedUser != null)
                {
                    var headerToken = _claim.KeyToken;
                    var accessIpAdd = _claim.IpAddress;
                    var tenantUser = _tenantCtx.Users.FirstOrDefault(x => x.CatalogUserId == loggedUser.Id);
                    if (tenantUser.TokenId != headerToken || tenantUser.IpAddress != accessIpAdd)
                    {
                        context.Fail();
                    }
                    else
                    {
                        context.Succeed(requirement);
                    }
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
