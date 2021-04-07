using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace ERP_API.Entities
{
    public class BaseEntity
    {
        public int CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public int UpdatedBy { get; set; }

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
