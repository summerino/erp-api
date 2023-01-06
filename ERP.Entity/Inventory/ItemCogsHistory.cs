using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ERP.Entity.Core;

namespace ERP.Entity.Inventory;

[Table("ItemCogsHistory", Schema = Schema.Inventory)]
public class ItemCogsHistory
{
    public long Id { get; set; }

    [Column(TypeName = "date")]
    public DateTime Date { get; set; }

    public int ItemId { get; set; }

    public int UomId { get; set; }

    public int BaseUnit { get; set; }

    [Precision(18, 6)]
    public decimal SumBaseQty { get; set; }

    [Precision(22, 9)]
    public decimal SumBaseNettPrice { get; set; }
}