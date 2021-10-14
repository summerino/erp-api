using System;
using System.ComponentModel.DataAnnotations;

namespace ERP.Web.API.Domain.Models.Mobile.ItemRequest
{
    public class ItemRequestHeader
    {
        [Required]
        public string Code { get; set; }
        [Required]
        public DateTime Date { get; set; }
        [Required]
        public string TransferCode { get; set; }
        [Required]
        public int? AreaId1 { get; set; }
        [Required]
        public int? AreaId2 { get; set; }
        [Required]
        public int? AreaId3 { get; set; }
        [Required]
        public int? AreaId4 { get; set; }
        [Required]
        public int? AreaId5 { get; set; }
        [Required]
        public string AreaName1 { get; set; }
        [Required]
        public string AreaName2 { get; set; }
        [Required]
        public string AreaName3 { get; set; }
        [Required]
        public string AreaName4 { get; set; }
        [Required]
        public string AreaName5 { get; set; }
        [Required]
        public string Mark { get; set; }
    }
}
