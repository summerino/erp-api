using System;
using System.Collections.Generic;

namespace ERP_API.Model.General
{
    public class VwApproval 
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string SourceTrans { get; set; }
        public DateTime UpdatedDate { get; set; }
        public int Seq { get; set; }
    }
    public class ApprovalRequest
    {
        public string Code { get; set; }
        public string SourceTrans { get; set; }
    }
   
    
}
