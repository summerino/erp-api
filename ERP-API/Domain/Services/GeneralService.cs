using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using ERP.Entity;
using Microsoft.EntityFrameworkCore;
using ERP_API.Domain.Extensions;
using ERP_API.Domain.Interfaces;
using ERP_API.Domain.Models;

namespace ERP_API.Domain.Services
{
    public abstract class GeneralService<T> : IGeneralService<T>
        where T : class
    {
        protected TenantContext Db;

        protected GeneralService(TenantContext db)
        {
            Db = db;
        }
        public virtual DataSourceResult GetData(int take, int skip, IEnumerable<Filter> filter, IEnumerable<Sort> sort, List<int> list)
        {
            throw new NotImplementedException();
        }
        public virtual DataSourceResult GetData(int take, int skip, IEnumerable<Filter> filter, IEnumerable<Sort> sort)
        {
            return GetData<T>(take, skip, filter, sort);
        }

        public DataSourceResult GetData<TEntity>(int take, int skip, IEnumerable<Filter> filter, IEnumerable<Sort> sort)
            where TEntity : class
        {
            return Db.Set<TEntity>()
                .AsNoTracking()
                .ToDataSourceResult(take, skip, filter, sort);
        }

        public virtual SaveResult Insert(T data)
        {
            Db.Set<T>().Add(data);
            return SaveChanges();
        }

        public virtual SaveResult Update(T data)
        {
            Db.Entry(data).State = EntityState.Modified;
            return SaveChanges();
        }

        public virtual SaveResult Update(T data, params Expression<Func<T, object>>[] properties)
        {
            Db.Set<T>().Attach(data);
            foreach (var property in properties)
            {
                Db.Entry(data).Property(property).IsModified = true;
            }
            return SaveChanges();
        }

        public virtual SaveResult ReverseUpdate(T data, params Expression<Func<T, object>>[] properties)
        {
            Db.Entry(data).State = EntityState.Modified;
            foreach (var property in properties)
            {
                Db.Entry(data).Property(property).IsModified = false;
            }
            return SaveChanges();
        }

        protected SaveResult SaveChanges()
        {
            var result = new SaveResult(false);

            try
            {
                var affected = Db.SaveChanges();

                result.Success = true;
                result.Count = affected;
            }
            catch (Exception ex)
            {
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }

            return result;
        }

        public string GetNewCode(string code, DateTime? date = null)
        {
            return Db.NewCodes
                .FromSqlInterpolated($"EXEC sp_generate_autono {code}, {date}").ToList()
                .FirstOrDefault()?.Value;
        }

        
    }
}
