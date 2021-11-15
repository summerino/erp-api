namespace ERP.Web.API.Domain.Models.Mobile.General
{
    public class ItemInformationModel
    {
        public decimal? Stock { get; set; }
        public DateTime? LastUpdateStock { get; set; }
        public decimal? LastOrder { get; set; }
        public decimal? MaxOrder { get; set; }
        public decimal? AvgOrder { get; set; }
    }
}
