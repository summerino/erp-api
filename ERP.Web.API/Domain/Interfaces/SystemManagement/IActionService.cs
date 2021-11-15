namespace ERP.Web.API.Domain.Interfaces.SystemManagement
{
    public interface IActionService
    {
        IEnumerable<Entity.SystemManagement.Action> GetData();
    }
}
