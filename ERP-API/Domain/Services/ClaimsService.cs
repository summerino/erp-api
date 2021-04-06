using System.Linq;
using Microsoft.AspNetCore.Http;

namespace ERP_API.Domain.Services
{
    public interface IClaimService
    {
        int UserId { get; }

        string TenantShard { get; }
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

        public string TenantShard =>
            _accessor.HttpContext?.User?.Claims?.SingleOrDefault(x => x.Type == "TenantShard")?.Value ?? "4b279e3e-fe0f-4517-86e9-40d59821cb73";
    }
}
