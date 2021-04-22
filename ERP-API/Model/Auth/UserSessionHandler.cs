using ERP_API.Domain.Entities;
using ERP_API.Domain.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        private readonly IClaimService _claimService;
        private readonly CatalogContext _catalogcontext;
        private readonly TenantContext _tenantContext;
        public UserSessionHandler(IClaimService claimService, CatalogContext catalogcontext, TenantContext tenantContext)
        {
            _claimService = claimService;
            _catalogcontext = catalogcontext;
            _tenantContext = tenantContext;
        }
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, UserSessionRequirement requirement)
        {
            string currentUserId = _claimService.CatalogUserId;
            if (currentUserId != null)
            {
                var loggedUser = _catalogcontext.Users.FirstOrDefault(x => x.Id.ToString() == currentUserId);
                if (loggedUser != null)
                {
                    var headerToken = _claimService.KeyToken;
                    var accessIpAdd = _claimService.IpAddress;
                    var tenantUser = _tenantContext.Users.FirstOrDefault(x => x.CatalogUserId == loggedUser.Id);
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
