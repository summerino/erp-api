using ERP.Common;
using ERP.Common.Models;
using ERP.Entity.Accounting;
using ERP.Web.API.Model.Accounting;

namespace ERP.Web.API.Domain.Interfaces.Accounting
{
    public interface IBeginningBalanceCreditMemoService : IGeneralService<BeginningBalanceCreditMemo>
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
            string search);

        SaveResult Delete(long id, int userId);

        IEnumerable<UploadBBCMRequest> VerifyUpload(IEnumerable<UploadBBCMRequest> data);

        SaveResult Posting(IEnumerable<UploadBBCMRequest> data, int userId);
    }
}
