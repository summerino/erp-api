using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERP.Web.API.Domain.Models.Mobile.TransferStock
{
    public class TransferStockRequestModel:MobileTransferStockHeaderModel
    {
        public IEnumerable<MobileTransferStockDetailModel> Details { get; set; }
    }
}
