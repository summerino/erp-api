using ERP.Entity.Sales;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERP.Web.API.Model.Sales
{
    public class CreditMemoRequest : CreditMemo
    {
        public DateTime? OriginalDate { get; set; }
    }
}
