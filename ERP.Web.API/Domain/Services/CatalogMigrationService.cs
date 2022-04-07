using Microsoft.EntityFrameworkCore;
using ERP.Common.Extensions;
using ERP.Entity;
using ERP.Entity.Catalog;

namespace ERP.Web.API.Domain.Services;

public interface ICatalogMigrationService
{
    Task ApplyMigrationAsync();
}

public class CatalogMigrationService : ICatalogMigrationService
{
    private readonly CatalogContext _catalogCtx;
    private readonly IWebHostEnvironment _env;

    public CatalogMigrationService(CatalogContext catalogCtx, IWebHostEnvironment env)
    {
        _catalogCtx = catalogCtx;
        _env = env;
    }

    public async Task ApplyMigrationAsync()
    {
        await _catalogCtx.Database.MigrateAsync();
        await EnsureSeededAsync(_catalogCtx);
    }

    public async Task EnsureSeededAsync(CatalogContext catalogCtx)
    {
        var jsonFilePath = $"{_env.WebRootPath}/data/catalog/";

        // dbo entities
        await catalogCtx.SeedEntityAsync<Tenant>(jsonFilePath, true);
        await catalogCtx.SeedEntityAsync<User>(jsonFilePath, true);
    }

    //public async Task SeedEntity<TEntity>(CatalogContext catalogCtx, bool firstInitialize = false, bool addIfNoExists = false, bool overwrite = false)
    //    where TEntity : class
    //{
    //    var schema = catalogCtx.Model.FindEntityType(typeof(TEntity))?.GetSchema() ?? "dbo";
    //    var entityName = typeof(TEntity).Name;
    //    var jsonString = await File.ReadAllTextAsync($"{_env.WebRootPath}/data/catalog/{schema}.{entityName}.json");
    //    var data = JsonConvert.DeserializeObject<List<TEntity>>(jsonString);

    //    if (data?.Any()?? false)
    //    {
    //        var (keyName, keyType) = catalogCtx.GetKey<TEntity>();

    //        if (firstInitialize && !catalogCtx.Set<TEntity>().Any())
    //        {
    //            await catalogCtx.AddSeedDataAsync(schema, entityName, keyType, data);
    //        }
    //        else if (addIfNoExists || overwrite)
    //        {
    //            object existingData = keyType switch
    //            {
    //                "String" => catalogCtx.Set<TEntity>().Select(x => EF.Property<string>(x, keyName)).ToArray(),
    //                "Byte" => catalogCtx.Set<TEntity>().Select(x => EF.Property<byte>(x, keyName)).ToArray(),
    //                "Int16" => catalogCtx.Set<TEntity>().Select(x => EF.Property<short>(x, keyName)).ToArray(),
    //                "Int32" => catalogCtx.Set<TEntity>().Select(x => EF.Property<int>(x, keyName)).ToArray(),
    //                "Int64" => catalogCtx.Set<TEntity>().Select(x => EF.Property<long>(x, keyName)).ToArray(),
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
    //                        catalogCtx.Entry(item).State = EntityState.Modified;

    //                        foreach (var baseColumn in item.GetProps().Where(x => baseColumns.Contains(x.Name)))
    //                        {
    //                            catalogCtx.Entry(item).Property(baseColumn.Name).IsModified = false;
    //                        }
    //                    }

    //                    catalogCtx.Set<TEntity>().UpdateRange(overrideData);
    //                    await catalogCtx.SaveChangesAsync();
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
    //                    await catalogCtx.AddSeedDataAsync(schema, entityName, keyType, newData);
    //                }
    //            }
    //        }
    //    }
    //}
}