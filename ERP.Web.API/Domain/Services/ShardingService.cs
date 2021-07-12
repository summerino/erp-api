using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ERP.Entity;

namespace ERP.Web.API.Domain.Services
{
    public interface IShardingService
    {
        Task ApplyMigrationAsync();
    }

    public class ShardingService : IShardingService
    {
        private readonly CatalogContext _catalogCtx;
        private readonly IClaimService _claim;

        public ShardingService(CatalogContext catalogCtx, IClaimService claim)
        {
            _catalogCtx = catalogCtx;
            _claim = claim;
        }

        public async Task ApplyMigrationAsync()
        {
            foreach (var tenant in _catalogCtx.Tenants)
            {
                if (!string.IsNullOrWhiteSpace(tenant.ServerName) || !string.IsNullOrWhiteSpace(tenant.DatabaseName) ||
                    !string.IsNullOrWhiteSpace(tenant.ServerUserId) || !string.IsNullOrWhiteSpace(tenant.ServerPassword))
                {
                    var optionsBuilder = new DbContextOptionsBuilder<TenantContext>();
                    optionsBuilder.UseSqlServer(
                        $"Server={tenant.ServerName};Database={tenant.DatabaseName};User Id={tenant.ServerUserId};Password={tenant.ServerPassword}");

                    var tenantCtx = new TenantContext(optionsBuilder.Options, _catalogCtx, _claim);
                    await tenantCtx.Database.MigrateAsync();
                }
            }
        }
    }
}
