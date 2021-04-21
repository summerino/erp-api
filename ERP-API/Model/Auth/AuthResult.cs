using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERP_API.Model.Auth
{
    public class AuthResult
    {
        public string accessToken { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
        public string userData { get; set; }
    }
}
