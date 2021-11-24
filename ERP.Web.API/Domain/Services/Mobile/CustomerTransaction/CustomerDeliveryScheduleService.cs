using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.Mobile.CustomerDeliverySchedule;
using ERP.Web.API.Domain.Models.Mobile.CustomerDeliverySchedule;

namespace ERP.Web.API.Domain.Services.Mobile.CustomerDeliverySchedule
{
    public class CustomerDeliveryScheduleService : ICustomerDeliveryScheduleService
    {
        protected TenantContext Db;

        public CustomerDeliveryScheduleService(TenantContext db)
        {
            Db = db;
        }

        public DataSourceResult GetDataDeliveryScheduleHeader(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort, DateTime? date, string search, string custCode)
        {
            var data =
                        //from header in Db.VwMobileDeliveryItemHeaders
                        //       join dlvPlanHeader in Db.VwDeliveryPlanHeaders on header.DlvPlanCode equals dlvPlanHeader.Code
                        //       join sDlvHeader in Db.VwSalesDeliveryHeaders on header.DlvPlanCode equals sDlvHeader.Code
                        from sDlvHeader in Db.VwSalesDeliveryHeaders
                        join warehouse in Db.VwWarehouses on sDlvHeader.WarehouseCode equals warehouse.Code
                        where sDlvHeader.CustCode.Equals(custCode)
                        //&& sDlvHeader.Mark.Equals("A or INV")
                        select new DeliveryScheduleHeaderModel
                        {
                            Code = sDlvHeader.Code,
                            CustCode = custCode,
                            CustName = sDlvHeader.CustName,
                            Date = sDlvHeader.Date,
                            TransCode = sDlvHeader.TransCode,
                            ShippedBy = sDlvHeader.ShippedBy,
                            SrcTrans = sDlvHeader.SrcTrans,
                            SubTotal = sDlvHeader.SubTotal,
                            WarehouseCode = sDlvHeader.WarehouseCode,
                            WarehouseName = warehouse.Name
                        };

            //if (date != null && date != "")
            //{
            //    var date1 = DateTime.ParseExact(date, "yyyy-MM-dd", null);
            //    data = data.Where(x => x.Date.Equals(date1));
            //}
            if (date.HasValue)
            {
                data = data.Where(x => x.Date.Date.Equals(date));
            }

            return data.ToDataSourceResult(skip, take, filter, sort);
        }

        public IEnumerable<DeliveryScheduleDetailModel> GetDataDeliveryScheduleDetail(string code)
        {
            var data = from sDlvdetail in Db.VwSalesDeliveryDetails
                       where sDlvdetail.Code == code
                       select new DeliveryScheduleDetailModel
                       {
                           Code = sDlvdetail.Code,
                           ItemId = sDlvdetail.ItemId,
                           ItemInitial = sDlvdetail.ItemInitial,
                           ItemName = sDlvdetail.ItemName,
                           UnitPrice = sDlvdetail.UnitPrice,
                           UnitId = sDlvdetail.UnitId,
                           UnitName = sDlvdetail.UnitName,
                           UomId = sDlvdetail.UomId
                       };

            return data;
        }
    }
}
