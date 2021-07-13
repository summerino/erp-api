using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace ERP.Common.Extensions
{
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
    }
}
