namespace ERP.Web.API.Model.Inventory;

public class UploadBBStockHeaderRequest
{
    public DateTime Date { get; set; }

    public string WarehouseCode { get; set; }

    public string Notes { get; set; }

    public IEnumerable<UploadBBStockDetailRequest> ItemDetails { get; set; }
}

public class UploadBBStockDetailRequest
{
    public string Inisialbarang { get; set; }

    public string Satuanbarang { get; set; }

    public decimal Qtybarang { get; set; }

    public decimal Hargabarang { get; set; }

    public string Catatan { get; set; }

    public int No { get; set; }

    public bool Mark { get; set; }
}