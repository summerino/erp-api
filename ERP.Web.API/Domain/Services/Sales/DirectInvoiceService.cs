using System.Linq.Dynamic.Core;
using Microsoft.EntityFrameworkCore;
using ERP.Common;
using ERP.Entity;
using ERP.Entity.Sales;
using ERP.Web.API.Domain.Interfaces.Sales;
using ERP.Web.API.Model.Sales;

namespace ERP.Web.API.Domain.Services.Sales;

public class DirectInvoiceService : GeneralService<SalesInvoiceHeader>, IDirectInvoiceService
{
    public DirectInvoiceService(TenantContext db)
        : base(db)
    {
    }

    public DirectInvoiceRequest FindByCode(string code)
    {
        var invData = Db.VwSalesInvoiceHeaders.FirstOrDefault(x => x.Code == code && x.FromDirectInvoice);

        if (invData == null)
            return null;

        var ordData = Db.SalesOrderHeaders.FirstOrDefault(x => x.Code == invData.SoCode);

        return new DirectInvoiceRequest
        {
            Code = invData.Code,
            Date = invData.Date,
            DueDate = invData.DueDate,
            SoCode = invData.SoCode,
            CurrCode = invData.CurrCode,
            CustCode = invData.CustCode,
            PaidAmount = invData.PaidAmount,
            Total = invData.Total,
            TaxInvoiceNo = invData.TaxInvoiceNo,
            TaxInvoiceDate = invData.TaxInvoiceDate,
            Notes = invData.Notes,
            FromDirectInvoice = invData.FromDirectInvoice,
            Mark = invData.Mark,
            CreatedBy = invData.CreatedBy,
            CreatedDate = invData.CreatedDate,
            UpdatedBy = invData.UpdatedBy,
            UpdatedDate = invData.UpdatedDate,
            ApprovedBy = invData.ApprovedBy,
            ApprovedDate = invData.ApprovedDate,
            CustName = invData.CustName,
            CreatedInitial = invData.CreatedInitial,
            UpdatedInitial = invData.UpdatedInitial,
            ApprovedInitial = invData.ApprovedInitial,
            Status = invData.Status,
            PaymentTermId = ordData.PaymentTermId,
            SalesBy = ordData?.SalesBy ?? 0,
            WarehouseCode = ordData?.WarehouseCode,
            ShipmentFee = ordData?.ShipmentFee ?? 0m,
            HandlingFee = ordData?.HandlingFee ?? 0m,
            SubTotal = ordData?.SubTotal ?? 0m,
            FinalDiscPercent = ordData?.FinalDiscPercent ?? 0m,
            FinalDisc = ordData?.FinalDisc ?? 0m,
            IncludeTax = ordData?.IncludeTax ?? false,
            TaxAmount = ordData?.TaxAmount ?? 0m,
            ExemptTaxAmount = ordData?.ExemptTaxAmount ?? 0m,
            Dpp = ordData?.Dpp ?? 0m
        };
    }
    public List<dynamic> GetRelatedTransactions(string code)
    {
        var dpD = from dlv in Db.DeliveryPlanDetails
            where dlv.TransCode == code
            select dlv.Code;

        var data = (new[] { new { Code = "", Date = new DateTime(), Total = (decimal)0, Type = "" } })
            .Union(from h in Db.GeneralCashBankHeaders
                join d in Db.GeneralCashBankDetails on h.Code equals d.Code
                where h.Mark == "A" && d.TransCode == code
                select new
                {
                    h.Code,
                    h.Date,
                    Total = h.Amount,
                    Type = "Kas Bank"
                }).Union(from dpH in Db.DeliveryPlanHeaders
                where dpD.Contains(dpH.Code) && dpH.Mark != "V"
                select new { dpH.Code, dpH.Date, Total = (decimal)0, Type = "Rencana Pengiriman" })
            .Skip(1); ;

        return data.ToDynamicList();
    }

