using System;
using System.Threading.Tasks;
using ERP.Entity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ERP_API.Database;
using ERP_API.Utils;

namespace ERP_API.Domain.Services
{
    public interface IShardingService
    {
        //string DbPrefix { get; }
        //Task<Guid> CreateNewInstanceAsync(string tenantName);
        //Task<Guid> RegisterNewShardAsync(string connectionString, string tenantName);
        //Task ApplyDatabaseMigrationAsync(string connectionString, Guid shardingKey);
        Task ApplyMigrationAsync();
    }

    public class ShardingService : IShardingService
    {
        private readonly CatalogContext _controlCtx;
        private readonly IClaimService _claim;
        private readonly IConfiguration _configuration;

        //public string DbPrefix { get => "ErpInstance"; }

        public ShardingService(CatalogContext controlCtx, IClaimService claim, IConfiguration configuration)
        {
            _controlCtx = controlCtx;
            _claim = claim;
            _configuration = configuration;
        }

        //public async Task<Guid> CreateNewInstanceAsync(string tenantName)
        //{
        //    string connectionString = _configuration.GetConnectionString("controlConnectionString");
        //    Guid tenantShard = await RegisterNewShardAsync(connectionString, tenantName.Trim());

        //    return tenantShard;
        //}

        //public async Task<Guid> RegisterNewShardAsync(string connectionString, string tenantName)
        //{
        //    if (!DatabaseUtility.DatabaseExists(connectionString))
        //    {
        //        DatabaseUtility.CreateDatabase(connectionString);
        //    }

        //    Guid tenantShard = await GetOrCreateUnclaimedShardAsync(connectionString);

        //    _controlCtx.Tenants.Add(new Tenant { Name = tenantName, Initial = DatabaseUtility.GetTenantSlug(tenantName) });
        //    await _controlCtx.SaveChangesAsync();

        //    return tenantShard;
        //}

        //public async Task ApplyDatabaseMigrationAsync(string connectionString, Guid shardingKey)
        //{
        //    SqlConnectionStringBuilder connectionStringBuilder = new SqlConnectionStringBuilder(connectionString);
        //    connectionStringBuilder.InitialCatalog = $"{DbPrefix}_{shardingKey}";
        //    string shardConnectionString = connectionStringBuilder.ConnectionString;

        //    if (!DatabaseUtility.DatabaseExists(shardConnectionString))
        //    {
        //        DatabaseUtility.CreateDatabase(shardConnectionString);
        //    }

        //    var optionsBuilder = new DbContextOptionsBuilder<ERPDbContext>();
        //    optionsBuilder.UseSqlServer(connectionStringBuilder.ConnectionString);

        //    using (ERPDbContext dbContext = new ERPDbContext(optionsBuilder.Options, _configuration, shardingKey))
        //    {
        //        await dbContext.Database.MigrateAsync();
        //        DbInitializer dbInitializer = new DbInitializer();
        //        dbInitializer.EnsureSeeded(dbContext);
        //    }
        //}

        public async Task ApplyMigrationAsync()
        {
            foreach (var tenant in _controlCtx.Tenants)
            {
                if (!string.IsNullOrWhiteSpace(tenant.ServerName) || !string.IsNullOrWhiteSpace(tenant.DatabaseName) ||
                    !string.IsNullOrWhiteSpace(tenant.ServerUserId) || !string.IsNullOrWhiteSpace(tenant.ServerPassword))
                {
                    var optionsBuilder = new DbContextOptionsBuilder<TenantContext>();
                    optionsBuilder.UseSqlServer(
                        $"Server={tenant.ServerName};Database={tenant.DatabaseName};User Id={tenant.ServerUserId};Password={tenant.ServerPassword}");

                    var tenantContext = new TenantContext(optionsBuilder.Options, _controlCtx, _claim);
                    await tenantContext.Database.MigrateAsync();
                }
            }
        }
        
        //private async Task<Guid> GetOrCreateUnclaimedShardAsync(string connectionStrung)
        //{
        //    Guid tenantShard = Guid.NewGuid();
        //    _controlCtx.ShardTable.Add(new ShardTable() { Shard = tenantShard });

        //    await _controlCtx.SaveChangesAsync();

        //    return tenantShard;
        //}
    }
}
