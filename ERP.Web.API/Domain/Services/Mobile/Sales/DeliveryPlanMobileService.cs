using ERP.Common;
using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Entity.MobileWarehouse;
using ERP.Web.API.Domain.Interfaces.Mobile.Sales;
using ERP.Web.API.Domain.Models.Mobile.Sales;

namespace ERP.Web.API.Domain.Services.Mobile.Sales
{
    public class DeliveryPlanMobileService : GeneralService<DeliveryPlanRequestModel>, IDeliveryPlanMobileService
    {
        private readonly TenantContext _db;
        public DeliveryPlanMobileService(TenantContext db) : base(db)
        {
            _db = db;
        }

        public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort, string search, string date, int userId)
        {
            var mobileDelivery = (from mobileDP in _db.MobileDeliveryItemHeaders
                                  select mobileDP).ToList();
            var empId = _db.Users.Where(x => x.Id.Equals(userId)).Select(y => y.EmployeeId).Single();
            var wh = _db.Employees.Where(x => x.Id.Equals(empId)).Select(y => y.WarehouseCode).Single();
            var data = (from dpHeader in _db.VwDeliveryPlanHeaders
                        join warehouse in _db.Warehouses on dpHeader.WarehouseCode equals warehouse.Code
                        where dpHeader.WarehouseCode.Equals(wh)
                        select new DeliveryPlanHeaderModel
                        {
                            Code = dpHeader.Code,
                            Date = dpHeader.Date,
                            srcTrans = dpHeader.SrcTrans,
                            WarehouseCode = dpHeader.WarehouseCode,
                            WarehouseName = warehouse.Name,
                            VehicleId = dpHeader.VehicleId,
                            Vehicle = dpHeader.VehicleNo,
                            DriverId = dpHeader.DriverId,
                            DriverName = dpHeader.DriverInitial,
                            Notes = dpHeader.Notes,
                            Mark = dpHeader.Mark,
                        }).AsQueryable();

            data = data.Where(x => !mobileDelivery.Select(t => t.DlvPlanCode).Contains(x.Code));
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
            var data_header = from dlv_pn_header in _db.DeliveryPlanHeaders
                              where dlv_pn_header.Code.Equals(code) && dlv_pn_header.Mark != "V"
                              select dlv_pn_header;

            var data_plan = from dlv_plan_h in data_header
                            join dlv_plan_d in _db.DeliveryPlanDetails on dlv_plan_h.Code equals dlv_plan_d.Code into details // left join
                            from detail in details.DefaultIfEmpty()
                            join si_h in _db.SalesInvoiceHeaders on detail.Code equals si_h.Code into invoices_header
                            from invoice_h in invoices_header.DefaultIfEmpty()
                            join si_d in _db.SalesInvoiceDetails on invoice_h.Code equals si_d.Code into invoices_detail
                            from invoice_d in invoices_detail.DefaultIfEmpty()
                            select new
                            {
                                detail.Code,
                                DOCode = (invoice_d.DoCode ?? "") == "" ? detail.TransCode : invoice_d.DoCode,
                                //detail.LineNo,
                            };

            var cte_normal_src = (from cte in data_plan
                                  join dlv_d in _db.SalesDeliveryDetails on cte.DOCode equals dlv_d.Code into normal
                                  from dlv_d in normal
                                  select new GoodsModel
                                  {
                                      Code = cte.Code,
                                      DOCode = cte.DOCode,
                                      //LineNo = cte.LineNo,
                                      ItemId = dlv_d.ItemId,
                                      UnitId = dlv_d.UnitId,
                                      Qty = dlv_d.Qty,
                                      FreeQty = (decimal)0
                                  }).ToArray();

            var cte_free_src = (from cte in data_plan
                                join dlv_d_fg in _db.SalesDeliveryDetailFreeGoods on cte.DOCode equals dlv_d_fg.Code into free
                                from dlv_d_fg in free
                                select new GoodsModel
                                {
                                    Code = cte.Code,
                                    DOCode = cte.DOCode,
                                    //LineNo = cte.LineNo,
                                    ItemId = dlv_d_fg.ItemId,
                                    UnitId = dlv_d_fg.UnitId,
                                    Qty = (decimal)0,
                                    FreeQty = dlv_d_fg.Qty
                                }).ToArray();

            var cte_union = cte_normal_src.Union(cte_free_src);

            var cte_calc_group = (from union in cte_union
                                  group union by new { union.Code, union.ItemId, union.UnitId } into un
                                  select new
                                  {
                                      un.Key.Code,
                                      //un.Key.LineNo,
                                      un.Key.ItemId,
                                      un.Key.UnitId,
                                      Qty = un.Sum(q => q.Qty),
                                      FreeQty = un.Sum(f => f.FreeQty)
                                  });

            var data_calc = (from result in (cte_calc_group)
                             join item in _db.Items on result.ItemId equals item.Id into items
                             from i in items.DefaultIfEmpty()
                             join unit in _db.UoMConversions on result.UnitId equals unit.Id into units
                             from u in units.DefaultIfEmpty()
                             select new DeliveryPlanDetailModel
                             {
                                 Code = result.Code,
                                 LineNo = 1,
                                 ItemId = result.ItemId,
                                 ItemInitial = i.Initial,
                                 ItemName = i.Name,
                                 LoadQty = result.FreeQty, // free quantity
                                 OrderQty = result.Qty,
                                 UomId = (int)i.UomId,
                                 UnitId = u.Id,
                                 UnitEquivalent = u.UnitEquivalent
                             }).AsEnumerable();

            var data = from item in data_calc
                       orderby item.ItemName
                       group item by new DeliveryPlanDetailModel
                       {
                           Code = item.Code,
                           LineNo = item.LineNo,
                           ItemId = item.ItemId,
                           ItemInitial = item.ItemInitial,
                           ItemName = item.ItemName,
                           UomId = item.UomId,
                           UnitId = item.UnitId,
                           UnitEquivalent = item.UnitEquivalent
                       } into di
                       select new DeliveryPlanDetailModel
                       {
                           Code = di.Key.Code,
                           LineNo = di.Key.LineNo,
                           ItemId = di.Key.ItemId,
                           ItemInitial = di.Key.ItemInitial,
                           ItemName = di.Key.ItemName,
                           LoadQty = di.Sum(q => q.LoadQty),
                           OrderQty = di.Sum(q => q.OrderQty),
                           UomId = di.Key.UomId,
                           UnitId = di.Key.UnitId,
                           UnitEquivalent = di.Key.UnitEquivalent
                       };

            return data;
        }

