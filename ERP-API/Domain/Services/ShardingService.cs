using System;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ERP_API.Database;
using ERP_API.Entities.Control;
using ERP_API.Utils;

namespace ERP_API.Domain.Services
{
    public interface IShardingService
    {
        string DbPrefix { get; }
        //Task ApplyDatabaseMigrationAsync(string connectionString, Guid shardingKey);
        Task<Guid> CreateNewInstanceAsync(string tenantName);
        Task<Guid> RegisterNewShardAsync(string connectionString, string tenantName);
        Task ApplyDatabaseMigrationAsync(string connectionString, Guid shardingKey);
    }

    public class ShardingService : IShardingService
    {
        private readonly ERPControlDbContext _controlDbConext;
        private readonly IConfiguration _configuration;

        public string DbPrefix { get => "ErpInstance"; }

        public ShardingService(ERPControlDbContext controlDbContext, IConfiguration configuration)
        {
            _controlDbConext = controlDbContext;
            _configuration = configuration;
        }

        public async Task<Guid> CreateNewInstanceAsync(string tenantName)
        {
            string connectionString = _configuration.GetConnectionString("controlConnectionString");
            Guid tenantShard = await RegisterNewShardAsync(connectionString, tenantName.Trim());

            return tenantShard;
        }

        public async Task<Guid> RegisterNewShardAsync(string connectionString, string tenantName)
        {
            if (!DatabaseUtility.DatabaseExists(connectionString))
            {
                DatabaseUtility.CreateDatabase(connectionString);
            }

            Guid tenantShard = await GetOrCreateUnclaimedShardAsync(connectionString);

            _controlDbConext.TenantDetails.Add(new TenantDetails() { Shard = tenantShard, TenantName = tenantName, TenantSlug = DatabaseUtility.GetTenantSlug(tenantName) });
            await _controlDbConext.SaveChangesAsync();

            return tenantShard;
        }

        public async Task ApplyDatabaseMigrationAsync(string connectionString, Guid shardingKey)
        {
            SqlConnectionStringBuilder connectionStringBuilder = new SqlConnectionStringBuilder(connectionString);
            connectionStringBuilder.InitialCatalog = $"{DbPrefix}_{shardingKey}";
            string shardConnectionString = connectionStringBuilder.ConnectionString;

            if (!DatabaseUtility.DatabaseExists(shardConnectionString))
            {
                DatabaseUtility.CreateDatabase(shardConnectionString);
            }

            var optionsBuilder = new DbContextOptionsBuilder<ERPDbContext>();
            optionsBuilder.UseSqlServer(connectionStringBuilder.ConnectionString);

            using (ERPDbContext dbContext = new ERPDbContext(optionsBuilder.Options, _configuration, shardingKey))
            {
                await dbContext.Database.MigrateAsync();
                DbInitializer dbInitializer = new DbInitializer();
                dbInitializer.EnsureSeeded(dbContext);
            }
        }

            private async Task<Guid> GetOrCreateUnclaimedShardAsync(string connectionStrung)
        {
            Guid tenantShard = Guid.NewGuid();
            _controlDbConext.ShardTable.Add(new ShardTable() { Shard = tenantShard });

            await _controlDbConext.SaveChangesAsync();

            return tenantShard;
        }
    }
}
