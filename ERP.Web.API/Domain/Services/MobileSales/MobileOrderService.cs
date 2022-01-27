using ERP.Common;
using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Entity.MobileSales;
using ERP.Entity.Sales;
using ERP.Web.API.Domain.Interfaces.MobileSales;
using ERP.Web.API.Model.MobileSales;
using Microsoft.EntityFrameworkCore;

namespace ERP.Web.API.Domain.Services.MobileSales
{
    public class MobileOrderService : GeneralService<MobileOrderHeader>, IMobileOrderService
    {
        public MobileOrderService(TenantContext db)
            :base(db)
        {

        }

        public SaveResult Approve(List<MobileOrderHeader> data, int userId)
        {
            var result = new SaveResult(false);

            if (!data.Any())
                return new SaveResult(false, "Tidak ada data yang di proses");

            if (data.Any(x => x.Mark != "A"))
                return new SaveResult(false, "Tidak dapat menyetujui data yang sudah disetujui atau ditolak");

            using var transaction = Db.Database.BeginTransaction();
            try
            {
                foreach (var itemData in data)
                {
                    var vlData = Db.MobileVisitLogs.FirstOrDefault(x => x.Code == itemData.VisitLogCode);
                    if (vlData != null)
                    {
                        if (vlData.VisitOrderCode == null && (vlData.Scheduled || !vlData.Scheduled) && vlData.ApprovedBy == null)
                        {
                            result.Message = $"Data catatan kunjungan mobile {itemData.VisitLogCode} harus di approve terlebih dahulu.";
                            return result;
                        }

                        var voData = Db.VisitOrders.FirstOrDefault(x => x.Code == vlData.VisitOrderCode);
                        if (voData == null)
                        {
                            result.Message = $"Data perintah kunjungan {vlData.VisitOrderCode} tidak ada.";
                            return result;
                        }

                        List<long> idOrderDetail = new();
                        var newCode = GetNewCode("DI_NUM_FMT", itemData.Date);
                        var salesData = Db.Employees.FirstOrDefault(x => x.Id == itemData.SalesBy);
                        var detailData = Db.MobileOrderDetails.Where(x => x.Code == itemData.Code).ToList();
                        var detailDiscData = Db.MobileOrderDetailDiscounts.Where(x => x.Code == itemData.Code).ToList();
                        var detailFreeData = Db.MobileOrderDetailFreeGoods.Where(x => x.Code == itemData.Code).ToList();
                        var memoData = (from h in Db.SalesInvoiceCreditMemos
                                        join d in Db.CreditMemos on h.CreditMemoCode equals d.Code
                                        where h.InvCode == newCode
                                        select new
                                        {
                                            h.Id,
                                            CreditMemoCode = d.Code,
                                            d.Date,
                                            Type = d.SrcTrans,
                                            CreditMemoAmount = h.CreditMemoAmount
                                        }).Union(from h in Db.SalesInvoiceCreditMemos
                                                 join d in Db.BeginningBalanceCreditMemos on h.CreditMemoCode equals d.Code
                                                 where h.InvCode == newCode
                                                 select new
                                                 {
                                                     h.Id,
                                                     CreditMemoCode = d.Code,
                                                     d.Date,
                                                     d.Type,
                                                     CreditMemoAmount = h.CreditMemoAmount
                                                 }).ToList();
                        // Sales Order
                        var soData = new SalesOrderHeader
                        {
                            Code = newCode,
                            Date = itemData.Date,
                            CustCode = itemData.CustCode,
                            PaymentTermId = itemData.PaymentTermId,
                            SalesBy = itemData.SalesBy,
                            WarehouseCode = salesData.WarehouseCode,
                            CurrCode = itemData.CurrCode,
                            Rate = itemData.Rate,
                            SubTotal = itemData.SubTotal,
                            FinalDiscPercent = itemData.FinalDiscPercent,
                            FinalDisc = itemData.FinalDisc,
                            IncludeTax = itemData.IncludeTax,
                            TaxAmount = itemData.TaxAmount,
                            Total = itemData.Total,
                            Dpp = itemData.Dpp,
                            Notes = $"Created from Mobile Order {itemData.Code}",
                            Mark = itemData.Mark,
                            CreatedBy = itemData.CreatedBy,
                            CreatedDate = itemData.CreatedDate,
                            UpdatedBy = userId,
                            UpdatedDate = DateTime.Now,
                            ApprovedBy = userId,
                            ApprovedDate = DateTime.Now,
                            FromDirectInvoice = true
                        };

                        Db.SalesOrderHeaders.Add(soData);

                        // Credit Used
                        UpdateCreditUsed(itemData.CustCode, itemData.Total);



                        short i = 0;
                        foreach (var itemDetail in detailData)
                        {
                            var barangData = Db.Items.FirstOrDefault(x => x.Id == itemDetail.ItemId);
                            var orderDetail = new SalesOrderDetail
                            {
                                Code = newCode,
                                LineNo = ++i,
                                ItemId = itemDetail.ItemId,
                                UomId = itemDetail.UomId,
                                UnitId = itemDetail.UnitId,
                                Qty = itemDetail.Qty,
                                Length = barangData.Length,
                                Width = barangData.Width,
                                Height = barangData.Height,
                                Weight = barangData.Weight,
                                DimensionMeasurement = barangData.DimensionMeasurement,
                                WeightMeasurement = barangData.WeightMeasurement,
                                QtyDlv = 0,
                                UnitPrice = itemDetail.UnitPrice,
                                Disc = itemDetail.Disc,
                                TaxId = itemDetail.TaxId,
                                TaxAmount = itemDetail.TaxAmount,
                                NettPrice = itemDetail.NettPrice,
                                Total = itemDetail.Total,
                                Dpp = itemDetail.Dpp,
                                Notes = $"Created from Mobile Order {itemData.Code}",
                                CoaInventory = barangData.CoaInventory,
                                CoaCogs = barangData.CoaCogs,
                                CoaSls = barangData.CoaSls,
                                CoaSlsDisc = barangData.CoaSlsDisc,
                                CoaSlsReturn = barangData.CoaSlsReturn
                            };

                            Db.SalesOrderDetails.Add(orderDetail);
                            idOrderDetail.Add(orderDetail.Id);

                            if (detailDiscData.Any() || detailFreeData.Any())
                            {
                                Db.SaveChanges();
                            }

                            if (detailDiscData.Any())
                            {
                                short d = 0;
                                foreach (var discItem in detailDiscData)
                                {
                                    Db.SalesOrderDetailDiscounts.Add(new SalesOrderDetailDiscount
                                    {
                                        Code = newCode,
                                        OrderDetailId = orderDetail.Id,
                                        LineNo = ++d,
                                        PromoCode = discItem.PromoCode,
                                        PromoDetailId = discItem.PromoDetailId,
                                        Name = discItem.Name,
                                        IsPercentage = discItem.IsPercentage,
                                        Value = discItem.Value,
                                        Amount = discItem.Amount,
                                        CoaCode = barangData.CoaSlsDisc
                                    });
                                }
                                Db.SaveChanges();
                            }

                            if (detailFreeData.Any())
                            {
                                short f = 0;
                                foreach (var freeItem in detailFreeData)
                                {
                                    Db.SalesOrderDetailFreeGoods.Add(new SalesOrderDetailFreeGood
                                    {
                                        Code = newCode,
                                        OrderDetailId = orderDetail.Id,
                                        LineNo = ++f,
                                        PromoCode = freeItem.PromoCode,
                                        ItemId = freeItem.ItemId,
                                        UomId = freeItem.UomId,
                                        UnitId = freeItem.UnitId,
                                        Qty = freeItem.Qty,
                                        QtyClosed = freeItem.Qty,
                                        UnitPrice = freeItem.UnitPrice,
                                        CoaCode = barangData.CoaSlsDisc
                                    });
                                }
                                Db.SaveChanges();
                            }
                        }

                        // Sales Delivery
                        Db.SalesDeliveryHeaders.Add(new SalesDeliveryHeader
                        {
                            Code = newCode,
                            Date = itemData.Date,
                            SrcTrans = 1,
                            TransCode = newCode,
                            CustCode = itemData.CustCode,
                            WarehouseCode = salesData.WarehouseCode,
                            ShippedBy = itemData.SalesBy,
                            CurrCode = itemData.CurrCode,
                            Rate = itemData.Rate,
                            SubTotal = itemData.SubTotal,
                            FinalDiscPercent = itemData.FinalDiscPercent,
                            FinalDisc = itemData.FinalDisc,
                            IncludeTax = itemData.IncludeTax,
                            TaxAmount = itemData.TaxAmount,
                            Total = itemData.Total,
                            Dpp = itemData.Dpp,
                            Mark = "INV",
                            Notes = $"Created from Mobile Order {itemData.Code}",
                            CreatedBy = itemData.CreatedBy,
                            CreatedDate = itemData.CreatedDate,
                            UpdatedBy = itemData.UpdatedBy,
                            UpdatedDate = itemData.UpdatedDate,
                            FromDirectInvoice = true
                        });

                        short j = 0;
                        foreach (var itemDetail in detailData)
                        {
                            var barangData = Db.Items.FirstOrDefault(x => x.Id == itemDetail.ItemId);
                            var deliveryDetail = new SalesDeliveryDetail
                            {
                                Code = newCode,
                                LineNo = ++j,
                                SoDetailId = idOrderDetail[j - 1],
                                ItemId = itemDetail.ItemId,
                                UomId = itemDetail.UomId,
                                UnitId = itemDetail.UnitId,
                                Qty = itemDetail.Qty,
                                Length = barangData.Length,
                                Width = barangData.Width,
                                Height = barangData.Height,
                                Weight = barangData.Weight,
                                DimensionMeasurement = barangData.DimensionMeasurement,
                                WeightMeasurement = barangData.WeightMeasurement,
                                UnitPrice = itemDetail.UnitPrice,
                                Disc = itemDetail.Disc,
                                TaxId = itemDetail.TaxId,
                                TaxAmount = itemDetail.TaxAmount,
                                NettPrice = itemDetail.NettPrice,
                                Total = itemDetail.Total,
                                Dpp = itemDetail.Dpp
                            };

                            Db.SalesDeliveryDetails.Add(deliveryDetail);

                            if (detailFreeData.Any())
                            {
                                Db.SaveChanges();
                            }

                            if (detailFreeData.Any())
                            {
                                short f = 0;
                                foreach (var freeItem in detailFreeData)
                                {
                                    Db.SalesDeliveryDetailFreeGoods.Add(new SalesDeliveryDetailFreeGood
                                    {
                                        Code = newCode,
                                        DlvOrderDetailId = deliveryDetail.Id,
                                        LineNo = ++f,
                                        PromoCode = freeItem.PromoCode,
                                        ItemId = freeItem.ItemId,
                                        UomId = freeItem.UomId,
                                        UnitId = freeItem.UnitId,
                                        Qty = freeItem.Qty,
                                        UnitPrice = freeItem.UnitPrice,
                                        CoaCode = barangData.CoaSlsDisc
                                    });

                                    var orderFreeDetail = Db.SalesOrderDetailFreeGoods.FirstOrDefault(x => x.Id == freeItem.Id);
                                    orderFreeDetail.QtyClosed += freeItem.Qty;
                                    Db.SalesOrderDetailFreeGoods.Update(orderFreeDetail);
                                }
                                Db.SaveChanges();
                            }
                        }

                        var ptData = Db.PaymentTerms.FirstOrDefault(x => x.Id == itemData.PaymentTermId);
                        // Sales Invoice
                        Db.SalesInvoiceHeaders.Add(new SalesInvoiceHeader
                        {
                            Code = newCode,
                            Date = itemData.Date,
                            DueDate = itemData.Date.AddDays(ptData?.Due ?? 0),
                            SoCode = newCode,
                            CustCode = itemData.CustCode,
                            IssuedBy = itemData.SalesBy,
                            CurrCode = itemData.CurrCode,
                            Total = itemData.Total,
                            Notes = $"Created from Mobile Order {itemData.Code}",
                            Mark = itemData.Mark,
                            PaidAmount = memoData.Sum(x => x.CreditMemoAmount),
                            CreatedBy = itemData.CreatedBy,
                            CreatedDate = itemData.CreatedDate,
                            UpdatedBy = itemData.UpdatedBy,
                            UpdatedDate = itemData.UpdatedDate,
                            FromDirectInvoice = true
                        });

                        Db.SalesInvoiceDetails.Add(new SalesInvoiceDetail
                        {
                            Code = newCode,
                            LineNo = 1,
                            DoCode = newCode,
                            SubTotal = itemData.SubTotal,
                            FinalDisc = itemData.FinalDisc,
                            TaxAmount = itemData.TaxAmount,
                            Total = itemData.Total,
                            Dpp = itemData.Dpp
                        });

                        // insert memo
                        foreach (var item in memoData)
                        {
                            Db.SalesInvoiceCreditMemos.Add(new SalesInvoiceCreditMemo
                            {
                                InvCode = newCode,
                                InvAmount = itemData.Total,
                                CreditMemoAmount = item.CreditMemoAmount,
                                CreditMemoCode = item.CreditMemoCode
                            });
                        }

                        Db.SaveChanges();

                        var DlvData = Db.SalesDeliveryHeaders.FirstOrDefault(x => x.TransCode == newCode);
                        // Execute sp_update_stock_mutation_from_so
                        Db.Database.ExecuteSqlRaw(
                            "EXEC sp_update_stock_mutation_from_so {0}, {1}",
                            newCode, itemData.Date);

                        // Execute sp_update_stock_mutation_from_do
                        Db.Database.ExecuteSqlRaw(
                            "EXEC sp_update_stock_mutation_from_do {0}, {1}, {2}",
                            DlvData.Code, itemData.Date, newCode);

                        // Execute sp_update_po_rcv_qty
                        Db.Database.ExecuteSqlRaw("EXEC sp_update_so_dlv_qty {0}", newCode);

                        // Update sales order to closed if all sales delivery are invoiced
                        if (
                            !Db.SalesDeliveryHeaders
                                .Any(x => x.TransCode == newCode && x.Mark != "INV"))
                        {
                            Db.Database.ExecuteSqlRaw(
                                "UPDATE Sales.SalesOrderHeader SET Mark='CLS' WHERE Code={0} AND Mark='CMP'", newCode);
                        }
                        UpdateCreditMemo(itemData.Code);

                        itemData.Mark = "APR";
                        itemData.SalesOrderCode = newCode;
                        itemData.ApprovedBy = userId;
                        itemData.ApprovedDate = soData.ApprovedDate;
                        Db.MobileOrderHeaders.Update(itemData);
                        Db.SaveChanges();
                    }

                    transaction.Commit();
                }
            }
            catch (Exception ex)
            {
                result.Message = ex.InnerException?.Message ?? ex.Message;
                return result;
            }
            result.Success = true;
            result.Message = "Data pesanan mobile berhasil disetujui.";
            return result;
        }

