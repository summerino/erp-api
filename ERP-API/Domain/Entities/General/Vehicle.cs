using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERP_API.Domain.Entities.Core;

namespace ERP_API.Domain.Entities.General
{
    [Table("Vehicle", Schema = Schema.General)]
    public class Vehicle : BaseEntityWithActive
    {
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string VehicleNo { get; set; }
        
        public int TypeId { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal MaxLoadVolume { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal MaxLoadWeight { get; set; }

        public long DriverId { get; set; }

        public long? HelperId1 { get; set; }

        public long? HelperId2 { get; set; }

        [StringLength(256)]
        public string Notes { get; set; }
    }

    public class VwVehicle : BaseEntityWithActive
    {
        public int Id { get; set; }

        public string VehicleNo { get; set; }

        public int TypeId { get; set; }

        public decimal MaxLoadVolume { get; set; }

        public decimal MaxLoadWeight { get; set; }

        public long DriverId { get; set; }

        public string DriverInitial { get; set; }

        public long? HelperId1 { get; set; }

        public long? HelperId2 { get; set; }

        public string Notes { get; set; }


        public string TypeName { get; set; }

        public string UpdatedInitial { get; set; }
    }

    [Table("VehicleType", Schema = Schema.General)]
    public class VehicleType : BaseEntityWithActive
    {
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string Initial { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }
    }

    public class VwVehicleType : BaseEntityWithActive
    {
        public int Id { get; set; }

        public string Initial { get; set; }

        public string Name { get; set; }


        public string UpdatedInitial { get; set; }
    }
}
