using System.Collections.Generic;
using ERP_API.Domain.Entities.General;
using ERP_API.Domain.Models;
using ERP_API.Model.General;

namespace ERP_API.Domain.Interfaces.General
{
    public interface IApprovalService 
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
            string search);
        SaveResult SaveChanges(List<ApprovalRequest> data, int userId);
    }
}
