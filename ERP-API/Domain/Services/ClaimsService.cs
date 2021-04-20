using System.Linq;
using Microsoft.AspNetCore.Http;

namespace ERP_API.Domain.Services
{
    public interface IClaimService
    {
        int UserId { get; }

        int TenantId { get; }
    }

    public class ClaimService : IClaimService
    {
        private readonly IHttpContextAccessor _accessor;

        public ClaimService(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }

        public int UserId =>
            int.TryParse(_accessor.HttpContext?.User?.Claims?.SingleOrDefault(x => x.Type == "UserId")?.Value,
                out var userId)
                ? userId
                : 0;

        public int TenantId =>
            int.TryParse(_accessor.HttpContext?.User?.Claims?.SingleOrDefault(x => x.Type == "TenantId")?.Value,
                out var tenantId)
                ? tenantId
                : 1;
    }
}
