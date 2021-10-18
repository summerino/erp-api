using System;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Bold.Licensing;
using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.Accounting;
using ERP.Web.API.Domain.Interfaces.AssetManagement;
using ERP.Web.API.Domain.Interfaces.Auth;
using ERP.Web.API.Domain.Interfaces.Catalog;
using ERP.Web.API.Domain.Interfaces.Expedition;
using ERP.Web.API.Domain.Interfaces.Finance;
using ERP.Web.API.Domain.Interfaces.General;
using ERP.Web.API.Domain.Interfaces.HumanResource;
using ERP.Web.API.Domain.Interfaces.Inventory;
using ERP.Web.API.Domain.Interfaces.Mobile.General;
using ERP.Web.API.Domain.Interfaces.Mobile.HumanResource;
using ERP.Web.API.Domain.Interfaces.Mobile.ItemRequest;
using ERP.Web.API.Domain.Interfaces.Mobile.NetRevenue;
using ERP.Web.API.Domain.Interfaces.Mobile.TransactionHistory;
using ERP.Web.API.Domain.Interfaces.MobileSales;
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
using ERP.Web.API.Domain.Services.Mobile.General;
using ERP.Web.API.Domain.Services.Mobile.HumanResource;
using ERP.Web.API.Domain.Services.Mobile.ItemRequest;
using ERP.Web.API.Domain.Services.Mobile.NetRevenue;
using ERP.Web.API.Domain.Services.Mobile.TransactionHistory;
using ERP.Web.API.Domain.Services.MobileSales;
using ERP.Web.API.Domain.Services.Purchase;
using ERP.Web.API.Domain.Services.Sales;
using ERP.Web.API.Domain.Services.SystemManagement;
using ERP.Web.API.Model;
using ERP.Web.API.Model.Auth;
using Newtonsoft.Json.Serialization;
using Swift.Framework;
using ERP.Web.API.Domain.Interfaces.MobileWarehouse;
using ERP.Web.API.Domain.Services.MobileWarehouse;

