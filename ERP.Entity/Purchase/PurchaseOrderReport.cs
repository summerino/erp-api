using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERP.Entity.Purchase
{
    public class ReportBySupplierPO
    {
        public string Code { get; set; }

        public string Name { get; set; }

        public int TotalTrans { get; set; }

        public decimal SubTotal { get; set; }

        public decimal Disc { get; set; }

        public decimal Dpp { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal Total { get; set; }
    }

    public class ReportByItemPO
    {
        public string Initial { get; set; }

        public string Name { get; set; }

        public int TotalTrans { get; set; }

        public decimal Qty { get; set; }

        //public string Unit { get; set; }

        public decimal SubTotal { get; set; }

        public decimal Disc { get; set; }

        public decimal DiscHeader { get; set; }

        public decimal Dpp { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal Total { get; set; }
    }

    public class ReportByPO
    {
        public DateTime? Date { get; set; }

        public string Code { get; set; }

        public string SupCode { get; set; }

        public string SupName { get; set; }

        public decimal SubTotal { get; set; }

        public decimal Disc { get; set; }

        public decimal Dpp { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal Total { get; set; }

        public string Status { get; set; }
    }

    public class ReportByDetailPO
    {
        public DateTime? Date { get; set; }

        public string Code { get; set; }

        public string SupCode { get; set; }

        public string SupName { get; set; }

        public string ItemInitial { get; set; }

        public string ItemName { get; set; }

        public decimal Qty { get; set; }

        public string Unit { get; set; }

        public decimal SubTotal { get; set; }

        public decimal Disc { get; set; }

        public decimal DiscHeader { get; set; }

        public decimal Dpp { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal Total { get; set; }

        public string Status { get; set; }
    }
}
