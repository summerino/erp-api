using ERP.Entity.Sales;
using ERP.Web.API.Domain.Interfaces.Sales;
using ERP.Web.API.Model.Sales;
using System.Linq.Dynamic.Core;
using ERP.Common;
using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;

namespace ERP.Web.API.Domain.Services.Sales
{
    public class DeliveryPlanService : GeneralService<DeliveryPlanHeader>, IDeliveryPlanService
    {
        public DeliveryPlanService(TenantContext db)
            :base(db)
        {

        }
        public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort, string search)
        {
            var data = Db.VwDeliveryPlanHeaders.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                data = DateTime.TryParse(search, out var searchDate)
                    ? data.Where(x => x.Date == searchDate)
                    : data.Where(x =>
                        x.Code.Contains(search) || x.VehicleNo.Contains(search) || x.DriverInitial.Contains(search) ||
                        x.WarehouseInitial.Contains(search));
            }

            return data.ToDataSourceResult(skip, take, filter, sort);
        }

        public IEnumerable<DeliveryPlanDetail> GetDetailData(string code)
        {
            return Db.DeliveryPlanDetails.Where(x => x.Code == code).OrderBy(x => x.LineNo);
        }

        public IEnumerable<DeliveryPlanUndeliveredItem> GetUndeliveredData()
        {
            return Db.DeliveryPlanUndeliveredItems.ToList();
        }
        public List<dynamic> GetRelatedTransactions(string code)
        {
            throw new NotImplementedException();
        }

        public List<dynamic> GetAllTransaction(string warehousecode)
        {
            var data = (
                        new[] { new { Code = "", SoCode = "", Date = new DateTime(), Mark = "", Type = "" } }
                        ).Union(from dt in Db.VwSalesDeliveryHeaders
                                where dt.WarehouseCode == warehousecode && !new[] { "V", "INV" }.Contains(dt.Mark)
                                select new { dt.Code, SoCode = dt.TransCode, dt.Date, dt.Mark, Type = "Surat Jalan" }
                        ).Union(
                        from dt in Db.VwSalesInvoiceHeaders
                        where dt.FromDirectInvoice == true && !new[] { "V", "INV" }.Contains(dt.Mark)
                        select new { dt.Code, dt.SoCode, dt.Date, dt.Mark, Type = "Penjualan Langsung" }
                        ).Skip(1);

            return data.ToDynamicList();
        }

        public SaveResult Insert(DeliveryPlanRequest data)
        {
            var result = new SaveResult(false);

            using var transaction = Db.Database.BeginTransaction();
            try
            {
                // Get new code
                var newCode = GetNewCode("DLV_PLAN_NUM_FMT", data.Date);

                // Insert header data
                data.Code = newCode;
                Db.DeliveryPlanHeaders.Add(data);

                // Insert detail data
                short i = 0;
                foreach (var item in data.ItemDetails)
                {
                    if (Db.DeliveryPlanDetails.Any(x => x.TransCode == item.TransCode))
                    {
                        result.Message = "Data rencana pengiriman tidak bisa ditambahkan karena terdapat surat jalan yang sudah digunakan.";
                        return result;
                    }

                    var newItem = new DeliveryPlanDetail
                    {
                        Code = newCode,
                        LineNo = ++i,
                        TransCode = item.TransCode,
                        Volume = item.Volume,
                        Weight = item.Weight,
                        SrcTrans = item.SrcTrans,
                        IsFailShipment = item.IsFailShipment,
                        NotesFailShipment = item.NotesFailShipment
                    };

                    Db.DeliveryPlanDetails.Add(newItem);

                    Db.SaveChanges();

                    var idNewItem = newItem.Id;

                    short j = 0;
                    if(item.UndeliveredItems.Any())
                    {
                        foreach (var uItem in item.UndeliveredItems)
                        {
                            Db.DeliveryPlanUndeliveredItems.Add(new DeliveryPlanUndeliveredItem
                            {
                                Code = newCode,
                                DlvPlanDetailId = idNewItem,
                                LineNo = ++j,
                                ItemId = uItem.ItemId,
                                UomId = uItem.UomId,
                                UnitId = uItem.UnitId,
                                Qty = uItem.Qty,
                                WarehouseCode = uItem.WarehouseCode,
                                Type = uItem.Type
                            });
                        }
                    }
                }

                // Save changes
                Db.SaveChanges();

                transaction.Commit();
            }
            catch (Exception ex)
            {
                result.Message = ex.InnerException?.Message ?? ex.Message;
                return result;
            }

            result.Success = true;
            result.Data = data.Code;
            result.Message = "Data rencana pengiriman berhasil disimpan.";
            return result;
        }

        public SaveResult Update(DeliveryPlanRequest data)
        {
            var result = new SaveResult(false);

            using var transaction = Db.Database.BeginTransaction();
            try
            {
                if (Db.DeliveryPlanHeaders.Any(x => x.Code == data.Code && x.Mark == "V"))
                {
                    result.Message = "Data rencana pengiriman tidak bisa diubah karena data sudah ditandai sebagai void.";
                    return result;
                }

                data.ApprovedBy = null;
                data.ApprovedDate = null;

                Db.DeliveryPlanHeaders.Update(data);
                Db.Entry(data).Property(e => e.Code).IsModified = false;
                Db.Entry(data).Property(e => e.CreatedBy).IsModified = false;
                Db.Entry(data).Property(e => e.CreatedDate).IsModified = false;

                var delDetails = Db.DeliveryPlanDetails
                    .Where(d => d.Code == data.Code && !data.ItemDetails.Select(x => x.Id).Contains(d.Id))
                    .ToList();

                Db.DeliveryPlanDetails.RemoveRange(delDetails);

                var delUnDetails = Db.DeliveryPlanUndeliveredItems
                    .Where(x => delDetails.Select(d => d.Id).Contains(x.DlvPlanDetailId));

                Db.DeliveryPlanUndeliveredItems.RemoveRange(delUnDetails);

                short i = 0;
                foreach (var item in data.ItemDetails)
                {
                    if (Db.DeliveryPlanDetails.Any(x => x.TransCode == item.TransCode && x.Code != data.Code))
                    {
                        result.Message = "Data rencana pengiriman tidak bisa diperbarui karena terdapat surat jalan yang sudah digunakan.";
                        return result;
                    }

                    if (item.Id < 0)
                    {
                        var newItem = new DeliveryPlanDetail
                        {
                            Code = data.Code,
                            LineNo = ++i,
                            TransCode = item.TransCode,
                            Volume = item.Volume,
                            Weight = item.Weight,
                            SrcTrans = item.SrcTrans,
                            IsFailShipment = item.IsFailShipment,
                            NotesFailShipment = item.NotesFailShipment
                        };

                        Db.DeliveryPlanDetails.Add(newItem);

                        Db.SaveChanges();

                        var idNewItem = newItem.Id;

                        short j = 0;
                        foreach (var uItem in item.UndeliveredItems)
                        {
                            Db.DeliveryPlanUndeliveredItems.Add(new DeliveryPlanUndeliveredItem
                            {
                                Code = data.Code,
                                DlvPlanDetailId = idNewItem,
                                LineNo = ++j,
                                ItemId = uItem.ItemId,
                                UomId = uItem.UomId,
                                UnitId = uItem.UnitId,
                                Qty = uItem.Qty,
                                WarehouseCode = uItem.WarehouseCode,
                                Type = uItem.Type
                            });
                        }
                    } 
                    else
                    {
                        item.LineNo = ++i;

                        Db.DeliveryPlanDetails.Update(item);
                        Db.Entry(item).Property(e => e.Code).IsModified = false;

                        short j = 0;
                        foreach (var uItem in item.UndeliveredItems)
                        {
                            uItem.LineNo = ++j;

                            var unItem = Db.DeliveryPlanUndeliveredItems.FirstOrDefault(x => x.DlvPlanDetailId == item.Id);
                            if (unItem != null)
                            {
                                unItem.Qty = uItem.Qty;

                                Db.DeliveryPlanUndeliveredItems.Update(unItem);
                                Db.Entry(uItem).Property(e => e.Code).IsModified = false;
                            } 
                            else
                            {
                                Db.DeliveryPlanUndeliveredItems.Add(new DeliveryPlanUndeliveredItem
                                {
                                    Code = data.Code,
                                    DlvPlanDetailId = item.Id,
                                    LineNo = ++j,
                                    ItemId = uItem.ItemId,
                                    UomId = uItem.UomId,
                                    UnitId = uItem.UnitId,
                                    Qty = uItem.Qty,
                                    WarehouseCode = uItem.WarehouseCode,
                                    Type = uItem.Type
                                });
                            }
                        }
                    }
                }

                // Save changes
                Db.SaveChanges();

                transaction.Commit();

            }
            catch (Exception ex)
            {
                result.Message = ex.InnerException?.Message ?? ex.Message;
                return result;
            }

            result.Success = true;
            result.Data = data.Code;
            result.Message = "Data rencana pengiriman berhasil diperbarui.";
            return result;
        }
        public SaveResult Delete(string code, int userId)
        {
            var result = new SaveResult(false);

            var data = Db.DeliveryPlanHeaders.Find(code);
            if (data != null)
            {
                // Checking mark header data
                if (data.Mark == "V")
                {
                    result.Message = "Data rencana pengiriman tidak bisa ditandai sebagai void karena sudah ditandai sebagai void.";
                    return result;
                }

                // Update header data
                data.Mark = "V";
                data.UpdatedBy = userId;
                data.UpdatedDate = DateTime.Now;

                Db.SaveChanges();
            }

            result.Success = true;
            result.Message = "Data rencana pengiriman berhasil ditandai sebagai void.";
            return result;
        }

    }
}
