using System.Collections.Generic;
using ERP.Entity.Core;
using ERP.Web.API.Model;

namespace ERP.Web.API.Domain.Models.General
{
    public class Approval
    {
        public string TableName { get; set; }

        public int ActionId { get; set; }
    }

    public class MapApproval
    {
        public MapApproval()
        {
            Approvals = new List<Approval>();

            // Inventory
            Approvals.Add(new Approval { TableName = $"{Schema.Inventory}.TransferStockHeader", ActionId = (int) Actions.ApproveTransferStock });
            Approvals.Add(new Approval { TableName = $"{Schema.Inventory}.TransferStockHeader", ActionId = (int) Actions.ApproveConsignee });
            Approvals.Add(new Approval { TableName = $"{Schema.Inventory}.AdjustmentHeader", ActionId = (int) Actions.ApproveAdjustment });

            // Purchase
            Approvals.Add(new Approval { TableName = $"{Schema.Purchasing}.PurchaseOrderHeader", ActionId = (int) Actions.ApprovePurchaseOrder });
            Approvals.Add(new Approval { TableName = $"{Schema.Purchasing}.PurchaseReceiveHeader", ActionId = (int) Actions.ApprovePurchaseReceive });
            Approvals.Add(new Approval { TableName = $"{Schema.Purchasing}.PurchaseInvoiceHeader", ActionId = (int) Actions.ApprovePurchaseInvoice });
            Approvals.Add(new Approval { TableName = $"{Schema.Purchasing}.PurchaseReturnHeader", ActionId = (int) Actions.ApprovePurchaseReturn });

            // Sales
            Approvals.Add(new Approval { TableName = $"{Schema.Sales}.PromoHeader", ActionId = (int) Actions.ApprovePromo });
            Approvals.Add(new Approval { TableName = $"{Schema.Sales}.SalesOrderHeader", ActionId = (int) Actions.ApproveSalesOrder });
            Approvals.Add(new Approval { TableName = $"{Schema.Sales}.SalesDeliveryHeader", ActionId = (int) Actions.ApproveDeliveryOrder });
            Approvals.Add(new Approval { TableName = $"{Schema.Sales}.SalesInvoiceHeader", ActionId = (int) Actions.ApproveSalesInvoice });
            Approvals.Add(new Approval { TableName = $"{Schema.Sales}.SalesReturnHeader", ActionId = (int) Actions.ApproveSalesReturn });
            Approvals.Add(new Approval { TableName = $"{Schema.Sales}.DeliveryPlanHeader", ActionId = (int) Actions.ApproveDeliveryPlan });
            //Approvals.Add(new Approval { TableName = $"{Schema.Sales}.VisitPlanHeader", ActionId = (int) Actions.ApproveVisitPlan });
            Approvals.Add(new Approval { TableName = $"{Schema.Sales}.VisitOrder", ActionId = (int) Actions.ApproveVisitOrder });

            // Expedition
            Approvals.Add(new Approval { TableName = $"{Schema.Expedition}.ExpeditionInvoiceHeader", ActionId = (int) Actions.ApproveExpeditionInvoice });

            // Finance
            Approvals.Add(new Approval { TableName = $"{Schema.Finance}.GeneralCashBankHeader", ActionId = (int) Actions.ApproveGeneralCashBank });
            Approvals.Add(new Approval { TableName = $"{Schema.Finance}.GeneralCashBankHeader", ActionId = (int) Actions.ApproveInterCashBank });

            // Accounting
            Approvals.Add(new Approval { TableName = $"{Schema.Accounting}.GeneralJournalHeader", ActionId = (int) Actions.ApproveGeneralJournal });

            // Asset Management
            Approvals.Add(new Approval { TableName = $"{Schema.AssetManagement}.FixedAsset", ActionId = (int) Actions.ApproveFixedAsset });
        }

        public List<Approval> Approvals { get; set; }
    }
}
