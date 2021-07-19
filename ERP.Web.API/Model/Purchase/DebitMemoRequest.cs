using ERP.Entity.Purchase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERP.Web.API.Model.Purchase
{
    public class DebitMemoRequest : DebitMemo
    {
        public DateTime? OriginalDate { get; set; }
    }
}
