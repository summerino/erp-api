using Newtonsoft.Json;
using Swift.Framework.Dtos;
using Swift.Framework.Dtos.PageAddEdit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERP_API.Dtos
{
    public class MasterAddEditDto
    {
        [JsonProperty("tableData")]
        public dynamic TableData { get; set; }

        [JsonProperty("metaData")]
        public List<FormInputFieldDto> MetaData { get; set; }
    }
}
