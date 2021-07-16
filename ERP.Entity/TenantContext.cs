using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ERP.Entity.Accounting;
using ERP.Entity.AssetManagement;
using ERP.Entity.Core;
using ERP.Entity.Expedition;
using ERP.Entity.Finance;
using ERP.Entity.General;
using ERP.Entity.Inventory;
using ERP.Entity.Purchase;
using ERP.Entity.Sales;
using ERP.Entity.SystemManagement;

namespace ERP.Entity
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

        // Accounting Entities
        public DbSet<BeginningBalanceAP> BeginningBalanceAPs { get; set; }
        public DbSet<VwBeginningBalanceAP> VwBeginningBalanceAPs { get; set; }
        public DbSet<BeginningBalanceAR> BeginningBalanceARs { get; set; }
        public DbSet<VwBeginningBalanceAR> VwBeginningBalanceARs { get; set; }
        public DbSet<BeginningBalanceCreditMemo> BeginningBalanceCreditMemos { get; set; }
        public DbSet<BeginningBalanceDebitMemo> BeginningBalanceDebitMemos { get; set; }
        public DbSet<ClosingMonth> ClosingMonths { get; set; }
        public DbSet<Coa> Coas { get; set; }
        public DbSet<VwCoa> VwCoas { get; set; }
        public DbSet<CoaType> CoaTypes { get; set; }
        public DbSet<CurrencyRate> CurrencyRates { get; set; }
        public DbSet<GeneralJournalHeader> GeneralJournalHeaders { get; set; }
        public DbSet<GeneralJournalDetail> GeneralJournalDetails { get; set; }
        public DbSet<VwGeneralJournalHeader> VwGeneralJournalHeaders { get; set; }
        public DbSet<Journal> Journals { get; set; }

        // Asset Management Entities
        public DbSet<FixedAsset> FixedAssets { get; set; }
        public DbSet<FixedAssetDepartment> FixedAssetDepartments { get; set; }
        public DbSet<FixedAssetHistory> FixedAssetHistories { get; set; }
        public DbSet<AssetType> AssetTypes { get; set; }
        public DbSet<VwFixedAsset> VwFixedAssets { get; set; }

        // Expedition entities
        public DbSet<ExpeditionInvoiceHeader> ExpeditionInvoiceHeaders { get; set; }
        public DbSet<VwExpeditionInvoiceHeader> VwExpeditionInvoiceHeaders { get; set; }
        public DbSet<ExpeditionInvoiceDetail> ExpeditionInvoiceDetails { get; set; }

        // Finance entities
        public DbSet<GeneralCashBankHeader> GeneralCashBankHeaders { get; set; }
        public DbSet<GeneralCashBankDetail> GeneralCashBankDetails { get; set; }
        public DbSet<VwGeneralCashBankHeader> VwGeneralCashBankHeaders { get; set; }
        public DbSet<VwGeneralCashBankDetail> VwGeneralCashBankDetails { get; set; }
        public DbSet<CashBankType> CashBankTypes { get; set; }
        public DbSet<VwAP> VwAPs { get; set; }
        public DbSet<VwAR> VwARs { get; set; }
        public DbSet<VwSR> VwSRs { get; set; }
        public DbSet<VwPR> VwPRs { get; set; }
        public DbSet<VwInterCashBankHeader> VwInterCashBankHeaders { get; set; }

        // General entities
        public DbSet<Currency> Currencies { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<VwCustomer> VwCustomers { get; set; }
        public DbSet<CustomerAddress> CustomerAddress { get; set; }
        public DbSet<CustomerType> CustomerTypes { get; set; }
        public DbSet<VwCustomerType> VwCustomerTypes { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<VwEmployee> VwEmployees { get; set; }
        public DbSet<PaymentTerm> PaymentTerms { get; set; }
        public DbSet<VwPaymentTerm> VwPaymentTerms { get; set; }
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
        public DbSet<VwApproval> VwApprovals { get; set; }

        // Inventory entities
        public DbSet<AdjustmentHeader> AdjustmentHeaders { get; set; }
        public DbSet<VwAdjustmentHeader> VwAdjustmentHeaders { get; set; }
        public DbSet<AdjustmentDetail> AdjustmentDetails { get; set; }
        public DbSet<VwAdjustmentDetail> VwAdjustmentDetails { get; set; }
        public DbSet<AdjustmentDetailDiffUnit> AdjustmentDetailDiffUnits { get; set; }
        public DbSet<VwAdjustmentItem> VwAdjustmentItems { get; set; }
        public DbSet<BeginningBalanceStockHeader> BeginningBalanceStockHeaders { get; set; }
        public DbSet<BeginningBalanceStockDetail> BeginningBalanceStockDetails { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<VwItem> VwItems { get; set; }
        public DbSet<ItemCategory> ItemCategories { get; set; }
        public DbSet<VwItemCategory> VwItemCategories { get; set; }
        public DbSet<ItemGroup> ItemGroups { get; set; }
        public DbSet<VwItemGroup> VwItemGroups { get; set; }
        public DbSet<ItemGroupSubGroup> ItemGroupSubGroups { get; set; }
        public DbSet<StockMutation> StockMutations { get; set; }
        public DbSet<TransferStockHeader> TransferStockHeaders { get; set; }
        public DbSet<VwTransferStockHeader> VwTransferStockHeaders { get; set; }
        public DbSet<TransferStockDetail> TransferStockDetails { get; set; }
        public DbSet<VwTransferStockDetail> VwTransferStockDetails { get; set; }
        public DbSet<UoM> UoMs { get; set; }
        public DbSet<VwUoM> VwUoMs { get; set; }
        public DbSet<UoMConversion> UoMConversions { get; set; }
        public DbSet<Warehouse> Warehouses { get; set; }
        public DbSet<VwWarehouse> VwWarehouses { get; set; }
        public DbSet<WarehouseQuantity> WarehouseQuantities { get; set; }
        public DbSet<VwWarehouseQuantity> VwWarehouseQuantities { get; set; }

        // Purchase entities
        public DbSet<DebitMemo> DebitMemos { get; set; }
        public DbSet<VwDebitMemo> VwDebitMemos { get; set; }
        public DbSet<PurchaseInvoiceHeader> PurchaseInvoiceHeaders { get; set; }
        public DbSet<VwPurchaseInvoiceHeader> VwPurchaseInvoiceHeaders { get; set; }
        public DbSet<PurchaseInvoiceDetail> PurchaseInvoiceDetails { get; set; }
        public DbSet<PurchaseInvoiceDebitMemo> PurchaseInvoiceDebitMemos { get; set; }
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
        public DbSet<VwArea> VwAreas { get; set; }
        public DbSet<CreditMemo> CreditMemos { get; set; }
        public DbSet<VwCreditMemo> VwCreditMemos { get; set; }
        public DbSet<DeliveryPlanHeader> DeliveryPlanHeaders { get; set; }
        public DbSet<VwDeliveryPlanHeader> VwDeliveryPlanHeaders { get; set; }
        public DbSet<DeliveryPlanDetail> DeliveryPlanDetails { get; set; }
        public DbSet<DeliveryPlanUndeliveredItem> DeliveryPlanUndeliveredItems { get; set; }
        public DbSet<PromoHeader> PromoHeaders { get; set; }
        public DbSet<VwPromoHeader> VwPromoHeaders { get; set; }
        public DbSet<PromoDetail> PromoDetails { get; set; }
        public DbSet<PromoDetailTier> PromoDetailTiers { get; set; }
        public DbSet<SalesDeliveryHeader> SalesDeliveryHeaders { get; set; }
        public DbSet<VwSalesDeliveryHeader> VwSalesDeliveryHeaders { get; set; }
        public DbSet<SalesDeliveryDetail> SalesDeliveryDetails { get; set; }
        public DbSet<VwSalesDeliveryDetail> VwSalesDeliveryDetails { get; set; }
        public DbSet<SalesDeliveryDetailFreeGood> SalesDeliveryDetailFreeGoods { get; set; }
        public DbSet<SalesInvoiceHeader> SalesInvoiceHeaders { get; set; }
        public DbSet<VwSalesInvoiceHeader> VwSalesInvoiceHeaders { get; set; }
        public DbSet<SalesInvoiceDetail> SalesInvoiceDetails { get; set; }
        public DbSet<SalesInvoiceCreditMemo> SalesInvoiceCreditMemos { get; set; }
        public DbSet<SalesmanGroup> SalesmanGroups { get; set; }
        public DbSet<VwSalesmanGroup> VwSalesmanGroups { get; set; }
        public DbSet<SalesmanSchedule> SalesmanSchedules { get; set; }
        public DbSet<SalesmanScheduleCustomer> SalesmanScheduleCustomers { get; set; }
        public DbSet<VwSalesmanSchedule> VwSalesmanSchedules { get; set; }
        public DbSet<VwSalesmanScheduleCustomer> VwSalesmanScheduleCustomers { get; set; }
        public DbSet<SalesOrderHeader> SalesOrderHeaders { get; set; }
        public DbSet<VwSalesOrderHeader> VwSalesOrderHeaders { get; set; }
        public DbSet<SalesOrderDetail> SalesOrderDetails { get; set; }
        public DbSet<VwSalesOrderDetail> VwSalesOrderDetails { get; set; }
        public DbSet<SalesOrderDetailDiscount> SalesOrderDetailDiscounts { get; set; }
        public DbSet<SalesOrderDetailFreeGood> SalesOrderDetailFreeGoods { get; set; }
        public DbSet<SalesReturnHeader> SalesReturnHeaders { get; set; }
        public DbSet<VwSalesReturnHeader> VwSalesReturnHeaders { get; set; }
        public DbSet<SalesReturnDetail> SalesReturnDetails { get; set; }
        public DbSet<VwSalesReturnDetail> VwSalesReturnDetails { get; set; }
        public DbSet<SalesReturnDetailExchDiffItem> SalesReturnDetailExchDiffItems { get; set; }
        public DbSet<VwSalesReturnDetailExchDiffItem> VwSalesReturnDetailExchDiffItems { get; set; }
        public DbSet<VisitOrder> VisitOrders { get; set; }
        public DbSet<VisitOrderCustomer> VisitOrderCustomers { get; set; }
        public DbSet<VisitOrderInvoice> VisitOrderInvoices { get; set; }
        public DbSet<VwVisitOrder> VwVisitOrders { get; set; }
        public DbSet<VwVisitOrderCustomer> VwVisitOrderCustomers { get; set; }
        public DbSet<VwVisitOrderInvoice> VwVisitOrderInvoices { get; set; }
        public DbSet<VisitPlanHeader> VisitPlanHeaders { get; set; }
        public DbSet<VisitPlanDetail> VisitPlanDetails { get; set; }
        public DbSet<VisitPlanDetailCustomer> VisitPlanDetailCustomers { get; set; }
        public DbSet<VwVisitPlanHeader> VwVisitPlanHeaders { get; set; }
        public DbSet<VwVisitPlanDetail> VwVisitPlanDetails { get; set; }
        public DbSet<VwVisitPlanDetailCustomer> VwVisitPlanDetailCustomers { get; set; }

        // System Management Entities
        public DbSet<SystemManagement.Action> Actions { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<VwCompany> VwCompanies { get; set; }
        public DbSet<Menu> Menus { get; set; }
        public DbSet<MenuAction> MenuActions { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<VwRole> VwRoles { get; set; }
        public DbSet<RoleMenu> RoleMenus { get; set; }
        public DbSet<RoleMenuAction> RoleMenuActions { get; set; }
        public DbSet<SequenceNumber> SequenceNumbers { get; set; }
        public DbSet<SystemParameter> SystemParameters { get; set; }
        public DbSet<SystemParameterModule> SystemParameterModules { get; set; }
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

            // Disable conventions--warning: this uses internal code and may break on any EF release
            //((Microsoft.EntityFrameworkCore.Metadata.Internal.Model)modelBuilder.Model).ConventionDispatcher.StartBatch();

            //foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            //{
            //    foreach (var index in entityType.GetIndexes().ToList())
            //    {
            //        // Logic to decide whether to remove the index here...
            //        entityType.RemoveIndex(index.Properties);
            //    }
            //}

            // Core entities
            modelBuilder.Entity<BaseNewCodeEntity>()
                .HasNoKey()
                .ToTable("BaseNewCodeEntity", t => t.ExcludeFromMigrations());

            // Accounting entities
            // Beginning Balance AP entities
            modelBuilder.Entity<BeginningBalanceAP>(entity =>
            {
                entity.HasOne<Supplier>()
                    .WithMany()
                    .HasForeignKey(d => d.SupCode)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne<Currency>()
                    .WithMany()
                    .HasForeignKey(d => d.CurrCode)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<VwBeginningBalanceAP>()
                .HasNoKey()
                .ToView("vwBeginningBalanceAP", Schema.Accounting);

            // Beginning Balance AR entities
            modelBuilder.Entity<BeginningBalanceAR>(entity =>
            {
                entity.HasOne<Customer>()
                    .WithMany()
                    .HasForeignKey(d => d.CustCode)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne<Currency>()
                    .WithMany()
                    .HasForeignKey(d => d.CurrCode)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<VwBeginningBalanceAR>()
                .HasNoKey()
                .ToView("vwBeginningBalanceAR", Schema.Accounting);

            // Beginning Balance Credit Memo entities
            modelBuilder.Entity<BeginningBalanceCreditMemo>(entity =>
            {
                entity.HasOne<Supplier>()
                    .WithMany()
                    .HasForeignKey(d => d.CustCode)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne<Currency>()
                    .WithMany()
                    .HasForeignKey(d => d.CurrCode)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // Beginning Balance Debit Memo entities
            modelBuilder.Entity<BeginningBalanceDebitMemo>(entity =>
            {
                entity.HasOne<Customer>()
                    .WithMany()
                    .HasForeignKey(d => d.SupCode)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne<Currency>()
                    .WithMany()
                    .HasForeignKey(d => d.CurrCode)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // COA entities
            modelBuilder.Entity<Coa>(entity =>
            {
                entity.HasOne<CoaType>()
                    .WithMany()
                    .HasForeignKey(d => d.TypeId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne<Currency>()
                    .WithMany()
                    .HasForeignKey(d => d.CurrCode)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<VwCoa>()
                .HasNoKey()
                .ToView("vwCoa", Schema.Accounting);

            // General Journal entities
            modelBuilder.Entity<GeneralJournalHeader>(entity =>
            {
                entity.Property(e => e.Mark)
                    .IsRequired();

                entity.HasOne<Currency>()
                    .WithMany()
                    .HasForeignKey(d => d.CurrCode)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<VwGeneralJournalHeader>()
                .HasNoKey()
                .ToView("vwGeneralJournalHeader", Schema.Accounting);

            modelBuilder.Entity<GeneralJournalDetail>(entity =>
            {
                entity.Property(e => e.Code)
                    .IsRequired();
            });

            // Journal entities
            modelBuilder.Entity<Journal>(entity =>
                entity.HasOne<Currency>()
                    .WithMany()
                    .HasForeignKey(d => d.CurrCode)
                    .OnDelete(DeleteBehavior.NoAction)
            );

            // Asset Management entities
            // Asset Type entities
            modelBuilder.Entity<VwAssetType>()
                .HasNoKey()
                .ToView("vwAssetType", Schema.AssetManagement);

            // Fixed Asset entities
            modelBuilder.Entity<FixedAsset>(entity =>
            {
                entity.Property(e => e.Mark)
                    .IsRequired();

                entity.HasOne<AssetType>()
                    .WithMany()
                    .HasForeignKey(d => d.TypeId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne<Supplier>()
                    .WithMany()
                    .HasForeignKey(d => d.SupCode)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<VwFixedAsset>()
                .HasNoKey()
                .ToView("vwFixedAsset", Schema.AssetManagement);

            modelBuilder.Entity<FixedAssetDepartment>(entity =>
            {
                entity.Property(e => e.Code)
                    .IsRequired();

                entity.HasOne<FixedAsset>()
                    .WithMany()
                    .HasForeignKey(d => d.Code)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<FixedAssetHistory>(entity =>
            {
                entity.Property(e => e.Code)
                    .IsRequired();

                entity.HasOne<FixedAsset>()
                    .WithMany()
                    .HasForeignKey(d => d.Code)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // Expedition entities
            modelBuilder.Entity<ExpeditionInvoiceHeader>(entity =>
            {
                entity.Property(e => e.Mark)
                    .IsRequired();

                entity.HasOne<Supplier>()
                    .WithMany()
                    .HasForeignKey(d => d.SupCode)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<VwExpeditionInvoiceHeader>()
                .HasNoKey()
                .ToView("vwExpeditionInvoiceHeader", Schema.Expedition);

            modelBuilder.Entity<ExpeditionInvoiceDetail>(entity =>
            {
                entity.Property(e => e.Code)
                    .IsRequired();

                entity.HasOne<ExpeditionInvoiceHeader>()
                    .WithMany()
                    .HasForeignKey(d => d.Code)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // Finance entities
            // General Cash Bank entities
            modelBuilder.Entity<GeneralCashBankHeader>(entity =>
            {
                entity.Property(e => e.Mark)
                    .IsRequired();

                entity.HasOne<Currency>()
                    .WithMany()
                    .HasForeignKey(d => d.CurrCode)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<VwGeneralCashBankHeader>()
                .HasNoKey()
                .ToView("vwGeneralCashBankHeader", Schema.Finance);

            modelBuilder.Entity<VwAP>()
                .HasNoKey()
                .ToView("vwAP", Schema.Finance);

            modelBuilder.Entity<VwAR>()
                .HasNoKey()
                .ToView("vwAR", Schema.Finance);

            modelBuilder.Entity<VwPR>()
                .HasNoKey()
                .ToView("VwPR", Schema.Finance);

            modelBuilder.Entity<VwSR>()
                .HasNoKey()
                .ToView("VwSR", Schema.Finance);

            modelBuilder.Entity<VwInterCashBankHeader>()
                .HasNoKey()
                .ToView("vwInterCashBankHeader", Schema.Finance);

            modelBuilder.Entity<GeneralCashBankDetail>(entity =>
            {
                entity.Property(e => e.Code)
                    .IsRequired();

                entity.HasOne<GeneralCashBankHeader>()
                    .WithMany()
                    .HasForeignKey(d => d.Code)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne<CashBankType>()
                    .WithMany()
                    .HasForeignKey(d => d.Type)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne<Currency>()
                    .WithMany()
                    .HasForeignKey(d => d.CurrCode)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<VwGeneralCashBankDetail>()
                .HasNoKey()
                .ToView("vwGeneralCashBankDetail", Schema.Finance);

            // General entities
            // Employee entities
            modelBuilder.Entity<Employee>(entity =>
            {
                entity.HasOne<SalesmanGroup>()
                    .WithMany()
                    .HasForeignKey(d => d.SalesGroupId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne<Warehouse>()
                    .WithMany()
                    .HasForeignKey(d => d.WarehouseCode)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<VwEmployee>()
                .HasNoKey()
                .ToView("vwEmployee", Schema.General);

            // Payment Term entities
            modelBuilder.Entity<VwPaymentTerm>()
                .HasNoKey()
                .ToView("vwPaymentTerm", Schema.General);

            // Customer entities
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasOne<CustomerType>()
                    .WithMany()
                    .HasForeignKey(d => d.TypeId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne<PaymentTerm>()
                    .WithMany()
                    .HasForeignKey(d => d.PaymentTermId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne<CustomerAddress>()
                    .WithMany()
                    .HasForeignKey(d => d.BillingAddressId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne<CustomerAddress>()
                    .WithMany()
                    .HasForeignKey(d => d.ShippingAddressId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne<Area>()
                    .WithMany()
                    .HasForeignKey(d => d.AreaId1)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne<Area>()
                    .WithMany()
                    .HasForeignKey(d => d.AreaId2)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne<Area>()
                    .WithMany()
                    .HasForeignKey(d => d.AreaId3)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne<Area>()
                    .WithMany()
                    .HasForeignKey(d => d.AreaId4)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne<Area>()
                    .WithMany()
                    .HasForeignKey(d => d.AreaId5)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<VwCustomer>()
                .HasNoKey()
                .ToView("vwCustomer", Schema.General);

            modelBuilder.Entity<CustomerAddress>(entity =>
            {
                entity.Property(e => e.Code)
                    .IsRequired();

                entity.HasOne<Customer>()
                    .WithMany()
                    .HasForeignKey(d => d.Code)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<VwCustomerType>()
                .HasNoKey()
                .ToView("vwCustomerType", Schema.General);

            // Supplier entities
            modelBuilder.Entity<Supplier>(entity =>
                entity.HasOne<SupplierType>()
                    .WithMany()
                    .HasForeignKey(d => d.TypeId)
                    .OnDelete(DeleteBehavior.NoAction)
            );

            modelBuilder.Entity<VwSupplier>()
                .HasNoKey()
                .ToView("vwSupplier", Schema.General);

            modelBuilder.Entity<VwSupplierType>()
                .HasNoKey()
                .ToView("vwSupplierType", Schema.General);

            modelBuilder.Entity<VwTax>()
                .HasNoKey()
                .ToView("vwTax", Schema.General);

            // Vehicle entities
            modelBuilder.Entity<Vehicle>(entity =>
            {
                entity.HasOne<VehicleType>()
                    .WithMany()
                    .HasForeignKey(d => d.TypeId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne<Employee>()
                    .WithMany()
                    .HasForeignKey(d => d.DriverId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<VwVehicle>()
                .HasNoKey()
                .ToView("vwVehicle", Schema.General);

            modelBuilder.Entity<VwVehicleType>()
                .HasNoKey()
                .ToView("vwVehicleType", Schema.General);

            // Approval entities
            modelBuilder.Entity<VwApproval>()
               .HasNoKey()
               .ToView("vwApproval", Schema.General);

            // Inventory entities
            // Adjustment entities
            modelBuilder.Entity<AdjustmentHeader>(entity =>
            {
                entity.Property(e => e.Mark)
                    .IsRequired();

                entity.HasOne<Warehouse>()
                    .WithMany()
                    .HasForeignKey(d => d.WarehouseCode)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<VwAdjustmentHeader>()
                .HasNoKey()
                .ToView("vwAdjustmentHeader", Schema.Inventory);

            modelBuilder.Entity<AdjustmentDetail>(entity =>
            {
                entity.Property(e => e.Code)
                    .IsRequired();

                entity.HasOne<AdjustmentHeader>()
                    .WithMany()
                    .HasForeignKey(d => d.Code)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne<Item>()
                    .WithMany()
                    .HasForeignKey(d => d.ItemId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne<UoM>()
                    .WithMany()
                    .HasForeignKey(d => d.UomId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne<UoMConversion>()
                    .WithMany()
                    .HasForeignKey(d => d.UnitId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<VwAdjustmentDetail>()
                .HasNoKey()
                .ToView("vwAdjustmentDetail", Schema.Inventory);

            modelBuilder.Entity<AdjustmentDetailDiffUnit>(entity =>
            {
                //entity.HasOne<AdjustmentDetail>()
                //    .WithMany()
                //    .HasForeignKey(d => d.AdjustmentDetailId)
                //    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne<UoMConversion>()
                    .WithMany()
                    .HasForeignKey(d => d.UnitId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<VwAdjustmentItem>()
                .HasNoKey()
                .ToView("vwAdjustmentItem", Schema.Inventory);

            // Beginning Balance Stock entities
            modelBuilder.Entity<BeginningBalanceStockHeader>(entity =>
                entity.HasOne<Warehouse>()
                    .WithMany()
                    .HasForeignKey(d => d.WarehouseCode)
                    .OnDelete(DeleteBehavior.NoAction)
            );

            modelBuilder.Entity<BeginningBalanceStockDetail>(entity =>
            {
                entity.Property(e => e.Code)
                    .IsRequired();

                entity.HasOne<BeginningBalanceStockHeader>()
                    .WithMany()
                    .HasForeignKey(d => d.Code)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne<Item>()
                    .WithMany()
                    .HasForeignKey(d => d.ItemId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne<UoM>()
                    .WithMany()
                    .HasForeignKey(d => d.UomId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne<UoMConversion>()
                    .WithMany()
                    .HasForeignKey(d => d.UnitId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // Item entities
            modelBuilder.Entity<Item>(entity =>
            {
                entity.HasOne<ItemCategory>()
                    .WithMany()
                    .HasForeignKey(d => d.CategoryId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne<UoM>()
                    .WithMany()
                    .HasForeignKey(d => d.UomId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne<UoMConversion>()
                    .WithMany()
                    .HasForeignKey(d => d.UomSellId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne<UoMConversion>()
                    .WithMany()
                    .HasForeignKey(d => d.UomBuyId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne<Tax>()
                    .WithMany()
                    .HasForeignKey(d => d.SalesTaxId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne<Tax>()
                    .WithMany()
                    .HasForeignKey(d => d.PurchaseTaxId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<VwItem>()
                .HasNoKey()
                .ToView("vwItem", Schema.Inventory);

            // Item Category entities
            modelBuilder.Entity<ItemCategory>(entity =>
                entity.HasOne<ItemGroup>()
                    .WithMany()
                    .HasForeignKey(d => d.GroupId)
                    .OnDelete(DeleteBehavior.NoAction)
            );

            modelBuilder.Entity<VwItemCategory>()
                .HasNoKey()
                .ToView("vwItemCategory", Schema.Inventory);

            // Item Group entities
            modelBuilder.Entity<VwItemGroup>()
                .HasNoKey()
                .ToView("vwItemGroup", Schema.Inventory);

            modelBuilder.Entity<ItemGroupSubGroup>(entity =>
                entity.HasOne<ItemGroup>()
                    .WithMany()
                    .HasForeignKey(d => d.ItemGroupId)
                    .OnDelete(DeleteBehavior.NoAction)
            );

            // Stock Mutation entities
            modelBuilder.Entity<StockMutation>(entity =>
            {
                entity.HasOne<Warehouse>()
                    .WithMany()
                    .HasForeignKey(d => d.WarehouseCode)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne<Item>()
                    .WithMany()
                    .HasForeignKey(d => d.ItemId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne<UoM>()
                    .WithMany()
                    .HasForeignKey(d => d.UomId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne<UoMConversion>()
                    .WithMany()
                    .HasForeignKey(d => d.UnitId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne<UoMConversion>()
                    .WithMany()
                    .HasForeignKey(d => d.BaseUnit)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // Transfer Stock entities
            modelBuilder.Entity<TransferStockHeader>(entity =>
            {
                entity.Property(e => e.Mark)
                    .IsRequired();

                entity.HasOne<Warehouse>()
                    .WithMany()
                    .HasForeignKey(d => d.WarehouseCodeFrom)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne<Warehouse>()
                    .WithMany()
                    .HasForeignKey(d => d.WarehouseCodeTo)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<VwTransferStockHeader>()
                .HasNoKey()
                .ToView("vwTransferStockHeader", Schema.Inventory);

            modelBuilder.Entity<TransferStockDetail>(entity =>
            {
                entity.Property(e => e.Code)
                    .IsRequired();

                entity.HasOne<TransferStockHeader>()
                    .WithMany()
                    .HasForeignKey(d => d.Code)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne<Item>()
                    .WithMany()
                    .HasForeignKey(d => d.ItemId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne<UoM>()
                    .WithMany()
                    .HasForeignKey(d => d.UomId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne<UoMConversion>()
                    .WithMany()
                    .HasForeignKey(d => d.UnitId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<VwTransferStockDetail>()
                .HasNoKey()
                .ToView("vwTransferStockDetail", Schema.Inventory);

            // UoM entities
            modelBuilder.Entity<VwUoM>()
                .HasNoKey()
                .ToView("vwUoM", Schema.Inventory);

            modelBuilder.Entity<UoMConversion>(entity =>
                entity.HasOne<UoM>()
                    .WithMany()
                    .HasForeignKey(d => d.UomId)
                    .OnDelete(DeleteBehavior.NoAction)
            );

            // Warehouse entities
            modelBuilder.Entity<Warehouse>(entity =>
                entity.HasOne<Customer>()
                    .WithMany()
                    .HasForeignKey(d => d.CustCode)
                    .OnDelete(DeleteBehavior.NoAction)
            );

            modelBuilder.Entity<VwWarehouse>()
                .HasNoKey()
                .ToView("vwWarehouse", Schema.Inventory);

            modelBuilder.Entity<WarehouseQuantity>(entity =>
            {
                entity.HasOne<Warehouse>()
                    .WithMany()
                    .HasForeignKey(d => d.WarehouseCode)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne<Item>()
                    .WithMany()
                    .HasForeignKey(d => d.ItemId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<VwWarehouseQuantity>()
                .HasNoKey()
                .ToView("vwWarehouseQuantity", Schema.Inventory);

            // Purchase entities
            // Debit Memo entities
            modelBuilder.Entity<DebitMemo>(entity =>
            {
                entity.Property(e => e.Mark)
                    .IsRequired();

                entity.HasOne<Supplier>()
                    .WithMany()
                    .HasForeignKey(d => d.SupCode)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne<Currency>()
                    .WithMany()
                    .HasForeignKey(d => d.CurrCode)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<VwDebitMemo>()
                .HasNoKey()
                .ToView("vwDebitMemo", Schema.Purchasing);

            // Purchase Invoice entities
            modelBuilder.Entity<PurchaseInvoiceHeader>(entity =>
            {
                entity.Property(e => e.Mark)
                    .IsRequired();

                entity.HasOne<PurchaseOrderHeader>()
                    .WithMany()
                    .HasForeignKey(d => d.PoCode)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne<Supplier>()
                    .WithMany()
                    .HasForeignKey(d => d.SupCode)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne<Employee>()
                    .WithMany()
                    .HasForeignKey(d => d.IssuedBy)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne<Currency>()
                    .WithMany()
                    .HasForeignKey(d => d.CurrCode)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<VwPurchaseInvoiceHeader>()
                .HasNoKey()
                .ToView("vwPurchaseInvoiceHeader", Schema.Purchasing);

            modelBuilder.Entity<PurchaseInvoiceDetail>(entity =>
            {
                entity.Property(e => e.Code)
                    .IsRequired();

                entity.HasOne<PurchaseInvoiceHeader>()
                    .WithMany()
                    .HasForeignKey(d => d.Code)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne<PurchaseReceiveHeader>()
                    .WithMany()
                    .HasForeignKey(d => d.RcvCode)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<PurchaseInvoiceDebitMemo>(entity =>
            {
                entity.Property(e => e.InvCode)
                    .IsRequired();

                entity.HasOne<PurchaseInvoiceHeader>()
                    .WithMany()
                    .HasForeignKey(d => d.InvCode)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne<DebitMemo>()
                    .WithMany()
                    .HasForeignKey(d => d.DebitMemoCode)
                    .OnDelete(DeleteBehavior.NoAction);
            });

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

            modelBuilder.Entity<VwPurchaseReturnDetail>()
                .HasNoKey()
                .ToView("vwPurchaseReturnDetail", Schema.Purchasing);

            modelBuilder.Entity<PurchaseReturnDetailExchDiffItem>(entity =>
                entity.Property(e => e.Code)
                    .IsRequired()
            );

            modelBuilder.Entity<VwPurchaseReturnDetailExchDiffItem>()
                .HasNoKey()
                .ToView("vwPurchaseReturnDetailExchDiffItem", Schema.Purchasing);

            // Sales entities
            // Area entities
            modelBuilder.Entity<VwArea>()
                .HasNoKey()
                .ToView("vwArea", Schema.Sales);

            // Credit Memo entities
            modelBuilder.Entity<CreditMemo>(entity =>
            {
                entity.Property(e => e.Mark)
                    .IsRequired();

                entity.HasOne<Customer>()
                    .WithMany()
                    .HasForeignKey(d => d.CustCode)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne<Currency>()
                    .WithMany()
                    .HasForeignKey(d => d.CurrCode)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<VwCreditMemo>()
                .HasNoKey()
                .ToView("vwCreditMemo", Schema.Sales);

            // Delivery Plan entities
            modelBuilder.Entity<DeliveryPlanHeader>(entity =>
            {
                entity.Property(e => e.Mark)
                    .IsRequired();

                entity.HasOne<Vehicle>()
                    .WithMany()
                    .HasForeignKey(d => d.VehicleId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne<Employee>()
                    .WithMany()
                    .HasForeignKey(d => d.DriverId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne<Warehouse>()
                    .WithMany()
                    .HasForeignKey(d => d.WarehouseCode)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<VwDeliveryPlanHeader>()
                .HasNoKey()
                .ToView("vwDeliveryPlanHeader", Schema.Sales);

            modelBuilder.Entity<DeliveryPlanDetail>(entity =>
            {
                entity.Property(e => e.Code)
                    .IsRequired();

                entity.HasOne<DeliveryPlanHeader>()
                    .WithMany()
                    .HasForeignKey(d => d.Code)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<DeliveryPlanUndeliveredItem>(entity =>
            {
                entity.Property(e => e.Code)
                    .IsRequired();

                entity.HasOne<DeliveryPlanHeader>()
                    .WithMany()
                    .HasForeignKey(d => d.Code)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne<DeliveryPlanDetail>()
                    .WithMany()
                    .HasForeignKey(d => d.DlvPlanDetailId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne<Item>()
                    .WithMany()
                    .HasForeignKey(d => d.ItemId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne<UoM>()
                    .WithMany()
                    .HasForeignKey(d => d.UomId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne<UoMConversion>()
                    .WithMany()
                    .HasForeignKey(d => d.UnitId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne<Warehouse>()
                    .WithMany()
                    .HasForeignKey(d => d.WarehouseCode)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // Promo entities
            modelBuilder.Entity<PromoHeader>(entity =>
            {
                entity.Property(e => e.Mark)
                    .IsRequired();

                entity.HasOne<Customer>()
                    .WithMany()
                    .HasForeignKey(d => d.CustCode)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne<CustomerType>()
                    .WithMany()
                    .HasForeignKey(d => d.CustTypeId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<VwPromoHeader>()
                .HasNoKey()
                .ToView("vwPromoHeader", Schema.Sales);

            modelBuilder.Entity<PromoDetail>(entity =>
                entity.Property(e => e.Code)
                    .IsRequired()
            );

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

            modelBuilder.Entity<SalesDeliveryDetailFreeGood>(entity =>
                entity.Property(e => e.Code)
                    .IsRequired()
            );

            // Sales Invoice entities
            modelBuilder.Entity<SalesInvoiceHeader>(entity =>
            {
                entity.Property(e => e.Mark)
                    .IsRequired();

                entity.HasOne<SalesOrderHeader>()
                    .WithMany()
                    .HasForeignKey(d => d.SoCode)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne<Customer>()
                    .WithMany()
                    .HasForeignKey(d => d.CustCode)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne<Employee>()
                    .WithMany()
                    .HasForeignKey(d => d.IssuedBy)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne<Currency>()
                    .WithMany()
                    .HasForeignKey(d => d.CurrCode)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<VwSalesInvoiceHeader>()
                .HasNoKey()
                .ToView("vwSalesInvoiceHeader", Schema.Sales);

            modelBuilder.Entity<SalesInvoiceDetail>(entity =>
            {
                entity.Property(e => e.Code)
                    .IsRequired();

                entity.HasOne<SalesInvoiceHeader>()
                    .WithMany()
                    .HasForeignKey(d => d.Code)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne<SalesDeliveryHeader>()
                    .WithMany()
                    .HasForeignKey(d => d.DoCode)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<SalesInvoiceCreditMemo>(entity =>
            {
                entity.Property(e => e.InvCode)
                    .IsRequired();

                entity.HasOne<SalesInvoiceHeader>()
                    .WithMany()
                    .HasForeignKey(d => d.InvCode)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne<CreditMemo>()
                    .WithMany()
                    .HasForeignKey(d => d.CreditMemoCode)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // Salesman Group entities
            modelBuilder.Entity<VwSalesmanGroup>()
                .HasNoKey()
                .ToView("vwSalesmanGroup", Schema.Sales);

            modelBuilder.Entity<VwSalesmanSchedule>()
                .HasNoKey()
                .ToView("vwSalesmanSchedule", Schema.Sales);

            modelBuilder.Entity<VwSalesmanScheduleCustomer>()
                .HasNoKey()
                .ToView("vwSalesmanScheduleCustomer", Schema.Sales);

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

            modelBuilder.Entity<SalesOrderDetailDiscount>(entity =>
                entity.Property(e => e.Code)
                    .IsRequired()
            );

            modelBuilder.Entity<SalesOrderDetailFreeGood>(entity =>
                entity.Property(e => e.Code)
                    .IsRequired()
            );

            // Sales Return entities
            modelBuilder.Entity<SalesReturnHeader>(entity =>
                entity.Property(e => e.Mark)
                    .IsRequired()
            );

            modelBuilder.Entity<VwSalesReturnHeader>()
                .HasNoKey()
                .ToView("vwSalesReturnHeader", Schema.Sales);

            modelBuilder.Entity<SalesReturnDetail>(entity =>
                entity.Property(e => e.Code)
                    .IsRequired()
            );

            modelBuilder.Entity<VwSalesReturnDetail>()
                .HasNoKey()
                .ToView("vwSalesReturnDetail", Schema.Sales);

            modelBuilder.Entity<SalesReturnDetailExchDiffItem>(entity =>
                entity.Property(e => e.Code)
                    .IsRequired()
            );

            modelBuilder.Entity<VwSalesReturnDetailExchDiffItem>()
                .HasNoKey()
                .ToView("vwSalesReturnDetailExchDiffItem", Schema.Sales);

            // Visit Order entities
            modelBuilder.Entity<VisitOrder>(entity =>
                entity.Property(e => e.Mark)
                    .IsRequired()
            );

            modelBuilder.Entity<VisitOrderCustomer>(entity =>
                entity.Property(e => e.Code)
                    .IsRequired()
            );

            modelBuilder.Entity<VisitOrderInvoice>(entity =>
                entity.Property(e => e.Code)
                    .IsRequired()
            );

            modelBuilder.Entity<VwVisitOrder>()
                .HasNoKey()
                .ToView("vwVisitOrder", Schema.Sales);

            modelBuilder.Entity<VwVisitOrderCustomer>()
                .HasNoKey()
                .ToView("vwVisitOrderCustomer", Schema.Sales);

            modelBuilder.Entity<VwVisitOrderInvoice>()
                .HasNoKey()
                .ToView("vwVisitOrderInvoice", Schema.Sales);

            // Visit Plan entities
            modelBuilder.Entity<VisitPlanHeader>(entity =>
                entity.Property(e => e.Mark)
                    .IsRequired()
            );

            modelBuilder.Entity<VwVisitPlanHeader>()
                .HasNoKey()
                .ToView("vwVisitPlanHeader", Schema.Sales);

            modelBuilder.Entity<VisitPlanDetail>(entity =>
                entity.Property(e => e.Code)
                    .IsRequired()
            );

            modelBuilder.Entity<VwVisitPlanDetail>()
                .HasNoKey()
                .ToView("vwVisitPlanDetail", Schema.Sales);

            modelBuilder.Entity<VwVisitPlanDetailCustomer>()
                .HasNoKey()
                .ToView("vwVisitPlanDetailCustomer", Schema.Sales);

            // System Management entities
            // Company entities
            modelBuilder.Entity<VwCompany>()
                .HasNoKey()
                .ToView("vwCompany", Schema.SystemManagement);

            // Menu entities
            modelBuilder.Entity<MenuAction>(entity =>
            {
                entity.HasOne<Menu>()
                    .WithMany()
                    .HasForeignKey(d => d.MenuId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne<SystemManagement.Action>()
                    .WithMany()
                    .HasForeignKey(d => d.ActionId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // Role entities
            modelBuilder.Entity<VwRole>()
                .HasNoKey()
                .ToView("vwRole", Schema.SystemManagement);

            modelBuilder.Entity<RoleMenu>(entity =>
            {
                entity.HasOne<Role>()
                    .WithMany()
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne<Menu>()
                    .WithMany()
                    .HasForeignKey(d => d.MenuId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<RoleMenuAction>(entity =>
            {
                entity.HasOne<Role>()
                    .WithMany()
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne<Menu>()
                    .WithMany()
                    .HasForeignKey(d => d.MenuId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne<SystemManagement.Action>()
                    .WithMany()
                    .HasForeignKey(d => d.ActionId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // System Parameter entities
            modelBuilder.Entity<SystemParameter>(entity =>
                entity.HasOne<SystemParameterModule>()
                    .WithMany()
                    .HasForeignKey(d => d.ModuleId)
                    .OnDelete(DeleteBehavior.NoAction)
            );

            // User entities
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasOne<Role>()
                    .WithMany()
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne<Employee>()
                    .WithMany()
                    .HasForeignKey(d => d.EmployeeId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<VwUser>()
                .HasNoKey()
                .ToView("vwUser", Schema.SystemManagement);
        }
    }
}
