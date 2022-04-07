using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERP.Entity.Core;
using Microsoft.EntityFrameworkCore;

namespace ERP.Entity.HumanResource;

[Table("Attendance", Schema = Schema.HumanResource)]
public class Attendance
{
    public long Id { get; set; }

    [Column(TypeName = "date")]
    public DateTime Date { get; set; }

    public long EmployeeId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CheckIn { get; set; }

    [Precision(9, 6)]
    public decimal? CheckInLat { get; set; }

    [Precision(9, 6)]
    public decimal? CheckInLng { get; set; }

    public string CheckInImage { get; set; }

    [StringLength(256)]
    public string CheckInNotes { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CheckOut { get; set; }

    [Precision(9, 6)]
    public decimal? CheckOutLat { get; set; }

    [Precision(9, 6)]
    public decimal? CheckOutLng { get; set; }

    public string CheckOutImage { get; set; }

    [StringLength(256)]
    public string CheckOutNotes { get; set; }

    [Precision(19, 6)]
    public decimal? TotalHours { get; set; }
}

public class VwAttendanceReport
{
    public long Id { get; set; }

    public DateTime Date { get; set; }

    public long EmployeeId { get; set; }

    public DateTime? CheckIn { get; set; }

    [Precision(9, 6)]
    public decimal? CheckInLat { get; set; }

    [Precision(9, 6)]
    public decimal? CheckInLng { get; set; }

    public string CheckInImage { get; set; }

    public string CheckInNotes { get; set; }

    public DateTime? CheckOut { get; set; }

    [Precision(9, 6)]
    public decimal? CheckOutLat { get; set; }

    [Precision(9, 6)]
    public decimal? CheckOutLng { get; set; }

    public string CheckOutImage { get; set; }

    public string CheckOutNotes { get; set; }

    [Precision(19, 6)]
    public decimal? TotalHours { get; set; }


    public string Initial { get; set; }

    public string Name { get; set; }

    public short Type { get; set; }

    public string TypeName { get; set; }

    public int? SalesGroupId { get; set; }

    public string CoordinatIn { get; set; }

    public string CoordinatOut { get; set; }
}