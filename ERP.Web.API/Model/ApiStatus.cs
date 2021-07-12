using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERP.Web.API.Model
{
    public class ApiStatus
    {
        public string Status { get; set; }
        public string Version { get; set; }
        public DateTime UtcTime { get; set; }
    }
}
