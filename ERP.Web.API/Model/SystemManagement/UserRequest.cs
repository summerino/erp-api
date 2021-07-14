using ERP.Entity.SystemManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERP.Web.API.Model.SystemManagement
{
    public class UserRequest : User
    {
        public string Password { get; set; }

        public string NewPassword { get; set; }

        public string ConfirmPassword { get; set; }
    }
}
