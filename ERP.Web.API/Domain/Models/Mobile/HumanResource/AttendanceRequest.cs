using System;
using System.ComponentModel.DataAnnotations;

namespace ERP.Web.API.Domain.Models.Mobile.HumanResource
{
    public class AttendanceRequest
    {
        [Required]
        public string Type { get; set; }
        [Required]
        public DateTime ClockTime { get; set; }
        [Required]
        public decimal Latitude { get; set; }
        [Required]
        public decimal Longitude { get; set; }
        [Required]
        public string Image { get; set; }
        public string Note { get; set; }
    }
}