        public DataSourceResult GetLogData(int skip,
            int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort, string search, string date, int userId)
        {
            var empId = _db.Users.Where(x => x.Id.Equals(userId)).Select(y => y.EmployeeId).Single();
            var wh = _db.Employees.Where(x => x.Id.Equals(empId)).Select(y => y.WarehouseCode).Single();
            var data = from header in _db.VwMobileDeliveryItemHeaders
                       join dlvPlanHeader in _db.VwDeliveryPlanHeaders on header.DlvPlanCode equals dlvPlanHeader.Code
                       join warehouse in _db.VwWarehouses on dlvPlanHeader.WarehouseCode equals warehouse.Code
                       join driver in _db.VwEmployees on dlvPlanHeader.DriverId equals driver.Id
                       where header.WarehouseCode.Equals(wh)
                       select new DeliveryItemHeaderModel
                       {
                           Code = header.Code,
                           Date = header.CreatedDate.Date,
                           DlvPlanCode = header.DlvPlanCode,
                           DlvPlanDate = dlvPlanHeader.Date,
                           DriverId = dlvPlanHeader.DriverId,
                           DriverName = dlvPlanHeader.DriverInitial,
                           VehicleId = dlvPlanHeader.VehicleId,
                           VehicleNo = dlvPlanHeader.VehicleNo,
                           WarehouseCode = dlvPlanHeader.WarehouseCode,
                           WarehouseName = warehouse.Name,
                           Notes = dlvPlanHeader.Notes
                       };

            if (date != null && date != "")
            {
                var date1 = DateTime.ParseExact(date, "yyyy-MM-dd", null);
                data = data.Where(x => x.Date.Equals(date1));
            }

            return data.ToDataSourceResult(skip, take, filter, sort);
        }

        public IEnumerable<VwMobileDeliveryItemDetail> GetLogDetailData(string code)
        {
            var data = from detail in _db.VwMobileDeliveryItemDetails
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

        public SaveResult Insert(DeliveryPlanRequestModel data, int userId)
        {
            var result = new SaveResult(false);

            var existed_transfer_stock_code = _db.MobileDeliveryItemHeaders.Any(x => x.DlvPlanCode == data.DlvPlanCode); // check code sebelumnya
            if (!existed_transfer_stock_code)
            {
                using var transaction = _db.Database.BeginTransaction();
                try
                {
                    var date = DateTime.Now;
                    var newCode = GetNewCode("MOB_DLV_NUM_FMT", DateTime.Now.Date);

                    data.Code = newCode;

                    _db.MobileDeliveryItemHeaders.Add(new MobileDeliveryItemHeader
                    {
                        Code = newCode,
                        DlvPlanCode = data.DlvPlanCode,
                        SignatureImage = data.SignatureImage,
                        Mark = "A",
                        CreatedBy = userId,
                        CreatedDate = date,
                        UpdatedBy = userId,
                        UpdatedDate = date
                    });

                    short i = 0;
                    foreach (var dlv in data.DPDetails)
                    {
                        _db.MobileDeliveryItemDetails.Add(new MobileDeliveryItemDetail
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

                    _db.SaveChanges();

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
            else
            {
                result.Success = false;
                result.Message = "Surat Jalan sudah pernah dibuat dan disimpan";
                return result;
            }
        }
    }
}