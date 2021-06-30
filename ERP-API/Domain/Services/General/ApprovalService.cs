using ERP_API.Domain.Entities;
using ERP_API.Domain.Extensions;
using ERP_API.Domain.Interfaces.General;
using ERP_API.Domain.Models;
using ERP_API.Model;
using ERP_API.Model.General;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace ERP_API.Domain.Services.General
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
                        x.Code.Contains(search) || x.Name.Contains(search) || x.Type.Contains(search));
            }

            return data.ToDataSourceResult(skip, take, filter, sort);
        }

        public SaveResult SaveChanges(List<ApprovalRequest> data, int userId)
        {
            var result = new SaveResult(false);
            var listQuery = new List<string>();
            foreach (var item in data)
            {
                listQuery.Add(GenerateQuery(item.Type, item.Code, userId));
            }
            var query = string.Join(';',listQuery);
            using (var context = _tenantCtx)
            using (var command = context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = query;
                context.Database.OpenConnection();
                command.ExecuteNonQuery();
            }
            result.Success = true;
            result.Message = "Data berhasil di approve.";
            return result;
        }

        private string GenerateQuery(string type, string code, int user) 
        {
            var map = new MapApproval();
            string tableName = "";
            string query = "Update [TABLE] set ApprovedBy='[USER]', ApprovedDate=GETDATE() Where Code = '[CODE]'";
            var temp = map.Approvals.SingleOrDefault(x => x.Description.Equals(type));
            if (temp != null) 
            {
                tableName = temp.TableName;
            }
            
            return query.Replace("[TABLE]", tableName).Replace("[CODE]", code).Replace("[USER]", user.ToString());
        }
    }
}