namespace ERP.Web.API
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        private string ControlDbConnectionString { get; set; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            ControlDbConnectionString = configuration.GetConnectionString("CatalogConnection");
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Configure JwtSetting
            services.Configure<JwtConfig>(Configuration.GetSection("JwtConfig"));

            // Configure CORS options
            services.AddCors(options =>
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
            services.AddDbContextPool<CatalogContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("CatalogConnection")));

            services.AddDbContext<TenantContext>();

            // Configure routing options
            services.AddRouting(options =>
            {
                options.LowercaseQueryStrings = true;
                options.LowercaseUrls = true;
            });

            // Configure controller options
            services
                .AddControllers()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                });

            // Add http context accessor service
            services.AddHttpContextAccessor();

            // Add memory cache service
            services.AddMemoryCache();

            // Add authorization service
            services.AddAuthorization(options =>
            {
                options.FallbackPolicy = new AuthorizationPolicyBuilder()
                    .AddRequirements(new UserSessionRequirement())
                    .Build();

                options.AddPolicy(AppConstant.ValidateMobileTokenPolicy, policy =>
                    policy.Requirements.Add(new MobileUserSessionRequirement()));
            });

            // Add authentication service
            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(jwt => {
                    var token = Configuration.GetSection("JwtConfig").Get<JwtConfig>();

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
            services.AddScoped<IAuthorizationHandler, UserSessionHandler>();
            services.AddScoped<IAuthorizationHandler, MobileUserSessionHandler>();
            services.AddScoped<ITenantService, TenantService>();
            services.AddScoped<IShardingService, ShardingService>();
            services.AddScoped<IClaimService, ClaimService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IMobileAuthService, MobileAuthService>();

            // Accounting services
            services.AddScoped<IBeginningBalanceAccountPayableService, BeginningBalanceAccountPayableService>();
            services.AddScoped<IBeginningBalanceAccountReceivableService, BeginningBalanceAccountReceivableService>();
            services.AddScoped<ICoaService, CoaService>();
            services.AddScoped<ICoaTypeService, CoaTypeService>();
            services.AddScoped<ICurrencyRateService, CurrencyRateService>();
            services.AddScoped<IGeneralJournalService, GeneralJournalService>();
            services.AddScoped<IJournalService, JournalService>();
            services.AddScoped<IBeginningBalanceDebitMemoService, BeginningBalanceDebitMemoService>();
            services.AddScoped<IBeginningBalanceCreditMemoService, BeginningBalanceCreditMemoService>();
            services.AddScoped<IClosingMonthService, ClosingMonthService>();
            services.AddScoped<IJournalReportService, JournalReportService>();
            services.AddScoped<IGeneralLedgerReportService, GeneralLedgerReportService>();
            services.AddScoped<ITrialBalanceReportService, TrialBalanceReportService>();
            services.AddScoped<IBalanceSheetReportService, BalanceSheetReportService>();
            services.AddScoped<IIncomeStatementReportService, IncomeStatementReportService>();
            services.AddScoped<IIncomeStatementFormatService, IncomeStatementFormatService>();

            // Asset Management services
            services.AddScoped<IAssetTypeService, AssetTypeService>();
            services.AddScoped<IFixedAssetService, FixedAssetService>();

            // Expedition services
            services.AddScoped<IExpeditionInvoiceService, ExpeditionInvoiceService>();
            services.AddScoped<IEPAPReportService, EPAPReportService>();

            // Finance services
            services.AddScoped<ICashBankService, CashBankService>();
            services.AddScoped<ICashBankTypeService, CashBankTypeService>();
            services.AddScoped<IInterCashBankService, InterCashBankService>();
            services.AddScoped<ICBReportService, CBReportService>();

            // General services
            services.AddScoped<IApprovalService, ApprovalService>();
            services.AddScoped<ICurrencyService, CurrencyService>();
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<ICustomerTypeService, CustomerTypeService>();
            services.AddScoped<IEmployeeService, EmployeeService>();
            services.AddScoped<IPaymentTermService, PaymentTermService>();
            services.AddScoped<ISupplierService, SupplierService>();
            services.AddScoped<ISupplierTypeService, SupplierTypeService>();
            services.AddScoped<ITaxService, TaxService>();
            services.AddScoped<IVehicleService, VehicleService>();
            services.AddScoped<IVehicleTypeService, VehicleTypeService>();

            // Human Resource Services
            services.AddScoped<IAttendanceReportService, AttendanceReportService>();

            // Inventory services
            services.AddScoped<IAdjustmentService, AdjustmentService>();
            services.AddScoped<IItemCategoryService, ItemCategoryService>();
            services.AddScoped<IItemGroupService, ItemGroupService>();
            services.AddScoped<IItemService, ItemService>();
            services.AddScoped<ITransferStockService, TransferStockService>();
            services.AddScoped<IUnitOfMeasurementService, UnitOfMeasurementService>();
            services.AddScoped<IUoMConversionService, UoMConversionService>();
            services.AddScoped<IWarehouseQuantityService, WarehouseQuantityService>();
            services.AddScoped<IWarehouseService, WarehouseService>();
            services.AddScoped<IConsigneeService, ConsigneeService>();
            services.AddScoped<ISMReportService, SMReportService>();
            services.AddScoped<IBeginningBalanceStockService, BeginningBalanceStockService>();

            // Mobile Sales services
            services.AddScoped<IMobileReasonService, MobileReasonService>();
            services.AddScoped<IMobileCustomerService, MobileCustomerService>();
            services.AddScoped<IMobileItemRequestService, MobileItemRequestService>();
            services.AddScoped<IMobileCostService, MobileCostService>();
            services.AddScoped<IMobileVisitLogService, MobileVisitLogService>();
            services.AddScoped<IMobilePaymentInvoiceService, MobilePaymentInvoiceService>();
            services.AddScoped<IMobileOrderService, MobileOrderService>();
            services.AddScoped<IMobilePaymentMethodService, MobilePaymentMethodService>();

            // Mobile Warehouse services
            services.AddScoped<IMobileReceiveItemService, MobileReceiveItemService>();
            services.AddScoped<IMobileDeliveryItemService, MobileDeliveryItemService>();

            // Purchase services
            services.AddScoped<IDebitMemoService, DebitMemoService>();
            services.AddScoped<IPurchaseInvoiceService, PurchaseInvoiceService>();
            services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();
            services.AddScoped<IPurchaseReceiveService, PurchaseReceiveService>();
            services.AddScoped<IPurchaseReturnService, PurchaseReturnService>();
            services.AddScoped<IAPReportService, APReportService>();

            // Sales services
            services.AddScoped<IAreaService, AreaService>();
            services.AddScoped<ICreditMemoService, CreditMemoService>();
            services.AddScoped<IDeliveryPlanService, DeliveryPlanService>();
            services.AddScoped<IDirectInvoiceService, DirectInvoiceService>();
            services.AddScoped<IPromoService, PromoService>();
            services.AddScoped<ISalesDeliveryService, SalesDeliveryService>();
            services.AddScoped<ISalesInvoiceService, SalesInvoiceService>();
            services.AddScoped<ISalesmanGroupService, SalesmanGroupService>();
            services.AddScoped<ISalesmanService, SalesmanService>();
            services.AddScoped<ISalesOrderService, SalesOrderService>();
            services.AddScoped<ISalesReturnService, SalesReturnService>();
            services.AddScoped<IVisitOrderService, VisitOrderService>();
            services.AddScoped<IVisitPlanService, VisitPlanService>();
            services.AddScoped<IARReportService, ARReportService>();

            // System Management services
            services.AddScoped<IActionService, ActionService>();
            services.AddScoped<ICompanyProfileService, CompanyProfileService>();
            services.AddScoped<IMenuService, MenuService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<ISystemParameterService, SystemParameterService>();
            services.AddScoped<IUserService, UserService>();

            #region Mobile
            // General services
            services.AddScoped<IImageService, ImageService>();
            services.AddScoped<IVisitInformationService, VisitInformationService>();

            // Human Resource Services
            services.AddScoped<IAttendanceService, AttendanceService>();

            // Item Request services
            services.AddScoped<IItemRequestService, ItemRequestService>();

            // Net Revenue services
            services.AddScoped<INetRevenueService, NetRevenueService>();

            // Transaction History services
            services.AddScoped<ITransactionHistoryService, TransactionHistoryService>();

            // Visit Order services
            services.AddScoped<Domain.Interfaces.Mobile.VisitOrder.IVisitOrderService, Domain.Services.Mobile.VisitOrder.VisitOrderService>();
            #endregion
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, CatalogContext catalogCtx,
            IShardingService shardingService, IServiceProvider service­Provider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();
            app.UseRouting();
            app.UseCors();
            app.UseAuthentication();
            app.UseAuthorization();

            //Register Bold license
            BoldLicenseProvider.RegisterLicense(Configuration["BoldLic"]);

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            EnsureDatabaseCreated(catalogCtx, shardingService);
            Builder.InitConfiguration(ControlDbConnectionString);
        }

        public virtual void EnsureDatabaseCreated(CatalogContext catalogCtx, IShardingService shardingService)
        {
            catalogCtx.Database.Migrate();
            shardingService.ApplyMigrationAsync().GetAwaiter().GetResult();
        }
    }
}
