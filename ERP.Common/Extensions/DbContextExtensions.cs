using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Omu.ValueInjecter.Utils;

namespace ERP.Common.Extensions;

public static class DbContextExtensions
{
    /// <summary>
    /// Extention method for upsert ONLY USE FOR TESTING OR DB INITIALISATION UNTIL OPTIMISED
    /// inspired by ;)
    /// </summary>
    /// <param name="dbSet"></param>
    /// <param name="entities"></param>
    public static void AddOrUpdateRange<T>(this DbSet<T> dbSet, List<T> entities, Guid shard, bool overrideValues = true) where T : class
    {
        foreach (var entity in entities)
        {
            var t = typeof(T);
            PropertyInfo keyField = null;
            foreach (var propt in t.GetProperties())
            {
                var keyAttr = propt.GetCustomAttribute<KeyAttribute>();
                if (keyAttr != null)
                {
                    keyField = propt;
                    break; // assume no composite keys
                }
            }
            if (keyField == null)
            {
                throw new Exception($"{t.FullName} does not have a KeyAttribute field. Unable to exec AddOrUpdate call.");
            }

            var rowCount = 0;
            var keyVal = keyField.GetValue(entity);
            //check for keytype
            if (keyField.PropertyType == typeof(string))
            {
                rowCount = dbSet.Where(e => EF.Property<string>(e, keyField.Name) == (string)keyVal).AsNoTracking().Count();
            }
            else
            {
                rowCount = dbSet.Where(e => EF.Property<int>(e, keyField.Name) == (int)keyVal).AsNoTracking().Count();
            }

            if (rowCount > 0)
            {
                if (overrideValues)
                {
                    dbSet.Update(entity);
                }
            }
            else
            {
                dbSet.Add(entity);
            }
        }
    }

    public static async Task SeedEntityAsync<TEntity>(this DbContext context, string jsonFilePath,
        bool firstInitialize = false, bool addIfNoExists = false, bool overwrite = false)
        where TEntity : class
    {
        var userId = 1;
        var now = DateTime.Now;

        var schema = context.Model.FindEntityType(typeof(TEntity))?.GetSchema() ?? "dbo";
        var entityName = typeof(TEntity).Name;
        var jsonString = await File.ReadAllTextAsync($"{jsonFilePath}{schema}.{entityName}.json");
        var data = JsonConvert.DeserializeObject<List<TEntity>>(jsonString);

        if (data?.Any() ?? false)
        {
            var (keyName, keyType) = context.GetKey<TEntity>();

            if (firstInitialize && !context.Set<TEntity>().Any())
            {
                if (entityName != "Tenant" && (schema != "dbo" || entityName != "User") && 
                    entityName != "Role" && entityName != "RoleMenu" && entityName != "RoleMenuAction" &&
                    entityName != "IncomeStatementFormat" && entityName != "IncomeStatementFormatSubtotal")
                {
                    foreach (var item in data)
                    {
                        ApplyDefaultBaseColumnValue(item, userId, now);
                    }
                }

                await context.AddSeedDataAsync(schema, entityName, keyType, data);
            }
            else if (addIfNoExists || overwrite)
            {
                object existingData = keyType switch
                {
                    "String" => context.Set<TEntity>().Select(x => EF.Property<string>(x, keyName)).ToArray(),
                    "Guid" => context.Set<TEntity>().Select(x => EF.Property<Guid>(x, keyName)).ToArray(),
                    "Byte" => context.Set<TEntity>().Select(x => EF.Property<byte>(x, keyName)).ToArray(),
                    "Int16" => context.Set<TEntity>().Select(x => EF.Property<short>(x, keyName)).ToArray(),
                    "Int32" => context.Set<TEntity>().Select(x => EF.Property<int>(x, keyName)).ToArray(),
                    "Int64" => context.Set<TEntity>().Select(x => EF.Property<long>(x, keyName)).ToArray(),
                    _ => null
                };

                if (overwrite)
                {
                    var baseColumns =
                        new[] { "IsActive", "CreatedBy", "CreatedDate", "UpdatedBy", "UpdatedDate" };

                    var overrideData =
                        data.AsQueryable().Where($"@0.Contains({keyName})", existingData).ToList();

                    if (overrideData.Any())
                    {
                        foreach (var item in overrideData)
                        {
                            context.Entry(item).State = EntityState.Modified;

                            foreach (var baseColumn in item.GetProps().Where(x => baseColumns.Contains(x.Name)))
                            {
                                context.Entry(item).Property(baseColumn.Name).IsModified = false;
                            }
                        }

                        context.Set<TEntity>().UpdateRange(overrideData);
                        await context.SaveChangesAsync();
                    }
                }

                if (addIfNoExists)
                {
                    var newData =
                        data.AsQueryable()
                            .Where($"!@0.Contains({keyName})", existingData)
                            .OrderBy($"{keyName}").ToList();

                    if (newData.Any())
                    {
                        if (entityName != "Menu" && entityName != "SystemParameterModule" &&
                            entityName != "SystemParameter" && entityName != "Currency")
                        {
                            foreach (var item in newData)
                            {
                                ApplyDefaultBaseColumnValue(item, userId, now);
                            }
                        }

                        await context.AddSeedDataAsync(schema, entityName, keyType, newData);
                    }
                }
            }
        }
    }

    [SuppressMessage("Security", "EF1002:Risk of vulnerability to SQL injection.")]
    public static async Task AddSeedDataAsync<TEntity>(this DbContext context, string schema, string entityName,
        string keyType, List<TEntity> data)
        where TEntity : class
    {
        if (keyType is "String" or "Guid")
        {
            await context.Set<TEntity>().AddRangeAsync(data);
            await context.SaveChangesAsync();
        }
        else
        {
            await using var transaction = await context.Database.BeginTransactionAsync();
            await context.Set<TEntity>().AddRangeAsync(data);
            await context.Database.ExecuteSqlRawAsync($"SET IDENTITY_INSERT [{schema}].[{entityName}] ON;");
            await context.SaveChangesAsync();
            await context.Database.ExecuteSqlRawAsync($"SET IDENTITY_INSERT [{schema}].[{entityName}] OFF;");
            await transaction.CommitAsync();
        }
    }

    public static (string, string) GetKey<TEntity>(this DbContext context)
        where TEntity : class
    {
        var entity = context.Model.FindEntityType(typeof(TEntity))?.FindPrimaryKey()?.Properties
            .Select(x => new { x.Name, Type = x.ClrType.Name })
            .Single();

        return (entity?.Name, entity?.Type);
    }

    private static void ApplyDefaultBaseColumnValue<TEntity>(TEntity item, int userId, DateTime now)
        where TEntity : class
    {
        typeof(TEntity).GetProperty("IsActive")?.SetValue(item, true);
        typeof(TEntity).GetProperty("CreatedBy")?.SetValue(item, userId);
        typeof(TEntity).GetProperty("CreatedDate")?.SetValue(item, now);
        typeof(TEntity).GetProperty("UpdatedBy")?.SetValue(item, userId);
        typeof(TEntity).GetProperty("UpdatedDate")?.SetValue(item, now);
    }
}