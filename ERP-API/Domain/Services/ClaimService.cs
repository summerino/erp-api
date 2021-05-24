using System.Linq;
using Microsoft.AspNetCore.Http;

namespace ERP_API.Domain.Services
{
    public interface IClaimService
    {
        int UserId { get; }

        string CatalogUserId { get; }

        int TenantId { get; }

        string KeyToken { get; }

        string IpAddress { get; }

        string ExpiredTime { get; }
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
                : 0;

        public string KeyToken =>
            _accessor.HttpContext?.Request?.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
        
        public string IpAddress =>
            _accessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

        public string CatalogUserId =>
            _accessor.HttpContext?.User?.Claims?.SingleOrDefault(x => x.Type == "CatalogUserId")?.Value.ToString();

        public string ExpiredTime =>
            _accessor.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "exp")?.Value.ToString();
    }
}
