using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ERP_API.Domain.Entities.Accounting;
using ERP_API.Domain.Entities.Core;
using ERP_API.Domain.Entities.General;
using ERP_API.Domain.Entities.Inventory;
using ERP_API.Domain.Entities.Purchase;
using ERP_API.Domain.Entities.Sales;
using ERP_API.Domain.Entities.SystemManagement;
using ERP_API.Domain.Services;

namespace ERP_API.Domain.Entities
{
    public class TenantContext : DbContext
    {
        private readonly CatalogContext _catalogCtx;
        private readonly IClaimService _claim;

        public TenantContext(DbContextOptions<TenantContext> options, CatalogContext catalogCtx, IClaimService claim)
            : base(options)
        {
            _catalogCtx = catalogCtx;
            _claim = claim;
        }

        // Core Entities
        public DbSet<BaseNewCodeEntity> NewCodes { get; set; }
        public DbSet<SequenceNumber> SequenceNumbers { get; set; }
        public DbSet<SystemParameter> SystemParameters { get; set; }

        // Accounting Entities
        public DbSet<Coa> Coas { get; set; }
        public DbSet<CoaType> CoaTypes { get; set; }
        public DbSet<CurrencyRate> CurrencyRates { get; set; }

        // General entities
        public DbSet<Currency> Currencies { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<VwCustomer> VwCustomers { get; set; }
        public DbSet<CustomerAddress> CustomerAddress { get; set; }
        public DbSet<CustomerType> CustomerTypes { get; set; }
        public DbSet<VwCustomerType> VwCustomerTypes { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<VwEmployee> VwEmployees { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<VwSupplier> VwSuppliers { get; set; }
        public DbSet<SupplierType> SupplierTypes { get; set; }
        public DbSet<VwSupplierType> VwSupplierTypes { get; set; }
        public DbSet<Tax> Taxes { get; set; }
        public DbSet<VwTax> VwTaxes { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<VwVehicle> VwVehicles { get; set; }
        public DbSet<VehicleType> VehicleTypes { get; set; }
        public DbSet<VwVehicleType> VwVehicleTypes { get; set; }

        // Inventory entities
        public DbSet<Item> Items { get; set; }
        public DbSet<VwItem> VwItems { get; set; }
        public DbSet<ItemCategory> ItemCategories { get; set; }
        public DbSet<ItemGroup> ItemGroups { get; set; }
        public DbSet<ItemGroupSubGroup> ItemGroupSubGroups { get; set; }
        public DbSet<StockMutation> StockMutations { get; set; }
        public DbSet<UoM> UoMs { get; set; }
        public DbSet<UoMConversion> UoMConversions { get; set; }
        public DbSet<Warehouse> Warehouses { get; set; }
        public DbSet<VwWarehouse> VwWarehouses { get; set; }
        public DbSet<AdjustmentHeader> AdjustmentHeaders { get; set; }
        public DbSet<VwAdjustmentHeader> VwAdjustmentHeaders { get; set; }
        public DbSet<AdjustmentDetail> AdjustmentDetails { get; set; }
        public DbSet<VwAdjustmentDetail> VwAdjustmentDetails { get; set; }
        public DbSet<VwAdjustmentItem> VwAdjustmentItems { get; set; }
        public DbSet<WarehouseQuantity> WarehouseQuantities { get; set; }

        // Purchase entities
        public DbSet<DebitMemo> DebitMemos { get; set; }
        public DbSet<VwDebitMemo> VwDebitMemos { get; set; }
        public DbSet<PurchaseInvoiceHeader> PurchaseInvoiceHeaders { get; set; }
        public DbSet<VwPurchaseInvoiceHeader> VwPurchaseInvoiceHeaders { get; set; }
        public DbSet<PurchaseInvoiceDetail> PurchaseInvoiceDetails { get; set; }
        public DbSet<PurchaseOrderHeader> PurchaseOrderHeaders { get; set; }
        public DbSet<VwPurchaseOrderHeader> VwPurchaseOrderHeaders { get; set; }
        public DbSet<PurchaseOrderDetail> PurchaseOrderDetails { get; set; }
        public DbSet<VwPurchaseOrderDetail> VwPurchaseOrderDetails { get; set; }
        public DbSet<PurchaseReceiveHeader> PurchaseReceiveHeaders { get; set; }
        public DbSet<VwPurchaseReceiveHeader> VwPurchaseReceiveHeaders { get; set; }
        public DbSet<PurchaseReceiveDetail> PurchaseReceiveDetails { get; set; }
        public DbSet<VwPurchaseReceiveDetail> VwPurchaseReceiveDetails { get; set; }
        public DbSet<PurchaseReturnHeader> PurchaseReturnHeaders { get; set; }
        public DbSet<VwPurchaseReturnHeader> VwPurchaseReturnHeaders { get; set; }
        public DbSet<PurchaseReturnDetail> PurchaseReturnDetails { get; set; }
        public DbSet<VwPurchaseReturnDetail> VwPurchaseReturnDetails { get; set; }
        public DbSet<PurchaseReturnDetailExchDiffItem> PurchaseReturnDetailExchDiffItems { get; set; }
        public DbSet<VwPurchaseReturnDetailExchDiffItem> VwPurchaseReturnDetailExchDiffItems { get; set; }

        // Sales entities
        public DbSet<Area> Areas { get; set; }
        public DbSet<CreditMemo> CreditMemos { get; set; }
        public DbSet<VwCreditMemo> VwCreditMemos { get; set; }
        public DbSet<SalesDeliveryHeader> SalesDeliveryHeaders { get; set; }
        public DbSet<VwSalesDeliveryHeader> VwSalesDeliveryHeaders { get; set; }
        public DbSet<SalesDeliveryDetail> SalesDeliveryDetails { get; set; }
        public DbSet<VwSalesDeliveryDetail> VwSalesDeliveryDetails { get; set; }
        public DbSet<SalesInvoiceHeader> SalesInvoiceHeaders { get; set; }
        public DbSet<VwSalesInvoiceHeader> VwSalesInvoiceHeaders { get; set; }
        public DbSet<SalesInvoiceDetail> SalesInvoiceDetails { get; set; }
        public DbSet<SalesOrderHeader> SalesOrderHeaders { get; set; }
        public DbSet<VwSalesOrderHeader> VwSalesOrderHeaders { get; set; }
        public DbSet<SalesOrderDetail> SalesOrderDetails { get; set; }
        public DbSet<VwSalesOrderDetail> VwSalesOrderDetails { get; set; }
        public DbSet<SalesReturnHeader> SalesReturnHeaders { get; set; }
        public DbSet<VwSalesReturnHeader> VwSalesReturnHeaders { get; set; }
        public DbSet<SalesReturnDetail> SalesReturnDetails { get; set; }
        public DbSet<VwSalesReturnDetail> VwSalesReturnDetails { get; set; }
        public DbSet<SalesReturnDetailExchDiffItem> SalesReturnDetailExchDiffItems { get; set; }
        public DbSet<VwSalesReturnDetailExchDiffItem> VwSalesReturnDetailExchDiffItems { get; set; }

        // System Management
        public DbSet<Menu> Menus { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<VwUser> VwUsers { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var tenant =
                    _catalogCtx.Tenants.SingleOrDefault(x => x.Id == _claim.TenantId);

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
            // Set database collation
            modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");

            // Force all string-based columns to non-unicode equivalent when no column type is explicitly set
            foreach (
                var property in modelBuilder.Model
                    .GetEntityTypes()
                    .SelectMany(t => t.GetProperties())
                    .Where(p => p.ClrType == typeof(string) &&  // Entity is a string
                                p.GetColumnType() == null &&    // No column type is set
                                !p.DeclaringEntityType.GetTableName().StartsWith("AspNet")))
            {
                property.SetIsUnicode(false);
            }

            // Core entities
            modelBuilder.Entity<BaseNewCodeEntity>()
                .HasNoKey()
                .ToTable("BaseNewCodeEntity", t => t.ExcludeFromMigrations());

            // General entities
            modelBuilder.Entity<VwCustomer>()
                .HasNoKey()
                .ToView("vwCustomer", Schema.General);

            modelBuilder.Entity<CustomerAddress>(entity =>
                entity.Property(e => e.Code)
                    .IsRequired()
            );

            modelBuilder.Entity<VwCustomerType>()
                .HasNoKey()
                .ToView("vwCustomerType", Schema.General);

            modelBuilder.Entity<VwEmployee>()
                .HasNoKey()
                .ToView("vwEmployee", Schema.General);

            modelBuilder.Entity<VwSupplier>()
                .HasNoKey()
                .ToView("vwSupplier", Schema.General);

            modelBuilder.Entity<VwSupplierType>()
                .HasNoKey()
                .ToView("vwSupplierType", Schema.General);

            modelBuilder.Entity<VwTax>()
                .HasNoKey()
                .ToView("vwTax", Schema.General);

            modelBuilder.Entity<VwVehicle>()
                .HasNoKey()
                .ToView("vwVehicle", Schema.General);

            modelBuilder.Entity<VwVehicleType>()
                .HasNoKey()
                .ToView("vwVehicleType", Schema.General);

            // Inventory entities
            modelBuilder.Entity<VwItem>()
                .HasNoKey()
                .ToView("vwItem", Schema.Inventory);

            modelBuilder.Entity<VwWarehouse>()
                .HasNoKey()
                .ToView("vwWarehouse", Schema.Inventory);


            // Adjustment entities
            modelBuilder.Entity<AdjustmentHeader>(entity =>
                entity.Property(e => e.Mark)
                    .IsRequired()
            );

            modelBuilder.Entity<VwAdjustmentHeader>()
                .HasNoKey()
                .ToView("VwAdjustmentHeader", Schema.Inventory);

            modelBuilder.Entity<AdjustmentDetail>(entity =>
                entity.Property(e => e.Code)
                    .IsRequired()
            );

            modelBuilder.Entity<VwAdjustmentDetail>()
                .HasNoKey()
                .ToView("VwAdjustmentDetail", Schema.Inventory);

            modelBuilder.Entity<VwAdjustmentItem>()
                .HasNoKey()
                .ToView("VwAdjustmentItem", Schema.Inventory);
            // Debit Memo entities
            modelBuilder.Entity<DebitMemo>(entity =>
                entity.Property(e => e.Mark)
                    .IsRequired()
            );

            modelBuilder.Entity<VwDebitMemo>()
                .HasNoKey()
                .ToView("vwDebitMemo", Schema.Purchasing);

            // Purchase Invoice entities
            modelBuilder.Entity<PurchaseInvoiceHeader>(entity =>
                entity.Property(e => e.Mark)
                    .IsRequired()
            );

            modelBuilder.Entity<VwPurchaseInvoiceHeader>()
                .HasNoKey()
                .ToView("vwPurchaseInvoiceHeader", Schema.Purchasing);

            modelBuilder.Entity<PurchaseInvoiceDetail>(entity =>
                entity.Property(e => e.Code)
                    .IsRequired()
            );

            // Purchase Order entities
            modelBuilder.Entity<PurchaseOrderHeader>(entity =>
                entity.Property(e => e.Mark)
                    .IsRequired()
            );

            modelBuilder.Entity<VwPurchaseOrderHeader>()
                .HasNoKey()
                .ToView("vwPurchaseOrderHeader", Schema.Purchasing);

            modelBuilder.Entity<PurchaseOrderDetail>(entity =>
                entity.Property(e => e.Code)
                    .IsRequired()
            );

            modelBuilder.Entity<VwPurchaseOrderDetail>()
                .HasNoKey()
                .ToView("vwPurchaseOrderDetail", Schema.Purchasing);
            
            // Purchase Receive entities
            modelBuilder.Entity<PurchaseReceiveHeader>(entity =>
                entity.Property(e => e.Mark)
                    .IsRequired()
            );

            modelBuilder.Entity<VwPurchaseReceiveHeader>()
                .HasNoKey()
                .ToView("vwPurchaseReceiveHeader", Schema.Purchasing);

            modelBuilder.Entity<PurchaseReceiveDetail>(entity =>
                entity.Property(e => e.Code)
                    .IsRequired()
            );

            modelBuilder.Entity<VwPurchaseReceiveDetail>()
                .HasNoKey()
                .ToView("vwPurchaseReceiveDetail", Schema.Purchasing);

            // Purchase Return entities
            modelBuilder.Entity<PurchaseReturnHeader>(entity =>
                entity.Property(e => e.Mark)
                    .IsRequired()
            );

            modelBuilder.Entity<VwPurchaseReturnHeader>()
                .HasNoKey()
                .ToView("vwPurchaseReturnHeader", Schema.Purchasing);

            modelBuilder.Entity<PurchaseReturnDetail>(entity =>
                entity.Property(e => e.Code)
                    .IsRequired()
            );

            modelBuilder.Entity<PurchaseReturnDetailExchDiffItem>(entity =>
                entity.Property(e => e.Code)
                    .IsRequired()
            );
            modelBuilder.Entity<VwPurchaseReturnDetailExchDiffItem>()
                .HasNoKey()
                .ToView("VwPurchaseReturnDetailExchDiffItem", Schema.Purchasing);

            modelBuilder.Entity<VwPurchaseReturnDetail>()
                .HasNoKey()
                .ToView("vwPurchaseReturnDetail", Schema.Purchasing);

            // Credit Memo entities
            modelBuilder.Entity<CreditMemo>(entity =>
                entity.Property(e => e.Mark)
                    .IsRequired()
            );

            modelBuilder.Entity<VwCreditMemo>()
                .HasNoKey()
                .ToView("VwCreditMemo", Schema.Sales);

            // Sales Delivery entities

            modelBuilder.Entity<SalesDeliveryHeader>(entity =>
                entity.Property(e => e.Mark)
                    .IsRequired()
            );

            modelBuilder.Entity<VwSalesDeliveryHeader>()
                .HasNoKey()
                .ToView("vwSalesDeliveryHeader", Schema.Sales);

            modelBuilder.Entity<SalesDeliveryDetail>(entity =>
                entity.Property(e => e.Code)
                    .IsRequired()
            );

            modelBuilder.Entity<VwSalesDeliveryDetail>()
                .HasNoKey()
                .ToView("vwSalesDeliveryDetail", Schema.Sales);

            // Sales Invoice entities
            modelBuilder.Entity<SalesInvoiceHeader>(entity =>
                entity.Property(e => e.Mark)
                    .IsRequired()
            );

            modelBuilder.Entity<VwSalesInvoiceHeader>()
                .HasNoKey()
                .ToView("vwSalesInvoiceHeader", Schema.Sales);

            modelBuilder.Entity<SalesInvoiceDetail>(entity =>
                entity.Property(e => e.Code)
                    .IsRequired()
            );

            // Sales Order entities
            modelBuilder.Entity<SalesOrderHeader>(entity =>
                entity.Property(e => e.Mark)
                    .IsRequired()
            );

            modelBuilder.Entity<VwSalesOrderHeader>()
                .HasNoKey()
                .ToView("vwSalesOrderHeader", Schema.Sales);

            modelBuilder.Entity<SalesOrderDetail>(entity =>
                entity.Property(e => e.Code)
                    .IsRequired()
            );

            modelBuilder.Entity<VwSalesOrderDetail>()
                .HasNoKey()
                .ToView("vwSalesOrderDetail", Schema.Sales);

            // Sales Return entities
            modelBuilder.Entity<SalesReturnHeader>(entity =>
                entity.Property(e => e.Mark)
                    .IsRequired()
            );

            modelBuilder.Entity<VwSalesReturnHeader>()
                .HasNoKey()
                .ToView("VwSalesReturnHeader", Schema.Sales);

            modelBuilder.Entity<SalesReturnDetail>(entity =>
                entity.Property(e => e.Code)
                    .IsRequired()
            );

            modelBuilder.Entity<VwSalesReturnDetail>()
                .HasNoKey()
                .ToView("VwSalesReturnDetail", Schema.Sales);

            modelBuilder.Entity<VwSalesReturnDetailExchDiffItem>()
                .HasNoKey()
                .ToView("VwSalesReturnDetailExchDiffItem", Schema.Sales);
            
            modelBuilder.Entity<SalesReturnDetailExchDiffItem>(entity =>
                entity.Property(e => e.Code)
                    .IsRequired()
            );

            // System Management entities
            modelBuilder.Entity<VwUser>()
                .HasNoKey()
                .ToView("vwUser", Schema.SystemManagement);
        }
    }
}
