using ERP.Web.API.Domain.Interfaces.General;
using ERP.Web.API.Domain.Models;
using ERP.Web.API.Model.General;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using ERP.Common;
using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Entity.Core;
using ERP.Web.API.Domain.Models.General;

namespace ERP.Web.API.Domain.Services.General
{
    public class ApprovalService : IApprovalService
    {
        private readonly TenantContext _tenantCtx;

        public ApprovalService(TenantContext tenantCtx)
        {
            _tenantCtx = tenantCtx;
        }

        public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort, string search)
        {
            var data = _tenantCtx.VwApprovals.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                data = data.Where(x =>
                        x.Code.Contains(search) || x.Name.Contains(search) || x.SourceTrans.Contains(search));
            }

            return data.ToDataSourceResult(skip, take, filter, sort);
        }

        public SaveResult SaveChanges(List<ApprovalRequest> data, int userId)
        {
            var result = new SaveResult(false);
            var listQuery = new List<string>();
            foreach (var item in data)
            {
                listQuery.Add(GenerateQuery(item.SourceTrans, item.Code, userId));
            }

            var query = string.Join(';',listQuery);
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
            result.Message = "Data berhasil di approve.";
            return result;
        }

        private string GenerateQuery(string sourceTrans, string code, int user) 
        {
            var map = new MapApproval();
            string tableName = "";
            string query = "Update [TABLE] set ApprovedBy='[USER]', ApprovedDate=GETDATE() Where Code = '[CODE]'";
            var temp = map.Approvals.SingleOrDefault(x => x.Description.Equals(sourceTrans));
            if (temp != null) 
            {
                tableName = temp.TableName;
            }
            return query.Replace("[TABLE]", tableName).Replace("[CODE]", code).Replace("[USER]", user.ToString());
        }
    }
}
