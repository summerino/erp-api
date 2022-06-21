using ERP.Web.API.Model;

namespace ERP.Web.API.Domain.Models.Finance;

public class CbTypeToAction
{
    public string Code { get; set; }

    public int ActionId { get; set; }
}

public class MapCbTypeToAction
{
    public MapCbTypeToAction()
    {
        CbTypeToActions = new List<CbTypeToAction>
        {
            new() { Code = "AR", ActionId = (int)Actions.CbTypeAccountReceivable },
            new() { Code = "AP", ActionId = (int)Actions.CbTypeAccountPayable },
            new() { Code = "EPAP", ActionId = (int)Actions.CbTypeExpeditionDebt },
            new() { Code = "TU", ActionId = (int)Actions.CbTypeGeneralTransaction },
            new() { Code = "DEPC", ActionId = (int)Actions.CbTypeDepositCustomer },
            new() { Code = "RDEPC", ActionId = (int)Actions.CbTypeReturnDepositCustomer },
            new() { Code = "DEPS", ActionId = (int)Actions.CbTypeDepositSupplier },
            new() { Code = "RDEPS", ActionId = (int)Actions.CbTypeReturnDepositSupplier },
            new() { Code = "SR", ActionId = (int)Actions.CbTypeSalesReturn },
            new() { Code = "PR", ActionId = (int)Actions.CbTypePurchaseReturn }
        };
    }

    public List<CbTypeToAction> CbTypeToActions { get; set; }
}