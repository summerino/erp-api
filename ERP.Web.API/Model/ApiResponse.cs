using Swift.Framework.Dtos.PageView;

namespace ERP.Web.API.Model;

public class ApiResponse
{
    public int RowCount { get; set; }

    public List<dynamic> TableData { get; set; }

    public List<GridViewColumnSchemaDto> MetaData { get; set; }

    public string PKColumnName { get; set; }
}

public class MobileApiResponse
{
    public int Count { get; set; }

    public List<dynamic> Data { get; set; }
}

public class MobileSimpleResponse
{
    public string Message { get; set; }

    public bool Success { get; set; }
}