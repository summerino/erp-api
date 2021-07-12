using ERP_API.Domain.Models;
using ERP_API.Model.Accounting;
using System.Collections.Generic;
using ERP.Entity.Accounting;

namespace ERP_API.Domain.Interfaces.Accounting
{
    public interface IGeneralJournalService : IGeneralService<GeneralJournalHeader>
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
           string search);

        IEnumerable<GeneralJournalDetail> GetDetailData(string code);

        SaveResult Insert(GeneralJournalRequest data);

        SaveResult Update(GeneralJournalRequest data);

        SaveResult Delete(string code, int userId);
    }
}
