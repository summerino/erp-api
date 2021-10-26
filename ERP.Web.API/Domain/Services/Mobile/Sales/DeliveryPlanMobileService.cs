using ERP.Common;
using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Entity.MobileWarehouse;
using ERP.Web.API.Domain.Interfaces.Mobile.Sales;
using ERP.Web.API.Domain.Models.Mobile.Sales;
using ERP.Web.API.Domain.Services;
using Microsoft.EntityFrameworkCore;
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
                            srcTrans = dpHeader.SrcTrans,
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
            var data = from detail in Db.DeliveryPlanDetails
                       select new DeliveryPlanDetailModel
                       {
                           Code = detail.Code,
                       };
            return data.ToList();

            //var dataDlv = Db.DeliveryPlanDetails.Find(code);
            //if (dataDlv != null)
            //{

            //    // Execute sp_update_stock_mutation_from_adj
            //    Db.Database.ExecuteSqlRaw(
            //        "EXEC sp_get_dlv_plan_picking_print_data {0}, {1}",
            //        dataDlv.Code, "ITEM");
            //}

            //var data = new DeliveryPlanDetailModel
            //{
            //    Code = dataDlv.Code,
            //    ItemId = dataDlv.item,
            //    ItemName = item.Name,
            //    QtyOrder = dataDlv.Qty,
            //    QtyLoad = dataDlv.Qty,
            //    UnitId = dataDlv.UnitId,
            //    UnitEquivalent = uom.UnitEquivalent
            //};
            //yield return data;
        }


        public DataSourceResult GetLogData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort, string search, string date)
        {
            var data = from header in Db.VwMobileDeliveryItemHeaders
                       join dlvPlanHeader in Db.VwDeliveryPlanHeaders on header.DlvPlanCode equals dlvPlanHeader.Code
                       join driver in Db.VwEmployees on dlvPlanHeader.DriverId equals driver.Id
                       select new DeliverItemHeaderModel
                       {
                           Code = header.Code,
                           Date = header.CreatedDate.Date,
                           DlvPlanCode = header.DlvPlanCode,
                           DlvPlanDate = dlvPlanHeader.Date,
                           DriverId = dlvPlanHeader.VehicleId,
                           DriverName = driver.FirstName,
                           VehicleId = dlvPlanHeader.VehicleId,
                           VehicleNo = dlvPlanHeader.VehicleNo,
                           Notes = dlvPlanHeader.Notes
                       };
            return data.ToDataSourceResult(skip, take, filter, sort);
        }

        public IEnumerable<VwMobileDeliveryItemDetail> GetLogDetailData(string code)
        {
            var data = from detail in Db.VwMobileDeliveryItemDetails
                       where detail.Code.Equals(code)
                       select new VwMobileDeliveryItemDetail
                       {
                           Id = detail.Id,
                           Code = detail.Code,
                           LineNo = detail.LineNo,
                           ItemId = detail.ItemId,
                           OriginalQty = detail.OriginalQty,
                           RealizeQty = detail.RealizeQty,
                           UomId = detail.UomId,
                           UnitId = detail.UnitId,
                           ItemInitial = detail.ItemInitial,
                           ItemName = detail.ItemName,
                           ItemUomSellId = detail.ItemUomSellId,
                           ItemUomSellName = detail.ItemUomSellName,
                           ItemSellPrice = detail.ItemSellPrice,
                           UomInitial = detail.UomInitial,
                           UnitName = detail.UnitName
                       };
            return data;
        }

        public SaveResult Insert(DeliveryPlanRequestModel data, int UserId)
        {
            var result = new SaveResult(false);

            using var transaction = Db.Database.BeginTransaction();
            try
            {
                var newCode = GetNewCode("DLV_PLAN_NUM_FMT", DateTime.Now.Date);

                data.Code = newCode;

                Db.MobileDeliveryItemHeaders.Add(data);

                short i = 0;
                foreach (var dlv in data.DPDetails)
                {
                    Db.MobileDeliveryItemDetails.Add(new MobileDeliveryItemDetail
                    {
                        Code = newCode,
                        LineNo = ++i,
                        ItemId = dlv.ItemId,
                        OriginalQty = dlv.OriginalQty,
                        RealizeQty = dlv.RealizeQty,
                        UomId = dlv.UomId,
                        UnitId = dlv.UnitId
                    });
                }

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
