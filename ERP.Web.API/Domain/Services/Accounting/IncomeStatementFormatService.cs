using ERP.Common;
using ERP.Entity;
using ERP.Entity.Accounting;
using ERP.Entity.SystemManagement;
using ERP.Web.API.Domain.Interfaces.Accounting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERP.Web.API.Domain.Services.Accounting
{
    public class IncomeStatementFormatService : GeneralService<IncomeStatementFormat>, IIncomeStatementFormatService
    {
        public IncomeStatementFormatService(TenantContext db)
            :base(db)
        {

        }

        public override SaveResult Insert(IncomeStatementFormat data)
        {
            var result = new SaveResult(false);

            using var transaction = Db.Database.BeginTransaction();
            try
            {
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
            result.Message = "Data format laba/rugi berhasil disimpan.";
            return result;
        }

        public override SaveResult Update(IncomeStatementFormat data)
        {
            var result = new SaveResult(false);

            Db.IncomeStatementFormats.Update(data);
            Db.Entry(data).Property(e => e.CreatedBy).IsModified = false;
            Db.Entry(data).Property(e => e.CreatedDate).IsModified = false;

            Db.SaveChanges();

            result.Success = true;
            result.Data = data.Code;
            result.Message = "Data format laba/rugi berhasil diperbarui.";
            return result;
        }

        public object GetFormatHierarchy(string category)
        {
            var data = Db.IncomeStatementFormats
                .Where(x => x.IsActive && x.Category == category);

            var users = Db.Users.ToList();

            var result = DefineChildNodes(data.ToList(), users);

            return result;
        }

        public IEnumerable<IncomeStatementFormat> GetFormatLists(string category)
        {
            var data = Db.IncomeStatementFormats
                .Where(x => x.IsActive && !x.Hidden && x.Category == category)
                .OrderBy(x => x.Sort)
                .ToList();

            return data;
        }

        private static object DefineChildNodes(List<IncomeStatementFormat> data, List<User> users, string parentId = null)
        {
            var nodes = data
                .Where(x => x.ParentCode == parentId)
                .OrderBy(x => x.Sort)
                .Select(x => new
                {
                    x.Code,
                    x.Name,
                    x.ParentCode,
                    x.Category,
                    x.Type,
                    x.PercentOf,
                    x.PercentFrom,
                    x.Position,
                    x.Deep,
                    x.Sort,
                    x.SubtotalSort,
                    x.Detail,
                    x.Hidden,
                    x.Bold,
                    x.ByAccount,
                    x.UpdatedDate,
                    UpdatedInitial = users.First(y => y.Id == x.UpdatedBy).Initial,
                    Children = DefineChildNodes(data, users, x.Code)
                });

            return nodes;
        }
    }
}
