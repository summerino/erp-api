namespace ERP.Web.API.Domain.Models.Mobile.TransactionHistory
{
    public class ItemSubGroupModel
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public string GroupInitial { get; set; }
        public string GroupName { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
