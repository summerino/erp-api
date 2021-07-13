using ERP.Entity.Finance;
using System.Collections.Generic;

namespace ERP.Web.API.Model.Finance
{
    public class CashBankInterRequest : GeneralCashBankHeader
    {
        public IEnumerable<GeneralCashBankDetail> ItemDetails { get; set; }
    }
}
