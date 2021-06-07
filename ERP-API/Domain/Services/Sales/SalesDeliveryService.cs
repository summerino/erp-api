using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using Microsoft.EntityFrameworkCore;
using ERP_API.Domain.Entities;
using ERP_API.Domain.Entities.Sales;
using ERP_API.Domain.Extensions;
using ERP_API.Domain.Interfaces.Sales;
using ERP_API.Domain.Models;
using ERP_API.Model.Sales;

namespace ERP_API.Domain.Services.Sales
{
    public class SalesDeliveryService : GeneralService<SalesDeliveryHeader>, ISalesDeliveryService
    {
        public SalesDeliveryService(TenantContext db)
            : base(db)
        {
        }

        public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
            string search)
        {
            var data = Db.VwSalesDeliveryHeaders.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                data = DateTime.TryParse(search, out var searchDate)
                    ? data.Where(x => x.Date == searchDate)
                    : data.Where(x =>
                        x.Code.Contains(search) || x.CustName.Contains(search) || x.TransCode == search ||
                        x.ShippedInitial.Contains(search));
            }

            return data.ToDataSourceResult(skip, take, filter, sort);
        }

        public IEnumerable<VwSalesDeliveryDetail> GetDetailData(string code)
        {
            return Db.VwSalesDeliveryDetails.Where(x => x.Code == code).OrderBy(x => x.LineNo);
        }

        public List<dynamic> GetRelatedTransactions(string code)
        {
            var siD = from dt in Db.SalesInvoiceDetails
                      where dt.DoCode == code
                      select dt.Code;

            var data = from siH in Db.SalesInvoiceHeaders
                       where siD.Contains(siH.Code) && siH.Mark == "A"
                       select new { siH.Code, siH.Date, siH.Total };

            return data.ToDynamicList();
        }

        public IEnumerable<SalesDeliveryHeader> GetUnInvoiceData(string soCode, string invCode)
        {
            var data = Db.SalesDeliveryHeaders.Where(x => x.TransCode == soCode);

            data = string.IsNullOrWhiteSpace(invCode)
                ? data.Where(x => x.Mark == "A")
                : data.Where(x => x.Mark == "A" ||
                                  Db.SalesInvoiceDetails
                                      .Where(i => i.Code == invCode)
                                      .Select(i => i.DoCode).Contains(x.Code));

            return data;
        }

        public SaveResult Insert(SalesDeliveryRequest data)
        {
            var result = new SaveResult(false);

            using var transaction = Db.Database.BeginTransaction();
            try
            {
                // Checking sales order mark
                if (IsSalesOrderInvalid(data.TransCode))
                {
                    result.Message = "Data pengiriman penjualan tidak bisa disimpan karena data order penjualan sudah ditandai sebagai void atau tutup.";
                    return result;
                }

                // Checking deliver qty is excess or not
                if (IsQtyExcess(null, data.SrcTrans, data.TransCode, data.ItemDetails))
                {
                    result.Message = "Data pengiriman penjualan tidak bisa disimpan karena qty yg diterima lebih besar dari qty yang tersedia.";
                    return result;
                }

                // Get new code
                var newCode = GetNewCode("DO_NUM_FMT", data.Date);
                    
                // Insert header data
                data.Code = newCode;
                Db.SalesDeliveryHeaders.Add(data);

                // Insert detail data
                short i = 0;
                foreach (var item in data.ItemDetails)
                {
                    Db.SalesDeliveryDetails.Add(new SalesDeliveryDetail
                    {
                        Code = newCode,
                        LineNo = ++i,
                        SoDetailId = item.SoDetailId,
                        ItemId = item.ItemId,
                        Qty = item.Qty,
                        UomId = item.UomId,
                        UnitId = item.UnitId,
                        Length = item.Length,
                        Width = item.Width,
                        Height = item.Height,
                        Weight = item.Weight,
                        DimensionMeasurement = item.DimensionMeasurement,
                        WeightMeasurement = item.WeightMeasurement,
                        UnitPrice = item.UnitPrice,
                        Disc = item.Disc,
                        TaxId = item.TaxId,
                        TaxAmount = item.TaxAmount,
                        NettPrice = item.NettPrice,
                        Total = item.Total,
                        Dpp = item.Dpp
                    });
                }

                // Save changes
                Db.SaveChanges();

                // Execute sp_update_stock_mutation_from_do
                Db.Database.ExecuteSqlRaw(
                    "EXEC sp_update_stock_mutation_from_do {0}, {1}, {2}",
                    data.Code, data.Date, data.TransCode);

                if (data.SrcTrans == 1)
                {
                    // Execute sp_update_so_dlv_qty
                    Db.Database.ExecuteSqlRaw("EXEC sp_update_so_dlv_qty {0}", data.TransCode);
                }
                else
                {
                    // Execute sp_update_sr_rcv_qty
                    Db.Database.ExecuteSqlRaw("EXEC sp_update_sr_dlv_qty {0}", data.TransCode);
                }

                transaction.Commit();
            }
            catch (Exception ex)
            {
                result.Message = ex.InnerException?.Message ?? ex.Message;
                return result;
            }

            result.Success = true;
            result.Data = data.Code;
            result.Message = "Data pengiriman penjualan berhasil disimpan.";
            return result;
        }

        public SaveResult Update(SalesDeliveryRequest data)
        {
            var result = new SaveResult(false);

            using var transaction = Db.Database.BeginTransaction();
            try
            {
                // Checking mark header data
                if (Db.SalesDeliveryHeaders.Any(x => x.Code == data.Code && x.Mark == "V"))
                {
                    result.Message = "Data pengiriman penjualan tidak bisa diubah karena data sudah ditandai sebagai void.";
                    return result;
                }

                // Checking sales order mark
                if (IsSalesOrderInvalid(data.TransCode))
                {
                    result.Message = "Data pengiriman penjualan tidak bisa diubah karena data order penjualan sudah ditandai sebagai void atau tutup.";
                    return result;
                }

                // Checking deliver qty is excess or not
                if (IsQtyExcess(data.Code, data.SrcTrans, data.TransCode, data.ItemDetails))
                {
                    result.Message = "Data pengiriman penjualan tidak bisa disimpan karena qty yg diterima lebih besar dari qty yang tersedia.";
                    return result;
                } 

                // Update header data
                Db.SalesDeliveryHeaders.Update(data);
                Db.Entry(data).Property(e => e.Code).IsModified = false;
                Db.Entry(data).Property(e => e.CreatedBy).IsModified = false;
                Db.Entry(data).Property(e => e.CreatedDate).IsModified = false;

                // Get detail data that exists in receive before
                var delDetails = Db.SalesDeliveryDetails
                    .Where(d => d.Code == data.Code && !data.ItemDetails.Select(x => x.Id).Contains(d.Id))
                    .ToList();

                // Get detail data that exists in receive before
                Db.SalesDeliveryDetails.RemoveRange(delDetails);

                // Update detail data
                short i = 0;
                foreach (var item in data.ItemDetails)
                {
                    if (item.Id <= 0)
                    {
                        Db.SalesDeliveryDetails.Add(new SalesDeliveryDetail
                        {
                            Code = data.Code,
                            LineNo = ++i,
                            SoDetailId = item.SoDetailId,
                            ItemId = item.ItemId,
                            Qty = item.Qty,
                            UomId = item.UomId,
                            UnitId = item.UnitId,
                            Length = item.Length,
                            Width = item.Width,
                            Height = item.Height,
                            Weight = item.Weight,
                            DimensionMeasurement = item.DimensionMeasurement,
                            WeightMeasurement = item.WeightMeasurement,
                            UnitPrice = item.UnitPrice,
                            Disc = item.Disc,
                            TaxId = item.TaxId,
                            TaxAmount = item.TaxAmount,
                            NettPrice = item.NettPrice,
                            Total = item.Total,
                            Dpp = item.Dpp
                        });
                    }
                    else
                    {
                        item.LineNo = ++i;

                        Db.SalesDeliveryDetails.Update(item);
                        Db.Entry(item).Property(e => e.Code).IsModified = false;
                    }
                }

                // Save changes
                Db.SaveChanges();

                // Execute sp_update_stock_mutation_from_do
                Db.Database.ExecuteSqlRaw(
                    "EXEC sp_update_stock_mutation_from_do {0}, {1}, {2}",
                    data.Code, data.Date, data.TransCode);

                if (data.SrcTrans == 1)
                {
                    // Execute sp_update_so_dlv_qty
                    Db.Database.ExecuteSqlRaw("EXEC sp_update_so_dlv_qty {0}", data.TransCode);
                }
                else
                {
                    // Execute sp_update_sr_rcv_qty
                    Db.Database.ExecuteSqlRaw("EXEC sp_update_sr_dlv_qty {0}", data.TransCode);
                }

                transaction.Commit();
            }
            catch (Exception ex)
            {
                result.Message = ex.InnerException?.Message ?? ex.Message;
                return result;
            }

            result.Success = true;
            result.Data = data.Code;
            result.Message = "Data pengiriman penjualan berhasil diperbarui.";
            return result;
        }

        public SaveResult Delete(string code, int userId)
        {
            var result = new SaveResult(false);

            var data = Db.SalesDeliveryHeaders.Find(code);
            if (data != null)
            {
                // Checking mark header data
                if (data.Mark == "V")
                {
                    result.Message = "Data pengiriman penjualan tidak bisa ditandai sebagai void karena sudah ditandai sebagai void.";
                    return result;
                }

                using var transaction = Db.Database.BeginTransaction();
                try
                {
                    // Update header data
                    data.Mark = "V";
                    data.UpdatedBy = userId;
                    data.UpdatedDate = DateTime.Now;

                    // Save changes
                    Db.SaveChanges();

                    if (data.SrcTrans == 1)
                    {
                        // Execute sp_update_so_dlv_qty
                        Db.Database.ExecuteSqlRaw("EXEC sp_update_so_dlv_qty {0}", data.TransCode);
                    }
                    else
                    {
                        // Execute sp_update_sr_rcv_qty
                        Db.Database.ExecuteSqlRaw("EXEC sp_update_sr_dlv_qty {0}", data.TransCode);
                    }

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    result.Message = ex.InnerException?.Message ?? ex.Message;
                    return result;
                }
            }

            result.Success = true;
            result.Message = "Data pengiriman penjualan berhasil ditandai sebagai void.";
            return result;
        }

        private bool IsSalesOrderInvalid(string soCode)
        {
            return Db.SalesOrderHeaders.Any(x => x.Code == soCode && new[] { "V", "CLS" }.Contains(x.Mark));
        }

        private bool IsQtyExcess(string code, int srcTrans, string transCode, IEnumerable<SalesDeliveryDetail> items)
        {
            var result = false;
            if (srcTrans == 1) // Sales Order
            {
                foreach (var item in items)
                {
                    var dataSODetail = Db.SalesOrderDetails.FirstOrDefault(x => x.Code == transCode && x.ItemId == item.ItemId);
                    if (code == null)
                    {
                        var availableStock = dataSODetail.Qty - dataSODetail.QtyDlv;
                        if (item.Qty > availableStock)
                        {
                            result = true;
                        }
                    }
                    else
                    {
                        var oldSDD = Db.SalesDeliveryDetails.AsNoTracking().FirstOrDefault(x => x.Code == code && x.ItemId == item.ItemId);
                        var availableStock = dataSODetail.Qty - (dataSODetail.QtyDlv - oldSDD.Qty);
                        if (item.Qty > availableStock)
                        {
                            result = true;
                        }
                    }
                }
            }
            else // Sales Return
            {
                foreach (var item in items)
                {
                    var dataSRDetail = Db.SalesReturnDetails.FirstOrDefault(x => x.Code == transCode && x.ItemId == item.ItemId);
                    if (code == null)
                    {
                        var availableStock = dataSRDetail.Qty - dataSRDetail.QtyDlv;
                        if (item.Qty > availableStock)
                        {
                            result = true;
                        }
                    }
                    else
                    {
                        var oldSDD = Db.SalesDeliveryDetails.AsNoTracking().FirstOrDefault(x => x.Code == code && x.ItemId == item.ItemId);
                        var availableStock = dataSRDetail.Qty - (dataSRDetail.QtyDlv - oldSDD.Qty);
                        if (item.Qty > availableStock)
                        {
                            result = true;
                        }
                    }
                }
            }
            return result;
        }
    }
}
