using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERP_API.Model.Auth
{
    public class JwtConfig
    {
        public string Secret { get; set; }
        public int TimeInMinute { get; set; }
        public string Issuer { get; set; }
    }
}
