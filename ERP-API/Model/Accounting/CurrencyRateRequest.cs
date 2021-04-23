using ERP_API.Domain.Entities.Accounting;
using System;
using System.ComponentModel.DataAnnotations;

namespace ERP_API.Model.Accounting
{
    public class CurrencyRateRequest : CurrencyRate
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
