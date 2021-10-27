using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERP.Web.API.Domain.Models.Mobile.General
{
    public class CreditLimitModel
    {
        public decimal CreditLimit { get; set; }

        public decimal CreditUsed { get; set; }
    }
}
