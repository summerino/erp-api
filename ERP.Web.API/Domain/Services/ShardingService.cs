using Microsoft.EntityFrameworkCore;
using ERP.Common.Extensions;
using ERP.Entity;
using ERP.Entity.Accounting;
using ERP.Entity.Finance;
using ERP.Entity.General;
using ERP.Entity.SystemManagement;

namespace ERP.Web.API.Domain.Services;

public interface IShardingService
{
    Task ApplyMigrationAsync();
}

public class ShardingService : IShardingService
{
    private readonly CatalogContext _catalogCtx;
    private readonly IClaimService _claim;
    private readonly IWebHostEnvironment _env;

    public ShardingService(CatalogContext catalogCtx, IClaimService claim, IWebHostEnvironment env)
    {
        _catalogCtx = catalogCtx;
        _claim = claim;
        _env = env;
    }

    public async Task ApplyMigrationAsync()
    {
        foreach (var tenant in _catalogCtx.Tenants)
        {
            if (!string.IsNullOrWhiteSpace(tenant.ServerName) || !string.IsNullOrWhiteSpace(tenant.DatabaseName) ||
                !string.IsNullOrWhiteSpace(tenant.ServerUserId) || !string.IsNullOrWhiteSpace(tenant.ServerPassword))
            {
                var optionsBuilder = new DbContextOptionsBuilder<TenantContext>();
                optionsBuilder.UseSqlServer(
                    $"Server={tenant.ServerName};Database={tenant.DatabaseName};User Id={tenant.ServerUserId};Password={tenant.ServerPassword};Command Timeout=600");

                var tenantCtx = new TenantContext(optionsBuilder.Options, _catalogCtx, _claim);
                await tenantCtx.Database.MigrateAsync();
                await EnsureSeededAsync(tenantCtx);
            }
        }
    }

    public async Task EnsureSeededAsync(TenantContext tenantCtx)
    {
        var jsonFilePath = $"{_env.WebRootPath}/data/";

        // System Management entities
        await tenantCtx.SeedEntityAsync<Company>(jsonFilePath, true);
        await tenantCtx.SeedEntityAsync<Menu>(jsonFilePath, addIfNoExists: true, overwrite: true);
        await tenantCtx.SeedEntityAsync<Entity.SystemManagement.Action>(jsonFilePath, addIfNoExists: true, overwrite: true);
        await tenantCtx.SeedEntityAsync<MenuAction>(jsonFilePath, addIfNoExists: true, overwrite: true);
        await tenantCtx.SeedEntityAsync<Role>(jsonFilePath, true);
        await tenantCtx.SeedEntityAsync<RoleMenu>(jsonFilePath, true);
        await tenantCtx.SeedEntityAsync<RoleMenuAction>(jsonFilePath, true);
        await tenantCtx.SeedEntityAsync<SystemParameterModule>(jsonFilePath, addIfNoExists: true);
        await tenantCtx.SeedEntityAsync<SystemParameter>(jsonFilePath, addIfNoExists: true);
        await tenantCtx.SeedEntityAsync<User>(jsonFilePath, true);

        // Accounting entities
        await tenantCtx.SeedEntityAsync<CoaType>(jsonFilePath, true);
        await tenantCtx.SeedEntityAsync<IncomeStatementFormat>(jsonFilePath, true);
        await tenantCtx.SeedEntityAsync<IncomeStatementFormatSubtotal>(jsonFilePath, true);

        // Finance entities
        await tenantCtx.SeedEntityAsync<CashBankType>(jsonFilePath, addIfNoExists: true);

        // General entities
        await tenantCtx.SeedEntityAsync<SupplierType>(jsonFilePath, true);
        await tenantCtx.SeedEntityAsync<Currency>(jsonFilePath, addIfNoExists: true, overwrite: true);
        await tenantCtx.SeedEntityAsync<Tax>(jsonFilePath, true);
    }

    //public async Task SeedEntityAsync<TEntity>(TenantContext tenantCtx, bool firstInitialize = false,
    //    bool addIfNoExists = false, bool overwrite = false)
    //    where TEntity : class
    //{
    //    var userId = 1;
    //    var now = DateTime.Now;

