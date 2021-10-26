using ERP.Common;
using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.Mobile.Sales;
using ERP.Web.API.Domain.Models.Mobile.Sales;
using ERP.Web.API.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERP.Web.API.Domain.Services.Mobile.Sales
{
    public class DeliveryPlanMobileService : GeneralService<DeliveryPlanRequestModel>, IDeliveryPlanMobileService
    {
        public DeliveryPlanMobileService(TenantContext db) : base(db)
        {
            Db = db;
        }

        public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort, string search, string date)
        {
            var data = (from dpHeader in Db.VwDeliveryPlanHeaders
                        join warehouse in Db.Warehouses on dpHeader.WarehouseCode equals warehouse.Code
                        select new DeliveryPlanHeaderModel
                        {
                            Code = dpHeader.Code,
                            Date = dpHeader.Date,
                            WarehouseCode = dpHeader.Code,
                            WarehouseName = warehouse.Name,
                            VehicleId = dpHeader.VehicleId,
                            Vehicle = dpHeader.VehicleNo,
                            DriverId = dpHeader.DriverId,
                            DriverName = dpHeader.DriverInitial,
                            Notes = dpHeader.Notes,
                            Mark = dpHeader.Mark
                        }).AsQueryable();

            data = data.Where(x => x.Mark != "CMP" && x.Mark != "CLS" && x.Mark != "V");

            if (!string.IsNullOrEmpty(search))
            {
                data = data.Where(x => x.Code.Contains(search));
            }

            if (date != null && date != "")
            {
                var date1 = DateTime.ParseExact(date, "yyyy-MM-dd", null);
                data = data.Where(x => x.Date.Equals(date1));
            }

            return data.ToDataSourceResult(skip, take, filter, sort);
        }

        public IEnumerable<DeliveryPlanDetailModel> GetDetailData(string code, int srcTrans)
        {
            if (srcTrans == 1)
            {
                var data = from detail in Db.DeliveryPlanDetails
                           join doHeader in Db.SalesDeliveryHeaders on detail.TransCode equals doHeader.Code
                           join doDetail in Db.SalesDeliveryDetails on doHeader.Code equals doDetail.Code
                           join item in Db.Items on doDetail.ItemId equals item.Id
                           join uom in Db.UoMConversions on item.UomId equals uom.Id
                           where detail.Code.Equals(code)
                           select new DeliveryPlanDetailModel
                           {
                               Code = detail.Code,
                               ItemId = doDetail.ItemId,
                               ItemName = item.Name,
                               QtyOrder = doDetail.Qty,
                               QtyLoad = doDetail.Qty,
                               UnitId = doDetail.UnitId,
                               UnitEquivalent = uom.UnitEquivalent
                           };
                return data.ToList();
            }
            else
            {
                var data = from detail in Db.DeliveryPlanDetails
                               //join doHeader in Db.SalesDeliveryHeaders on detail.TransCode equals doHeader.Code
                               //join doDetail in Db.SalesDeliveryDetails on doHeader.Code equals doDetail.Code
                               //join item in Db.Items on doDetail.ItemId equals item.Id
                               //join uom in Db.UoMConversions on item.UomId equals uom.Id
                               //where detail.Code.Equals(code)
                           select new DeliveryPlanDetailModel
                           {
                               Code = detail.Code,
                               //ItemId = doDetail.ItemId,
                               //ItemName = item.Name,
                               //QtyOrder = doDetail.Qty,
                               //QtyLoad = doDetail.Qty,
                               //UnitId = doDetail.UnitId,
                               //UnitEquivalent = uom.UnitEquivalent
                           };
                return data.ToList();
            }
        }


        public DataSourceResult GetLogData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort, string search, string date)
        {
            var data = Db.MobileReceiveItemHeaders.Select(header => new DeliverItemHeaderModel
            {
                Code = header.Code,
                Date = header.Date,
                //RcvCode = header.RcvCode,
                //ReceiveBy = header.ReceiveBy,
                SrcTrans = header.SrcTrans,
                SupCode = header.SupCode,
                TransCode = header.TransCode
            });

            return data.ToDataSourceResult(skip, take, filter, sort);
        }

        public IEnumerable<DeliverItemDetailModel> GetLogDetailData(string code)
        {
            var data = Db.MobileReceiveItemDetails.Where(x => x.Code == code).Select(detail => new DeliverItemDetailModel
            {
                Code = detail.Code,
                Id = detail.Id,
                ItemId = detail.ItemId,
                LineNo = detail.LineNo,
                Qty = detail.Qty,
                TransDetailId = detail.TransDetailId,
                Type = detail.Type,
                UnitId = detail.UnitId,
                UomId = detail.UomId,
                //WarehouseCode = detail.WarehouseCode
                //column apa saja?
            });

            return data;
        }


        public SaveResult Insert(DeliveryPlanRequestModel data, int UserId)
        {
            var result = new SaveResult(false);

            using var transaction = Db.Database.BeginTransaction();
            try
            {
                //var newCode = GetNewCode("DLV_PLAN_NUM_FMT", data.Date);

                //data.Code = newCode;

                //Db.MobileReceiveItemHeaders.Add(data);

                //short i = 0;
                //foreach (var rcv in data.)
                //{
                //    Db.MobileReceiveItemDetails.Add(new MobileReceiveItemDetail
                //    {
                //        Code = newCode,
                //        LineNo = ++i,
                //        ItemId = rcv.ItemId,
                //        Qty = rcv.Qty,
                //        TransDetailId = rcv.TransDetailId,
                //        Type = rcv.Type,
                //        UnitId = rcv.UnitId,
                //        UomId = rcv.UomId,
                //        WarehouseCode = rcv.WarehouseCode,
                //    });
                //}

                Db.SaveChanges();

                transaction.Commit();
            }
            catch (Exception ex)
            {
                result.Message = ex.InnerException?.Message ?? ex.Message;
                return result;
            }

            result.Success = true;
            result.Data = data;
            result.Message = "Data pengeluaran barang berhasil disimpan.";
            return result;
        }
    }
}
