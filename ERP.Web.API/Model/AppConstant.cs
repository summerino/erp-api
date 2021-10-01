namespace ERP.Web.API.Model
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
        ApproveTransferStock = 9,
        Post = 10,
        PayWithDebitMemo = 11,
        PayWithCreditMemo = 12,
        ApproveConsignee = 13,
        ApproveAdjustment = 14,
        ApprovePurchaseOrder = 15,
        ApprovePurchaseReceive = 16,
        ApprovePurchaseInvoice = 17,
        ApprovePurchaseReturn = 18,
        ApprovePromo = 19,
        ApproveSalesOrder = 20,
        ApproveDeliveryOrder = 21,
        ApproveSalesInvoice = 22,
        ApproveSalesReturn = 23,
        ApproveDeliveryPlan = 24,
        ApproveVisitOrder = 25,
        ApproveExpeditionInvoice = 26,
        ApproveGeneralCashBank = 27,
        ApproveInterCashBank = 28,
        ApproveGeneralJournal = 29,
        ApproveFixedAsset = 30,
        CbTypeAccountReceivable = 31,
        CbTypeAccountPayable = 32,
        CbTypeExpeditionDebt = 33,
        CbTypeGeneralTransaction = 34,
        CbTypeSalesDownPayment = 35,
        CbTypeSalesDownPaymentReturn = 36,
        CbTypePurchaseDownPayment = 37,
        CbTypePurchaseDownPaymentReturn = 38,
        CbTypeSalesReturn = 39,
        CbTypePurchaseReturn = 40,
        Print = 41,
        OverLimit = 42,
        Approve = 43,
        Reject = 44
    }

    public enum Menu 
    {
        // General
        Customer = 17,
        CustomerType = 18,
        Supplier = 20,
        SupplierType = 21,
        Employee = 22,
        Vehicle = 24,
        VehicleType = 25,
        Tax = 26,
        Currency = 8,
        PaymentTerm = 57,
        Approval = 72,

        // Inventory
        Item = 28,
        ItemCategory = 29,
        ItemGroup = 30,
        Uom = 31,
        Warehouse = 32,
        TransferStock = 33,
        Consignee = 91,
        Adjustment = 34,
        BeginningBalanceStock = 87,

        // Purchase
        PurchaseOrder = 36,
        PurchaseReceive = 37,
        PurchaseInvoice = 38,
        PurchaseReturn = 39,
        DebitMemo = 40,

        // Sales
        Area = 42,
        SalesmanGroup = 58,
        Promo = 56,
        SalesOrder = 44,
        SalesDelivery = 45,
        SalesInvoice = 46,
        DirectInvoice = 46,
        SalesReturn = 47,
        DeliveryPlan = 48,
        CreditMemo = 49,
        VisitPlan = 54,
        VisitOrder = 55,

        // Expedition
        ExpeditionInvoice = 70,

        // Finance
        CashBank = 82,
        InterCashBank = 83,

        // Accounting
        COA = 66,
        COAType = 67,
        CurrencyRate = 18,
        GeneralJournal = 84,
        BeginningBalanceAccountPayable = 78,
        BeginningBalanceAccountReceivable = 79,
        BeginningBalanceDebitMemo = 89,
        BeginningBalanceCreditMemo = 90,
        JournalReport = 100,
        GeneralLedgerReport = 104,
        TrialBalanceReport = 105,
        BalanceSheetReport = 110,
        IncomeStatementReport = 111,
        Posting = 86,
        ClosingMonth = 88,
        IncomeStatementFormat = 112,

        // Asset Management
        AssetType = 62,
        FixedAsset = 63,

        // Mobile Sales
        MobileReason = 108,
        MobileCustomer = 113,
        MobileItemRequest = 115,
        MobileSalesCost = 116,

        // System Management
        CompanyProfile = 75,
        User = 51,
        Role = 52,
        Parameter = 73
    }

    public static class ApprovalType
    {
        // Inventory
        public static string TransferStock = "Transfer Persediaan";
        public static string Consignee = "Konsinyasi";
        public static string Adjustment = "Penyesuaian";

        // Purchase
        public static string PurchaseOrder = "Order Pembelian";
        public static string PurchaseReceive = "Penerimaan";
        public static string PurchaseInvoice = "Faktur Pembelian";
        public static string PurchaseReturn = "Retur Pembelian";

        // Sales
        public static string Promo = "Promo";
        public static string SalesOrder = "Order Penjualan";
        public static string SalesDelivery = "Surat Jalan";
        public static string SalesInvoice = "Faktur Penjualan";
        public static string SalesReturn = "Retur Penjualan";
        public static string DeliveryPlan = "Rencana Pengiriman"; 
        public static string VisitPlan = "Rencana Kunjungan";
        public static string VisitOrder = "Perintah Kunjungan";

        // Expedition
        public static string ExpeditionInvoice = "Expedition Invoice";

        // Finance
        public static string CashBank = "Kas Bank Umum";
        public static string InterCashBank = "Pemindahan Dana";

        // Accounting
        public static string GeneralJournal = "Jurnal Umum";

        // Asset Management
        public static string FixedAsset = "Aktiva Tetap";
    }
}
