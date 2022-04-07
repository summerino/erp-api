namespace ERP.Web.API.Domain.Models.Mobile.Purchase;

public class ItemModel
{
    public int Id { get; set; }
    public string Initial { get; set; }
    public string Name { get; set; }
    public int? UoMId { get; set; }
    public int UnitId { get; set; }
    public string UnitEquivalent { get; set; }
}