using Newtonsoft.Json;
using Swift.Framework.Dtos.PageView;

namespace ERP.Web.API.Dtos;

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