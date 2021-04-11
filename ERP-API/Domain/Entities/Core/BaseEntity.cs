using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ERP_API.Domain.Entities.Core
{
    public class BaseEntity
    {
        public int CreatedBy { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime CreatedDate { get; set; }

        public int UpdatedBy { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime UpdatedDate { get; set; }
    }

    public class BaseEntityWithActive : BaseEntity
    {
        public bool IsActive { get; set; }
    }

    public class BaseEntityWithMark : BaseEntity
    {
        [StringLength(3)]
        public string Mark { get; set; }
    }

    [Keyless]
    public class BaseNewCodeEntity
    {
        public string Value { get; set; }
    }
}
