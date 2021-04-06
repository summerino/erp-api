using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ERP_API.Domain.Entities.General;
using ERP_API.Domain.Entities.Inventory;
using ERP_API.Domain.Entities.Purchase;
using ERP_API.Domain.Services;
using ERP_API.Entities;

namespace ERP_API.Domain.Entities
{
    public class TenantContext : DbContext
    {
        private readonly ERPControlDbContext _controlCtx;
        private readonly IClaimService _claim;

        public TenantContext(DbContextOptions<TenantContext> options, ERPControlDbContext controlCtx, IClaimService claim)
            : base(options)
        {
            _controlCtx = controlCtx;
            _claim = claim;
        }

        // Core Entities
        public DbSet<BaseNewCodeEntity> NewCodes { get; set; }

        // General entities
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<SystemParameter> SystemParameters { get; set; }

        // Inventory entities
        public DbSet<Item> Items { get; set; }
        public DbSet<VwItem> VwItems { get; set; }
        public DbSet<ItemCategory> ItemCategories { get; set; }
        public DbSet<UoMConversion> UoMConversions { get; set; }

        // Purchase entities
        public DbSet<PurchaseOrderHeader> PurchaseOrderHeaders { get; set; }
        public DbSet<VwPurchaseOrderHeader> VwPurchaseOrderHeaders { get; set; }
        public DbSet<PurchaseOrderDetail> PurchaseOrderDetails { get; set; }
        public DbSet<VwPurchaseOrderDetail> VwPurchaseOrderDetails { get; set; }
        public DbSet<PurchaseReceiveHeader> PurchaseReceiveHeaders { get; set; }
        public DbSet<PurchaseReceiveDetail> PurchaseReceiveDetails { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var tenant =
                    _controlCtx.TenantDetails.SingleOrDefault(x => x.Shard == Guid.Parse(_claim.TenantShard));

                if (tenant != null &&
                    (!string.IsNullOrWhiteSpace(tenant.ServerName) || !string.IsNullOrWhiteSpace(tenant.DatabaseName) ||
                     !string.IsNullOrWhiteSpace(tenant.ServerUserId) || !string.IsNullOrWhiteSpace(tenant.ServerPassword)))
                {
                    optionsBuilder.UseSqlServer(
                        $"Server={tenant.ServerName};Database={tenant.DatabaseName};User Id={tenant.ServerUserId};Password={tenant.ServerPassword}");
                }
                else
                {
                    throw new Exception("Tenant doesn't exists.");
                }
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Core Entities
            modelBuilder.Entity<BaseNewCodeEntity>().HasNoKey();

            // Inventory entities
            modelBuilder.Entity<VwItem>().HasNoKey();

            // Purchase entities
            modelBuilder.Entity<VwPurchaseOrderHeader>().HasNoKey();

            modelBuilder.Entity<PurchaseOrderDetail>(entity =>
                entity.Property(e => e.Code)
                    .IsRequired()
            );

            modelBuilder.Entity<VwPurchaseOrderDetail>().HasNoKey();
        }
    }
}
