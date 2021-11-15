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
        public string WarehouseNameFrom { get; set; }

        public string WarehouseNameTo { get; set; }

        public string SalesNameFrom { get; set; }

        public string SalesNameTo { get; set; }
        public string TypeInitial { get; set; }
    }
}
