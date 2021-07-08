using ERP_API.Domain.Entities.Finance;
using System.Collections.Generic;

namespace ERP_API.Model.Finance
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
