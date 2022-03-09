using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Bold.Licensing;
using ERP.Entity;
using ERP.Web.API.Domain.Interfaces;
using ERP.Web.API.Domain.Interfaces.Accounting;
using ERP.Web.API.Domain.Interfaces.AssetManagement;
using ERP.Web.API.Domain.Interfaces.Auth;
using ERP.Web.API.Domain.Interfaces.Catalog;
using ERP.Web.API.Domain.Interfaces.Expedition;
using ERP.Web.API.Domain.Interfaces.Finance;
using ERP.Web.API.Domain.Interfaces.General;
using ERP.Web.API.Domain.Interfaces.HumanResource;
using ERP.Web.API.Domain.Interfaces.Inventory;
using ERP.Web.API.Domain.Interfaces.Mobile.CustomerDeliverySchedule;
using ERP.Web.API.Domain.Interfaces.Mobile.CustomerTransaction;
using ERP.Web.API.Domain.Interfaces.Mobile.General;
using ERP.Web.API.Domain.Interfaces.Mobile.HumanResource;
using ERP.Web.API.Domain.Interfaces.Mobile.ItemRequest;
using ERP.Web.API.Domain.Interfaces.Mobile.NetRevenue;
using ERP.Web.API.Domain.Interfaces.Mobile.Sales;
using ERP.Web.API.Domain.Interfaces.Mobile.TransactionHistory;
using ERP.Web.API.Domain.Interfaces.MobileSales;
using ERP.Web.API.Domain.Interfaces.MobileWarehouse;
using ERP.Web.API.Domain.Interfaces.Purchase;
using ERP.Web.API.Domain.Interfaces.Sales;
using ERP.Web.API.Domain.Interfaces.SystemManagement;
using ERP.Web.API.Domain.Services;
using ERP.Web.API.Domain.Services.Accounting;
using ERP.Web.API.Domain.Services.AssetManagement;
using ERP.Web.API.Domain.Services.Auth;
using ERP.Web.API.Domain.Services.Catalog;
using ERP.Web.API.Domain.Services.Expedition;
using ERP.Web.API.Domain.Services.Finance;
using ERP.Web.API.Domain.Services.General;
using ERP.Web.API.Domain.Services.HumanResource;
using ERP.Web.API.Domain.Services.Inventory;
using ERP.Web.API.Domain.Services.Mobile.CustomerDeliverySchedule;
using ERP.Web.API.Domain.Services.Mobile.CustomerTransaction;
using ERP.Web.API.Domain.Services.Mobile.General;
using ERP.Web.API.Domain.Services.Mobile.HumanResource;
using ERP.Web.API.Domain.Services.Mobile.ItemRequest;
using ERP.Web.API.Domain.Services.Mobile.NetRevenue;
using ERP.Web.API.Domain.Services.Mobile.Sales;
using ERP.Web.API.Domain.Services.Mobile.TransactionHistory;
using ERP.Web.API.Domain.Services.MobileSales;
using ERP.Web.API.Domain.Services.MobileWarehouse;
using ERP.Web.API.Domain.Services.Purchase;
using ERP.Web.API.Domain.Services.Sales;
using ERP.Web.API.Domain.Services.SystemManagement;
using ERP.Web.API.Model;
using ERP.Web.API.Model.Auth;
using Newtonsoft.Json.Serialization;
using Swift.Framework;
using ERP.Web.API.Domain.Interfaces.Mobile.Warehouse;
using ERP.Web.API.Domain.Services.Mobile.Warehouse;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Configure JwtSetting
builder.Services.Configure<JwtConfig>(builder.Configuration.GetSection("JwtConfig"));

// Configure CORS options
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        builder =>
        {
            builder.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

// Add database service
builder.Services.AddDbContextPool<CatalogContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("CatalogConnection")));

builder.Services.AddDbContext<TenantContext>();

// Configure routing options
builder.Services.AddRouting(options =>
{
    options.LowercaseQueryStrings = true;
    options.LowercaseUrls = true;
});

// Configure controller options
builder.Services
    .AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
    });

// Add http context accessor service
builder.Services.AddHttpContextAccessor();

// Add memory cache service
builder.Services.AddMemoryCache();

