using System;
using ERP_API.Domain.Entities.General;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ERP_API
{
    public class ERPDbContext : DbContext
    {
        private readonly IConfiguration _configuration;
        public Guid Shardingkey { get; set; }

        public ERPDbContext(DbContextOptions options, IConfiguration configuration, Guid shardingKey) : base(options) 
        {
            Shardingkey = shardingKey;
            _configuration = configuration;
        }

        public ERPDbContext(DbContextOptions options, IConfiguration configuration) : base(options)
        {
            _configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (Shardingkey != null || Shardingkey != Guid.Empty)
            {
                var connectionStringBuilder = new SqlConnectionStringBuilder(_configuration.GetConnectionString("tenantConnectionString"))
                {
                    InitialCatalog = "ErpInstance_" + Shardingkey,
                };

                var connectionString = connectionStringBuilder.ToString();

                optionsBuilder.UseSqlServer(
                    connectionString,
                    (options) =>
                    {
                        options.CommandTimeout(180);
                        options.EnableRetryOnFailure();
                    });
            }
        }

        public DbSet<Supplier> SupplierTable { get; set; }
    }
}
