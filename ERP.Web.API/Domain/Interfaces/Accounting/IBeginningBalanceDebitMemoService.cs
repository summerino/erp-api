using System.Collections.Generic;
using ERP.Common;
using ERP.Common.Models;
using ERP.Entity.Accounting;
using ERP.Web.API.Model.Accounting;

namespace ERP.Web.API.Domain.Interfaces.Accounting
{
    public interface IBeginningBalanceDebitMemoService : IGeneralService<BeginningBalanceDebitMemo>
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
            string search);

        SaveResult Delete(long id, int userId);

        IEnumerable<UploadBBDMRequest> VerifyUpload(IEnumerable<UploadBBDMRequest> data);

        SaveResult Posting(IEnumerable<UploadBBDMRequest> data, int userId);
    }
}
