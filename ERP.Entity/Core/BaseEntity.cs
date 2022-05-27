using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.Entity.Core;

public class BaseEntity
{
    public int CreatedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreatedDate { get; set; }

    public int UpdatedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime UpdatedDate { get; set; }
}

public class BaseEntityWithActive
{
    public bool IsActive { get; set; }

    public int CreatedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreatedDate { get; set; }

    public int UpdatedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime UpdatedDate { get; set; }
}

public class BaseEntityWithMark
{
    [StringLength(3)]
    public string Mark { get; set; }

    public int CreatedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreatedDate { get; set; }

    public int UpdatedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime UpdatedDate { get; set; }
}

public class BaseEntityWithMarkAndApproved
{
    [Column(Order = 100)]
    [StringLength(3)]
    public string Mark { get; set; }
        
    [Column(Order = 101)]
    public int CreatedBy { get; set; }

    [Column(TypeName = "datetime", Order = 102)]
    public DateTime CreatedDate { get; set; }

    [Column(Order = 103)]
    public int UpdatedBy { get; set; }

    [Column(TypeName = "datetime", Order = 104)]
    public DateTime UpdatedDate { get; set; }

    [Column(Order = 105)]
    public int? ApprovedBy { get; set; }

    [Column(TypeName = "datetime", Order = 106)]
    public DateTime? ApprovedDate { get; set; }
}

public class BaseEntityWithMarkApprovedAndViewed : BaseEntityWithMarkAndApproved
{
    [Column(Order = 101)]
    public int? ViewedBy { get; set; }

    [Column(TypeName = "datetime", Order = 101)]
    public DateTime? ViewedDate { get; set; }
}

public class BaseEntityWithMarkApprovedAndRejected
{
    [StringLength(3)]
    public string Mark { get; set; }

    public int CreatedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreatedDate { get; set; }

    public int UpdatedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime UpdatedDate { get; set; }

    public int? ApprovedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ApprovedDate { get; set; }

    public int? RejectedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? RejectedDate { get; set; }
}

public class BaseCreatedEntity
{
    public int CreatedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreatedDate { get; set; }
}

public class BaseNewCodeEntity
{
    public string Value { get; set; }
}