using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERP.Entity.Purchase
{
    public class ReportBySupplierMutation
    {
        public string Code { get; set; }

        public string Name { get; set; }

        public int? TotalTrans { get; set; }

        public decimal BeginningBalance { get; set; }

        public decimal TransAmount { get; set; }

        public decimal PaidAmount { get; set; }

        public decimal EndingBalance { get; set; }
    }

    public class ReportByInvoiceAPMutation
    {
        public DateTime? Date { get; set; }

        public DateTime? DueDate { get; set; }

        public string Code { get; set; }

        public string OrderCode { get; set; }

        public string SupCode { get; set; }

        public string SupName { get; set; }

        public decimal BeginningBalance { get; set; }

        public decimal TransAmount { get; set; }

        public decimal PaidAmount { get; set; }

        public decimal EndingBalance { get; set; }
    }
}
