namespace ERP.Web.API.Domain.Models.Mobile.CustomerPromotion
{
    public class PromotionHeaderModel
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public short ApplyTo { get; set; }
        public string CoaCost { get; set; }
        public string CreatedInitial { get; set; }
        public string UpdatedInitial { get; set; }
        public string ApprovedInitial { get; set; }
        public string Status { get; set; }
    }
}
