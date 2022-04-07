using ERP.Common.Models;
using ERP.Web.API.Domain.Models.Mobile.CustomerDeliverySchedule;

namespace ERP.Web.API.Domain.Interfaces.Mobile.CustomerDeliverySchedule;

public interface ICustomerDeliveryScheduleService
{
    DataSourceResult GetDataDeliveryScheduleHeader(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort, string date, string search, string custCode);
    IEnumerable<DeliveryScheduleDetailModel> GetDataDeliveryScheduleDetail(string code);
}