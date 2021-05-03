using ERP_API.Domain.Entities.SystemManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERP_API.Model.SystemManagement
{
    public class UserRequest : User
    {
        public string Password { get; set; }
    }
}