    public SaveResult Insert(SalesInvoiceRequest data)
    {
        var result = new SaveResult(false);
        List<long> idOrderDetail = new();

        using var transaction = Db.Database.BeginTransaction();
        try
        {
            // Check & assign overlimit
            var isOverLimit = !CheckCreditLimit(data.CustCode, data.Total);
            if (isOverLimit)
                data.Mark = "OL";

            // Checking deliver qty is excess or not
            if (IsQtyExcess(data.WarehouseCode, data.ItemDetails, null))
            {
                result.Message = "Data penjualan langsung tidak bisa disimpan karena qty yg diterima lebih besar dari qty yang tersedia.";
                return result;
            }

            if (data.FinalDisc > data.SubTotal)
            {
                result.Message = "Data penjualan langsung tidak bisa disimpan karena nilai diskon final lebih besar dari nilai total.";
                return result;
            }

            var (isDuplicate, message) = CheckDuplicateDetail(data.ItemDetails);
            if (isDuplicate)
            {
                result.Message = message;
                return result;
            }

            if (data.Memos.Sum(x => x.CreditMemoAmount) > data.Total)
            {
                result.Message = "Data penjualan langsung tidak bisa diubah karena jumlah pembayaran lebih besar dari nilai faktur";
                return result;
            }

            var taxes = Db.Taxes.ToList();
            var promos = Db.PromoHeaders
                .Select(x => new
                {
                    x.Code,
                    x.Name,
                    x.StartDate,
                    x.EndDate,
                    x.ApplyTo,
                    x.CoaCost,
                    x.Content,
                    x.Mark,
                    Subject = Db.PromoSubjects.Where(y => y.Code == x.Code).ToList(),
                }).Where(x => x.StartDate <= data.Date && data.Date <= x.EndDate && x.Mark == "A").ToList();
            var items = Db.Items.ToList();
            var uomConversions = Db.UoMConversions.ToList();
            List<decimal> totalDetail = new();
            List<decimal> totalTax = new();
            List<decimal> totalExemptTax = new();
            List<decimal> totalDpp = new();

            // Sales Order
            var newCode = GetNewCode("DI_NUM_FMT", data.Date);
            data.Code = newCode;

            short i = 0;
            List<SalesOrderDetailFreeGood> bonusPromoMulti = new();
            var dummyDetails = data.ItemDetails.ToList();
            var qtyOriginal = new Dictionary<long, decimal>();
            foreach (var itemDummy in dummyDetails)
            {
                qtyOriginal.Add(itemDummy.Id, itemDummy.Qty);
                var ctItemDummy = items.FirstOrDefault(x => x.Id == itemDummy.ItemId);
                var curUomQtyDummy = uomConversions.FirstOrDefault(x => x.Id == itemDummy.UnitId);
                var itemUomQtyDummy = uomConversions.FirstOrDefault(x => x.Id == ctItemDummy.UomSellId);
                if (curUomQtyDummy.Seq > itemUomQtyDummy.Seq)
                {
                    var qtyField = Db.UoMConversions.Where(x => x.UomId == itemDummy.UomId && x.Seq <= curUomQtyDummy.Seq && x.Seq > itemUomQtyDummy.Seq).Select(x => x.Conversion).ToList();
                    var multipliedQty = qtyField.Aggregate(1, (x, y) => (int)(x * y));
                    itemDummy.Qty *= multipliedQty;
                }
                else if (curUomQtyDummy.Seq < itemUomQtyDummy.Seq)
                {
                    var qtyField = Db.UoMConversions.Where(x => x.UomId == itemDummy.UomId && x.Seq > curUomQtyDummy.Seq && x.Seq <= itemUomQtyDummy.Seq).Select(x => x.Conversion).ToList();
                    var multipliedQty = qtyField.Aggregate(1, (x, y) => (int)(x * y));
                    itemDummy.Qty /= multipliedQty;
                }
            }

            List<SalesOrderPromo> soPromo = new();

            foreach (var item in data.ItemDetails)
            {
                //Promo
                var listPromo = promos.Where(x => x.ApplyTo == 1 ||
                                                  x.Subject.Select(y => y.CustCode).Contains(data.CustCode) ||
                                                  x.Subject.Select(y => y.CustTypeId).Contains(data.CustTypeId)).ToList();

                List<SalesOrderDetailDiscount> discPromo = new();
                List<SalesOrderDetailFreeGood> bonusPromo = new();
                var ctItem = items.FirstOrDefault(j => j.Id == item.ItemId);
                if (listPromo.Any())
                {
                    foreach (var dataPromo in listPromo)
                    {
                        var listDetailPromo = Db.PromoDetails.Where(x => x.Code == dataPromo.Code).ToList();
                        foreach (var detailPromo in listDetailPromo)
                        {
                            var detailTierPromo = Db.PromoDetailTiers.Where(x => x.PromoDetailId == detailPromo.Id).ToList();
                            var detailMultiPromo = Db.PromoDetailMultipleItems.Where(x => x.PromoDetailId == detailPromo.Id).ToList();
                            var applyTo = detailPromo.ApplyTo;
                            var itemCategoryData = FindItemCategoryHierarchy(ctItem.CategoryId);
                            if ((applyTo == 1 && detailPromo.ItemId == item.ItemId)
                                || (applyTo == 3 && itemCategoryData.Select(x => x.Id).Contains(detailPromo.ItemId)))
                            {
                                switch (detailPromo.PromoType)
                                {
                                    case 1:
                                        // Apply to Barang - Promo Method Reguler
                                        var curUom = uomConversions.FirstOrDefault(x => x.Id == item.UnitId);
                                        var itemUom = uomConversions.FirstOrDefault(x => x.Id == ctItem.UomSellId);
                                        var discAmount = 0m;
                                        if (curUom.Seq > itemUom.Seq)
                                        {
                                            var qtyField = Db.UoMConversions.Where(x => x.UomId == item.UomId && x.Seq <= curUom.Seq && x.Seq > itemUom.Seq).Select(x => x.Conversion).ToList();
                                            var multipliedQty = qtyField.Aggregate(1, (x, y) => (int)(x * y));
                                            discAmount = detailPromo.IsPercentage ?
                                                item.UnitPrice * (detailPromo.ValuePercentage / 100) :
                                                detailPromo.ValueAmount * multipliedQty;
                                        }
                                        else if (curUom.Seq < itemUom.Seq)
                                        {
                                            var qtyField = Db.UoMConversions.Where(x => x.UomId == item.UomId && x.Seq > curUom.Seq && x.Seq <= itemUom.Seq).Select(x => x.Conversion).ToList();
                                            var multipliedQty = qtyField.Aggregate(1, (x, y) => (int)(x * y));
                                            discAmount = detailPromo.IsPercentage ?
                                                item.UnitPrice * (detailPromo.ValuePercentage / 100) :
                                                detailPromo.ValueAmount / multipliedQty;
                                        }
                                        else
                                        {
                                            discAmount = detailPromo.IsPercentage ?
                                                item.UnitPrice * (detailPromo.ValuePercentage / 100) :
                                                detailPromo.ValueAmount;
                                        }

                                        discPromo.Add(new SalesOrderDetailDiscount
                                        {
                                            PromoCode = dataPromo.Code,
                                            PromoDetailId = detailPromo.Id,
                                            Name = dataPromo.Name,
                                            //promoMethod: 1, 
                                            Value = detailPromo.IsPercentage ?
                                                detailPromo.ValuePercentage :
                                                discAmount,
                                            //nettPrice: 0, 
                                            CoaCode = dataPromo.CoaCost,
                                            Amount = discAmount,
                                            //fromPromo: true, 
                                            IsPercentage = detailPromo.IsPercentage
                                        });
                                        break;
                                    case 2:
                                        // Apply to Barang - Promo Method Qty Barang
                                        var originalQty2 = qtyOriginal[item.Id];
                                        var tierData = applyTo == 3 ? detailTierPromo.FirstOrDefault(x => item.Qty >= x.FromQty && item.Qty <= x.ToQty) : detailTierPromo.FirstOrDefault(x => originalQty2 >= x.FromQty && originalQty2 <= x.ToQty && x.SaleUnit == item.UnitId);
                                        if (tierData != null)
                                        {
                                            if (tierData.ApplyToAllUnit)
                                            {
                                                var promoUnit = uomConversions.FirstOrDefault(x => x.Id == tierData.SaleUnit);
                                                var itemUnit = uomConversions.FirstOrDefault(x => x.Id == item.UnitId);
                                                if (itemUnit.Seq >= promoUnit.Seq)
                                                {
                                                    discPromo.Add(new SalesOrderDetailDiscount
                                                    {
                                                        PromoCode = dataPromo.Code,
                                                        PromoDetailId = detailPromo.Id,
                                                        Name = dataPromo.Name,
                                                        //promoMethod: 1, 
                                                        Value = tierData.Value,
                                                        //nettPrice: 0, 
                                                        CoaCode = dataPromo.CoaCost,
                                                        Amount = detailPromo.IsPercentage ?
                                                            item.UnitPrice * (tierData.Value / 100) : tierData.Value,
                                                        //fromPromo: true, 
                                                        IsPercentage = detailPromo.IsPercentage
                                                    });
                                                }
                                            }
                                            else
                                            {
                                                discPromo.Add(new SalesOrderDetailDiscount
                                                {
                                                    PromoCode = dataPromo.Code,
                                                    PromoDetailId = detailPromo.Id,
                                                    Name = dataPromo.Name,
                                                    //promoMethod: 1,
                                                    Value = tierData.Value,
                                                    //nettPrice: 0,
                                                    CoaCode = dataPromo.CoaCost,
                                                    Amount = detailPromo.IsPercentage ?
                                                        item.UnitPrice * (tierData.Value / 100) : tierData.Value,
                                                    //fromPromo: true,
                                                    IsPercentage = detailPromo.IsPercentage
                                                });
                                            }
                                        }
                                        break;
                                    case 3:
                                        // Apply to Barang - Promo Method Bonus
                                        var originalQty3 = qtyOriginal[item.Id];
                                        var tierData3 = applyTo == 3 ? detailTierPromo.FirstOrDefault(x => item.Qty >= x.FromQty && item.Qty <= x.ToQty) : detailTierPromo.FirstOrDefault(x => originalQty3 >= x.FromQty && originalQty3 <= x.ToQty && x.SaleUnit == item.UnitId);
                                        if (tierData3 != null)
                                        {
                                            var sellPrice = 0m;
                                            var freeItem = items.FirstOrDefault(x => x.Id == tierData3.FreeGoodItemId);
                                            var curUom3 = uomConversions.FirstOrDefault(x => x.Id == Convert.ToInt32(tierData3.UnitFreeGood));
                                            var itemUom3 = uomConversions.FirstOrDefault(x => x.Id == ctItem.UomSellId);
                                            if (itemUom3.Seq > curUom3.Seq)
                                            {
                                                var qtyField = Db.UoMConversions.Where(x => x.UomId == freeItem.UomId && x.Seq <= itemUom3.Seq && x.Seq > curUom3.Seq).Select(x => x.Conversion).ToList();
                                                var multipliedQty = qtyField.Aggregate(1, (x, y) => (int)(x * y));
                                                sellPrice = (decimal)freeItem.SellPrice / multipliedQty;
                                            }
                                            else if (itemUom3.Seq < curUom3.Seq)
                                            {
                                                var qtyField = Db.UoMConversions.Where(x => x.UomId == freeItem.UomId && x.Seq > itemUom3.Seq && x.Seq <= curUom3.Seq).Select(x => x.Conversion).ToList();
                                                var multipliedQty = qtyField.Aggregate(1, (x, y) => (int)(x * y));
                                                sellPrice = (decimal)freeItem.SellPrice * multipliedQty;

                                            }
                                            else
                                            {
                                                sellPrice = (decimal)freeItem.SellPrice;
                                            }

                                            if (tierData3.IsMultiple)
                                            {
                                                var multipleValue = Math.Floor((applyTo == 3 ? item.Qty : originalQty3) / tierData3.FromQty);
                                                bonusPromo.Add(new SalesOrderDetailFreeGood
                                                {
                                                    PromoCode = dataPromo.Code,
                                                    ItemId = freeItem.Id,
                                                    UomId = (int)freeItem.UomId,
                                                    UnitId = Convert.ToInt32(tierData3.UnitFreeGood),
                                                    Qty = multipleValue * tierData3.Value,
                                                    QtyClosed = 0m,
                                                    UnitPrice = sellPrice,
                                                    CoaCode = dataPromo.CoaCost
                                                });
                                            }
                                            else
                                            {
                                                bonusPromo.Add(new SalesOrderDetailFreeGood
                                                {
                                                    PromoCode = dataPromo.Code,
                                                    ItemId = freeItem.Id,
                                                    UomId = (int)freeItem.UomId,
                                                    UnitId = Convert.ToInt32(tierData3.UnitFreeGood),
                                                    Qty = tierData3.Value,
                                                    QtyClosed = 0m,
                                                    UnitPrice = sellPrice,
                                                    CoaCode = dataPromo.CoaCost
                                                });
                                            }
                                        }
                                        break;
                                    case 4:
                                        // Apply to Barang - Promo Method Payment Term
                                        var tierData4 = detailTierPromo.FirstOrDefault(x => x.PaymentTermId == data.PaymentTermId);
                                        if (tierData4 != null)
                                        {
                                            discPromo.Add(new SalesOrderDetailDiscount
                                            {
                                                PromoCode = dataPromo.Code,
                                                PromoDetailId = detailPromo.Id,
                                                Name = dataPromo.Name,
                                                //promoMethod: 1,
                                                Value = tierData4.Value,
                                                //nettPrice: 0,
                                                CoaCode = dataPromo.CoaCost,
                                                Amount = tierData4.IsPercentage ?
                                                    item.UnitPrice * (tierData4.Value / 100) :
                                                    tierData4.Value,
                                                //fromPromo: true,
                                                IsPercentage = detailPromo.IsPercentage
                                            });
                                        }
                                        break;
                                }
                            }
                            else if (applyTo == 4)
                            {
                                var multiItem = detailMultiPromo.Select(y => y.ItemId);
                                var isApplicable = multiItem.Intersect(data.ItemDetails.Select(x => x.ItemId));
                                if (isApplicable.Any() && isApplicable.Contains(item.ItemId))
                                {
                                    switch (detailPromo.PromoType)
                                    {
                                        case 2:
                                            // Apply to Barang - Promo Method Qty Barang
                                            var miData2 = dummyDetails.Where(x => multiItem.Contains(x.ItemId)).ToList();

                                            var tierData = detailTierPromo.FirstOrDefault(x => miData2.Sum(x => x.Qty) >= x.FromQty && miData2.Sum(x => x.Qty) <= x.ToQty);
                                            if (tierData != null)
                                            {
                                                var valueDisc = tierData.Value;
                                                var sumMulti = data.ItemDetails.Where(x => isApplicable.Contains(x.ItemId)).Sum(x => x.UnitPrice * x.Qty);
                                                var prorateDisc = valueDisc / sumMulti * (item.Qty * item.UnitPrice);
                                                discPromo.Add(new SalesOrderDetailDiscount
                                                {
                                                    PromoCode = dataPromo.Code,
                                                    PromoDetailId = detailPromo.Id,
                                                    Name = dataPromo.Name,
                                                    //promoMethod: 1,
                                                    Value = detailPromo.IsPercentage ? tierData.Value : prorateDisc,
                                                    //nettPrice: 0,
                                                    CoaCode = dataPromo.CoaCost,
                                                    Amount = detailPromo.IsPercentage ?
                                                            item.UnitPrice * (tierData.Value / 100) : prorateDisc,
                                                    //fromPromo: true,
                                                    IsPercentage = detailPromo.IsPercentage
                                                });
                                            }
                                            break;
                                        case 3:
                                            // Apply to Barang - Promo Method Bonus
                                            var miData3 = dummyDetails.Where(x => multiItem.Contains(x.ItemId)).ToList();

                                            var tierData3 = detailTierPromo.FirstOrDefault(x => miData3.Sum(x => x.Qty) >= x.FromQty && miData3.Sum(x => x.Qty) <= x.ToQty);
                                            if (tierData3 != null)
                                            {
                                                var sellPrice = 0m;
                                                var freeItem = items.FirstOrDefault(x => x.Id == tierData3.FreeGoodItemId);
                                                var curUom3 = uomConversions.FirstOrDefault(x => x.Id == Convert.ToInt32(tierData3.UnitFreeGood));
                                                var itemUom3 = uomConversions.FirstOrDefault(x => x.Id == ctItem.UomSellId);
                                                if (itemUom3.Seq > curUom3.Seq)
                                                {
                                                    var qtyField = Db.UoMConversions.Where(x => x.UomId == freeItem.UomId && x.Seq <= itemUom3.Seq && x.Seq > curUom3.Seq).Select(x => x.Conversion).ToList();
                                                    var multipliedQty = qtyField.Aggregate(1, (x, y) => (int)(x * y));
                                                    sellPrice = (decimal)freeItem.SellPrice / multipliedQty;
                                                }
                                                else if (itemUom3.Seq < curUom3.Seq)
                                                {
                                                    var qtyField = Db.UoMConversions.Where(x => x.UomId == freeItem.UomId && x.Seq > itemUom3.Seq && x.Seq <= curUom3.Seq).Select(x => x.Conversion).ToList();
                                                    var multipliedQty = qtyField.Aggregate(1, (x, y) => (int)(x * y));
                                                    sellPrice = (decimal)freeItem.SellPrice * multipliedQty;

                                                }
                                                else
                                                {
                                                    sellPrice = (decimal)freeItem.SellPrice;
                                                }

                                                if (tierData3.IsMultiple)
                                                {
                                                    var multipleValue = Math.Floor(miData3.Sum(x => x.Qty) / tierData3.FromQty);
                                                    bonusPromoMulti.Add(new SalesOrderDetailFreeGood
                                                    {
                                                        PromoCode = dataPromo.Code,
                                                        ItemId = freeItem.Id,
                                                        UomId = (int)freeItem.UomId,
                                                        UnitId = Convert.ToInt32(tierData3.UnitFreeGood),
                                                        Qty = multipleValue * tierData3.Value,
                                                        QtyClosed = 0m,
                                                        UnitPrice = sellPrice,
                                                        CoaCode = dataPromo.CoaCost
                                                    });
                                                }
                                                else
                                                {
                                                    bonusPromoMulti.Add(new SalesOrderDetailFreeGood
                                                    {
                                                        PromoCode = dataPromo.Code,
                                                        ItemId = freeItem.Id,
                                                        UomId = (int)freeItem.UomId,
                                                        UnitId = Convert.ToInt32(tierData3.UnitFreeGood),
                                                        Qty = tierData3.Value,
                                                        QtyClosed = 0m,
                                                        UnitPrice = sellPrice,
                                                        CoaCode = dataPromo.CoaCost
                                                    });
                                                }
                                            }
                                            break;
                                    }
                                }
                            }
                        }
                    }
                }

                item.Qty = qtyOriginal[item.Id];

                if (item.DiscountItemDetails != null && item.DiscountItemDetails.Any())
                {
                    discPromo.AddRange(item.DiscountItemDetails);
                }

                item.Disc = discPromo.Sum(x => x.Amount);

                var taxData = taxes.FirstOrDefault(x => x.Id == item.TaxId);
                var discHeaderProrate = 0m;
                var sumDetail = data.ItemDetails.Sum(x => (x.UnitPrice - x.Disc) * x.Qty);
                if (data.FinalDiscPercent > 0)
                {
                    var amountPercent = sumDetail * (data.FinalDiscPercent / 100);
                    discHeaderProrate = amountPercent / sumDetail * (item.Qty * (item.UnitPrice - item.Disc));
                    discHeaderProrate /= item.Qty;
                }
                else if (data.FinalDisc > 0)
                {
                    discHeaderProrate = data.FinalDisc / sumDetail * (item.Qty * (item.UnitPrice - item.Disc));
                    discHeaderProrate /= item.Qty;
                }

                if (data.IncludeTax)
                {
                    item.TaxAmount = (item.UnitPrice - item.Disc - discHeaderProrate) - ((item.UnitPrice - item.Disc - discHeaderProrate) / (1 + (taxData.Rate / 100)));
                    item.ExemptTaxAmount = (item.UnitPrice - item.Disc - discHeaderProrate) - ((item.UnitPrice - item.Disc - discHeaderProrate) / (1 + (taxData.ExemptRate / 100)));
                    item.NettPrice = item.UnitPrice - item.Disc - discHeaderProrate;
                    item.Dpp = item.UnitPrice - item.Disc - discHeaderProrate - item.TaxAmount + item.ExemptTaxAmount;
                }
                else
                {
                    item.TaxAmount = (item.UnitPrice - item.Disc - discHeaderProrate) * (taxData.Rate / 100);
                    item.ExemptTaxAmount = (item.UnitPrice - item.Disc - discHeaderProrate) * (taxData.ExemptRate / 100);
                    item.NettPrice = item.UnitPrice - item.Disc - discHeaderProrate + item.TaxAmount - item.ExemptTaxAmount;
                    item.Dpp = item.UnitPrice - item.Disc - discHeaderProrate;
                }

                if (item.NettPrice < 0)
                {
                    result.Message = $"Data penjualan langsung tidak bisa disimpan karena nilai bersih barang {ctItem.Initial} minus.";
                    return result;
                }

                item.FinalDiscHeader = discHeaderProrate;
                item.Total = item.Qty * item.NettPrice;
                totalDetail.Add(item.Total);
                totalTax.Add(item.Qty * item.TaxAmount);
                totalExemptTax.Add(item.Qty * item.ExemptTaxAmount);
                totalDpp.Add(item.Qty * item.Dpp);

                var orderDetail = new SalesOrderDetail
                {
                    Code = newCode,
                    LineNo = ++i,
                    ItemId = item.ItemId,
                    UomId = item.UomId,
                    UnitId = item.UnitId,
                    Qty = item.Qty,
                    Length = item.Length,
                    Width = item.Width,
                    Height = item.Height,
                    Weight = item.Weight,
                    DimensionMeasurement = item.DimensionMeasurement,
                    WeightMeasurement = item.WeightMeasurement,
                    QtyDlv = 0,
                    UnitPrice = item.UnitPrice,
                    Disc = item.Disc,
                    FinalDiscHeader = item.FinalDiscHeader,
                    TaxId = item.TaxId,
                    TaxAmount = item.TaxAmount,
                    ExemptTaxAmount = item.ExemptTaxAmount,
                    NettPrice = item.NettPrice,
                    Total = item.Total,
                    Dpp = item.Dpp,
                    Notes = item.Notes,
                    CoaInventory = item.CoaInventory,
                    CoaCogs = item.CoaCogs,
                    CoaSls = item.CoaSls,
                    CoaSlsDisc = item.CoaSlsDisc,
                    CoaSlsReturn = item.CoaSlsReturn
                };

                Db.SalesOrderDetails.Add(orderDetail);

                Db.SaveChanges();

                idOrderDetail.Add(orderDetail.Id);

                if (discPromo.Any())
                {
                    short d = 0;
                    foreach (var discItem in discPromo)
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
                            CoaCode = discItem.CoaCode
                        });

                        if (!soPromo.Any(x => x.PromoCode == discItem.PromoCode))
                        {
                            if (discItem.PromoCode != null)
                            {
                                soPromo.Add(new SalesOrderPromo
                                {
                                    Code = newCode,
                                    LineNo = (short)(soPromo.Count + 1),
                                    PromoCode = discItem.PromoCode,
                                    IsActive = true
                                });
                            }
                        }
                    }
                    Db.SaveChanges();
                }

                if (bonusPromoMulti.Any() && orderDetail.LineNo == 1)
                {
                    bonusPromo.AddRange(bonusPromoMulti);
                }

