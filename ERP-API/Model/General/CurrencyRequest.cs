using System.Collections.Generic;
using ERP.Entity.General;

namespace ERP_API.Model.General
{
    public class CurrencyRequest : Currency
    {
        public int SortValue { get; set; }
    }
}
