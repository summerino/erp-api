using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace ERP_API.Enum
{
    public enum UserRole
    {
        [Description("Standard")]
        Standard = 1,
        [Description("SuperUser")]
        SuperUser = 10
    }
}