// Add authorization service
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .AddRequirements(new UserSessionRequirement())
        .Build();

    options.AddPolicy(AppConstant.ValidateMobileTokenPolicy, policy =>
        policy.Requirements.Add(new MobileUserSessionRequirement()));

    options.AddPolicy(AppConstant.ValidateMobileCustomerTokenPolicy, policy =>
        policy.Requirements.Add(new MobileCustomerSessionRequirement()));

    options.AddPolicy(AppConstant.ValidateAllMobileTokenPolicy, policy =>
        policy.Requirements.Add(new MobileAllSessionRequirement()));
});

// Add authentication service
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(jwt => {
        var token = builder.Configuration.GetSection("JwtConfig").Get<JwtConfig>();

        jwt.RequireHttpsMetadata = false;
        jwt.ClaimsIssuer = token.Issuer;
        jwt.SaveToken = true;
        jwt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(token.Secret)),
            ValidIssuer = token.Issuer,
            ValidateIssuer = true,
            ValidateAudience = false,
            ValidateLifetime = true,
            RequireExpirationTime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

// Add application service
// Core services
builder.Services.AddScoped<IAuthorizationHandler, UserSessionHandler>();
builder.Services.AddScoped<IAuthorizationHandler, MobileUserSessionHandler>();
builder.Services.AddScoped<IAuthorizationHandler, MobileCustomerSessionHandler>();
builder.Services.AddScoped<IAuthorizationHandler, MobileAllSessionHandler>();
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<IShardingService, ShardingService>();
builder.Services.AddScoped<IClaimService, ClaimService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IMobileAuthService, MobileAuthService>();

// Accounting services
builder.Services.AddScoped<IBeginningBalanceAccountPayableService, BeginningBalanceAccountPayableService>();
builder.Services.AddScoped<IBeginningBalanceAccountReceivableService, BeginningBalanceAccountReceivableService>();
builder.Services.AddScoped<ICoaService, CoaService>();
builder.Services.AddScoped<ICoaTypeService, CoaTypeService>();
builder.Services.AddScoped<ICurrencyRateService, CurrencyRateService>();
builder.Services.AddScoped<IGeneralJournalService, GeneralJournalService>();
builder.Services.AddScoped<IJournalService, JournalService>();
builder.Services.AddScoped<IBeginningBalanceDebitMemoService, BeginningBalanceDebitMemoService>();
builder.Services.AddScoped<IBeginningBalanceCreditMemoService, BeginningBalanceCreditMemoService>();
builder.Services.AddScoped<IClosingMonthService, ClosingMonthService>();
builder.Services.AddScoped<IJournalReportService, JournalReportService>();
builder.Services.AddScoped<IGeneralLedgerReportService, GeneralLedgerReportService>();
builder.Services.AddScoped<ITrialBalanceReportService, TrialBalanceReportService>();
builder.Services.AddScoped<IBalanceSheetReportService, BalanceSheetReportService>();
builder.Services.AddScoped<IIncomeStatementReportService, IncomeStatementReportService>();
builder.Services.AddScoped<IIncomeStatementFormatService, IncomeStatementFormatService>();

// Asset Management services
builder.Services.AddScoped<IAssetTypeService, AssetTypeService>();
builder.Services.AddScoped<IFixedAssetService, FixedAssetService>();

// Expedition services
builder.Services.AddScoped<IExpeditionInvoiceService, ExpeditionInvoiceService>();
builder.Services.AddScoped<IEPAPReportService, EPAPReportService>();

// Finance services
builder.Services.AddScoped<ICashBankService, CashBankService>();
builder.Services.AddScoped<ICashBankTypeService, CashBankTypeService>();
builder.Services.AddScoped<IInterCashBankService, InterCashBankService>();
builder.Services.AddScoped<ICBReportService, CBReportService>();
builder.Services.AddScoped<IOutstandingChequeReportService, OutstandingChequeReportService>();

// General services
builder.Services.AddScoped<IActiveTransactionService, ActiveTransactionService>();
builder.Services.AddScoped<IApprovalService, ApprovalService>();
builder.Services.AddScoped<ICurrencyService, CurrencyService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<ICustomerTypeService, CustomerTypeService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IPaymentTermService, PaymentTermService>();
builder.Services.AddScoped<ISupplierService, SupplierService>();
builder.Services.AddScoped<ISupplierTypeService, SupplierTypeService>();
builder.Services.AddScoped<ITaxService, TaxService>();
builder.Services.AddScoped<IVehicleService, VehicleService>();
builder.Services.AddScoped<IVehicleTypeService, VehicleTypeService>();
builder.Services.AddScoped<IDynamicReportTemplateService, DynamicReportTemplateService>();

// Human Resource Services
builder.Services.AddScoped<IAttendanceReportService, AttendanceReportService>();

// Inventory services
builder.Services.AddScoped<IAdjustmentService, AdjustmentService>();
builder.Services.AddScoped<IItemCategoryService, ItemCategoryService>();
builder.Services.AddScoped<IItemGroupService, ItemGroupService>();
builder.Services.AddScoped<IItemService, ItemService>();
builder.Services.AddScoped<ITransferStockService, TransferStockService>();
builder.Services.AddScoped<IUnitOfMeasurementService, UnitOfMeasurementService>();
builder.Services.AddScoped<IUoMConversionService, UoMConversionService>();
builder.Services.AddScoped<IWarehouseQuantityService, WarehouseQuantityService>();
builder.Services.AddScoped<IWarehouseService, WarehouseService>();
builder.Services.AddScoped<IConsigneeService, ConsigneeService>();
builder.Services.AddScoped<ISMReportService, SMReportService>();
builder.Services.AddScoped<IBeginningBalanceStockService, BeginningBalanceStockService>();

// Mobile Sales services
builder.Services.AddScoped<IMobileReasonService, MobileReasonService>();
builder.Services.AddScoped<IMobileCustomerService, MobileCustomerService>();
builder.Services.AddScoped<IMobileItemRequestService, MobileItemRequestService>();
builder.Services.AddScoped<IMobileCostService, MobileCostService>();
builder.Services.AddScoped<IMobileVisitLogService, MobileVisitLogService>();
builder.Services.AddScoped<IMobilePaymentInvoiceService, MobilePaymentInvoiceService>();
builder.Services.AddScoped<IMobileOrderService, MobileOrderService>();
builder.Services.AddScoped<IMobilePaymentMethodService, MobilePaymentMethodService>();
builder.Services.AddScoped<IMobileVisitPerformanceReportService, MobileVisitPerformanceReportService>();
builder.Services.AddScoped<IMobileActivityLogReportService, MobileActivityLogReportService>();
builder.Services.AddScoped<IMobileMapTrackingReportService, MobileMapTrackingReportService>();

// Mobile Warehouse services
builder.Services.AddScoped<IMobileReceiveItemService, MobileReceiveItemService>();
builder.Services.AddScoped<IMobileDeliveryItemService, MobileDeliveryItemService>();
builder.Services.AddScoped<IMobileTransferStockService, MobileTransferStockService>();

// Purchase services
builder.Services.AddScoped<IDebitMemoService, DebitMemoService>();
builder.Services.AddScoped<IPurchaseInvoiceService, PurchaseInvoiceService>();
builder.Services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();
builder.Services.AddScoped<IPurchaseReceiveService, PurchaseReceiveService>();
builder.Services.AddScoped<IPurchaseReturnService, PurchaseReturnService>();
builder.Services.AddScoped<IAPReportService, APReportService>();
builder.Services.AddScoped<IAPMutationReportService, APMutationReportService>();
builder.Services.AddScoped<IAPAgingReportService, APAgingReportService>();
builder.Services.AddScoped<IDebitMemoReportService, DebitMemoReportService>();
builder.Services.AddScoped<IPurchaseOrderReportService, PurchaseOrderReportService>();
builder.Services.AddScoped<IPurchaseReceiveReportService, PurchaseReceiveReportService>();
builder.Services.AddScoped<IPurchaseInvoiceReportService, PurchaseInvoiceReportService>();
builder.Services.AddScoped<IPurchaseReturnReportService, PurchaseReturnReportService>();

// Sales services
builder.Services.AddScoped<IAreaService, AreaService>();
builder.Services.AddScoped<ICreditMemoService, CreditMemoService>();
builder.Services.AddScoped<IDeliveryPlanService, DeliveryPlanService>();
builder.Services.AddScoped<IDirectInvoiceService, DirectInvoiceService>();
builder.Services.AddScoped<IPromoService, PromoService>();
builder.Services.AddScoped<ISalesDeliveryService, SalesDeliveryService>();
builder.Services.AddScoped<ISalesInvoiceService, SalesInvoiceService>();
builder.Services.AddScoped<ISalesmanGroupService, SalesmanGroupService>();
builder.Services.AddScoped<ISalesmanService, SalesmanService>();
builder.Services.AddScoped<ISalesOrderService, SalesOrderService>();
builder.Services.AddScoped<ISalesReturnService, SalesReturnService>();
builder.Services.AddScoped<IVisitOrderService, VisitOrderService>();
builder.Services.AddScoped<IVisitPlanService, VisitPlanService>();
builder.Services.AddScoped<IARReportService, ARReportService>();
builder.Services.AddScoped<IARMutationReportService, ARMutationReportService>();
builder.Services.AddScoped<IARAgingReportService, ARAgingReportService>();
builder.Services.AddScoped<ICreditMemoReportService, CreditMemoReportService>();
builder.Services.AddScoped<ISalesOrderReportService, SalesOrderReportService>();
builder.Services.AddScoped<ISalesDeliveryReportService, SalesDeliveryReportService>();
builder.Services.AddScoped<ISalesInvoiceReportService, SalesInvoiceReportService>();
builder.Services.AddScoped<ISalesReturnReportService, SalesReturnReportService>();
builder.Services.AddScoped<ISalesTargetService, SalesTargetService>();
builder.Services.AddScoped<ISalesTargetReportService, SalesTargetReportService>();


// System Management services
builder.Services.AddScoped<IActionService, ActionService>();
builder.Services.AddScoped<ICompanyProfileService, CompanyProfileService>();
builder.Services.AddScoped<IMenuService, MenuService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<ISystemParameterService, SystemParameterService>();
builder.Services.AddScoped<IUserService, UserService>();

//Special services
builder.Services.AddScoped<IFireForgetService, FireForgetService>();

#region Mobile
// General services
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<ICustomerFirebaseTokenService, CustomerFirebaseTokenService>();
builder.Services.AddScoped<IVisitInformationService, VisitInformationService>();

// Human Resource Services
builder.Services.AddScoped<IAttendanceService, AttendanceService>();

// Item Request services
builder.Services.AddScoped<IItemRequestService, ItemRequestService>();

// Net Revenue services
builder.Services.AddScoped<INetRevenueService, NetRevenueService>();

// Sales services
builder.Services.AddScoped<IDeliveryPlanMobileService, DeliveryPlanMobileService>();

// Transaction History services
builder.Services.AddScoped<ITransactionHistoryService, TransactionHistoryService>();

// Visit Order services
builder.Services.AddScoped<ERP.Web.API.Domain.Interfaces.Mobile.VisitOrder.IVisitOrderService, ERP.Web.API.Domain.Services.Mobile.VisitOrder.VisitOrderService>();

// Customer Transaction (Mobile Customer) services
builder.Services.AddScoped<ICustomerTransactionService, CustomerTransactionService>();

//Transfer Stock
builder.Services.AddScoped<ERP.Web.API.Domain.Interfaces.Mobile.TransferStock.IMobileTransferStockService, ERP.Web.API.Domain.Services.Mobile.TransferStock.MobileTransferStockService>();

// Warehouse Profile
builder.Services.AddScoped<IWarehouseProfileService, WarehouseProfileService>();

//Activity Log
builder.Services.AddScoped<IActivityLogService, ActivityLogService>();

// Cutomer Delivery Schedule
builder.Services.AddScoped<ICustomerDeliveryScheduleService, CustomerDeliveryScheduleService>();

// Cutomer Promotion
builder.Services.AddScoped<ICustomerPromotionService, CustomerPromotionService>();

// Cutomer Order
builder.Services.AddScoped<ICustomerOrderService, CustomerOrderService>();
#endregion

var app = builder.Build();

app.UseStaticFiles();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

// Register Bold license
BoldLicenseProvider.RegisterLicense(builder.Configuration["BoldLic"]);

app.MapControllers();

// Db migrations
using (var scope = app.Services.CreateScope())
{
    // Catalog db migrations
    var catalogCtx = scope.ServiceProvider.GetRequiredService<CatalogContext>();
    catalogCtx.Database.Migrate();

    // Tenant db migrations in sharding service
    var shardingService = scope.ServiceProvider.GetRequiredService<IShardingService>();
    shardingService.ApplyMigrationAsync().GetAwaiter().GetResult();
}

// Initialize configuration for swift
Builder.InitConfiguration(builder.Configuration.GetConnectionString("CatalogConnection"));

app.Run();
