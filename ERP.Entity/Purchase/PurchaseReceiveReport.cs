using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERP.Entity.Purchase
{
    public class ReportByRCV
    {
        public DateTime? Date { get; set; }

        public string Code { get; set; }

        public int SrcTrans { get; set; }

        public string TransCode { get; set; }

        public string RefNo { get; set; }

        public string SupCode { get; set; }

        public string SupName { get; set; }

        public decimal SubTotal { get; set; }

        public decimal Disc { get; set; }

        public decimal Dpp { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal Total { get; set; }
    }

    public class ReportByDetailRCV
    {
        public DateTime? Date { get; set; }

        public string Code { get; set; }

        public int SrcTrans { get; set; }

        public string TransCode { get; set; }

        public string RefNo { get; set; }

        public string SupCode { get; set; }

        public string SupName { get; set; }

        public string ItemInitial { get; set; }

        public string ItemName { get; set; }

        public string WarehouseName { get; set; }

        public decimal Qty { get; set; }

        public string Unit { get; set; }

        public decimal SubTotal { get; set; }

        public decimal Disc { get; set; }

        public decimal DiscHeader { get; set; }

        public decimal Dpp { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal Total { get; set; }
    }
}
