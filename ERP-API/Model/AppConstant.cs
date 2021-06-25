namespace ERP_API.Model
{
    public static class AppConstant
    {
        public static string UnAuthMessage = "Operasi tidak dapat dilanjutkan, harap hubungi administrator.";
    }

    public enum Actions 
    {
        Insert = 1,
        Update = 2,
        Delete = 3,
        Void = 4,
        ChangeWarehouse = 5,
        Close = 6
    }

    public enum Menu 
    {
        Warehouse = 32,
        Adjustment = 34,
        ItemCategory = 29,
        Item = 28,
        ItemGroup = 30,
        TransferStock = 33,
        Uom = 31,
        Currency = 8,
        Customer = 17,
        CustomerType = 18,
        Employee = 22,
        Supplier = 20,
        SupplierType = 21,
        Tax = 26,
        Vehicle = 24,
        VehicleType = 25,
        COA = 17,
        CurrencyRate = 18,
        DebitMemo = 40,
        CreditMemo = 49,
        PurchaseInvoice = 38,
        PurchaseOrder = 36,
        PurchaseReceive = 37,
        PurchaseReturn = 39,
        Area = 42,
        DeliveryPlan = 25,
        DirectInvoice = 26,
        Promo = 56,
        SalesDelivery = 45,
        SalesInvoice = 46,
        SalesOrder = 44,
        SalesReturn = 47,
        Role = 52,
        User = 51,
        AssetType = 62, 
        SalesmanGroup = 58,
        FixedAsset = 63,
        COAType = 67
    }
}
