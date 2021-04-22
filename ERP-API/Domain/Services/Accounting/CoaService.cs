using System;
using System.Collections.Generic;
using System.Linq;
using ERP_API.Domain.Entities;
using ERP_API.Domain.Entities.Accounting;
using ERP_API.Domain.Extensions;
using ERP_API.Domain.Interfaces.Accounting;
using ERP_API.Domain.Models;

namespace ERP_API.Domain.Services.Accounting
{
    public class CoaService : GeneralService<Coa>, ICoaService
    {
        public CoaService(TenantContext db)
            : base(db)
        {
        }

        public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
            string search)
        {
            var data = Db.Coas.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                data = data.Where(x => x.Code.Contains(search) || x.Name.Contains(search));
            }

            return data.ToDataSourceResult(skip, take, filter, sort);
        }

        public DataSourceResult GetLists(IEnumerable<Filter> filters, IEnumerable<Sort> sorts)
        {
            var data = Db.Coas.Where(x => x.IsActive);

            return data.ToDataSourceResult(0, -1, filters, sorts);
        }
        
        public override SaveResult Insert(Coa data)
        {
            var result = new SaveResult(false);

            using var transaction = Db.Database.BeginTransaction();
            try
            {
                // Checking code already exists or not
                if (IsCoaExists(data.Code, 0))
                {
                    result.Message = "Code is already exists. Please use another code.";
                    return result;
                }

                // Insert data
                Db.Add(data);

                Db.SaveChanges();
                transaction.Commit();
            }
            catch (Exception ex)
            {
                result.Message = ex.InnerException?.Message ?? ex.Message;
                return result;
            }

            result.Success = true;
            result.Data = data.Code;
            result.Message = "Success insert coa.";
            return result;
        }

        public override SaveResult Update(Coa data)
        {
            var result = new SaveResult(false);

            // Checking code already exists or not
            if (IsCoaExists(data.Code, data.Id))
            {
                result.Message = "Code is already exists. Please use another code.";
                return result;
            }

            // Update data
            Db.Coas.Update(data);
            Db.Entry(data).Property(e => e.Code).IsModified = false;
            Db.Entry(data).Property(e => e.CreatedBy).IsModified = false;
            Db.Entry(data).Property(e => e.CreatedDate).IsModified = false;

            Db.SaveChanges();

            result.Success = true;
            result.Data = data.Code;
            result.Message = "Success update coa.";
            return result;
        }

        public SaveResult Delete(int id, int userId)
        {
            var result = new SaveResult(false);

            var data = Db.Coas.Find(id);
            if (data != null)
            {
                // Checking active
                if (data.IsActive == false)
                {
                    result.Message = "Can't inactive coa because data already inactive.";
                    return result;
                }

                // Update data
                data.IsActive = false;
                data.UpdatedBy = userId;
                data.UpdatedDate = DateTime.Now;

                Db.SaveChanges();
            }

            result.Success = true;
            result.Message = "Success inactive coa.";
            return result;
        }

        private bool IsCoaExists(string code, int id)
        {
            return Db.Coas.Any(x => x.Code == code && x.IsActive && x.Id != id);
        }
    }
}
