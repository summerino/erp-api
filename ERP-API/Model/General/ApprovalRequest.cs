using System;
using System.Collections.Generic;

namespace ERP_API.Model.General
{
    public class VwApproval 
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
    public class ApprovalRequest
    {
        public string Code { get; set; }
        public string Type { get; set; }
    }
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
            Approvals.Add(new Approval { TableName = "AssetManagement.FixedAsset ", Description = ApprovalType.FixedAsset });
            Approvals.Add(new Approval { TableName = "Expedition.ExpeditionInvoiceHeader", Description = ApprovalType.ExpeditionInvoice });
            
            Approvals.Add(new Approval { TableName = "Inventory.AdjustmentHeader", Description = ApprovalType.Adjustment });
            Approvals.Add(new Approval { TableName = "Inventory.TransferStockHeader", Description = ApprovalType.TransferStock });
            
            Approvals.Add(new Approval { TableName = "Purchasing.PurchaseOrderHeader", Description = ApprovalType.PurchaseOrder });
            Approvals.Add(new Approval { TableName = "Purchasing.PurchaseInvoiceHeader", Description = ApprovalType.PurchaseInvoice });
            Approvals.Add(new Approval { TableName = "Purchasing.PurchaseReceiveHeader", Description = ApprovalType.PurchaseReceive });
            Approvals.Add(new Approval { TableName = "Purchasing.PurchaseReturnHeader", Description = ApprovalType.PurchaseReturn });
            
            Approvals.Add(new Approval { TableName = "Sales.SalesOrderHeader", Description = ApprovalType.SalesOrder });
            Approvals.Add(new Approval { TableName = "Sales.SalesReturnHeader", Description = ApprovalType.SalesReturn });
            Approvals.Add(new Approval { TableName = "Sales.SalesInvoiceHeader", Description = ApprovalType.SalesInvoice });
            Approvals.Add(new Approval { TableName = "Sales.SalesDeliveryHeader", Description = ApprovalType.SalesDelivery });
            Approvals.Add(new Approval { TableName = "Sales.PromoHeader", Description = ApprovalType.Promo });
            Approvals.Add(new Approval { TableName = "Sales.VisitPlanHeader", Description = ApprovalType.VisitPlan });
            Approvals.Add(new Approval { TableName = "Sales.VisitOrder", Description = ApprovalType.VisitOrder });
        }
        public List<Approval> Approvals  { get; set; }
    }
}
