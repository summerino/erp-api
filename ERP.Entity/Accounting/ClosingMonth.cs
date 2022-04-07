using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERP.Entity.Core;

namespace ERP.Entity.Accounting;

[Table("ClosingMonth", Schema = Schema.Accounting)]
public class ClosingMonth : BaseEntity
{
    [Key]
    [StringLength(6)]
    public string Period { get; set; }

    public bool IsClose { get; set; }
}

public class VwClosingMonth : BaseEntity
{
    public string Period { get; set; }

    public bool IsClose { get; set; }

    public string CreatedInitial { get; set; }

    public string UpdatedInitial { get; set; }

    public string PeriodName { get; set; }
}