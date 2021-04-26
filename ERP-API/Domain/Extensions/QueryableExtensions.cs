using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using ERP_API.Domain.Models;
using Newtonsoft.Json.Linq;

namespace ERP_API.Domain.Extensions
{
    public static class QueryableExtensions
    {
        public static DataSourceResult ToDataSourceResult<T>(this IQueryable<T> queryable, int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort, bool calcTotal = true)
        {
            var errors = new List<object>();

            // Filter the data first
            queryable = Filter(queryable, filter, errors);

            // Calculate the total number of records (needed for paging)     
            var total = calcTotal ? queryable.Count() : 0;
            
            // Sort the data
            queryable = Sort(queryable, sort);

            // Finally page the data
            if (take > 0)
            {
                queryable = Page(queryable, skip, take);
            }

            var result = new DataSourceResult
            {
                Total = total,
                Data = queryable.ToList()
            };

            // Set errors if any
            if (errors.Any())
            {
                result.Errors = errors;
            }

            return result;
        }
        
        private static IQueryable<T> Filter<T>(IQueryable<T> queryable, IEnumerable<Filter> filter, List<object> errors)
        {
            if (filter?.Any() ?? false)
            {
                // Pretreatment some work
                filter = PreliminaryWork(typeof(T), filter);

                // Get all filter values as array (needed by the Where method of Dynamic Linq)
                var values = filter.Select(f => f.Keyword).ToArray();

                // Create a predicate expression e.g. Field1 = @0 And Field2 > @1
                string predicate;
                try
                {
                    predicate = string.Join(
                        " AND ",
                        filter.Select(f => f.ToExpression(typeof(T), filter.ToList())).ToArray());
                }
                catch (Exception ex)
                {
                    errors.Add(ex.Message);
                    return queryable;
                }

                // Use the Where method of Dynamic Linq to filter the data
                queryable = queryable.Where(predicate, values);
            }

            return queryable;
        }

        private static IQueryable<T> Sort<T>(IQueryable<T> queryable, IEnumerable<Sort> sort)
        {
            if (sort?.Any() ?? false)
            {
                // Create ordering expression e.g. Field1 asc, Field2 desc
                var ordering = string.Join(",", sort.Select(s => s.ToExpression()));

                // Use the OrderBy method of Dynamic Linq to sort the data
                return queryable.OrderBy(ordering);
            }

            return queryable;
        }

        private static IQueryable<T> Page<T>(IQueryable<T> queryable, int skip, int take)
        {            
            return queryable.Skip(skip).Take(take);
        }

        private static IEnumerable<Filter> PreliminaryWork(Type type, IEnumerable<Filter> filter)
        {
            foreach (var item in filter)
            {
                var currentPropertyType = item.GetLastPropertyType(type, item.Field);

                // When we have a decimal value, it gets converted to an integer/double that will result in the query break
                if ((currentPropertyType == typeof(decimal) || currentPropertyType == typeof(decimal?)) &&
                    decimal.TryParse(item.Keyword.ToString(), out var number))
                {
                    item.Keyword = number;
                    continue;
                }

                // Convert datetime-string to DateTime
                if (currentPropertyType == typeof(DateTime) &&
                    DateTime.TryParse(item.Keyword.ToString(), out var dateTime))
                {
                    item.Keyword = dateTime;

                    // Copy the time from the filter
                    //var localTime = dateTime.ToLocalTime();

                    // Used when the datetime's operator value is eq and local time is 00:00:00 
                    //if (item.Operator == "eq")
                    //{
                    //    if (localTime.Hour != 0 || localTime.Minute != 0 || localTime.Second != 0)
                    //        continue;

                    //    var newFilters = new List<Filter>
                    //    {
                    //        // Instead of comparing for exact equality, we compare as greater than the start of the day...
                    //        new Filter
                    //        {
                    //            Field = item.Field,
                    //            Operator = "gte",
                    //            Keyword = new DateTime(localTime.Year, localTime.Month, localTime.Day, 0, 0, 0)
                    //        },
                    //        // ...and less than the end of that same day (we're making an additional filter here)
                    //        new Filter
                    //        {
                    //            Field = item.Field,
                    //            Operator = "lte",
                    //            Keyword = new DateTime(localTime.Year, localTime.Month, localTime.Day, 23, 59, 59)
                    //        }
                    //    };
                    //}

                    // Convert datetime to local 
                    //item.Keyword = new DateTime(localTime.Year, localTime.Month, localTime.Day, localTime.Hour, localTime.Minute, localTime.Second, localTime.Millisecond);
                }

                switch (item.Keyword)
                {
                    case JArray v:
                        if (currentPropertyType == typeof(int))
                        {
                            item.Keyword = v.Select(x => Convert.ChangeType(x, currentPropertyType)).Cast<int>().ToArray();
                        }
                        else if (currentPropertyType == typeof(long))
                        {
                            item.Keyword = v.Select(x => Convert.ChangeType(x, currentPropertyType)).Cast<long>().ToArray();
                        }
                        else if (currentPropertyType == typeof(short))
                        {
                            item.Keyword = v.Select(x => Convert.ChangeType(x, currentPropertyType)).Cast<short>().ToArray();
                        }
                        else
                        {
                            item.Keyword = v.Select(x => Convert.ChangeType(x, currentPropertyType)).ToArray();
                        }
                        break;
                    default:
                        item.Keyword = item.Keyword;
                        break;
                }
            }

            return filter;
        }
    }
}
