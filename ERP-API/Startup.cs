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
using ERP_API.Utils;
using Newtonsoft.Json.Serialization;
using Swift.Framework;

namespace ERP_API
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        private string ControlDbConnectionString { get; set; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            ControlDbConnectionString = configuration.GetConnectionString("controlConnectionString");
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
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
            SetupDatabase(services);

            services.AddRouting(options =>
            {
                options.LowercaseQueryStrings = true;
                options.LowercaseUrls = true;
            });

            services.AddControllers()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                });

            services.AddHttpContextAccessor();

            // Add application service
            // Core services
            services.AddScoped<IShardingService, ShardingService>();
            services.AddScoped<IClaimService, ClaimService>();

            // General services
            services.AddScoped<ICustomersService, CustomersService>();

            // Inventory services
            services.AddScoped<IItemCategoryService, ItemCategoryService>();
            services.AddScoped<IItemService, ItemService>();
            services.AddScoped<IUoMConversionService, UoMConversionService>();

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
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ERPControlDbContext controlDbContext,
            IShardingService shardingService, IServiceProvider service­Provider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            
            //app.UseHttpsRedirection();
            app.UseRouting();
            app.UseCors();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            EnsureDatabaseCreated(controlDbContext, shardingService);
            Builder.InitConfiguration(ControlDbConnectionString);
        }

        public virtual void SetupDatabase(IServiceCollection services)
        {
            services.AddDbContextPool<ERPControlDbContext>(options => options.UseSqlServer(ControlDbConnectionString));
            //services.AddDbContext<ERPDbContext>(options => options.EnableSensitiveDataLogging());
            services.AddDbContext<TenantContext>();
        }

        public virtual void EnsureDatabaseCreated(ERPControlDbContext controlDbContext,
            IShardingService shardingService)
        {
            if (!DatabaseUtility.DatabaseExists(ControlDbConnectionString))
            {
                DatabaseUtility.CreateDatabase(ControlDbConnectionString);
            }

            controlDbContext.Database.Migrate();
            shardingService.ApplyMigrationAsync().GetAwaiter().GetResult();
        }
    }
}
