using ERP.Common;
using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Entity.Core;
using ERP.Entity.MobileWarehouse;
using ERP.Entity.SQLQuery;
using ERP.Web.API.Domain.Interfaces.Mobile.Sales;
using ERP.Web.API.Domain.Models.Mobile.Sales;
using ERP.Web.API.Domain.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace ERP.Web.API.Domain.Services.Mobile.Sales
{
    public class DeliveryPlanMobileService : GeneralService<DeliveryPlanRequestModel>, IDeliveryPlanMobileService
    {
        private readonly TenantContext _db;
        public DeliveryPlanMobileService(TenantContext db) : base(db)
        {
            _db = db;
        }

        public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort, string search, string date)
        {
            var data = (from dpHeader in Db.VwDeliveryPlanHeaders
                        //join dpDetail in Db.DeliveryPlanDetails on
                        join warehouse in Db.Warehouses on dpHeader.WarehouseCode equals warehouse.Code
                        //join sup in Db.Suppliers on dpDetail.SupCode equals sup.Code
                        //join supType in Db.SupplierTypes on sup.TypeId equals supType.Id
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
                            Mark = dpHeader.Mark,
                            // CustCode = "",
                            // CustName = "",
                            // CustTypeId = 1,
                            // CustTypeName = "",
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

        public IEnumerable<DeliveryPlanDetailModel> GetDetailData(string code)
        {
            var data = from dlv_plan_d in Db.DeliveryPlanDetails
                       join dlv_plan_h in Db.DeliveryPlanHeaders on dlv_plan_d.Code equals dlv_plan_h.Code
                       //join si_h in Db.SalesInvoiceHeaders on dlv_plan_d.TransCode equals si_h.Code
                       //join si_d in Db.SalesInvoiceDetails on si_h.Code equals si_d.Code
                       join s_dlv_d in Db.SalesDeliveryDetails on dlv_plan_d.TransCode equals s_dlv_d.Code // jika dari DO maka ke 
                       join item in Db.Items on s_dlv_d.ItemId equals item.Id
                       join unit in Db.UoMConversions on item.UomId equals unit.UomId
                       where dlv_plan_h.Code.Equals(code) && dlv_plan_h.Mark != "V"
                       select new DeliveryPlanDetailModel
                       {
                           Code = dlv_plan_d.Code,
                           LineNo = dlv_plan_d.LineNo,
                           ItemId = s_dlv_d.ItemId,
                           ItemInitial = item.Initial,
                           ItemName = item.Name,
                           LoadQty = s_dlv_d.Qty, // free quantity
                           OrderQty = 0, // qty?
                           UomId = item.UomId ?? 0,
                           UnitId = unit.Id,
                           UnitEquivalent = unit.UnitEquivalent,
                           TransCode = dlv_plan_d.TransCode,
                       };
            return data.ToList();
        }

        public DataSourceResult GetLogData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort, string search, string date)
        {
            var data = from header in Db.VwMobileDeliveryItemHeaders
                       join dlvPlanHeader in Db.VwDeliveryPlanHeaders on header.DlvPlanCode equals dlvPlanHeader.Code
                       join driver in Db.VwEmployees on dlvPlanHeader.DriverId equals driver.Id
                       select new DeliveryItemHeaderModel
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
