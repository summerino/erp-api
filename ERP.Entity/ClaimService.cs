using System.Linq;
using Microsoft.AspNetCore.Http;

namespace ERP.Entity
{
    public interface IClaimService
    {
        string Jti { get; }

        int UserId { get; }

        int RoleId { get; }

        int TenantId { get; }

        string CatalogUserId { get; }

        string KeyToken { get; }

        string IpAddress { get; }
    }

    public class ClaimService : IClaimService
    {
        private readonly IHttpContextAccessor _accessor;

        public ClaimService(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }

        public string Jti =>
            _accessor.HttpContext?.User?.Claims?.SingleOrDefault(x => x.Type == "jti")?.Value.ToString();

        public int UserId =>
            int.TryParse(_accessor.HttpContext?.User?.Claims?.SingleOrDefault(x => x.Type == "UserId")?.Value,
                out var userId)
                ? userId
                : 0;

        public int RoleId =>
            int.TryParse(_accessor.HttpContext?.User?.Claims?.SingleOrDefault(x => x.Type == "RoleId")?.Value,
                out var roleId)
                ? roleId
                : 0;

        public int TenantId =>
            int.TryParse(_accessor.HttpContext?.User?.Claims?.SingleOrDefault(x => x.Type == "TenantId")?.Value,
                out var tenantId)
                ? tenantId
                : 0;

        public string CatalogUserId =>
            _accessor.HttpContext?.User?.Claims?.SingleOrDefault(x => x.Type == "CatalogUserId")?.Value.ToString();

        public string KeyToken =>
            _accessor.HttpContext?.Request?.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
        
        public string IpAddress =>
            _accessor.HttpContext?.Connection?.RemoteIpAddress?.MapToIPv4().ToString();
    }
}
