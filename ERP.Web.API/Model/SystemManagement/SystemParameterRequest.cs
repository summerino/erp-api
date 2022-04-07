namespace ERP.Web.API.Model.SystemManagement;

public class SystemParameterRequest
{
    public SystemParameterRequest()
    {
        Children = new List<SystemParameterRequest>();
        ListParameters = new List<SystemParameterRequest>();
    }
    public int Id { get; set; }
    public int? ParentId { get; set; }
    public int ModuleId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Code { get; set; }
    public string DataType { get; set; }
    public int Depth { get; set; }
    public int Seq { get; set; }
    public object Value { get; set; }
    public List<SystemParameterRequest> Children { get; set; }
    public List<SystemParameterRequest> ListParameters { get; set; }
}