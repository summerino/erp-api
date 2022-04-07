using Newtonsoft.Json;
using Swift.Framework.Dtos.PageAddEdit;

namespace ERP.Web.API.Dtos;

public class MasterAddEditDto
{
    [JsonProperty("tableData")]
    public dynamic TableData { get; set; }

    [JsonProperty("metaData")]
    public List<FormInputFieldDto> MetaData { get; set; }
}