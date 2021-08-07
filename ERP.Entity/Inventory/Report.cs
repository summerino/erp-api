using System;

namespace ERP.Entity.Inventory
{
    public class ReportByStockMutation
    {
        public string WarehouseCode { get; set; }

        public int ItemId { get; set; }

        public DateTime Date { get; set; }

        public string TransCode { get; set; }

        public string SrcTrans { get; set; }

        public decimal QtyIn { get; set; }

        public decimal QtyOut { get; set; }

        public decimal QtyEnd { get; set; }
    }

    public class ReportByItem
    {
        public int Id { get; set; }

        public string Initial { get; set; }

        public string Name { get; set; }

        public string Unit { get; set; }

        public decimal QtyBegin { get; set; }

        public decimal QtyIn { get; set; }

        public decimal QtyOut { get; set; }

        public decimal QtyEnd { get; set; }
    }

    public class ReportByWarehouse
    {
        public string Initial { get; set; }

        public string Code { get; set; }

        public string Name { get; set; }

        public decimal QtyBegin { get; set; }

        public decimal QtyIn { get; set; }

        public decimal QtyOut { get; set; }

        public decimal QtyEnd { get; set; }
    }
}