                if (bonusPromo.Any())
                {
                    if (IsQtyExcessFree(data.WarehouseCode, bonusPromo, null))
                    {
                        result.Message = "Data penjualan langsung tidak bisa disimpan karena qty bonus yang dipesan lebih besar dari qty yang tersedia.";
                        return result;
                    }

                    short f = 0;
                    foreach (var freeItem in bonusPromo)
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
                            QtyClosed = freeItem.QtyClosed,
                            UnitPrice = freeItem.UnitPrice,
                            CoaCode = freeItem.CoaCode
                        });

                        if (!soPromo.Any(x => x.PromoCode == freeItem.PromoCode))
                        {
                            soPromo.Add(new SalesOrderPromo
                            {
                                Code = newCode,
                                LineNo = (short)(soPromo.Count + 1),
                                PromoCode = freeItem.PromoCode,
                                IsActive = true
                            });
                        }
                    }
                    Db.SaveChanges();
                }
            }

            if (data.FinalDiscPercent > 0)
                data.FinalDisc = data.ItemDetails.Sum(x => x.FinalDiscHeader * x.Qty);
            data.SubTotal = totalDetail.Sum();
            data.TaxAmount = totalTax.Sum();
            data.ExemptTaxAmount = totalExemptTax.Sum();
            data.Dpp = totalDpp.Sum();
            data.Total = data.SubTotal;

            foreach (var item in data.ItemDetails)
            {
                //Promo
                var listPromo = promos.Where(x => x.ApplyTo == 1 ||
                                                  x.Subject.Select(y => y.CustCode).Contains(data.CustCode) ||
                                                  x.Subject.Select(y => y.CustTypeId).Contains(data.CustTypeId)).ToList();

                List<SalesOrderDetailDiscount> discPromo = new();
                List<SalesOrderDetailFreeGood> bonusPromo = new();
                var ctItem = items.FirstOrDefault(j => j.Id == item.ItemId);
                if (listPromo.Any())
                {
                    foreach (var dataPromo in listPromo)
                    {
                        var listDetailPromo = Db.PromoDetails.Where(x => x.Code == dataPromo.Code).ToList();
                        foreach (var detailPromo in listDetailPromo)
                        {
                            var detailTierPromo = Db.PromoDetailTiers.Where(x => x.PromoDetailId == detailPromo.Id).ToList();
                            var applyTo = detailPromo.ApplyTo;
                            if (applyTo == 2)
                            {
                                switch (detailPromo.PromoType)
                                {
                                    case 4:
                                        // Apply to Barang - Promo Method Payment Term
                                        var tierData = detailTierPromo.FirstOrDefault(x => x.PaymentTermId == data.PaymentTermId);
                                        if (tierData != null)
                                        {
                                            var curUom = uomConversions.FirstOrDefault(x => x.Id == item.UnitId);
                                            var itemUom = uomConversions.FirstOrDefault(x => x.Id == ctItem.UomSellId);
                                            var discAmount = tierData.IsPercentage ? data.SubTotal * (tierData.Value / 100) : tierData.Value;
                                            var prorateDisc = (discAmount / data.SubTotal * (item.Qty * (item.UnitPrice - item.Disc))) / item.Qty;

                                            discPromo.Add(new SalesOrderDetailDiscount
                                            {
                                                PromoCode = dataPromo.Code,
                                                PromoDetailId = detailPromo.Id,
                                                Name = dataPromo.Name,
                                                //promoMethod: 1,
                                                Value = prorateDisc,
                                                //nettPrice: 0,
                                                CoaCode = dataPromo.CoaCost,
                                                Amount = prorateDisc,
                                                //fromPromo: true,
                                                IsPercentage = false
                                            });
                                        }
                                        break;
                                    case 5:
                                        // Apply to Faktur - Promo Method Nilai Trans
                                        var tierData5 = detailTierPromo.FirstOrDefault(x => data.SubTotal >= x.FromQty && data.SubTotal <= x.ToQty);
                                        if (tierData5 != null)
                                        {
                                            var curUom = uomConversions.FirstOrDefault(x => x.Id == item.UnitId);
                                            var itemUom = uomConversions.FirstOrDefault(x => x.Id == ctItem.UomSellId);
                                            var discAmount = tierData5.IsPercentage ? data.SubTotal * (tierData5.Value / 100) : tierData5.Value;
                                            var prorateDisc = (discAmount / data.SubTotal * (item.Qty * (item.UnitPrice - item.Disc))) / item.Qty;

                                            discPromo.Add(new SalesOrderDetailDiscount
                                            {
                                                PromoCode = dataPromo.Code,
                                                PromoDetailId = detailPromo.Id,
                                                Name = dataPromo.Name,
                                                //promoMethod: 1,
                                                Value = prorateDisc,
                                                //nettPrice: 0,
                                                CoaCode = dataPromo.CoaCost,
                                                Amount = prorateDisc,
                                                //fromPromo: true,
                                                IsPercentage = false
                                            });
                                        }
                                        break;
                                }
                            }
                        }
                    }
                }

                if (discPromo.Any())
                {
                    item.Disc += discPromo.Sum(x => x.Amount);

                    var taxData = taxes.FirstOrDefault(x => x.Id == item.TaxId);
                    var discHeaderProrate = 0m;
                    var sumDetail = data.ItemDetails.Sum(x => (x.UnitPrice - x.Disc) * x.Qty);
                    if (data.FinalDiscPercent > 0)
                    {
                        var amountPercent = sumDetail * (data.FinalDiscPercent / 100);
                        discHeaderProrate = amountPercent / sumDetail * (item.Qty * (item.UnitPrice - item.Disc));
                        discHeaderProrate /= item.Qty;
                    }
                    else if (data.FinalDisc > 0)
                    {
                        discHeaderProrate = data.FinalDisc / sumDetail * (item.Qty * (item.UnitPrice - item.Disc));
                        discHeaderProrate /= item.Qty;
                    }

                    if (data.IncludeTax)
                    {
                        item.TaxAmount = (item.UnitPrice - item.Disc - discHeaderProrate) - ((item.UnitPrice - item.Disc - discHeaderProrate) / (1 + (taxData.Rate / 100)));
                        item.ExemptTaxAmount = (item.UnitPrice - item.Disc - discHeaderProrate) - ((item.UnitPrice - item.Disc - discHeaderProrate) / (1 + (taxData.ExemptRate / 100)));
                        item.NettPrice = item.UnitPrice - item.Disc - discHeaderProrate;
                        item.Dpp = item.UnitPrice - item.Disc - discHeaderProrate - item.TaxAmount + item.ExemptTaxAmount;
                    }
                    else
                    {
                        item.TaxAmount = (item.UnitPrice - item.Disc - discHeaderProrate) * (taxData.Rate / 100);
                        item.ExemptTaxAmount = (item.UnitPrice - item.Disc - discHeaderProrate) * (taxData.ExemptRate / 100);
                        item.NettPrice = item.UnitPrice - item.Disc - discHeaderProrate + item.TaxAmount - item.ExemptTaxAmount;
                        item.Dpp = item.UnitPrice - item.Disc - discHeaderProrate;
                    }

                    if (item.NettPrice < 0)
                    {
                        result.Message = $"Data order penjualan tidak bisa disimpan karena nilai bersih barang {ctItem.Initial} minus.";
                        return result;
                    }

                    item.FinalDiscHeader = discHeaderProrate;
                    item.Total = item.Qty * item.NettPrice;
                    totalDetail.Add(item.Total);
                    totalTax.Add(item.Qty * item.TaxAmount);
                    totalExemptTax.Add(item.Qty * item.ExemptTaxAmount);
                    totalDpp.Add(item.Qty * item.Dpp);

                    var soDetail = Db.SalesOrderDetails.FirstOrDefault(x => x.Code == newCode && x.ItemId == item.ItemId && x.UnitId == item.UnitId);
                    var orderDetail = new SalesOrderDetail();
                    if (soDetail == null)
                    {
                        orderDetail = new SalesOrderDetail
                        {
                            Code = newCode,
                            LineNo = ++i,
                            ItemId = item.ItemId,
                            UomId = item.UomId,
                            UnitId = item.UnitId,
                            Qty = item.Qty,
                            Length = item.Length,
                            Width = item.Width,
                            Height = item.Height,
                            Weight = item.Weight,
                            DimensionMeasurement = item.DimensionMeasurement,
                            WeightMeasurement = item.WeightMeasurement,
                            QtyDlv = 0,
                            UnitPrice = item.UnitPrice,
                            Disc = item.Disc,
                            FinalDiscHeader = item.FinalDiscHeader,
                            TaxId = item.TaxId,
                            TaxAmount = item.TaxAmount,
                            ExemptTaxAmount = item.ExemptTaxAmount,
                            NettPrice = item.NettPrice,
                            Total = item.Total,
                            Dpp = item.Dpp,
                            Notes = item.Notes,
                            CoaInventory = item.CoaInventory,
                            CoaCogs = item.CoaCogs,
                            CoaSls = item.CoaSls,
                            CoaSlsDisc = item.CoaSlsDisc,
                            CoaSlsReturn = item.CoaSlsReturn,
                            CoaTransit = string.IsNullOrEmpty(item.CoaTransit) ? Db.SystemParameters.FirstOrDefault(x => x.Code == "TRANSIT_ITEM_COA")?.Value ?? "" : item.CoaTransit
                        };

                        Db.SalesOrderDetails.Add(orderDetail);
                    }
                    else
                    {
                        totalDetail.Add(-soDetail.Total);
                        totalTax.Add(-(soDetail.Qty * soDetail.TaxAmount));
                        totalExemptTax.Add(-(soDetail.Qty * soDetail.ExemptTaxAmount));
                        totalDpp.Add(-(soDetail.Qty * soDetail.Dpp));

                        soDetail.Disc = item.Disc;
                        soDetail.FinalDiscHeader = item.FinalDiscHeader;
                        soDetail.TaxId = item.TaxId;
                        soDetail.TaxAmount = item.TaxAmount;
                        soDetail.ExemptTaxAmount = item.ExemptTaxAmount;
                        soDetail.NettPrice = item.NettPrice;
                        soDetail.Total = item.Total;
                        soDetail.Dpp = item.Dpp;

                        Db.SalesOrderDetails.Update(soDetail);
                    }

                    Db.SaveChanges();
                    short d = 0;
                    foreach (var discItem in discPromo)
                    {
                        Db.SalesOrderDetailDiscounts.Add(new SalesOrderDetailDiscount
                        {
                            Code = newCode,
                            OrderDetailId = soDetail == null ? orderDetail.Id : soDetail.Id,
                            LineNo = ++d,
                            PromoCode = discItem.PromoCode,
                            PromoDetailId = discItem.PromoDetailId,
                            Name = discItem.Name,
                            IsPercentage = discItem.IsPercentage,
                            Value = discItem.Value,
                            Amount = discItem.Amount,
                            CoaCode = discItem.CoaCode
                        });

                        if (!soPromo.Any(x => x.PromoCode == discItem.PromoCode))
                        {
                            if (discItem.PromoCode != null)
                            {
                                soPromo.Add(new SalesOrderPromo
                                {
                                    Code = newCode,
                                    LineNo = (short)(soPromo.Count + 1),
                                    PromoCode = discItem.PromoCode,
                                    IsActive = true
                                });
                            }
                        }
                    }
                    Db.SaveChanges();
                }
            }

            if (soPromo.Any())
                Db.SalesOrderPromos.AddRange(soPromo);

            if (data.FinalDiscPercent > 0)
                data.FinalDisc = data.ItemDetails.Sum(x => x.FinalDiscHeader * x.Qty);
            data.SubTotal = totalDetail.Sum();
            data.TaxAmount = totalTax.Sum();
            data.ExemptTaxAmount = totalExemptTax.Sum();
            data.Dpp = totalDpp.Sum();
            data.Total = data.SubTotal;

            Db.SalesOrderHeaders.Add(new SalesOrderHeader
            {
                Code = newCode,
                Date = data.Date,
                CustCode = data.CustCode,
                PaymentTermId = data.PaymentTermId,
                SalesBy = data.SalesBy,
                WarehouseCode = data.WarehouseCode,
                CurrCode = data.CurrCode,
                Rate = data.Rate,
                SubTotal = data.SubTotal,
                FinalDiscPercent = data.FinalDiscPercent,
                FinalDisc = data.FinalDisc,
                IncludeTax = data.IncludeTax,
                TaxAmount = data.TaxAmount,
                ExemptTaxAmount = data.ExemptTaxAmount,
                Total = data.Total,
                Dpp = data.Dpp,
                Notes = data.Notes,
                Mark = data.Mark,
                CreatedBy = data.CreatedBy,
                CreatedDate = data.CreatedDate,
                UpdatedBy = data.UpdatedBy,
                UpdatedDate = data.UpdatedDate,
                FromDirectInvoice = true
            });

            // Sales Delivery
            Db.SalesDeliveryHeaders.Add(new SalesDeliveryHeader
            {
                Code = newCode,
                Date = data.Date,
                SrcTrans = 1,
                TransCode = newCode,
                CustCode = data.CustCode,
                WarehouseCode = data.WarehouseCode,
                ShippedBy = data.SalesBy,
                CurrCode = data.CurrCode,
                Rate = data.Rate,
                SubTotal = data.SubTotal,
                FinalDiscPercent = data.FinalDiscPercent,
                FinalDisc = data.FinalDisc,
                IncludeTax = data.IncludeTax,
                TaxAmount = data.TaxAmount,
                ExemptTaxAmount = data.ExemptTaxAmount,
                Total = data.Total,
                Dpp = data.Dpp,
                Mark = isOverLimit ? "OL" : "INV",
                Notes = data.Notes,
                CreatedBy = data.CreatedBy,
                CreatedDate = data.CreatedDate,
                UpdatedBy = data.UpdatedBy,
                UpdatedDate = data.UpdatedDate,
                FromDirectInvoice = true
            });

            short j = 0;
            foreach (var item in data.ItemDetails)
            {
                var deliveryDetail = new SalesDeliveryDetail
                {
                    Code = newCode,
                    LineNo = ++j,
                    SoDetailId = idOrderDetail[j - 1],
                    ItemId = item.ItemId,
                    UomId = item.UomId,
                    UnitId = item.UnitId,
                    Qty = item.Qty,
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
                    ExemptTaxAmount = item.ExemptTaxAmount,
                    Total = item.Total,
                    Dpp = item.Dpp
                };

                Db.SalesDeliveryDetails.Add(deliveryDetail);

                var freeOrderDataList = Db.SalesOrderDetailFreeGoods.Where(x => x.OrderDetailId == deliveryDetail.SoDetailId).ToList();

                if (freeOrderDataList.Any())
                {
                    Db.SaveChanges();
                }

                if (freeOrderDataList.Any())
                {
                    short f = 0;
                    foreach (var freeItem in freeOrderDataList)
                    {
                        var dlvFreeDetail = new SalesDeliveryDetailFreeGood
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
                            CoaCode = freeItem.CoaCode
                        };

                        Db.SalesDeliveryDetailFreeGoods.Add(dlvFreeDetail);
                        Db.SaveChanges();

                        var orderFreeDetail = Db.SalesOrderDetailFreeGoods.FirstOrDefault(x => x.Code == newCode && x.ItemId == dlvFreeDetail.ItemId && x.UnitId == dlvFreeDetail.UnitId);
                        orderFreeDetail.QtyClosed += dlvFreeDetail.Qty;
                        Db.SalesOrderDetailFreeGoods.Update(orderFreeDetail);
                    }
                    Db.SaveChanges();
                }
            }

            // Sales Invoice
            Db.SalesInvoiceHeaders.Add(new SalesInvoiceHeader
            {
                Code = newCode,
                Date = data.Date,
                DueDate = data.DueDate,
                SoCode = newCode,
                CustCode = data.CustCode,
                CurrCode = data.CurrCode,
                Total = data.Total,
                TaxInvoiceNo = data.TaxInvoiceNo,
                TaxInvoiceDate = data.TaxInvoiceDate,
                Notes = data.Notes,
                Mark = data.Mark == "OL" ? "OL" : data.Memos.Sum(x => x.CreditMemoAmount) > 0 ? data.Total == data.Memos.Sum(x => x.CreditMemoAmount) ? "CMP" : "PP" : "A",
                PaidAmount = data.Memos.Sum(x => x.CreditMemoAmount),
                CreatedBy = data.CreatedBy,
                CreatedDate = data.CreatedDate,
                UpdatedBy = data.UpdatedBy,
                UpdatedDate = data.UpdatedDate,
                FromDirectInvoice = true
            });

            Db.SalesInvoiceDetails.Add(new SalesInvoiceDetail
            {
                Code = newCode,
                LineNo = 1,
                DoCode = newCode,
                SubTotal = data.SubTotal,
                FinalDisc = data.FinalDisc,
                TaxAmount = data.TaxAmount,
                ExemptTaxAmount = data.ExemptTaxAmount,
                Total = data.Total,
                Dpp = data.Dpp
            });

            // Insert memo
            foreach (var item in data.Memos)
            {
                Db.SalesInvoiceCreditMemos.Add(new SalesInvoiceCreditMemo
                {
                    InvCode = newCode,
                    InvAmount = data.Total,
                    CreditMemoAmount = item.CreditMemoAmount,
                    CreditMemoCode = item.CreditMemoCode,
                    Src = item.Src
                });
            }

            // Credit Used
            if (!isOverLimit)
                UpdateCreditUsed(data.CustCode, data.Total);

            Db.SaveChanges();

            if (!isOverLimit)
            {
                var dlvData = Db.SalesDeliveryHeaders.FirstOrDefault(x => x.TransCode == newCode);

                // Execute sp_update_stock_mutation_from_so
                Db.Database.ExecuteSqlRaw(
                    "EXEC sp_update_stock_mutation_from_so {0}, {1}",
                    newCode, data.Date);

                // Execute sp_update_stock_mutation_from_do
                Db.Database.ExecuteSqlRaw(
                    "EXEC sp_update_stock_mutation_from_do {0}, {1}, {2}",
                    dlvData?.Code, data.Date, newCode);

                // Execute sp_update_stock_mutation_from_si
                Db.Database.ExecuteSqlRaw(
                    "EXEC sp_update_stock_mutation_from_si {0}, {1}, {2}",
                    newCode, data.Date, newCode);

                // Execute sp_update_po_rcv_qty
                Db.Database.ExecuteSqlRaw("EXEC sp_update_so_dlv_qty {0}", newCode);
            }
            
            // Update sales order to closed if all sales delivery are invoiced
            //if (
            //    !Db.SalesDeliveryHeaders
            //        .Any(x => x.TransCode == newCode && x.Mark != "INV"))
            //{
            //    Db.Database.ExecuteSqlRaw(
            //        "UPDATE Sales.SalesOrderHeader SET Mark='CLS' WHERE Code={0} AND Mark='CMP'", newCode);
            //}
            UpdateCreditMemo(data);

            transaction.Commit();
        }
        catch (Exception ex)
        {
            result.Message = ex.InnerException?.Message ?? ex.Message;
            return result;
        }

        result.Success = true;
        result.Data = data.Code;
        result.Message = "Data penjualan langsung berhasil disimpan.";
        return result;
    }

    public SaveResult Update(SalesInvoiceRequest data)
    {
        var result = new SaveResult(false);
        var listOrderIdDetail = new List<long>();
        var listDeliveryIdDetail = new List<long>();

        using var transaction = Db.Database.BeginTransaction();
        try
        {
            if (Db.SalesInvoiceHeaders.Any(x => x.Code == data.Code && x.Mark == "V"))
            {
                result.Message = "Data penjualan langsung tidak bisa diubah karena status data bukan aktif.";
                return result;
            }

            // Checking deliver qty is excess or not
            if (IsQtyExcess(data.WarehouseCode, data.ItemDetails, data.Code))
            {
                result.Message = "Data penjualan langsung tidak bisa disimpan karena qty yg diterima lebih besar dari qty yang tersedia.";
                return result;
            }

            if (data.FinalDisc > data.SubTotal)
            {
                result.Message = "Data penjualan langsung tidak bisa disimpan karena nilai diskon final lebih besar dari nilai total.";
                return result;
            }

            // Check & assign overlimit
            var isOverLimit = !CheckCreditLimit(data.CustCode, data.Total);
            if (isOverLimit)
                data.Mark = "OL";

            var (isDuplicate, message) = CheckDuplicateDetail(data.ItemDetails);
            if (isDuplicate)
            {
                result.Message = message;
                return result;
            }

            if (data.Memos.Sum(x => x.CreditMemoAmount) > data.Total)
            {
                result.Message = "Data penjualan langsung tidak bisa diubah karena jumlah pembayaran lebih besar dari nilai faktur";
                return result;
            }

            //Restore stock mutation
            RestoreWarehouseQty(data.Code);

            var taxes = Db.Taxes.ToList();
            var promos = Db.PromoHeaders
                .Select(x => new
                {
                    x.Code,
                    x.Name,
                    x.StartDate,
                    x.EndDate,
                    x.ApplyTo,
                    x.CoaCost,
                    x.Content,
                    x.Mark,
                    Subject = Db.PromoSubjects.Where(y => y.Code == x.Code).ToList(),
                }).Where(x => x.StartDate <= data.Date && data.Date <= x.EndDate && x.Mark == "A").ToList();
            var items = Db.Items.ToList();
            var uomConversions = Db.UoMConversions.ToList();
            List<decimal> totalDetail = new();
            List<decimal> totalTax = new();
            List<decimal> totalExemptTax = new();
            List<decimal> totalDpp = new();

            data.ApprovedBy = null;
            data.ApprovedDate = null;

            var originData = Db.SalesOrderHeaders.AsNoTracking().FirstOrDefault(x => x.Code == data.Code);
            // Restore Credit Used
            RestoreCreditUsed(data.Code, originData.CustCode);

            var orderData = Db.SalesOrderHeaders.FirstOrDefault(x => x.Code == data.SoCode);
            var invDetail = Db.SalesInvoiceDetails.FirstOrDefault(x => x.Code == data.Code);
            var deliveryData = Db.SalesDeliveryHeaders.FirstOrDefault(x => x.Code == invDetail.DoCode);
            var InvoiceDetailData = Db.SalesInvoiceDetails.FirstOrDefault(x => x.Code == data.Code);


            // Update Order Detail data
            // Get detail data that exists in order before
            var delOrderDetails = Db.SalesOrderDetails
                .Where(d => d.Code == data.SoCode && !data.ItemDetails.Select(x => x.Id).Contains(d.Id))
                .ToList();

            // Delete detail data that exists in order before
            Db.SalesOrderDetails.RemoveRange(delOrderDetails);

            short i = 0;
            List<SalesOrderDetailFreeGood> bonusPromoMulti = new();
            var dummyDetails = data.ItemDetails.ToList();
            var qtyOriginal = new Dictionary<long, decimal>();
            foreach (var itemDummy in dummyDetails)
            {
                qtyOriginal.Add(itemDummy.Id, itemDummy.Qty);
                var ctItemDummy = items.FirstOrDefault(x => x.Id == itemDummy.ItemId);
                var curUomQtyDummy = uomConversions.FirstOrDefault(x => x.Id == itemDummy.UnitId);
                var itemUomQtyDummy = uomConversions.FirstOrDefault(x => x.Id == ctItemDummy.UomSellId);
                if (curUomQtyDummy.Seq > itemUomQtyDummy.Seq)
                {
                    var qtyField = Db.UoMConversions.Where(x => x.UomId == itemDummy.UomId && x.Seq <= curUomQtyDummy.Seq && x.Seq > itemUomQtyDummy.Seq).Select(x => x.Conversion).ToList();
                    var multipliedQty = qtyField.Aggregate(1, (x, y) => (int)(x * y));
                    itemDummy.Qty *= multipliedQty;
                }
                else if (curUomQtyDummy.Seq < itemUomQtyDummy.Seq)
                {
                    var qtyField = Db.UoMConversions.Where(x => x.UomId == itemDummy.UomId && x.Seq > curUomQtyDummy.Seq && x.Seq <= itemUomQtyDummy.Seq).Select(x => x.Conversion).ToList();
                    var multipliedQty = qtyField.Aggregate(1, (x, y) => (int)(x * y));
                    itemDummy.Qty /= multipliedQty;
                }
            }

            var delPromo = Db.SalesOrderPromos.Where(x => x.Code == data.Code);
            Db.SalesOrderPromos.RemoveRange(delPromo);

            List<SalesOrderPromo> soPromo = new();

            foreach (var item in data.ItemDetails)
            {
                //Promo
                var listPromo = promos.Where(x => x.ApplyTo == 1 ||
                                                  x.Subject.Select(y => y.CustCode).Contains(data.CustCode) ||
                                                  x.Subject.Select(y => y.CustTypeId).Contains(data.CustTypeId)).ToList();

                List<SalesOrderDetailDiscount> discPromo = new();
                List<SalesOrderDetailFreeGood> bonusPromo = new();
                var ctItem = items.FirstOrDefault(j => j.Id == item.ItemId);
                if (listPromo.Any())
                {
                    foreach (var dataPromo in listPromo)
                    {
                        var listDetailPromo = Db.PromoDetails.Where(x => x.Code == dataPromo.Code).ToList();
                        foreach (var detailPromo in listDetailPromo)
                        {
                            var detailTierPromo = Db.PromoDetailTiers.Where(x => x.PromoDetailId == detailPromo.Id).ToList();
                            var detailMultiPromo = Db.PromoDetailMultipleItems.Where(x => x.PromoDetailId == detailPromo.Id).ToList();
                            var applyTo = detailPromo.ApplyTo;
                            var itemCategoryData = FindItemCategoryHierarchy(ctItem.CategoryId);
                            if ((applyTo == 1 && detailPromo.ItemId == item.ItemId)
                                || (applyTo == 3 && itemCategoryData.Select(x => x.Id).Contains(detailPromo.ItemId)))
                            {
                                switch (detailPromo.PromoType)
                                {
                                    case 1:
                                        // Apply to Barang - Promo Method Reguler
                                        var curUom = uomConversions.FirstOrDefault(x => x.Id == item.UnitId);
                                        var itemUom = uomConversions.FirstOrDefault(x => x.Id == ctItem.UomSellId);
                                        var discAmount = 0m;
                                        if (curUom.Seq > itemUom.Seq)
                                        {
                                            var qtyField = Db.UoMConversions.Where(x => x.UomId == item.UomId && x.Seq <= curUom.Seq && x.Seq > itemUom.Seq).Select(x => x.Conversion).ToList();
                                            var multipliedQty = qtyField.Aggregate(1, (x, y) => (int)(x * y));
                                            discAmount = detailPromo.IsPercentage ?
                                                item.UnitPrice * (detailPromo.ValuePercentage / 100) :
                                                detailPromo.ValueAmount * multipliedQty;
                                        }
                                        else if (curUom.Seq < itemUom.Seq)
                                        {
                                            var qtyField = Db.UoMConversions.Where(x => x.UomId == item.UomId && x.Seq > curUom.Seq && x.Seq <= itemUom.Seq).Select(x => x.Conversion).ToList();
                                            var multipliedQty = qtyField.Aggregate(1, (x, y) => (int)(x * y));
                                            discAmount = detailPromo.IsPercentage ?
                                                item.UnitPrice * (detailPromo.ValuePercentage / 100) :
                                                detailPromo.ValueAmount / multipliedQty;
                                        }
                                        else
                                        {
                                            discAmount = detailPromo.IsPercentage ?
                                                item.UnitPrice * (detailPromo.ValuePercentage / 100) :
                                                detailPromo.ValueAmount;
                                        }

                                        discPromo.Add(new SalesOrderDetailDiscount
                                        {
                                            PromoCode = dataPromo.Code,
                                            PromoDetailId = detailPromo.Id,
                                            Name = dataPromo.Name,
                                            //promoMethod: 1, 
                                            Value = detailPromo.IsPercentage ?
                                                detailPromo.ValuePercentage :
                                                discAmount,
                                            //nettPrice: 0, 
                                            CoaCode = dataPromo.CoaCost,
                                            Amount = discAmount,
                                            //fromPromo: true, 
                                            IsPercentage = detailPromo.IsPercentage
                                        });
                                        break;
                                    case 2:
                                        // Apply to Barang - Promo Method Qty Barang
                                        var originalQty2 = qtyOriginal[item.Id];
                                        var tierData = applyTo == 3 ? detailTierPromo.FirstOrDefault(x => item.Qty >= x.FromQty && item.Qty <= x.ToQty) : detailTierPromo.FirstOrDefault(x => originalQty2 >= x.FromQty && originalQty2 <= x.ToQty && x.SaleUnit == item.UnitId);
                                        if (tierData != null)
                                        {
                                            if (tierData.ApplyToAllUnit)
                                            {
                                                var promoUnit = uomConversions.FirstOrDefault(x => x.Id == tierData.SaleUnit);
                                                var itemUnit = uomConversions.FirstOrDefault(x => x.Id == item.UnitId);
                                                if (itemUnit.Seq >= promoUnit.Seq)
                                                {
                                                    discPromo.Add(new SalesOrderDetailDiscount
                                                    {
                                                        PromoCode = dataPromo.Code,
                                                        PromoDetailId = detailPromo.Id,
                                                        Name = dataPromo.Name,
                                                        //promoMethod: 1, 
                                                        Value = tierData.Value,
                                                        //nettPrice: 0, 
                                                        CoaCode = dataPromo.CoaCost,
                                                        Amount = detailPromo.IsPercentage ?
                                                            item.UnitPrice * (tierData.Value / 100) : tierData.Value,
                                                        //fromPromo: true, 
                                                        IsPercentage = detailPromo.IsPercentage
                                                    });
                                                }
                                            }
                                            else
                                            {
                                                discPromo.Add(new SalesOrderDetailDiscount
                                                {
                                                    PromoCode = dataPromo.Code,
                                                    PromoDetailId = detailPromo.Id,
                                                    Name = dataPromo.Name,
                                                    //promoMethod: 1,
                                                    Value = tierData.Value,
                                                    //nettPrice: 0,
                                                    CoaCode = dataPromo.CoaCost,
                                                    Amount = detailPromo.IsPercentage ?
                                                        item.UnitPrice * (tierData.Value / 100) : tierData.Value,
                                                    //fromPromo: true,
                                                    IsPercentage = detailPromo.IsPercentage
                                                });
                                            }
                                        }
                                        break;
                                    case 3:
                                        // Apply to Barang - Promo Method Bonus
                                        var originalQty3 = qtyOriginal[item.Id];
                                        var tierData3 = applyTo == 3 ? detailTierPromo.FirstOrDefault(x => item.Qty >= x.FromQty && item.Qty <= x.ToQty) : detailTierPromo.FirstOrDefault(x => originalQty3 >= x.FromQty && originalQty3 <= x.ToQty && x.SaleUnit == item.UnitId);
                                        if (tierData3 != null)
                                        {
                                            var sellPrice = 0m;
                                            var freeItem = items.FirstOrDefault(x => x.Id == tierData3.FreeGoodItemId);
                                            var curUom3 = uomConversions.FirstOrDefault(x => x.Id == Convert.ToInt32(tierData3.UnitFreeGood));
                                            var itemUom3 = uomConversions.FirstOrDefault(x => x.Id == ctItem.UomSellId);
                                            if (itemUom3.Seq > curUom3.Seq)
                                            {
                                                var qtyField = Db.UoMConversions.Where(x => x.UomId == freeItem.UomId && x.Seq <= itemUom3.Seq && x.Seq > curUom3.Seq).Select(x => x.Conversion).ToList();
                                                var multipliedQty = qtyField.Aggregate(1, (x, y) => (int)(x * y));
                                                sellPrice = (decimal)freeItem.SellPrice / multipliedQty;
                                            }
                                            else if (itemUom3.Seq < curUom3.Seq)
                                            {
                                                var qtyField = Db.UoMConversions.Where(x => x.UomId == freeItem.UomId && x.Seq > itemUom3.Seq && x.Seq <= curUom3.Seq).Select(x => x.Conversion).ToList();
                                                var multipliedQty = qtyField.Aggregate(1, (x, y) => (int)(x * y));
                                                sellPrice = (decimal)freeItem.SellPrice * multipliedQty;

                                            }
                                            else
                                            {
                                                sellPrice = (decimal)freeItem.SellPrice;
                                            }

                                            if (tierData3.IsMultiple)
                                            {
                                                var multipleValue = Math.Floor((applyTo == 3 ? item.Qty : originalQty3) / tierData3.FromQty);
                                                bonusPromo.Add(new SalesOrderDetailFreeGood
                                                {
                                                    PromoCode = dataPromo.Code,
                                                    ItemId = freeItem.Id,
                                                    UomId = (int)freeItem.UomId,
                                                    UnitId = Convert.ToInt32(tierData3.UnitFreeGood),
                                                    Qty = multipleValue * tierData3.Value,
                                                    QtyClosed = 0m,
                                                    UnitPrice = sellPrice,
                                                    CoaCode = dataPromo.CoaCost
                                                });
                                            }
                                            else
                                            {
                                                bonusPromo.Add(new SalesOrderDetailFreeGood
                                                {
                                                    PromoCode = dataPromo.Code,
                                                    ItemId = freeItem.Id,
                                                    UomId = (int)freeItem.UomId,
                                                    UnitId = Convert.ToInt32(tierData3.UnitFreeGood),
                                                    Qty = tierData3.Value,
                                                    QtyClosed = 0m,
                                                    UnitPrice = sellPrice,
                                                    CoaCode = dataPromo.CoaCost
                                                });
                                            }
                                        }
                                        break;
                                    case 4:
                                        // Apply to Barang - Promo Method Payment Term
                                        var tierData4 = detailTierPromo.FirstOrDefault(x => x.PaymentTermId == data.PaymentTermId);
                                        if (tierData4 != null)
                                        {
                                            discPromo.Add(new SalesOrderDetailDiscount
                                            {
                                                PromoCode = dataPromo.Code,
                                                PromoDetailId = detailPromo.Id,
                                                Name = dataPromo.Name,
                                                //promoMethod: 1,
                                                Value = tierData4.Value,
                                                //nettPrice: 0,
                                                CoaCode = dataPromo.CoaCost,
                                                Amount = tierData4.IsPercentage ?
                                                    item.UnitPrice * (tierData4.Value / 100) :
                                                    tierData4.Value,
                                                //fromPromo: true,
                                                IsPercentage = detailPromo.IsPercentage
                                            });
                                        }
                                        break;
                                }
                            }
                            else if (applyTo == 4)
                            {
                                var multiItem = detailMultiPromo.Select(y => y.ItemId);
                                var isApplicable = multiItem.Intersect(data.ItemDetails.Select(x => x.ItemId));
                                if (isApplicable.Any() && isApplicable.Contains(item.ItemId))
                                {
                                    switch (detailPromo.PromoType)
                                    {
                                        case 2:
                                            // Apply to Barang - Promo Method Qty Barang
                                            var miData2 = dummyDetails.Where(x => multiItem.Contains(x.ItemId)).ToList();

                                            var tierData = detailTierPromo.FirstOrDefault(x => miData2.Sum(x => x.Qty) >= x.FromQty && miData2.Sum(x => x.Qty) <= x.ToQty);
                                            if (tierData != null)
                                            {
                                                var valueDisc = tierData.Value;
                                                var sumMulti = data.ItemDetails.Where(x => isApplicable.Contains(x.ItemId)).Sum(x => x.UnitPrice * x.Qty);
                                                var prorateDisc = valueDisc / sumMulti * (item.Qty * item.UnitPrice);
                                                discPromo.Add(new SalesOrderDetailDiscount
                                                {
                                                    PromoCode = dataPromo.Code,
                                                    PromoDetailId = detailPromo.Id,
                                                    Name = dataPromo.Name,
                                                    //promoMethod: 1,
                                                    Value = detailPromo.IsPercentage ? tierData.Value : prorateDisc,
                                                    //nettPrice: 0,
                                                    CoaCode = dataPromo.CoaCost,
                                                    Amount = detailPromo.IsPercentage ?
                                                            item.UnitPrice * (tierData.Value / 100) : prorateDisc,
                                                    //fromPromo: true,
                                                    IsPercentage = detailPromo.IsPercentage
                                                });
                                            }
                                            break;
                                        case 3:
                                            // Apply to Barang - Promo Method Bonus
                                            var miData3 = dummyDetails.Where(x => multiItem.Contains(x.ItemId)).ToList();

                                            var tierData3 = detailTierPromo.FirstOrDefault(x => miData3.Sum(x => x.Qty) >= x.FromQty && miData3.Sum(x => x.Qty) <= x.ToQty);
                                            if (tierData3 != null)
                                            {
                                                var sellPrice = 0m;
                                                var freeItem = items.FirstOrDefault(x => x.Id == tierData3.FreeGoodItemId);
                                                var curUom3 = uomConversions.FirstOrDefault(x => x.Id == Convert.ToInt32(tierData3.UnitFreeGood));
                                                var itemUom3 = uomConversions.FirstOrDefault(x => x.Id == ctItem.UomSellId);
                                                if (itemUom3.Seq > curUom3.Seq)
                                                {
                                                    var qtyField = Db.UoMConversions.Where(x => x.UomId == freeItem.UomId && x.Seq <= itemUom3.Seq && x.Seq > curUom3.Seq).Select(x => x.Conversion).ToList();
                                                    var multipliedQty = qtyField.Aggregate(1, (x, y) => (int)(x * y));
                                                    sellPrice = (decimal)freeItem.SellPrice / multipliedQty;
                                                }
                                                else if (itemUom3.Seq < curUom3.Seq)
                                                {
                                                    var qtyField = Db.UoMConversions.Where(x => x.UomId == freeItem.UomId && x.Seq > itemUom3.Seq && x.Seq <= curUom3.Seq).Select(x => x.Conversion).ToList();
                                                    var multipliedQty = qtyField.Aggregate(1, (x, y) => (int)(x * y));
                                                    sellPrice = (decimal)freeItem.SellPrice * multipliedQty;

                                                }
                                                else
                                                {
                                                    sellPrice = (decimal)freeItem.SellPrice;
                                                }

                                                if (tierData3.IsMultiple)
                                                {
                                                    var multipleValue = Math.Floor(miData3.Sum(x => x.Qty) / tierData3.FromQty);
                                                    bonusPromoMulti.Add(new SalesOrderDetailFreeGood
                                                    {
                                                        PromoCode = dataPromo.Code,
                                                        ItemId = freeItem.Id,
                                                        UomId = (int)freeItem.UomId,
                                                        UnitId = Convert.ToInt32(tierData3.UnitFreeGood),
                                                        Qty = multipleValue * tierData3.Value,
                                                        QtyClosed = 0m,
                                                        UnitPrice = sellPrice,
                                                        CoaCode = dataPromo.CoaCost
                                                    });
                                                }
                                                else
                                                {
                                                    bonusPromoMulti.Add(new SalesOrderDetailFreeGood
                                                    {
                                                        PromoCode = dataPromo.Code,
                                                        ItemId = freeItem.Id,
                                                        UomId = (int)freeItem.UomId,
                                                        UnitId = Convert.ToInt32(tierData3.UnitFreeGood),
                                                        Qty = tierData3.Value,
                                                        QtyClosed = 0m,
                                                        UnitPrice = sellPrice,
                                                        CoaCode = dataPromo.CoaCost
                                                    });
                                                }
                                            }
                                            break;
                                    }
                                }
                            }
                        }
                    }
                }

                item.Qty = qtyOriginal[item.Id];

                if (item.DiscountItemDetails != null)
                {
                    item.DiscountItemDetails = item.DiscountItemDetails.Where(x => x.PromoCode == null);
                    if (item.DiscountItemDetails.Any())
                    {
                        discPromo.AddRange(item.DiscountItemDetails);
                    }
                }

                item.Disc = 0m;
                if (discPromo.Any())
                {
                    foreach (var discItem in discPromo)
                    {
                        if (!data.ListPromo.Select(x => x.PromoCode).Contains(discItem.PromoCode)
                            || discItem.PromoCode == null
                            || data.ListPromo.Where(x => x.IsActive).Select(x => x.PromoCode).Contains(discItem.PromoCode))
                        {
                            item.Disc += discItem.Amount;
                        }
                    }
                }

                var taxData = taxes.FirstOrDefault(x => x.Id == item.TaxId);
                var discHeaderProrate = 0m;
                var sumDetail = data.ItemDetails.Sum(x => (x.UnitPrice - x.Disc) * x.Qty);
                if (data.FinalDiscPercent > 0)
                {
                    var amountPercent = sumDetail * (data.FinalDiscPercent / 100);
                    discHeaderProrate = amountPercent / sumDetail * (item.Qty * (item.UnitPrice - item.Disc));
                    discHeaderProrate /= item.Qty;
                }
                else if (data.FinalDisc > 0)
                {
                    discHeaderProrate = data.FinalDisc / sumDetail * (item.Qty * (item.UnitPrice - item.Disc));
                    discHeaderProrate /= item.Qty;
                }

                if (data.IncludeTax)
                {
                    item.TaxAmount = (item.UnitPrice - item.Disc - discHeaderProrate) - ((item.UnitPrice - item.Disc - discHeaderProrate) / (1 + (taxData.Rate / 100)));
                    item.ExemptTaxAmount = (item.UnitPrice - item.Disc - discHeaderProrate) - ((item.UnitPrice - item.Disc - discHeaderProrate) / (1 + (taxData.ExemptRate / 100)));
                    item.NettPrice = item.UnitPrice - item.Disc - discHeaderProrate;
                    item.Dpp = item.UnitPrice - item.Disc - discHeaderProrate - item.TaxAmount + item.ExemptTaxAmount;
                }
                else
                {
                    item.TaxAmount = (item.UnitPrice - item.Disc - discHeaderProrate) * (taxData.Rate / 100);
                    item.ExemptTaxAmount = (item.UnitPrice - item.Disc - discHeaderProrate) * (taxData.ExemptRate / 100);
                    item.NettPrice = item.UnitPrice - item.Disc - discHeaderProrate + item.TaxAmount - item.ExemptTaxAmount;
                    item.Dpp = item.UnitPrice - item.Disc - discHeaderProrate;
                }

                if (item.NettPrice < 0)
                {
                    result.Message = $"Data penjualan langsung tidak bisa diperbarui karena nilai bersih barang {ctItem.Initial} minus.";
                    return result;
                }

                item.FinalDiscHeader = discHeaderProrate;
                item.Total = item.Qty * item.NettPrice;
                totalDetail.Add(item.Total);
                totalTax.Add(item.Qty * item.TaxAmount);
                totalExemptTax.Add(item.Qty * item.ExemptTaxAmount);
                totalDpp.Add(item.Qty * item.Dpp);

                var orderDetail = new SalesOrderDetail();

                if (item.Id < 0)
                {
                    orderDetail = new SalesOrderDetail
                    {
                        Code = item.Code,
                        LineNo = ++i,
                        ItemId = item.ItemId,
                        UomId = item.UomId,
                        UnitId = item.UnitId,
                        Qty = item.Qty,
                        Length = item.Length,
                        Width = item.Width,
                        Height = item.Height,
                        Weight = item.Weight,
                        DimensionMeasurement = item.DimensionMeasurement,
                        WeightMeasurement = item.WeightMeasurement,
                        QtyDlv = 0,
                        UnitPrice = item.UnitPrice,
                        Disc = item.Disc,
                        FinalDiscHeader = item.FinalDiscHeader,
                        TaxId = item.TaxId,
                        TaxAmount = item.TaxAmount,
                        ExemptTaxAmount = item.ExemptTaxAmount,
                        NettPrice = item.NettPrice,
                        Total = item.Total,
                        Dpp = item.Dpp,
                        Notes = item.Notes,
                        CoaInventory = item.CoaInventory,
                        CoaCogs = item.CoaCogs,
                        CoaSls = item.CoaSls,
                        CoaSlsDisc = item.CoaSlsDisc,
                        CoaSlsReturn = item.CoaSlsReturn
                    };

                    Db.SalesOrderDetails.Add(orderDetail);

                    Db.SaveChanges();
                    listOrderIdDetail.Add(orderDetail.Id);
                }
                else
                {
                    orderDetail = Db.SalesOrderDetails.FirstOrDefault(x => x.Id == item.Id);
                    if (orderDetail != null)
                    {
                        orderDetail.LineNo = ++i;
                        orderDetail.ItemId = item.ItemId;
                        orderDetail.UomId = item.UomId;
                        orderDetail.UnitId = item.UnitId;
                        orderDetail.Qty = item.Qty;
                        orderDetail.Length = item.Length;
                        orderDetail.Width = item.Width;
                        orderDetail.Height = item.Height;
                        orderDetail.Weight = item.Weight;
                        orderDetail.DimensionMeasurement = item.DimensionMeasurement;
                        orderDetail.WeightMeasurement = item.WeightMeasurement;
                        orderDetail.QtyDlv = 0;
                        orderDetail.UnitPrice = item.UnitPrice;
                        orderDetail.Disc = item.Disc;
                        orderDetail.FinalDiscHeader = item.FinalDiscHeader;
                        orderDetail.TaxId = item.TaxId;
                        orderDetail.TaxAmount = item.TaxAmount;
                        orderDetail.ExemptTaxAmount = item.ExemptTaxAmount;
                        orderDetail.NettPrice = item.NettPrice;
                        orderDetail.Total = item.Total;
                        orderDetail.Dpp = item.Dpp;
                        orderDetail.Notes = item.Notes;
                        orderDetail.CoaInventory = item.CoaInventory;
                        orderDetail.CoaCogs = item.CoaCogs;
                        orderDetail.CoaSls = item.CoaSls;
                        orderDetail.CoaSlsDisc = item.CoaSlsDisc;
                        orderDetail.CoaSlsReturn = item.CoaSlsReturn;

                        Db.SalesOrderDetails.Update(orderDetail);
                        Db.Entry(item).Property(e => e.Code).IsModified = false;

                        listOrderIdDetail.Add(item.Id);
                    }
                }

                var delDiscDetails = Db.SalesOrderDetailDiscounts
                    .Where(d => d.Code == data.Code && d.OrderDetailId == item.Id && !discPromo.Select(x => x.Id).Contains(d.Id))
                    .ToList();

                Db.SalesOrderDetailDiscounts.RemoveRange(delDiscDetails);

                if (discPromo.Any())
                {
                    short d = 0;
                    foreach (var discItem in discPromo)
                    {
                        if (!data.ListPromo.Select(x => x.PromoCode).Contains(discItem.PromoCode)
                                || discItem.PromoCode == null
                                || data.ListPromo.Where(x => x.IsActive).Select(x => x.PromoCode).Contains(discItem.PromoCode))
                        {
                            if (discItem.Id <= 0)
                            {
                                Db.SalesOrderDetailDiscounts.Add(new SalesOrderDetailDiscount
                                {
                                    Code = data.Code,
                                    OrderDetailId = listOrderIdDetail[i - 1],
                                    LineNo = ++d,
                                    PromoCode = discItem.PromoCode,
                                    PromoDetailId = discItem.PromoDetailId,
                                    Name = discItem.Name,
                                    IsPercentage = discItem.IsPercentage,
                                    Value = discItem.Value,
                                    Amount = discItem.Amount,
                                    CoaCode = discItem.CoaCode
                                });
                            }
                            else
                            {
                                discItem.LineNo = ++d;

                                Db.SalesOrderDetailDiscounts.Update(discItem);
                                Db.Entry(discItem).Property(e => e.Id).IsModified = false;
                                Db.Entry(discItem).Property(e => e.Code).IsModified = false;
                            }

                            if (!soPromo.Any(x => x.PromoCode == discItem.PromoCode))
                            {
                                if (discItem.PromoCode != null)
                                {
                                    soPromo.Add(new SalesOrderPromo
                                    {
                                        Code = data.Code,
                                        LineNo = (short)(soPromo.Count + 1),
                                        PromoCode = discItem.PromoCode,
                                        IsActive = true
                                    });
                                }
                            }
                        }
                        else
                        {
                            if (!soPromo.Any(x => x.PromoCode == discItem.PromoCode))
                            {
                                if (discItem.PromoCode != null)
                                {

                                    soPromo.Add(new SalesOrderPromo
                                    {
                                        Code = data.Code,
                                        LineNo = (short)(soPromo.Count + 1),
                                        PromoCode = discItem.PromoCode,
                                        IsActive = false
                                    });
                                }
                            }
                        }
                    }
                }

                if (bonusPromoMulti.Any() && item.LineNo == 1)
                {
                    bonusPromo.AddRange(bonusPromoMulti);
                }

                var delFreeDetails = Db.SalesOrderDetailFreeGoods
                    .Where(d => d.Code == data.Code && d.OrderDetailId == item.Id && !bonusPromo.Select(x => x.Id).Contains(d.Id))
                    .ToList();

                Db.SalesOrderDetailFreeGoods.RemoveRange(delFreeDetails);

                if (bonusPromo.Any())
                {
                    if (IsQtyExcessFree(data.WarehouseCode, bonusPromo, data.Code))
                    {
                        result.Message = "Data penjualan langsung tidak bisa disimpan karena qty bonus yang dipesan lebih besar dari qty yang tersedia.";
                        return result;
                    }

                    short f = 0;
                    foreach (var freeItem in bonusPromo)
                    {
                        if (!data.ListPromo.Select(x => x.PromoCode).Contains(freeItem.PromoCode)
                                || data.ListPromo.Where(x => x.IsActive).Select(x => x.PromoCode).Contains(freeItem.PromoCode))
                        {
                            if (freeItem.Id <= 0)
                            {
                                Db.SalesOrderDetailFreeGoods.Add(new SalesOrderDetailFreeGood
                                {
                                    Code = data.Code,
                                    OrderDetailId = listOrderIdDetail[i - 1],
                                    LineNo = ++f,
                                    PromoCode = freeItem.PromoCode,
                                    ItemId = freeItem.ItemId,
                                    UomId = freeItem.UomId,
                                    UnitId = freeItem.UnitId,
                                    Qty = freeItem.Qty,
                                    QtyClosed = freeItem.QtyClosed,
                                    UnitPrice = freeItem.UnitPrice,
                                    CoaCode = freeItem.CoaCode
                                });
                            }
                            else
                            {
                                freeItem.LineNo = ++f;

                                Db.SalesOrderDetailFreeGoods.Update(freeItem);
                                Db.Entry(freeItem).Property(e => e.Id).IsModified = false;
                                Db.Entry(freeItem).Property(e => e.Code).IsModified = false;
                            }

                            if (!soPromo.Any(x => x.PromoCode == freeItem.PromoCode))
                            {
                                soPromo.Add(new SalesOrderPromo
                                {
                                    Code = data.Code,
                                    LineNo = (short)(soPromo.Count + 1),
                                    PromoCode = freeItem.PromoCode,
                                    IsActive = data.ListPromo.FirstOrDefault(x => x.PromoCode == freeItem.PromoCode)?.IsActive ?? true
                                });
                            }
                        }
                        else
                        {
                            if (!soPromo.Any(x => x.PromoCode == freeItem.PromoCode))
                            {
                                soPromo.Add(new SalesOrderPromo
                                {
                                    Code = data.Code,
                                    LineNo = (short)(soPromo.Count + 1),
                                    PromoCode = freeItem.PromoCode,
                                    IsActive = false
                                });
                            }
                        }
                    }
                }
            }

            // Update Delivery detail data
            // Get detail data that exists in order before
            var delDetails = Db.SalesDeliveryDetails
                .Where(d => d.Code == InvoiceDetailData.DoCode)
                .ToList();

            // Get detail data that exists in receive before
            Db.SalesDeliveryDetails.RemoveRange(delDetails);

            short j = 0;
            foreach (var item in data.ItemDetails)
            {
                var deliveryDetail = new SalesDeliveryDetail
                {
                    Code = InvoiceDetailData.DoCode,
                    LineNo = ++j,
                    ItemId = item.ItemId,
                    UomId = item.UomId,
                    UnitId = item.UnitId,
                    Qty = item.Qty,
                    Length = item.Length,
                    Width = item.Width,
                    Height = item.Height,
                    Weight = item.Weight,
                    DimensionMeasurement = item.DimensionMeasurement,
                    WeightMeasurement = item.WeightMeasurement,
                    UnitPrice = item.UnitPrice,
                    Disc = item.Disc,
                    FinalDiscHeader = item.FinalDiscHeader,
                    TaxId = item.TaxId,
                    TaxAmount = item.TaxAmount,
                    ExemptTaxAmount = item.ExemptTaxAmount,
                    NettPrice = item.NettPrice,
                    Total = item.Total,
                    Dpp = item.Dpp
                };
                Db.SalesDeliveryDetails.Add(deliveryDetail);

                Db.SaveChanges();
                listDeliveryIdDetail.Add(deliveryDetail.Id);

                var delFreeDetails = Db.SalesDeliveryDetailFreeGoods
                    .Where(d => d.Code == data.Code)
                    .ToList();

                Db.SalesDeliveryDetailFreeGoods.RemoveRange(delFreeDetails);

                var orderFree = Db.SalesOrderDetailFreeGoods.Where(x => x.Code == data.Code).ToList();
                if (orderFree.Any())
                {
                    short f = 0;
                    foreach (var freeItem in orderFree)
                    {
                        Db.SalesDeliveryDetailFreeGoods.Add(new SalesDeliveryDetailFreeGood
                        {
                            Code = InvoiceDetailData.DoCode,
                            DlvOrderDetailId = listDeliveryIdDetail[j - 1],
                            LineNo = ++f,
                            PromoCode = freeItem.PromoCode,
                            ItemId = freeItem.ItemId,
                            UomId = freeItem.UomId,
                            UnitId = freeItem.UnitId,
                            Qty = freeItem.Qty,
                            UnitPrice = freeItem.UnitPrice,
                            CoaCode = freeItem.CoaCode
                        });

                        freeItem.QtyClosed = freeItem.Qty;
                        Db.SalesOrderDetailFreeGoods.Update(freeItem);
                    }
                    Db.SaveChanges();
                }
            }

            if (data.FinalDiscPercent > 0)
                data.FinalDisc = data.ItemDetails.Sum(x => x.FinalDiscHeader * x.Qty);
            data.SubTotal = totalDetail.Sum();
            data.TaxAmount = totalTax.Sum();
            data.ExemptTaxAmount = totalExemptTax.Sum();
            data.Dpp = totalDpp.Sum();
            data.Total = data.SubTotal;

            foreach (var item in data.ItemDetails)
            {
                //Promo
                var listPromo = promos.Where(x => x.ApplyTo == 1 ||
                                                  x.Subject.Select(y => y.CustCode).Contains(data.CustCode) ||
                                                  x.Subject.Select(y => y.CustTypeId).Contains(data.CustTypeId)).ToList();

                List<SalesOrderDetailDiscount> discPromo = new();
                List<SalesOrderDetailFreeGood> bonusPromo = new();
                var ctItem = items.FirstOrDefault(j => j.Id == item.ItemId);
                if (listPromo.Any())
                {
                    foreach (var dataPromo in listPromo)
                    {
                        var listDetailPromo = Db.PromoDetails.Where(x => x.Code == dataPromo.Code).ToList();
                        foreach (var detailPromo in listDetailPromo)
                        {
                            var detailTierPromo = Db.PromoDetailTiers.Where(x => x.PromoDetailId == detailPromo.Id).ToList();
                            var applyTo = detailPromo.ApplyTo;
                            if (applyTo == 2)
                            {
                                switch (detailPromo.PromoType)
                                {
                                    case 4:
                                        // Apply to Barang - Promo Method Payment Term
                                        var tierData = detailTierPromo.FirstOrDefault(x => x.PaymentTermId == data.PaymentTermId);
                                        if (tierData != null)
                                        {
                                            var curUom = uomConversions.FirstOrDefault(x => x.Id == item.UnitId);
                                            var itemUom = uomConversions.FirstOrDefault(x => x.Id == ctItem.UomSellId);
                                            var discAmount = tierData.IsPercentage ? data.SubTotal * (tierData.Value / 100) : tierData.Value;
                                            var prorateDisc = (discAmount / data.SubTotal * (item.Qty * (item.UnitPrice - item.Disc))) / item.Qty;

                                            discPromo.Add(new SalesOrderDetailDiscount
                                            {
                                                PromoCode = dataPromo.Code,
                                                PromoDetailId = detailPromo.Id,
                                                Name = dataPromo.Name,
                                                //promoMethod: 1,
                                                Value = prorateDisc,
                                                //nettPrice: 0,
                                                CoaCode = dataPromo.CoaCost,
                                                Amount = prorateDisc,
                                                //fromPromo: true,
                                                IsPercentage = false
                                            });
                                        }
                                        break;
                                    case 5:
                                        // Apply to Faktur - Promo Method Nilai Trans
                                        var tierData5 = detailTierPromo.FirstOrDefault(x => data.SubTotal >= x.FromQty && data.SubTotal <= x.ToQty);
                                        if (tierData5 != null)
                                        {
                                            var curUom = uomConversions.FirstOrDefault(x => x.Id == item.UnitId);
                                            var itemUom = uomConversions.FirstOrDefault(x => x.Id == ctItem.UomSellId);
                                            var discAmount = tierData5.IsPercentage ? data.SubTotal * (tierData5.Value / 100) : tierData5.Value;
                                            var prorateDisc = (discAmount / data.SubTotal * (item.Qty * (item.UnitPrice - item.Disc))) / item.Qty;

                                            discPromo.Add(new SalesOrderDetailDiscount
                                            {
                                                PromoCode = dataPromo.Code,
                                                PromoDetailId = detailPromo.Id,
                                                Name = dataPromo.Name,
                                                //promoMethod: 1,
                                                Value = prorateDisc,
                                                //nettPrice: 0,
                                                CoaCode = dataPromo.CoaCost,
                                                Amount = prorateDisc,
                                                //fromPromo: true,
                                                IsPercentage = false
                                            });
                                        }
                                        break;
                                }
                            }
                        }
                    }
                }

                if (discPromo.Any())
                {
                    foreach (var discItem in discPromo)
                    {
                        if (!data.ListPromo.Select(x => x.PromoCode).Contains(discItem.PromoCode)
                            || discItem.PromoCode == null
                            || data.ListPromo.Where(x => x.IsActive).Select(x => x.PromoCode).Contains(discItem.PromoCode))
                        {
                            item.Disc += discItem.Amount;
                        }
                    }

                    var taxData = taxes.FirstOrDefault(x => x.Id == item.TaxId);
                    var discHeaderProrate = 0m;
                    var sumDetail = data.ItemDetails.Sum(x => (x.UnitPrice - x.Disc) * x.Qty);
                    if (data.FinalDiscPercent > 0)
                    {
                        var amountPercent = sumDetail * (data.FinalDiscPercent / 100);
                        discHeaderProrate = amountPercent / sumDetail * (item.Qty * (item.UnitPrice - item.Disc));
                        discHeaderProrate /= item.Qty;
                    }
                    else if (data.FinalDisc > 0)
                    {
                        discHeaderProrate = data.FinalDisc / sumDetail * (item.Qty * (item.UnitPrice - item.Disc));
                        discHeaderProrate /= item.Qty;
                    }

                    if (data.IncludeTax)
                    {
                        item.TaxAmount = (item.UnitPrice - item.Disc - discHeaderProrate) - ((item.UnitPrice - item.Disc - discHeaderProrate) / (1 + (taxData.Rate / 100)));
                        item.ExemptTaxAmount = (item.UnitPrice - item.Disc - discHeaderProrate) - ((item.UnitPrice - item.Disc - discHeaderProrate) / (1 + (taxData.ExemptRate / 100)));
                        item.NettPrice = item.UnitPrice - item.Disc - discHeaderProrate;
                        item.Dpp = item.UnitPrice - item.Disc - discHeaderProrate - item.TaxAmount + item.ExemptTaxAmount;
                    }
                    else
                    {
                        item.TaxAmount = (item.UnitPrice - item.Disc - discHeaderProrate) * (taxData.Rate / 100);
                        item.ExemptTaxAmount = (item.UnitPrice - item.Disc - discHeaderProrate) * (taxData.ExemptRate / 100);
                        item.NettPrice = item.UnitPrice - item.Disc - discHeaderProrate + item.TaxAmount - item.ExemptTaxAmount;
                        item.Dpp = item.UnitPrice - item.Disc - discHeaderProrate;
                    }

                    if (item.NettPrice < 0)
                    {
                        result.Message = $"Data order penjualan tidak bisa disimpan karena nilai bersih barang {ctItem.Initial} minus.";
                        return result;
                    }

                    item.FinalDiscHeader = discHeaderProrate;
                    item.Total = item.Qty * item.NettPrice;
                    totalDetail.Add(item.Total);
                    totalTax.Add(item.Qty * item.TaxAmount);
                    totalExemptTax.Add(item.Qty * item.ExemptTaxAmount);
                    totalDpp.Add(item.Qty * item.Dpp);

                    var soDetail = Db.SalesOrderDetails.FirstOrDefault(x => x.Code == data.Code && x.ItemId == item.ItemId && x.UnitId == item.UnitId);
                    var orderDetail = new SalesOrderDetail();
                    if (soDetail == null)
                    {
                        orderDetail = new SalesOrderDetail
                        {
                            Code = data.Code,
                            LineNo = ++i,
                            ItemId = item.ItemId,
                            UomId = item.UomId,
                            UnitId = item.UnitId,
                            Qty = item.Qty,
                            Length = item.Length,
                            Width = item.Width,
                            Height = item.Height,
                            Weight = item.Weight,
                            DimensionMeasurement = item.DimensionMeasurement,
                            WeightMeasurement = item.WeightMeasurement,
                            QtyDlv = 0,
                            UnitPrice = item.UnitPrice,
                            Disc = item.Disc,
                            FinalDiscHeader = item.FinalDiscHeader,
                            TaxId = item.TaxId,
                            TaxAmount = item.TaxAmount,
                            ExemptTaxAmount = item.ExemptTaxAmount,
                            NettPrice = item.NettPrice,
                            Total = item.Total,
                            Dpp = item.Dpp,
                            Notes = item.Notes,
                            CoaInventory = item.CoaInventory,
                            CoaCogs = item.CoaCogs,
                            CoaSls = item.CoaSls,
                            CoaSlsDisc = item.CoaSlsDisc,
                            CoaSlsReturn = item.CoaSlsReturn,
                            CoaTransit = string.IsNullOrEmpty(item.CoaTransit) ? Db.SystemParameters.FirstOrDefault(x => x.Code == "TRANSIT_ITEM_COA")?.Value ?? "" : item.CoaTransit
                        };

                        Db.SalesOrderDetails.Add(orderDetail);
                    }
                    else
                    {
                        totalDetail.Add(-soDetail.Total);
                        totalTax.Add(-(soDetail.Qty * soDetail.TaxAmount));
                        totalExemptTax.Add(-(soDetail.Qty * soDetail.ExemptTaxAmount));
                        totalDpp.Add(-(soDetail.Qty * soDetail.Dpp));

                        soDetail.Disc = item.Disc;
                        soDetail.FinalDiscHeader = item.FinalDiscHeader;
                        soDetail.TaxId = item.TaxId;
                        soDetail.TaxAmount = item.TaxAmount;
                        soDetail.ExemptTaxAmount = item.ExemptTaxAmount;
                        soDetail.NettPrice = item.NettPrice;
                        soDetail.Total = item.Total;
                        soDetail.Dpp = item.Dpp;

                        Db.SalesOrderDetails.Update(soDetail);
                    }
                }
                Db.SaveChanges();

                if (discPromo != null)
                {
                    if (discPromo.Any())
                    {
                        short d = 0;
                        foreach (var discItem in discPromo)
                        {
                            if (!data.ListPromo.Select(x => x.PromoCode).Contains(discItem.PromoCode)
                                || discItem.PromoCode == null
                                || data.ListPromo.Where(x => x.IsActive).Select(x => x.PromoCode).Contains(discItem.PromoCode))
                            {
                                if (discItem.Id <= 0)
                                {
                                    Db.SalesOrderDetailDiscounts.Add(new SalesOrderDetailDiscount
                                    {
                                        Code = data.Code,
                                        OrderDetailId = listOrderIdDetail[i - 1],
                                        LineNo = ++d,
                                        PromoCode = discItem.PromoCode,
                                        PromoDetailId = discItem.PromoDetailId,
                                        Name = discItem.Name,
                                        IsPercentage = discItem.IsPercentage,
                                        Value = discItem.Value,
                                        Amount = discItem.Amount,
                                        CoaCode = discItem.CoaCode
                                    });
                                }
                                else
                                {
                                    discItem.LineNo = ++d;

                                    Db.SalesOrderDetailDiscounts.Update(discItem);
                                    Db.Entry(discItem).Property(e => e.Id).IsModified = false;
                                    Db.Entry(discItem).Property(e => e.Code).IsModified = false;
                                }

                                if (!soPromo.Any(x => x.PromoCode == discItem.PromoCode))
                                {
                                    if (discItem.PromoCode != null)
                                    {
                                        soPromo.Add(new SalesOrderPromo
                                        {
                                            Code = data.Code,
                                            LineNo = (short)(soPromo.Count + 1),
                                            PromoCode = discItem.PromoCode,
                                            IsActive = true
                                        });
                                    }
                                }
                            }
                            else
                            {
                                if (!soPromo.Any(x => x.PromoCode == discItem.PromoCode))
                                {
                                    if (discItem.PromoCode != null)
                                    {

                                        soPromo.Add(new SalesOrderPromo
                                        {
                                            Code = data.Code,
                                            LineNo = (short)(soPromo.Count + 1),
                                            PromoCode = discItem.PromoCode,
                                            IsActive = false
                                        });
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (soPromo.Any())
                Db.SalesOrderPromos.AddRange(soPromo);

            if (data.FinalDiscPercent > 0)
                data.FinalDisc = data.ItemDetails.Sum(x => x.FinalDiscHeader * x.Qty);
            data.SubTotal = totalDetail.Sum();
            data.TaxAmount = totalTax.Sum();
            data.ExemptTaxAmount = totalExemptTax.Sum();
            data.Dpp = totalDpp.Sum();
            data.Total = data.SubTotal;

            // Update Order header data
            orderData.Date = data.Date;
            orderData.CustCode = data.CustCode;
            orderData.PaymentTermId = data.PaymentTermId;
            orderData.SalesBy = data.SalesBy;
            orderData.WarehouseCode = data.WarehouseCode;
            orderData.CurrCode = data.CurrCode;
            orderData.Rate = data.Rate;
            orderData.SubTotal = data.SubTotal;
            orderData.FinalDiscPercent = data.FinalDiscPercent;
            orderData.FinalDisc = data.FinalDisc;
            orderData.IncludeTax = data.IncludeTax;
            orderData.TaxAmount = data.TaxAmount;
            orderData.ExemptTaxAmount = data.ExemptTaxAmount;
            orderData.Total = data.Total;
            orderData.Dpp = data.Dpp;
            orderData.Notes = data.Notes;
            orderData.Mark = data.Mark;
            orderData.UpdatedBy = data.UpdatedBy;
            orderData.UpdatedDate = data.UpdatedDate;
            Db.SalesOrderHeaders.Update(orderData);
            Db.Entry(orderData).Property(e => e.Code).IsModified = false;
            Db.Entry(orderData).Property(e => e.CreatedBy).IsModified = false;
            Db.Entry(orderData).Property(e => e.CreatedDate).IsModified = false;


            // Update Delivery header data
            deliveryData.Date = data.Date;
            deliveryData.TransCode = orderData.Code;
            deliveryData.CustCode = data.CustCode;
            deliveryData.WarehouseCode = data.WarehouseCode;
            deliveryData.ShippedBy = data.SalesBy;
            deliveryData.CurrCode = data.CurrCode;
            deliveryData.Rate = data.Rate;
            deliveryData.SubTotal = data.SubTotal;
            deliveryData.FinalDiscPercent = data.FinalDiscPercent;
            deliveryData.FinalDisc = data.FinalDisc;
            deliveryData.IncludeTax = data.IncludeTax;
            deliveryData.TaxAmount = data.TaxAmount;
            deliveryData.ExemptTaxAmount = data.ExemptTaxAmount;
            deliveryData.Total = data.Total;
            deliveryData.Dpp = data.Dpp;
            deliveryData.Mark = data.Mark == "OL" ? "OL" : "INV";
            deliveryData.Notes = data.Notes;
            deliveryData.UpdatedBy = data.UpdatedBy;
            deliveryData.UpdatedDate = data.UpdatedDate;
            Db.SalesDeliveryHeaders.Update(deliveryData);
            Db.Entry(deliveryData).Property(e => e.Code).IsModified = false;
            Db.Entry(deliveryData).Property(e => e.SrcTrans).IsModified = false;
            Db.Entry(deliveryData).Property(e => e.CreatedBy).IsModified = false;
            Db.Entry(deliveryData).Property(e => e.CreatedDate).IsModified = false;

            // Update Invoice detail data
            InvoiceDetailData.SubTotal = data.SubTotal;
            InvoiceDetailData.FinalDisc = data.FinalDisc;
            InvoiceDetailData.TaxAmount = data.TaxAmount;
            InvoiceDetailData.ExemptTaxAmount = data.ExemptTaxAmount;
            InvoiceDetailData.Total = data.Total;
            InvoiceDetailData.Dpp = data.Dpp;
            Db.SalesInvoiceDetails.Update(InvoiceDetailData);
            Db.Entry(InvoiceDetailData).Property(e => e.Code).IsModified = false;


            // Restore Credit Note
            RestoreCreditMemo(data.Code);

            // Get detail data that exists in invoice before
            var delMemos = Db.SalesInvoiceCreditMemos
                .Where(d => d.InvCode == data.Code)
                .ToList();

            // Delete memo data that exists in invoice before
            Db.SalesInvoiceCreditMemos.RemoveRange(delMemos);

            var newMemos = new List<SalesInvoiceCreditMemo>();
            foreach (var item in data.Memos)
            {
                newMemos.Add(new SalesInvoiceCreditMemo
                {
                    InvCode = data.Code,
                    InvAmount = data.Total,
                    CreditMemoAmount = item.CreditMemoAmount,
                    CreditMemoCode = item.CreditMemoCode,
                    Src = item.Src
                });
            }

            // Insert detail if new data exists
            if (newMemos.Any())
                Db.SalesInvoiceCreditMemos.AddRange(newMemos);


            // Update Invoice header data
            data.PaidAmount = newMemos.Any() ? newMemos.Sum(x => x.CreditMemoAmount) : 0;
            data.Mark = data.Mark == "OL" ? "OL" : data.PaidAmount > 0 ? data.Total == data.PaidAmount ? "CMP" : "PP" : "A";
            Db.SalesInvoiceHeaders.Update(data);
            Db.Entry(data).Property(e => e.Code).IsModified = false;
            Db.Entry(data).Property(e => e.CreatedBy).IsModified = false;
            Db.Entry(data).Property(e => e.CreatedDate).IsModified = false;

            // Save changes
            Db.SaveChanges();

            // Credit Used
            if (!isOverLimit)
            {
                UpdateCreditUsed(data.CustCode, data.Total);

                // Execute sp_update_stock_mutation_from_so
                Db.Database.ExecuteSqlRaw(
                    "EXEC sp_update_stock_mutation_from_so {0}, {1}",
                    data.SoCode, data.Date);

                // Execute sp_update_stock_mutation_from_do
                Db.Database.ExecuteSqlRaw(
                    "EXEC sp_update_stock_mutation_from_do {0}, {1}, {2}",
                    InvoiceDetailData.DoCode, data.Date, data.SoCode);

                // Execute sp_update_stock_mutation_from_si
                Db.Database.ExecuteSqlRaw(
                    "EXEC sp_update_stock_mutation_from_si {0}, {1}, {2}",
                    data.Code, data.Date, data.Code);

                Db.Database.ExecuteSqlRaw("EXEC sp_update_so_dlv_qty {0}", data.Code);
            }

            // Check all sales delivery are invoiced
            //if (
            //    !Db.SalesDeliveryHeaders
            //        .Any(x => x.TransCode == data.SoCode && x.Mark != "INV"))
            //{
            //    // Update sales order to closed
            //    Db.Database.ExecuteSqlRaw(
            //        "UPDATE Sales.SalesOrderHeader SET Mark='CLS' WHERE Code={0} AND Mark='CMP'", data.SoCode);
            //}
            //else
            //{
            //    // Update sales order to partial receive or completed
            //    var soMark = Db.SalesOrderDetails.Any(x => x.Code == data.SoCode && x.Qty > x.QtyDlv)
            //        ? "PS"
            //        : "CMP";

            //    Db.Database.ExecuteSqlRaw(
            //        "UPDATE Sales.SalesOrderHeader SET Mark={0} WHERE Code={1}", soMark, data.SoCode);
            //}

            UpdateCreditMemo(data);
            transaction.Commit();
        }
        catch (Exception ex)
        {
            result.Message = ex.InnerException?.Message ?? ex.Message;
            return result;
        }
        result.Success = true;
        result.Data = data.Code;
        result.Message = "Data penjualan langsung berhasil diperbarui.";
        return result;
    }

    public SaveResult Delete(string code, int userId)
    {
        var result = new SaveResult(false);

        var data = Db.SalesInvoiceHeaders.Find(code);
        if (data != null)
        {

            var related = GetRelatedTransactions(data.Code);
            if (related.Count > 0)
            {
                result.Message = "Data penjualan langsung tidak bisa ditandai sebagai void karena terdapat transaksi terkait.";
                return result;
            }

            // Checking mark header data
            if (data.Mark == "V")
            {
                result.Message = "Data penjualan langsung tidak bisa ditandai sebagai void karena sudah ditandai sebagai void.";
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

                // Get sales delivery code and join
                var doCodeJoin = string.Join("','",
                    Db.SalesInvoiceDetails
                        .Where(x => x.Code == code)
                        .Select(x => x.DoCode.Replace("'", "''")));

                // Update sales delivery to active
                Db.Database.ExecuteSqlRaw(
                    $"UPDATE Sales.SalesDeliveryHeader SET Mark='A' WHERE Code IN ('{doCodeJoin}')");

                // Update sales order to partial receive or completed
                var soMark = Db.SalesOrderDetails.Any(x => x.Code == data.SoCode && x.Qty > x.QtyDlv)
                    ? "PS"
                    : "CMP";

                Db.Database.ExecuteSqlRaw(
                    "UPDATE Sales.SalesOrderHeader SET Mark={0} WHERE Code={1}", soMark, data.SoCode);

                // Restore Credit Note
                RestoreCreditMemo(code);

                // Decrease CreditUsed
                RestoreCreditUsed(data.SoCode, data.CustCode);

                transaction.Commit();
            }
            catch (Exception ex)
            {
                result.Message = ex.InnerException?.Message ?? ex.Message;
                return result;
            }
        }

        result.Success = true;
        result.Message = "Data penjualan langsung berhasil ditandai sebagai void.";
        return result;
    }

    private bool IsQtyExcess(string warehouseCode, IEnumerable<SalesOrderDetail> items, string code)
    {
        var result = false;
        foreach (var item in items)
        {
            var uom = Db.UoMConversions.FirstOrDefault(x => x.Id == item.UnitId);
            var stock = Db.WarehouseQuantities.FirstOrDefault(x => x.WarehouseCode == warehouseCode && x.ItemId == item.ItemId);
            if (stock != null)
            {
                if (code == null)
                {
                    if (uom.IsBaseUnit)
                    {
                        if (item.Qty > stock.QtyOnHand)
                        {
                            result = true;
                        }
                    }
                    else
                    {
                        var qtyField = Db.UoMConversions.Where(x => x.UomId == item.UomId && x.Seq <= uom.Seq).Select(x => x.Conversion).ToList();
                        var multipliedQty = qtyField.Aggregate(1, (x, y) => (int)(x * y));
                        var baseQty = item.Qty * multipliedQty;
                        if (baseQty > stock.QtyOnHand)
                        {
                            result = true;
                        }
                    }
                }
                else
                {
                    var oldStock = Db.StockMutations.FirstOrDefault(x => x.ItemId == item.ItemId && x.UnitId == item.UnitId && x.RefCode1 == code);
                    if (oldStock != null)
                    {
                        if (uom.IsBaseUnit)
                        {
                            if (item.Qty > (stock.QtyOnHand + oldStock.BaseQty))
                            {
                                result = true;
                            }
                        }
                        else
                        {
                            var qtyField = Db.UoMConversions.Where(x => x.UomId == item.UomId && x.Seq <= uom.Seq).Select(x => x.Conversion).ToList();
                            var multipliedQty = qtyField.Aggregate(1, (x, y) => (int)(x * y));
                            var baseQty = item.Qty * multipliedQty;
                            if (baseQty > (stock.QtyOnHand + oldStock.BaseQty))
                            {
                                result = true;
                            }
                        }
                    }
                }
            }
            else
            {
                result = true;
            }
        }
        return result;
    }

    private bool IsQtyExcessFree(string warehouseCode, IEnumerable<SalesOrderDetailFreeGood> items, string code)
    {
        var result = false;
        foreach (var item in items)
        {
            var uom = Db.UoMConversions.FirstOrDefault(x => x.Id == item.UnitId);
            var stock = Db.WarehouseQuantities.FirstOrDefault(x => x.WarehouseCode == warehouseCode && x.ItemId == item.ItemId);
            if (stock != null)
            {
                if (code == null)
                {
                    if (uom.IsBaseUnit)
                    {
                        if (item.Qty > (stock.QtyOnHand - stock.QtyOnOrder))
                        {
                            result = true;
                        }
                    }
                    else
                    {
                        var qtyField = Db.UoMConversions.Where(x => x.UomId == item.UomId && x.Seq <= uom.Seq).Select(x => x.Conversion).ToList();
                        var multipliedQty = qtyField.Aggregate(1, (x, y) => (int)(x * y));
                        var baseQty = item.Qty * multipliedQty;
                        if (baseQty > (stock.QtyOnHand - stock.QtyOnOrder))
                        {
                            result = true;
                        }
                    }
                }
                else
                {
                    var oldStock = Db.StockMutations.FirstOrDefault(x => x.ItemId == item.ItemId && x.RefCode1 == code);
                    if (oldStock != null)
                    {
                        if (uom.IsBaseUnit)
                        {
                            if (item.Qty > (stock.QtyOnHand - (stock.QtyOnOrder - oldStock.BaseQty)))
                            {
                                result = true;
                            }
                        }
                        else
                        {
                            var qtyField = Db.UoMConversions.Where(x => x.UomId == item.UomId && x.Seq <= uom.Seq).Select(x => x.Conversion).ToList();
                            var multipliedQty = qtyField.Aggregate(1, (x, y) => (int)(x * y));
                            var baseQty = item.Qty * multipliedQty;
                            if (baseQty > (stock.QtyOnHand - (stock.QtyOnOrder - oldStock.BaseQty)))
                            {
                                result = true;
                            }
                        }
                    }
                }
            }
            else
            {
                result = true;
            }
        }
        return result;
    }

    private (bool, string) CheckDuplicateDetail(IEnumerable<SalesOrderDetail> data)
    {
        var tData = data.GroupBy(x => new { x.ItemId, x.UnitId }).Where(y => y.Count() > 1);
        var errorList = "";
        foreach (var itemData in tData)
        {
            var item = Db.Items.FirstOrDefault(x => x.Id == itemData.Key.ItemId);
            var uom = Db.UoMConversions.FirstOrDefault(x => x.Id == itemData.Key.UnitId);
            errorList += $"&bull; Barang {item.Initial} dengan satuan {uom.UnitEquivalent} tidak dapat duplikat.<br/>";
        }

        return (errorList != "", errorList);
    }

    private List<dynamic> FindItemCategoryHierarchy(int? categoryId)
    {
        var result = new List<dynamic>();
        if (categoryId.HasValue)
        {
            var query = @$"DECLARE @id INT
                            SET @id = {categoryId.Value};
                            WITH hierarchy AS (
	                            SELECT t.*
	                            FROM Inventory.ItemCategory t
	                            WHERE t.Id = @id
	                            UNION ALL
	                            SELECT x.*
	                            FROM Inventory.ItemCategory x
	                            JOIN hierarchy h ON h.ParentId = x.Id)
                            SELECT *
                            FROM hierarchy h";
            result = Db.ItemCategories.FromSqlRaw(query).ToDynamicList();
        }

        return result;
    }

    private void RestoreWarehouseQty(string code)
    {
        var diSMData = Db.StockMutations.Where(x => x.RefCode1 == code).ToList();
        if (diSMData.Any())
        {
            foreach (var itemData in diSMData)
            {
                var whQtyData = new Entity.Inventory.WarehouseQuantity();
                if (itemData.Type == "OH" && new[] { "DO", "DOF" }.Contains(itemData.Src))
                {
                    whQtyData = Db.WarehouseQuantities.FirstOrDefault(x => x.WarehouseCode == itemData.WarehouseCode && x.ItemId == itemData.ItemId);
                    whQtyData.QtyOnHand = whQtyData.QtyOnHand + itemData.BaseQty;
                    Db.WarehouseQuantities.Update(whQtyData);
                }
                else if (itemData.Type == "OO" && new[] { "DO", "DOF" }.Contains(itemData.Src))
                {
                    whQtyData = Db.WarehouseQuantities.FirstOrDefault(x => x.WarehouseCode == itemData.WarehouseCode && x.ItemId == itemData.ItemId);
                    whQtyData.QtyOnOrder = whQtyData.QtyOnOrder + itemData.BaseQty;
                    Db.WarehouseQuantities.Update(whQtyData);
                }
                else if (itemData.Type == "OTS" && new[] { "DO", "DOF" }.Contains(itemData.Src))
                {
                    whQtyData = Db.WarehouseQuantities.FirstOrDefault(x => x.WarehouseCode == itemData.WarehouseCode && x.ItemId == itemData.ItemId);
                    whQtyData.QtyOnTransit = whQtyData.QtyOnTransit - itemData.BaseQty;
                    Db.WarehouseQuantities.Update(whQtyData);
                }
                else if (itemData.Type == "OO" && new[] { "SO", "SOF" }.Contains(itemData.Src))
                {
                    whQtyData = Db.WarehouseQuantities.FirstOrDefault(x => x.WarehouseCode == itemData.WarehouseCode && x.ItemId == itemData.ItemId);
                    whQtyData.QtyOnOrder = whQtyData.QtyOnOrder - itemData.BaseQty;
                    Db.WarehouseQuantities.Update(whQtyData);
                }
                else if (itemData.Type == "OTS" && new[] { "SI", "SIF" }.Contains(itemData.Src))
                {
                    whQtyData = Db.WarehouseQuantities.FirstOrDefault(x => x.WarehouseCode == itemData.WarehouseCode && x.ItemId == itemData.ItemId);
                    whQtyData.QtyOnTransit = whQtyData.QtyOnTransit + itemData.BaseQty;
                    Db.WarehouseQuantities.Update(whQtyData);
                }
            }
            Db.SaveChanges();
        }
    }

    #region Credit Used - Limit
    private bool CheckCreditLimit(string custCode, decimal total) => Db.Customers.Any(c => c.Code.Equals(custCode) && (c.CreditLimit <= 0 || (c.CreditLimit - c.CreditUsed) >= total));
    private void RestoreCreditUsed(string transCode, string custCode)
    {
        var prevAmount = Db.SalesOrderHeaders.AsNoTracking().FirstOrDefault(x => x.Code.Equals(transCode))?.Total;
        string query = $"update General.Customer set CreditUsed= (CreditUsed - {prevAmount}) where code = '{custCode}'";
        Db.Database.ExecuteSqlRaw(query);
    }
    private void UpdateCreditUsed(string custCode, decimal total)
    {
        string query = $"update General.Customer set CreditUsed= (CreditUsed + {total}) where code = '{custCode}'";
        Db.Database.ExecuteSqlRaw(query);
    }
    #endregion

    #region Update & Restore Credit Memo
    private void UpdateCreditMemo(SalesInvoiceRequest data)
    {
        var listQuery = new List<string>();
        if (data.Memos.Any())
        {
            var listCodeMemo = data.Memos.Select(x => x.CreditMemoCode).ToList();
            var listCreditMemo = GetListCreditMemo(listCodeMemo);
            foreach (var item in data.Memos)
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
    private void RestoreCreditMemo(string code)
    {
        var listQuery = new List<string>();
        var usedMemo = Db.SalesInvoiceCreditMemos.Where(x => x.InvCode.Equals(code)).ToList();
        if (usedMemo.Any())
        {
            var listCodeMemo = usedMemo.Select(x => x.CreditMemoCode).ToList();
            var listCreditMemo = GetListCreditMemo(listCodeMemo);
            foreach (var item in usedMemo)
            {
                var selectedMemo = listCreditMemo.FirstOrDefault(x => x.Code.Equals(item.CreditMemoCode));
                if (selectedMemo != null)
                {
                    decimal used = selectedMemo.Used - item.CreditMemoAmount;
                    string status = used > 0 ? "PU" : "A";
                    string query = selectedMemo.Source == "cm" ? $"update Sales.CreditMemo set Mark = '{status}', Used = {used} where code = '{selectedMemo.Code}'" : $"update Accounting.BeginningBalanceCreditMemo set Used = {used} where code = '{selectedMemo.Code}'";
                    listQuery.Add(query);
                }
            }
            ExecuteQuery(listQuery);
        }
    }
    private List<dynamic> GetListCreditMemo(List<string> listCodeMemo)
    {
        var result = (from cm in Db.CreditMemos
                      where listCodeMemo.Contains(cm.Code)
                      select new { cm.Code, cm.Amount, cm.Used, Source = new[] { 1, 2 }.Contains(cm.SrcTrans) ? "cm" : "dp" })
                      .ToDynamicList();

        var bbData = (from cm in Db.BeginningBalanceCreditMemos
                      where listCodeMemo.Contains(cm.Code)
                      select new { cm.Code, cm.Amount, cm.Used, Source = "bb" }).ToDynamicList();

        result.AddRange(bbData);

        return result;
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