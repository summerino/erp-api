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
            new() { Code = "DPC", ActionId = (int)Actions.CbTypeSalesDownPayment },
            new() { Code = "RDPC", ActionId = (int)Actions.CbTypeSalesDownPaymentReturn },
            new() { Code = "DPS", ActionId = (int)Actions.CbTypePurchaseDownPayment },
            new() { Code = "RDPS", ActionId = (int)Actions.CbTypePurchaseDownPaymentReturn },
            new() { Code = "SR", ActionId = (int)Actions.CbTypeSalesReturn },
            new() { Code = "PR", ActionId = (int)Actions.CbTypePurchaseReturn }
        };
    }

    public List<CbTypeToAction> CbTypeToActions { get; set; }
}