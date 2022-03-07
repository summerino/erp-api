using ERP.Entity.Core;
using ERP.Web.API.Model;

namespace ERP.Web.API.Domain.Models.General
{
    public class Approval
    {
        public string TableName { get; set; }

        public int ActionId { get; set; }

        public string Src { get; set; }
    }

    public class MapApproval
    {
        public MapApproval()
        {
            Approvals = new List<Approval>();

            // Inventory
            Approvals.Add(new Approval { TableName = $"{Schema.Inventory}.TransferStockHeader", ActionId = (int) Actions.ApproveTransferStock, Src = "TS" });
            Approvals.Add(new Approval { TableName = $"{Schema.Inventory}.TransferStockHeader", ActionId = (int) Actions.ApproveConsignee, Src = "CNEE" });
            Approvals.Add(new Approval { TableName = $"{Schema.Inventory}.AdjustmentHeader", ActionId = (int) Actions.ApproveAdjustment, Src = "ADJ" });

            // Purchase
            Approvals.Add(new Approval { TableName = $"{Schema.Purchasing}.PurchaseOrderHeader", ActionId = (int) Actions.ApprovePurchaseOrder, Src = "PO" });
            Approvals.Add(new Approval { TableName = $"{Schema.Purchasing}.PurchaseReceiveHeader", ActionId = (int) Actions.ApprovePurchaseReceive, Src = "RCV" });
            Approvals.Add(new Approval { TableName = $"{Schema.Purchasing}.PurchaseInvoiceHeader", ActionId = (int) Actions.ApprovePurchaseInvoice, Src = "PI" });
            Approvals.Add(new Approval { TableName = $"{Schema.Purchasing}.PurchaseReturnHeader", ActionId = (int) Actions.ApprovePurchaseReturn, Src = "PR" });

            // Sales
            Approvals.Add(new Approval { TableName = $"{Schema.Sales}.PromoHeader", ActionId = (int) Actions.ApprovePromo, Src = "PROMO" });
            Approvals.Add(new Approval { TableName = $"{Schema.Sales}.SalesOrderHeader", ActionId = (int) Actions.ApproveSalesOrder, Src = "SO" });
            Approvals.Add(new Approval { TableName = $"{Schema.Sales}.SalesDeliveryHeader", ActionId = (int) Actions.ApproveDeliveryOrder, Src = "DO" });
            Approvals.Add(new Approval { TableName = $"{Schema.Sales}.SalesInvoiceHeader", ActionId = (int) Actions.ApproveSalesInvoice, Src = "SI" });
            Approvals.Add(new Approval { TableName = $"{Schema.Sales}.SalesInvoiceHeader", ActionId = (int)Actions.ApproveSalesInvoice, Src = "DI" });
            Approvals.Add(new Approval { TableName = $"{Schema.Sales}.SalesReturnHeader", ActionId = (int) Actions.ApproveSalesReturn, Src = "SR" });
            Approvals.Add(new Approval { TableName = $"{Schema.Sales}.DeliveryPlanHeader", ActionId = (int) Actions.ApproveDeliveryPlan, Src = "DP" });
            //Approvals.Add(new Approval { TableName = $"{Schema.Sales}.VisitPlanHeader", ActionId = (int) Actions.ApproveVisitPlan, Src = "VP" });
            Approvals.Add(new Approval { TableName = $"{Schema.Sales}.VisitOrder", ActionId = (int) Actions.ApproveVisitOrder, Src = "VO" });

            // Expedition
            Approvals.Add(new Approval { TableName = $"{Schema.Expedition}.ExpeditionInvoiceHeader", ActionId = (int) Actions.ApproveExpeditionInvoice, Src = "EI" });

            // Finance
            Approvals.Add(new Approval { TableName = $"{Schema.Finance}.GeneralCashBankHeader", ActionId = (int) Actions.ApproveGeneralCashBank, Src = "CB" });
            Approvals.Add(new Approval { TableName = $"{Schema.Finance}.GeneralCashBankHeader", ActionId = (int) Actions.ApproveInterCashBank, Src = "ICB" });

            // Accounting
            Approvals.Add(new Approval { TableName = $"{Schema.Accounting}.GeneralJournalHeader", ActionId = (int) Actions.ApproveGeneralJournal, Src = "GEN-JR" });

            // Asset Management
            Approvals.Add(new Approval { TableName = $"{Schema.AssetManagement}.FixedAsset", ActionId = (int) Actions.ApproveFixedAsset, Src = "FA" });
        }

        public List<Approval> Approvals { get; set; }
    }
}
