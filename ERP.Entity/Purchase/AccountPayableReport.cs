using System;

namespace ERP.Entity.Purchase
{
    public class ReportBySupplier
    {
        public string Code { get; set; }

        public string Initial { get; set; }

        public string Name { get; set; }

        public int TotalTrans { get; set; }

        public decimal TotalAmount { get; set; }

        public decimal PaidAmount { get; set; }

        public decimal RemainderAmount { get; set; }
    }

    public class ReportByReceive
    {
        public DateTime Date { get; set; }

        public DateTime DueDate { get; set; }

        public string Code { get; set; }

        public string SrcCode { get; set; }

        public string InvCode { get; set; }

        public string SupCode { get; set; }

        public string SupName { get; set; }

        public decimal TotalAmount { get; set; }

        public decimal PaidAmount { get; set; }

        public decimal RemainderAmount { get; set; }
    }
}
