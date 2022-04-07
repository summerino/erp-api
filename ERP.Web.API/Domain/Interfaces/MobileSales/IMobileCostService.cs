using ERP.Common;
using ERP.Common.Models;
using ERP.Entity.Accounting;
using ERP.Entity.MobileSales;
using ERP.Web.API.Domain.Models.Mobile.Operational;
using ERP.Web.API.Model.MobileSales;

namespace ERP.Web.API.Domain.Interfaces.MobileSales;

public interface IMobileCostService : IGeneralService<MobileCostHeader>
{
    DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
        string search);

    IEnumerable<VwMobileCostDetail> GetDetailData(string code);

    IEnumerable<MobileCostImage> GetImageData(string code);

    bool IsCostExists(string date, int userId);

    SaveResult Insert(MobileCostRequest data);

    SaveResult Update(MobileCostRequest data);

    SaveResult Approve(List<MobileCostRequest> data, int userId, string date, string coa, string notes);

    SaveResult Reject(List<MobileCostRequest> data, int userId);

    #region Mobile
    DataSourceResult GetDataForMobile(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts, int userId, string date);

    IEnumerable<CostDetailModel> GetDetailForMobile(string Code);

    SaveResult InsertForMobile(CostRequestModel data, int UserId);

    IEnumerable<Coa> GetMobileCoaForMobile(string lastUpdate);

    IEnumerable<CostImageModel> GetImageForMobile(string Code);

    CostTodayTransactionModel GetTodayTransactionForMobile(string date, int userId);
    #endregion
}