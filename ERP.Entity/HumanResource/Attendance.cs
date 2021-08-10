using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERP.Entity.Core;

namespace ERP.Entity.General
{
    [Table("Attendance", Schema = Schema.HumanResource)]
    public class Attendance
    {
        public long Id { get; set; }

        [Column(TypeName = "date")]
        public DateTime Date { get; set; }

        public long EmployeeId { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? CheckIn { get; set; }

        [Column(TypeName = "decimal(9, 6)")]
        public decimal? CheckInLat { get; set; }

        [Column(TypeName = "decimal(9, 6)")]
        public decimal? CheckInLng { get; set; }

        public string CheckInImage { get; set; }

        [StringLength(256)]
        public string CheckInNotes { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? CheckOut { get; set; }

        [Column(TypeName = "decimal(9, 6)")]
        public decimal? CheckOutLat { get; set; }

        [Column(TypeName = "decimal(9, 6)")]
        public decimal? CheckOutLng { get; set; }

        public string CheckOutImage { get; set; }

        [StringLength(256)]
        public string CheckOutNotes { get; set; }

        [Column(TypeName = "decimal(19, 6)")]
        public decimal? TotalHours { get; set; }
    }
}
