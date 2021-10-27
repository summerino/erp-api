using ERP.Common.Models;
using ERP.Web.API.Domain.Models.Mobile.General;
using ERP.Web.API.Domain.Models.Mobile.TransactionHistory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERP.Web.API.Domain.Interfaces.Mobile.CustomerTransaction
{
    public interface ICustomerTransactionService
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,DateTime? date,string custCode);
        IEnumerable<TransactionItemDetail> GetTransactionItem(string transNo);
        CreditLimitModel GetCreditLimit(string custCode); 
       
    }
}
