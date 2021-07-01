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
        ViewRelatedTrans = 6,
        Close = 7,
        ChangeDate = 8,
        Approve = 9
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
        COA = 66,
        CurrencyRate = 18,
        DebitMemo = 40,
        CreditMemo = 49,
        PurchaseInvoice = 38,
        PurchaseOrder = 36,
        PurchaseReceive = 37,
        PurchaseReturn = 39,
        Area = 42,
        DeliveryPlan = 48,
        DirectInvoice = 46,
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
        COAType = 67,
        ExpeditionInvoice = 70,
        CompanyProfile = 75,
        Parameter = 73,
        Approval = 72
    }
    public static class ApprovalType
    {
        public static string FixedAsset = "Aktiva Tetap";

        public static string ExpeditionInvoice = "Expedition Invoice";
        // Inventory
        public static string Adjustment = "Penyesuaian";
        public static string TransferStock = "Transfer Persediaan";
        // Purchase
        public static string PurchaseOrder = "Order Pembelian";
        public static string PurchaseInvoice = "Faktur Pembelian";
        public static string PurchaseReceive = "Penerimaan";
        public static string PurchaseReturn = "Retur Pembelian";
        // Sales
        public static string SalesOrder = "Order Penjualan";
        public static string SalesReturn = "Retur Penjualan";
        public static string SalesInvoice = "Faktur Penjualan";
        public static string SalesDelivery = "Surat Jalan";
        public static string SalesDeliveryPlan = "Rencana Pengiriman"; 
        public static string Promo = "Promo";
        public static string VisitPlan = "Rencana Kunjungan";
        public static string VisitOrder = "Perintah Kunjungan";

      
    }

}
