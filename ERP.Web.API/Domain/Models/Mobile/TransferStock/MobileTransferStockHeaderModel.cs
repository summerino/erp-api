using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERP.Web.API.Domain.Models.Mobile.TransferStock
{
    public class MobileTransferStockHeaderModel
    {
        public string Code { get; set; }

        public DateTime Date { get; set; }

        public string Type { get; set; }

        public string WarehouseCodeFrom { get; set; }

        public string WarehouseCodeTo { get; set; }

        public string WarehouseInitialFrom { get; set; }

        public string WarehouseInitialTo { get; set; }

        public string TypeInitial { get; set; }
    }
}
