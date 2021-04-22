using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace ERP_API.Domain.Services
{
    public interface IClaimService
    {
        int UserId { get; }

        string CatalogUserId { get; }

        int TenantId { get; }

        string KeyToken { get; }

        string UserEmail { get; }

        string IpAddress { get; }
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

        public string KeyToken =>
            _accessor.HttpContext?.Request?.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

        public string UserEmail =>
            _accessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;

        public string IpAddress =>
            _accessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

        public string CatalogUserId =>
            _accessor.HttpContext?.User?.Claims?.SingleOrDefault(x => x.Type == "CatalogUserId")?.Value.ToString();
    }
}
