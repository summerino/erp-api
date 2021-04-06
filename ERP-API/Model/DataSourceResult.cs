using System.Collections;

namespace ERP_API.Model
{
    public class DataSourceResult
    {
        public IEnumerable Data { get; set; }
        public int Total { get; set; }
        public object Errors { get; set; }
    }
}
