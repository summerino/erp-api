using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERP.Web.API.Domain.Models.Mobile.TransactionHistory
{
    public class TransactionHistoryBySubGroup
    {
        public DateTime Date { get; set; }
        public decimal Total { get; set; }
    }
}
