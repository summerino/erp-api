namespace ERP.Web.API.Domain.Models.Mobile.Purchase
{
    public class ReceiveItemHeaderModel
    {
        public string Code { get; set; }
        public DateTime Date { get; set; }
        public DateTime PODate { get; set; }
        public string PONumber { get; set; }
        public string RcvCode { get; set; }
        public string TransCode { get; set; }
        public short SrcTrans { get; set; }
        public string SupCode { get; set; }
        public string SupName { get; set; }
        public string SupPhone { get; set; }
        public long ReceiveBy { get; set; }
    }
}
