using System;
using System.ComponentModel.DataAnnotations.Schema;
using ERP.Entity.Core;

namespace ERP.Entity.Accounting;

[Table("PostingState", Schema = Schema.Accounting)]
public class PostingState
{
    public int Id { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime Date { get; set; }

    [Column(TypeName = "date")]
    public DateTime ProcessDate { get; set; }

    public int UserId { get; set; }

    public int Step { get; set; }

    public string Status { get; set; }

    public string Notes { get; set; }
}