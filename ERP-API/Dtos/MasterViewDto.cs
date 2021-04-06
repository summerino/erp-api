using Newtonsoft.Json;
using Swift.Framework.Dtos;
using Swift.Framework.Dtos.PageView;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ERP_API.Dtos
{
    public class MasterViewDto
    {
        [JsonProperty("rowCount")]
        public int RowCount { get; set; }

        [JsonProperty("tableData")]
        public List<dynamic> TableData { get; set; }

        [JsonProperty("metaData")]
        public List<GridViewColumnSchemaDto> MetaData { get; set; }

        [JsonProperty("PKColumnName")]
        public string PKColumnName { get; set; }
    }
}