        public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts, string search)
        {
            var data = Db.VwMobileOrderHeaders.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                data = DateTime.TryParse(search, out var searchDate)
                    ? data.Where(x => x.Date == searchDate)
                    : data.Where(x =>
                        x.Code.Contains(search) || x.SalesInitial.Contains(search) || x.CustCode.StartsWith(search) ||
                        x.CustName.Contains(search));
            }

            return data.ToDataSourceResult(skip, take, filters, sorts);
        }

        public IEnumerable<VwMobileOrderDetail> GetDetailData(string code)
        {
            var data = Db.VwMobileOrderDetails.Where(x => x.Code == code);

            return data.OrderBy(x => x.LineNo);
        }

        public IEnumerable<MobileOrderDetailDiscount> GetDiscDetailData(string code)
        {
            return Db.MobileOrderDetailDiscounts.Where(x => x.Code == code).ToList();
        }

        public IEnumerable<MobileDetailFreeGoodData> GetFreeDetailData(string code)
        {
            var result = (from dc in Db.MobileOrderDetailFreeGoods
                          join i in Db.Items on dc.ItemId equals i.Id
                          join u in Db.UoMConversions on dc.UnitId equals u.Id
                          where dc.Code == code
                          select new MobileDetailFreeGoodData
                          {
                              Id = dc.Id,
                              Code = dc.Code,
                              OrderDetailId = dc.OrderDetailId,
                              LineNo = dc.LineNo,
                              PromoCode = dc.PromoCode,
                              ItemId = dc.ItemId,
                              UomId = dc.UomId,
                              UnitId = dc.UnitId,
                              Qty = dc.Qty,
                              UnitPrice = dc.UnitPrice,
                              Initial = i.Initial,
                              Name = i.Name,
                              UnitName = u.UnitEquivalent
                          }).ToList();

            return result;
        }

        public SaveResult Reject(List<MobileOrderHeader> data, int userId)
        {
            var result = new SaveResult(false);

            if (!data.Any())
                return new SaveResult(false, "Tidak ada data yang di proses");

            if (data.Any(x => x.Mark != "A"))
                return new SaveResult(false, "Tidak dapat menolak data yang sudah disetujui atau ditolak");

            foreach (var item in data)
            {
                var orderData = Db.MobileOrderHeaders.FirstOrDefault(x => x.Code == item.Code);
                orderData.RejectedBy = userId;
                orderData.RejectedDate = DateTime.Now;
                orderData.Mark = "REJ";
                Db.MobileOrderHeaders.Update(orderData);
            }

            Db.SaveChanges();

            result.Success = true;
            result.Message = "Data pesanan mobile berhasil ditolak.";
            return result;
        }

        public SaveResult Update(MobileOrderRequest data)
        {
            var result = new SaveResult(false);
            var listOrderIdDetail = new List<long>();

            using var transaction = Db.Database.BeginTransaction();
            try
            {
                Db.MobileOrderHeaders.Update(data);
                Db.Entry(data).Property(e => e.Code).IsModified = false;
                Db.Entry(data).Property(e => e.CreatedBy).IsModified = false;
                Db.Entry(data).Property(e => e.CreatedDate).IsModified = false;

                var delOrderDetails = Db.MobileOrderDetails
                    .Where(d => d.Code == data.Code && !data.ItemDetails.Select(x => x.Id).Contains(d.Id))
                    .ToList();

                Db.MobileOrderDetails.RemoveRange(delOrderDetails);

                short i = 0;
                foreach (var item in data.ItemDetails)
                {
                    if (item.Id == 0)
                    {
                        var orderDetail = new MobileOrderDetail
                        {
                            Code = item.Code,
                            LineNo = ++i,
                            ItemId = item.ItemId,
                            UomId = item.UomId,
                            UnitId = item.UnitId,
                            Qty = item.Qty,
                            UnitPrice = item.UnitPrice,
                            Disc = item.Disc,
                            TaxId = item.TaxId,
                            TaxAmount = item.TaxAmount,
                            NettPrice = item.NettPrice,
                            Total = item.Total,
                            Dpp = item.Dpp,
                        };

                        Db.MobileOrderDetails.Add(orderDetail);

                        if (item.FreeItemDetails.Any() || item.DiscountItemDetails.Any())
                        {
                            Db.SaveChanges();
                            listOrderIdDetail.Add(orderDetail.Id);
                        }
                    }
                    else
                    {
                        item.LineNo = ++i;

                        Db.MobileOrderDetails.Update(item);
                        Db.Entry(item).Property(e => e.Code).IsModified = false;

                        if (item.FreeItemDetails.Any() || item.DiscountItemDetails.Any())
                        {
                            listOrderIdDetail.Add(item.Id);
                        }
                    }

                    var delDiscDetails = Db.MobileOrderDetailDiscounts
                        .Where(d => d.Code == data.Code && d.OrderDetailId == item.Id && !item.DiscountItemDetails.Select(x => x.Id).Contains(d.Id))
                        .ToList();

                    Db.MobileOrderDetailDiscounts.RemoveRange(delDiscDetails);

                    if (item.DiscountItemDetails.Any())
                    {
                        short d = 0;
                        foreach (var discItem in item.DiscountItemDetails)
                        {
                            if (discItem.Id < 0)
                            {
                                Db.MobileOrderDetailDiscounts.Add(new MobileOrderDetailDiscount
                                {
                                    Code = data.Code,
                                    OrderDetailId = listOrderIdDetail[i - 1],
                                    LineNo = ++d,
                                    PromoCode = discItem.PromoCode,
                                    PromoDetailId = discItem.PromoDetailId,
                                    Name = discItem.Name,
                                    IsPercentage = discItem.IsPercentage,
                                    Value = discItem.Value,
                                    Amount = discItem.Amount
                                });
                            }
                            else
                            {
                                discItem.LineNo = ++d;

                                Db.MobileOrderDetailDiscounts.Update(discItem);
                                Db.Entry(discItem).Property(e => e.Id).IsModified = false;
                                Db.Entry(discItem).Property(e => e.Code).IsModified = false;
                            }
                        }
                    }

                    var delFreeDetails = Db.MobileOrderDetailFreeGoods
                        .Where(d => d.Code == data.Code && d.OrderDetailId == item.Id && !item.FreeItemDetails.Select(x => x.Id).Contains(d.Id))
                        .ToList();

                    Db.MobileOrderDetailFreeGoods.RemoveRange(delFreeDetails);

                    if (item.FreeItemDetails.Any())
                    {
                        short f = 0;
                        foreach (var freeItem in item.FreeItemDetails)
                        {
                            if (freeItem.Id < 0)
                            {
                                Db.MobileOrderDetailFreeGoods.Add(new MobileOrderDetailFreeGood
                                {
                                    Code = data.Code,
                                    OrderDetailId = listOrderIdDetail[i - 1],
                                    LineNo = ++f,
                                    PromoCode = freeItem.PromoCode,
                                    ItemId = freeItem.ItemId,
                                    UomId = freeItem.UomId,
                                    UnitId = freeItem.UnitId,
                                    Qty = freeItem.Qty,
                                    UnitPrice = freeItem.UnitPrice
                                });
                            }
                            else
                            {
                                freeItem.LineNo = ++f;

                                Db.MobileOrderDetailFreeGoods.Update(freeItem);
                                Db.Entry(freeItem).Property(e => e.Id).IsModified = false;
                                Db.Entry(freeItem).Property(e => e.Code).IsModified = false;
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
            result.Message = "Data pesanan mobile berhasil diperbarui.";
            return result;
        }

        #region Credit Used - Limit
        private void UpdateCreditUsed(string custCode, decimal total)
        {
            string query = $"update General.Customer set CreditUsed= (CreditUsed + {total}) where code = '{custCode}'";
            Db.Database.ExecuteSqlRaw(query);
        }
        #endregion

        #region Update & Restore Credit Memo
        private void UpdateCreditMemo(string code)
        {
            var listQuery = new List<string>();
            var memoData = (from h in Db.SalesInvoiceCreditMemos
                            join d in Db.CreditMemos on h.CreditMemoCode equals d.Code
                            where h.InvCode == code
                            select new
                            {
                                h.Id,
                                CreditMemoCode = d.Code,
                                d.Date,
                                Type = d.SrcTrans,
                                CreditMemoAmount = h.CreditMemoAmount
                            }).Union(from h in Db.SalesInvoiceCreditMemos
                                     join d in Db.BeginningBalanceCreditMemos on h.CreditMemoCode equals d.Code
                                     where h.InvCode == code
                                     select new
                                     {
                                         h.Id,
                                         CreditMemoCode = d.Code,
                                         d.Date,
                                         d.Type,
                                         CreditMemoAmount = h.CreditMemoAmount
                                     });
            if (memoData.Any())
            {
                var listCodeMemo = memoData.Select(x => x.CreditMemoCode).ToList();
                var listCreditMemo = GetListCreditMemo(listCodeMemo);
                foreach (var item in memoData)
                {
                    var selectedMemo = listCreditMemo.FirstOrDefault(x => x.Code.Equals(item.CreditMemoCode));
                    if (selectedMemo != null)
                    {
                        decimal used = selectedMemo.Used + item.CreditMemoAmount;
                        string status = used == selectedMemo.Amount ? "FU" : "PU";
                        string query = selectedMemo.Source == "cm" ? $"update Sales.CreditMemo set Mark = '{status}', Used = {used} where code = '{selectedMemo.Code}'" : $"update Accounting.BeginningBalanceCreditMemo set Used = {used} where code = '{selectedMemo.Code}'";
                        listQuery.Add(query);
                    }
                }
                ExecuteQuery(listQuery);
            }
        }
        private List<dynamic> GetListCreditMemo(List<string> listCodeMemo)
        {
            return (from cm in Db.CreditMemos
                    where listCodeMemo.Contains(cm.Code)
                    select new { cm.Code, cm.Amount, cm.Used, Source = "cm" })
                        .Union
                        (from cm in Db.BeginningBalanceCreditMemos
                         where listCodeMemo.Contains(cm.Code)
                         select new { cm.Code, cm.Amount, cm.Used, Source = "bb" }).ToList<dynamic>();
        }
        private void ExecuteQuery(List<string> listQuery)
        {
            if (listQuery.Any())
            {
                var querys = string.Join(";", listQuery);
                Db.Database.ExecuteSqlRaw(querys);
            }
        }
        #endregion
    }
}
