using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using ERP.Common;
using ERP.Common.Models;

namespace ERP.Web.API.Domain.Interfaces
{
    public interface IGeneralService<T> where T : class
    {
        DataSourceResult GetData(int take, int skip, IEnumerable<Filter> filter, IEnumerable<Sort> sort, List<int> list);

        DataSourceResult GetData<TEntity>(int take, int skip, IEnumerable<Filter> filter, IEnumerable<Sort> sort)
            where TEntity : class;

        SaveResult Insert(T data);

        SaveResult Update(T data);

        SaveResult Update(T data, params Expression<Func<T, object>>[] properties);

        SaveResult ReverseUpdate(T data, params Expression<Func<T, object>>[] properties);

        string GetNewCode(string code, DateTime? date = null);
    }
}
