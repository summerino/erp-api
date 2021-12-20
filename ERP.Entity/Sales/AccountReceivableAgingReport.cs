using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERP.Entity.Sales
{
    public class ReportByCustomerAging
    {
        public string Code { get; set; }

        public string Name { get; set; }

        public int? TotalTrans { get; set; }

        public decimal RemainderAmount { get; set; }

        public decimal Past90 { get; set; }

        public decimal Past61To90 { get; set; }

        public decimal Past31To60 { get; set; }

        public decimal Past15To30 { get; set; }

        public decimal Past8To14 { get; set; }

        public decimal Past1To7 { get; set; }

        public decimal DueToday { get; set; }

        public decimal Due1To7 { get; set; }

        public decimal Due8To14 { get; set; }

        public decimal Due15To30 { get; set; }

        public decimal Due31To60 { get; set; }

        public decimal Due61To90 { get; set; }

        public decimal Due90 { get; set; }
    }

    public class ReportByDeliveryARAging
    {
        public DateTime? Date { get; set; }

        public DateTime? DueDate { get; set; }

        public string Code { get; set; }

        public string SrcCode { get; set; }

        public string InvCode { get; set; }

        public string SlsName { get; set; }

        public string CustCode { get; set; }

        public string CustName { get; set; }

        public decimal RemainderAmount { get; set; }

        public decimal Past90 { get; set; }

        public decimal Past61To90 { get; set; }

        public decimal Past31To60 { get; set; }

        public decimal Past15To30 { get; set; }

        public decimal Past8To14 { get; set; }

        public decimal Past1To7 { get; set; }

        public decimal DueToday { get; set; }

        public decimal Due1To7 { get; set; }

        public decimal Due8To14 { get; set; }

        public decimal Due15To30 { get; set; }

        public decimal Due31To60 { get; set; }

        public decimal Due61To90 { get; set; }

        public decimal Due90 { get; set; }
    }
}
