using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using ERP_API.Domain.Entities;
using ERP_API.Domain.Interfaces.Inventory;
using ERP_API.Domain.Interfaces.Purchase;
using ERP_API.Domain.Interfaces.General;
using ERP_API.Domain.Interfaces.Sales;
using ERP_API.Domain.Services;
using ERP_API.Domain.Services.Inventory;
using ERP_API.Domain.Services.Purchase;
using ERP_API.Domain.Services.General;
using ERP_API.Domain.Services.Sales;
using Newtonsoft.Json.Serialization;
using Swift.Framework;
using ERP_API.Model.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using Microsoft.IdentityModel.Tokens;

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
            services.Configure<JwtConfig>(Configuration.GetSection("JwtConfig"));
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

            // Add framework services
            services.AddDbContextPool<CatalogContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("CatalogConnection")));

            services.AddDbContext<TenantContext>();

            services.AddRouting(options =>
            {
                options.LowercaseQueryStrings = true;
                options.LowercaseUrls = true;
            });

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(jwt => {
                var key = Encoding.ASCII.GetBytes(Configuration["JwtConfig:Secret"]);

                jwt.SaveToken = true;
                jwt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    RequireExpirationTime = true
                };
            });

            services.AddControllers()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                });

            services.AddHttpContextAccessor();

            services.AddAuthorization(options =>
            {
                options.AddPolicy("Session", policy =>
                    policy.Requirements.Add(new UserSessionRequirement()));

            });

            // Add application service
            // Auth Services
            services.AddScoped<IAuthorizationHandler, UserSessionHandler>();
            // Core services
            services.AddScoped<IShardingService, ShardingService>();
            services.AddScoped<IClaimService, ClaimService>();

            // General services
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<ICustomerTypeService, CustomerTypeService>();
            services.AddScoped<ISupplierService, SupplierService>();
            services.AddScoped<ISupplierTypeService, SupplierTypeService>();

            // Inventory services
            services.AddScoped<IItemCategoryService, ItemCategoryService>();
            services.AddScoped<IItemService, ItemService>();
            services.AddScoped<IUoMConversionService, UoMConversionService>();
            services.AddScoped<IWarehouseService, WarehouseService>();

            // Purchase services
            services.AddScoped<IPurchaseInvoiceService, PurchaseInvoiceService>();
            services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();
            services.AddScoped<IPurchaseReceiveService, PurchaseReceiveService>();

            // Sales services
            services.AddScoped<ISalesDeliveryService, SalesDeliveryService>();
            services.AddScoped<ISalesInvoiceService, SalesInvoiceService>();
            services.AddScoped<ISalesOrderService, SalesOrderService>();
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
            app.UseAuthentication();
            app.UseCors();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
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
