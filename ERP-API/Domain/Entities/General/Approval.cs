using System;

namespace ERP_API.Domain.Entities.General
{
    public class VwApproval
    {
        public string Code { get; set; }

        public string Name { get; set; }

        public string SourceTrans { get; set; }

        public DateTime UpdatedDate { get; set; }

        public int Seq { get; set; }
    }
}
