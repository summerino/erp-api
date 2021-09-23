using ERP.Common;
using ERP.Entity.Accounting;
using System.Collections.Generic;

namespace ERP.Web.API.Domain.Interfaces.Accounting
{
    public interface IIncomeStatementFormatService : IGeneralService<IncomeStatementFormat>
    {
        object GetFormatHierarchy(string category);

        IEnumerable<IncomeStatementFormat> GetFormatLists(string category);

        SaveResult Move(IncomeStatementFormat data, string type);

        SaveResult Delete(string code, int userId);

        IEnumerable<IncomeStatementFormat> GetSubFormat(string code);

        IEnumerable<IncomeStatementFormat> GetUnSubFormat(string code, string category);

        SaveResult InsertSub(string subCode, string code, int userId);

        SaveResult RemoveSub(string subCode, string code);
    }
}
