using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ERP.Common;
using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.General;
using ERP.Web.API.Domain.Models.General;
using ERP.Web.API.Model.General;

namespace ERP.Web.API.Domain.Services.General
{
    public class ApprovalService : IApprovalService
    {
        private readonly TenantContext _tenantCtx;

        public ApprovalService(TenantContext tenantCtx)
        {
            _tenantCtx = tenantCtx;
        }

        public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
            List<int> actionId, string search)
        {
            var data = _tenantCtx.VwApprovals.AsQueryable();

            if (actionId?.Any() ?? false)
            {
                data = data.Where(x => actionId.Contains(x.ActionId));
            }

            if (!string.IsNullOrEmpty(search))
            {
                data = DateTime.TryParse(search, out var searchDate)
                    ? data.Where(x => x.Date == searchDate)
                    : data.Where(x =>
                        x.Code.Contains(search) || x.Descr.Contains(search) || x.SourceTrans.Contains(search));
            }

            return data.ToDataSourceResult(skip, take, filter, sort);
        }

        public SaveResult SaveChanges(List<ApprovalRequest> data, int userId)
        {
            var result = new SaveResult(false);
            var listQuery = data.Select(item => GenerateQuery(item.ActionId, item.Code, userId));

            var query = string.Join(';', listQuery);
            using var transaction = _tenantCtx.Database.BeginTransaction();
            try
            {
                _tenantCtx.Database.ExecuteSqlRaw(query);
                transaction.Commit();
            }
            catch (Exception ex) 
            {
                result.Message = ex.InnerException?.Message ?? ex.Message;
                transaction.Rollback();
            }

            result.Success = true;
            result.Message = "Data persetujuan berhasil di setujui.";
            return result;
        }

        private string GenerateQuery(int actionId, string code, int user) 
        {
            var map = new MapApproval();
            var tableName = "";
            var query = "UPDATE [TABLE] SET ApprovedBy='[USER]', ApprovedDate=GETDATE() WHERE Code='[CODE]'";
            var temp = map.Approvals.SingleOrDefault(x => x.ActionId == actionId);
            if (temp != null) 
            {
                tableName = temp.TableName;
            }
            return query.Replace("[TABLE]", tableName).Replace("[CODE]", code).Replace("[USER]", user.ToString());
        }
    }
}
