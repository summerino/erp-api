using System;
using System.ComponentModel.DataAnnotations;
using ERP.Entity.Accounting;

namespace ERP_API.Model.Accounting
{
    public class CurrencyRateRequest : CurrencyRate
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
