using System;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ERP_API.Domain.Entities;
using ERP_API.Domain.Interfaces;
using ERP_API.Domain.Interfaces.Accounting;
using ERP_API.Domain.Interfaces.General;
using ERP_API.Domain.Interfaces.Inventory;
using ERP_API.Domain.Interfaces.Purchase;
using ERP_API.Domain.Interfaces.Sales;
using ERP_API.Domain.Services;
using ERP_API.Domain.Services.Auth;
using ERP_API.Domain.Services.Accounting;
using ERP_API.Domain.Services.General;
using ERP_API.Domain.Services.Inventory;
using ERP_API.Domain.Services.Purchase;
using ERP_API.Domain.Services.Sales;
using ERP_API.Model.Auth;
using Newtonsoft.Json.Serialization;
using Swift.Framework;
using ERP_API.Domain.Interfaces.SystemManagement;
using ERP_API.Domain.Services.SystemManagement;

namespace ERP_API
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
                .AddControllers(options =>
                {
                    options.Filters.Add(new AuthorizeFilter("ValidateToken"));
                })
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                });

            // Add http context accessor service
            services.AddHttpContextAccessor();

            // Add authorization service
            services.AddAuthorization(options =>
            {
                options.AddPolicy("ValidateToken", policy =>
                    policy.Requirements.Add(new UserSessionRequirement()));

            });

            // Add authentication service
            services.AddAuthentication(options =>
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
            services.AddScoped<IShardingService, ShardingService>();
            services.AddScoped<IClaimService, ClaimService>();
            services.AddScoped<IAuthService, AuthService>();
            
            // Accounting services
            services.AddScoped<ICoaService, CoaService>();
            services.AddScoped<ICurrencyRateService, CurrencyRateService>();

            // General services
            services.AddScoped<ICurrencyService, CurrencyService>();
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<ICustomerTypeService, CustomerTypeService>();
            services.AddScoped<IEmployeeService, EmployeeService>();
            services.AddScoped<ISupplierService, SupplierService>();
            services.AddScoped<ISupplierTypeService, SupplierTypeService>();
            services.AddScoped<ITaxService, TaxService>();
            services.AddScoped<IVehicleService, VehicleService>();
            services.AddScoped<IVehicleTypeService, VehicleTypeService>();
            services.AddScoped<ICurrencyService, CurrencyService>();
            services.AddScoped<IItemGroupService, ItemGroupService>();

            // Inventory services
            services.AddScoped<IItemCategoryService, ItemCategoryService>();
            services.AddScoped<IItemService, ItemService>();
            services.AddScoped<IUnitOfMeasurementService, UnitOfMeasurementService>();
            services.AddScoped<IUoMConversionService, UoMConversionService>();
            services.AddScoped<IWarehouseService, WarehouseService>();
            services.AddScoped<IAdjustmentService, AdjustmentService>();

            // Purchase services
            services.AddScoped<IDebitMemoService, DebitMemoService>();
            services.AddScoped<IPurchaseInvoiceService, PurchaseInvoiceService>();
            services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();
            services.AddScoped<IPurchaseReceiveService, PurchaseReceiveService>();
            services.AddScoped<IPurchaseReturnService, PurchaseReturnService>();

            // Sales services
            services.AddScoped<ICreditMemoService, CreditMemoService>();
            services.AddScoped<ISalesDeliveryService, SalesDeliveryService>();
            services.AddScoped<ISalesInvoiceService, SalesInvoiceService>();
            services.AddScoped<ISalesOrderService, SalesOrderService>();
            services.AddScoped<IAreaService, AreaService>();

            // System Management services
            services.AddScoped<IUserService, UserService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, CatalogContext controlDbContext,
            IShardingService shardingService, IServiceProvider service­Provider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //app.UseHttpsRedirection();
            app.UseRouting();
            app.UseCors();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                //endpoints.MapControllers().RequireAuthorization();
                endpoints.MapControllers().AllowAnonymous();
            });

            EnsureDatabaseCreated(controlDbContext, shardingService);
            Builder.InitConfiguration(ControlDbConnectionString);
        }

        public virtual void EnsureDatabaseCreated(CatalogContext controlDbContext,
            IShardingService shardingService)
        {
            //if (!DatabaseUtility.DatabaseExists(ControlDbConnectionString))
            //{
            //    DatabaseUtility.CreateDatabase(ControlDbConnectionString);
            //}
            controlDbContext.Database.Migrate();
            shardingService.ApplyMigrationAsync().GetAwaiter().GetResult();
        }
    }
}
