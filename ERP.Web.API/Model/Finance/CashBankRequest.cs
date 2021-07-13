using System.Collections.Generic;
using ERP.Entity.Finance;

namespace ERP.Web.API.Model.Finance
{
    public class CashBankRequest : GeneralCashBankHeader
    {
        public CashBankRequest()
        {
            ItemDetails = new List<GeneralCashBankDetail>();
        }

        public List<GeneralCashBankDetail> ItemDetails { get; set; }
    }
}
