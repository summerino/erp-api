using System.Collections.Generic;
using ERP.Common;
using ERP.Common.Models;
using ERP.Web.API.Domain.Models;
using ERP.Web.API.Model.General;

namespace ERP.Web.API.Domain.Interfaces.General
{
    public interface IApprovalService 
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
            string search);
        SaveResult SaveChanges(List<ApprovalRequest> data, int userId);
    }
}
