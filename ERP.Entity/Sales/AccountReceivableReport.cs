using System;

namespace ERP.Entity.Sales
{
    public class ReportByCustomer
    {
        public string Code { get; set; }

        public string Initial { get; set; }

        public string Name { get; set; }

        public int TotalTrans { get; set; }

        public decimal TotalAmount { get; set; }

        public decimal PaidAmount { get; set; }

        public decimal RemainderAmount { get; set; }
    }

    public class ReportByDelivery
    {
        public DateTime Date { get; set; }

        public DateTime DueDate { get; set; }

        public string Code { get; set; }

        public string SrcCode { get; set; }

        public string InvCode { get; set; }

        public string SlsInitial { get; set; }

        public string SlsName { get; set; }

        public string CustCode { get; set; }

        public string CustName { get; set; }

        public decimal TotalAmount { get; set; }

        public decimal PaidAmount { get; set; }

        public decimal RemainderAmount { get; set; }
    }
}
