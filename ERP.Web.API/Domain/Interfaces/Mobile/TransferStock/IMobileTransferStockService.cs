using ERP.Common;
using ERP.Entity.Inventory;
using ERP.Entity.MobileWarehouse;
using ERP.Web.API.Domain.Models.Mobile.TransferStock;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERP.Web.API.Domain.Interfaces.Mobile.TransferStock
{
    public interface IMobileTransferStockService
    {
        IEnumerable<MobileTransferStockHeaderModel> getTranferStockHeader(DateTime? date, string search, int userId);
        IEnumerable<MobileTransferStockDetailModel> getTransferStockDetail(string code);
        IEnumerable<MobileTransferStockHeaderModel> getMobileTranferStockHeader(DateTime? date, string search, int userId);
        IEnumerable<MobileTransferStockDetailModel> getMobileTransferStockDetail(string code);
        SaveResult Insert(TransferStockRequestModel data, int userId);

    }
}
