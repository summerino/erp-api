namespace ERP.Web.API.Domain.Models.Mobile.TransferStock
{
    public class TransferStockRequestModel
    {
        public string Code { get; set; }

        public string SignatureImage {get; set;}
        
        public IEnumerable<MobileTransferStockDetailModel> Details { get; set; }
    }
}
