using Microsoft.EntityFrameworkCore;
using System;

namespace ERP.Entity.Inventory
{
    public class ReportByStockMutation
    {
        public string WarehouseCode { get; set; }

        public int ItemId { get; set; }

        public DateTime? Date { get; set; }

        public string TransCode { get; set; }

        public string SrcTrans { get; set; }

        [Precision(18, 6)]
        public decimal QtyIn { get; set; }

        [Precision(18, 6)]
        public decimal QtyOut { get; set; }

        [Precision(18, 6)]
        public decimal QtyEnd { get; set; }

        [Precision(19, 6)]
        public decimal HPP { get; set; }

        [Precision(19, 6)]
        public decimal InvIn { get; set; }

        [Precision(19, 6)]
        public decimal InvOut { get; set; }

        [Precision(19, 6)]
        public decimal InvEnd { get; set; }

        public bool IsBold { get; set; }

        public string SrcType { get; set; }

    }

    public class ReportByItem
    {
        public int Id { get; set; }

        public string Initial { get; set; }

        public string Name { get; set; }

        public string Unit { get; set; }

        [Precision(18, 6)]
        public decimal QtyBegin { get; set; }

        [Precision(18, 6)]
        public decimal QtyIn { get; set; }

        [Precision(18, 6)]
        public decimal QtyOut { get; set; }

        [Precision(18, 6)]
        public decimal QtyEnd { get; set; }

        [Precision(19, 6)]
        public decimal InvBegin { get; set; }

        [Precision(19, 6)]
        public decimal InvIn { get; set; }

        [Precision(19, 6)]
        public decimal InvOut { get; set; }

        [Precision(19, 6)]
        public decimal InvEnd { get; set; }

        public string CategoryInitial { get; set; }
    }

    public class ReportByWarehouse
    {
        public string Initial { get; set; }

        public string Code { get; set; }

        public string Name { get; set; }

        [Precision(18, 6)]
        public decimal QtyBegin { get; set; }

        [Precision(18, 6)]
        public decimal QtyIn { get; set; }

        [Precision(18, 6)]
        public decimal QtyOut { get; set; }

        [Precision(18, 6)]
        public decimal QtyEnd { get; set; }

        [Precision(19, 6)]
        public decimal InvBegin { get; set; }

        [Precision(19, 6)]
        public decimal InvIn { get; set; }

        [Precision(19, 6)]
        public decimal InvOut { get; set; }

        [Precision(19, 6)]
        public decimal InvEnd { get; set; }
    }

    public class ReportByTypeSM
    {
        public int Id { get; set; }

        public string Initial { get; set; }

        public string Name { get; set; }

        public string CategoryInitial { get; set; }

        public string Unit { get; set; }

        [Precision(18, 6)]
        public decimal QtyBegin { get; set; }

        [Precision(18, 6)]
        public decimal QtyInPO { get; set; }

        [Precision(18, 6)]
        public decimal QtyInRtn { get; set; }

        [Precision(18, 6)]
        public decimal QtyInTS { get; set; }

        [Precision(18, 6)]
        public decimal QtyInCNEE { get; set; }

        [Precision(18, 6)]
        public decimal QtyInADJ { get; set; }

        [Precision(18, 6)]
        public decimal QtyOutDO { get; set; }

        [Precision(18, 6)]
        public decimal QtyOutDI { get; set; }

        [Precision(18, 6)]
        public decimal QtyOutRtn { get; set; }

        [Precision(18, 6)]
        public decimal QtyOutTS { get; set; }

        [Precision(18, 6)]
        public decimal QtyOutCNEE { get; set; }

        [Precision(18, 6)]
        public decimal QtyOutADJ { get; set; }

        [Precision(18, 6)]
        public decimal QtyEnd { get; set; }

        [Precision(19, 6)]
        public decimal InvBegin { get; set; }

        [Precision(19, 6)]
        public decimal InvInPO { get; set; }

        [Precision(19, 6)]
        public decimal InvInRtn { get; set; }

        [Precision(19, 6)]
        public decimal InvInTS { get; set; }

        [Precision(19, 6)]
        public decimal InvInCNEE { get; set; }

        [Precision(19, 6)]
        public decimal InvInADJ { get; set; }

        [Precision(19, 6)]
        public decimal InvOutDO { get; set; }

        [Precision(19, 6)]
        public decimal InvOutDI { get; set; }

        [Precision(19, 6)]
        public decimal InvOutRtn { get; set; }

        [Precision(19, 6)]
        public decimal InvOutTS { get; set; }

        [Precision(19, 6)]
        public decimal InvOutCNEE { get; set; }

        [Precision(19, 6)]
        public decimal InvOutADJ { get; set; }

        [Precision(19, 6)]
        public decimal InvEnd { get; set; }
    }
}
