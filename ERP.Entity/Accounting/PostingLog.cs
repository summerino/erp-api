using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERP.Entity.Core;

namespace ERP.Entity.Accounting;

[Table("PostingLog", Schema = Schema.Accounting)]
public class PostingLog
{
    [Key]
    [StringLength(6)]
    public string Period { get; set; }

    public bool IsPosted { get; set; }

    public int? PostedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? PostedDate { get; set; }
}