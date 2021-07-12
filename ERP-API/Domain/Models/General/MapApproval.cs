using System.Collections.Generic;
using ERP.Entity.Core;
using ERP.Web.API.Model;

namespace ERP.Web.API.Domain.Models.General
{
    public class Approval
    {
        public string TableName { get; set; }
        public string Description { get; set; }
    }

    public class MapApproval
    {
        public MapApproval()
        {
            Approvals = new List<Approval>();
            Approvals.Add(new Approval { TableName = $"{Schema.AssetManagement}.FixedAsset ", Description = ApprovalType.FixedAsset });
            Approvals.Add(new Approval { TableName = $"{Schema.Expedition}.ExpeditionInvoiceHeader", Description = ApprovalType.ExpeditionInvoice });

            Approvals.Add(new Approval { TableName = $"{Schema.Inventory}.AdjustmentHeader", Description = ApprovalType.Adjustment });
            Approvals.Add(new Approval { TableName = $"{Schema.Inventory}.TransferStockHeader", Description = ApprovalType.TransferStock });

            Approvals.Add(new Approval { TableName = $"{Schema.Purchasing}.PurchaseOrderHeader", Description = ApprovalType.PurchaseOrder });
            Approvals.Add(new Approval { TableName = $"{Schema.Purchasing}.PurchaseInvoiceHeader", Description = ApprovalType.PurchaseInvoice });
            Approvals.Add(new Approval { TableName = $"{Schema.Purchasing}.PurchaseReceiveHeader", Description = ApprovalType.PurchaseReceive });
            Approvals.Add(new Approval { TableName = $"{Schema.Purchasing}.PurchaseReturnHeader", Description = ApprovalType.PurchaseReturn });

            Approvals.Add(new Approval { TableName = $"{Schema.Sales}.SalesOrderHeader", Description = ApprovalType.SalesOrder });
            Approvals.Add(new Approval { TableName = $"{Schema.Sales}.SalesReturnHeader", Description = ApprovalType.SalesReturn });
            Approvals.Add(new Approval { TableName = $"{Schema.Sales}.SalesInvoiceHeader", Description = ApprovalType.SalesInvoice });
            Approvals.Add(new Approval { TableName = $"{Schema.Sales}.SalesDeliveryHeader", Description = ApprovalType.SalesDelivery });
            Approvals.Add(new Approval { TableName = $"{Schema.Sales}.DeliveryPlanHeader", Description = ApprovalType.SalesDeliveryPlan });
            Approvals.Add(new Approval { TableName = $"{Schema.Sales}.PromoHeader", Description = ApprovalType.Promo });
            Approvals.Add(new Approval { TableName = $"{Schema.Sales}.VisitPlanHeader", Description = ApprovalType.VisitPlan });
            Approvals.Add(new Approval { TableName = $"{Schema.Sales}.VisitOrder", Description = ApprovalType.VisitOrder });
        }
        public List<Approval> Approvals { get; set; }
    }
}
