using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERP.Entity.Core;

namespace ERP.Entity.SystemManagement
{
    [Table("JournalState", Schema = Schema.SystemManagement)]
    public class JournalState
    {
        public int Id { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime Date { get; set; }

        [Column(TypeName = "date")]
        public DateTime ProcessDate { get; set; }

        public int UserId { get; set; }

        public int Step { get; set; }

        public string Status { get; set; }

        public string Notes { get; set; }
    }
}
