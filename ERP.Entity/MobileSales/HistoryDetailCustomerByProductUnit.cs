using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERP.Entity.MobileSales
{
    public class HistoryDetailCustomerByProductUnit
    {
        public string CustCode { get; set; }
        public string CustName { get; set; }
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public int UnitId { get; set; }
        public string UnitName { get; set; }
        public int Seq { get; set; }
        public decimal Qty { get; set; }
        public decimal Total { get; set; }
        public int UomId { get; set; }
        public int UnitBuyId { get; set; }
        public string UnitBuyName { get; set; }
        public int UnitBuySeq { get; set; }
        public int UnitSellId { get; set; }
        public string UnitSellName { get; set; }
        public int UnitSellSeq { get; set; }
        public int UnitMaxId { get; set; }
        public string UnitMaxName { get; set; }
        public int UnitMaxSeq { get; set; }
        public int UnitMinId { get; set; }
        public string UnitMinName { get; set; }
        public int UnitMinSeq { get; set; }
        public int UnitBaseId { get; set; }
        public string UnitBaseName { get; set; }
        public int UnitBaseSeq { get; set; }
        public long SalesBy { get; set; }
    }
}