    //    var schema = tenantCtx.Model.FindEntityType(typeof(TEntity))?.GetSchema() ?? "dbo";
    //    var entityName = typeof(TEntity).Name;
    //    var jsonString = await File.ReadAllTextAsync($"{_env.WebRootPath}/data/{schema}.{entityName}.json");
    //    var data = JsonConvert.DeserializeObject<List<TEntity>>(jsonString);

    //    if (data?.Any()?? false)
    //    {
    //        var (keyName, keyType) = tenantCtx.GetKey<TEntity>();

    //        if (firstInitialize && !tenantCtx.Set<TEntity>().Any())
    //        {
    //            if (entityName != "Role" && entityName != "RoleMenu" && entityName != "RoleMenuAction" &&
    //                entityName != "IncomeStatementFormat" && entityName != "IncomeStatementFormatSubtotal")
    //            {
    //                foreach (var item in data)
    //                {
    //                    ApplyDefaultBaseColumnValue(item, userId, now);
    //                }
    //            }

    //            await tenantCtx.AddSeedDataAsync(schema, entityName, keyType, data);
    //        }
    //        else if (addIfNoExists || overwrite)
    //        {
    //            object existingData = keyType switch
    //            {
    //                "String" => tenantCtx.Set<TEntity>().Select(x => EF.Property<string>(x, keyName)).ToArray(),
    //                "Byte" => tenantCtx.Set<TEntity>().Select(x => EF.Property<byte>(x, keyName)).ToArray(),
    //                "Int16" => tenantCtx.Set<TEntity>().Select(x => EF.Property<short>(x, keyName)).ToArray(),
    //                "Int32" => tenantCtx.Set<TEntity>().Select(x => EF.Property<int>(x, keyName)).ToArray(),
    //                "Int64" => tenantCtx.Set<TEntity>().Select(x => EF.Property<long>(x, keyName)).ToArray(),
    //                _ => null
    //            };

    //            if (overwrite)
    //            {
    //                var baseColumns =
    //                    new[] { "IsActive", "CreatedBy", "CreatedDate", "UpdatedBy", "UpdatedDate" };

    //                var overrideData =
    //                    data.AsQueryable().Where($"@0.Contains({keyName})", existingData).ToList();

    //                if (overrideData.Any())
    //                {
    //                    foreach (var item in overrideData)
    //                    {
    //                        tenantCtx.Entry(item).State = EntityState.Modified;

    //                        foreach (var baseColumn in item.GetProps().Where(x => baseColumns.Contains(x.Name)))
    //                        {
    //                            tenantCtx.Entry(item).Property(baseColumn.Name).IsModified = false;
    //                        }
    //                    }

    //                    tenantCtx.Set<TEntity>().UpdateRange(overrideData);
    //                    await tenantCtx.SaveChangesAsync();
    //                }
    //            }

    //            if (addIfNoExists)
    //            {
    //                var newData =
    //                    data.AsQueryable()
    //                        .Where($"!@0.Contains({keyName})", existingData)
    //                        .OrderBy($"{keyName}").ToList();

    //                if (newData.Any())
    //                {
    //                    if (entityName != "Menu" && entityName != "SystemParameterModule" &&
    //                        entityName != "SystemParameter" && entityName != "Currency")
    //                    {
    //                        foreach (var item in newData)
    //                        {
    //                            ApplyDefaultBaseColumnValue(item, userId, now);
    //                        }
    //                    }

    //                    await tenantCtx.AddSeedDataAsync(schema, entityName, keyType, newData);
    //                }
    //            }
    //        }
    //    }
    //}

    //private void ApplyDefaultBaseColumnValue<TEntity>(TEntity item, int userId, DateTime now)
    //    where TEntity : class
    //{
    //    typeof(TEntity).GetProperty("IsActive")?.SetValue(item, true);
    //    typeof(TEntity).GetProperty("CreatedBy")?.SetValue(item, userId);
    //    typeof(TEntity).GetProperty("CreatedDate")?.SetValue(item, now);
    //    typeof(TEntity).GetProperty("UpdatedBy")?.SetValue(item, userId);
    //    typeof(TEntity).GetProperty("UpdatedDate")?.SetValue(item, now);
    //}
}