using System;

namespace ERP.Entity.Expedition
{
    public class ReportByExpeditionSupplier
    {
        public string Code { get; set; }

        public string Initial { get; set; }

        public string Name { get; set; }

        public int TotalTrans { get; set; }

        public decimal TotalAmount { get; set; }

        public decimal PaidAmount { get; set; }

        public decimal RemainderAmount { get; set; }
    }

    public class ReportByExpeditionInvoice
    {
        public DateTime Date { get; set; }

        public DateTime DueDate { get; set; }

        public string Code { get; set; }

        public string SupCode { get; set; }

        public string SupName { get; set; }

        public decimal TotalAmount { get; set; }

        public decimal PaidAmount { get; set; }

        public decimal RemainderAmount { get; set; }
    }
}
