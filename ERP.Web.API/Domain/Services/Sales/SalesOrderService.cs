using System.Linq.Dynamic.Core;
using Microsoft.EntityFrameworkCore;
using ERP.Common;
using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Entity.Sales;
using ERP.Web.API.Domain.Interfaces.Sales;
using ERP.Web.API.Model.Sales;

namespace ERP.Web.API.Domain.Services.Sales;

public class SalesOrderService : GeneralService<SalesOrderHeader>, ISalesOrderService
{
    public SalesOrderService(TenantContext db)
        : base(db)
    {
    }

    public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
        string search)
    {
        var data = Db.VwSalesOrderHeaders.AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            data = DateTime.TryParse(search, out var searchDate)
                ? data.Where(x => x.Date == searchDate)
                : data.Where(x =>
                    x.Code.Contains(search) || x.SalesInitial.Contains(search) || x.CustCode.StartsWith(search) ||
                    x.CustName.Contains(search));
        }

        return data.ToDataSourceResult(skip, take, filter, sort);
    }

    public IEnumerable<VwSalesOrderDetail> GetDetailData(string code, bool? fullReceived)
    {
        var data = Db.VwSalesOrderDetails.Where(x => x.Code == code);

        if (fullReceived.HasValue)
        {
            data = (bool) fullReceived
                ? data.Where(x => x.Qty <= x.QtyDlv)
                : data.Where(x => x.Qty > x.QtyDlv);
        }

        return data.OrderBy(x => x.LineNo);
    }

    public List<dynamic> GetRelatedTransactions(string code)
    {
        var data = from dlv in Db.SalesDeliveryHeaders
            where dlv.TransCode == code && dlv.Mark != "V"
            select new { dlv.Code, dlv.Date, dlv.Mark };

        return data.ToDynamicList();
    }

    public IEnumerable<VwSalesOrderHeader> GetInCompleteInvoiceData(string searchBy, string search, string invCode)
    {
        var data = Db.VwSalesOrderHeaders.Where(x => new[] { "A", "PS" }.Contains(x.Mark));

        if (!string.IsNullOrEmpty(search))
        {
            data = searchBy switch
            {
                "code" => data.Where(x => x.Code.Contains(search)),
                "date" => data.Where(x => x.Date == Convert.ToDateTime(search)),
                "custName" => data.Where(x => x.CustName.Contains(search)),
                _ => data
            };
        }

        data = string.IsNullOrWhiteSpace(invCode)
            ? data.Where(x => Db.SalesDeliveryHeaders
                .Where(r => !r.FromDirectInvoice && r.SrcTrans == 1)
                .Select(r => r.TransCode).Contains(x.Code))
            : data.Where(x => Db.SalesDeliveryHeaders
                                  .Where(r => !r.FromDirectInvoice && r.SrcTrans == 1)
                                  .Select(r => r.TransCode).Contains(x.Code) ||
                              Db.SalesInvoiceHeaders
                                  .Where(i => i.Code == invCode)
                                  .Select(i => i.SoCode).Contains(x.Code));

        return searchBy switch
        {
            "code" => data.OrderBy(x => x.Code),
            "date" => data.OrderBy(x => x.Date),
            "custName" => data.OrderBy(x => x.CustName),
            _ => data
        };
    }

    public SaveResult Insert(SalesOrderRequest data)
    {
        var result = new SaveResult(false);
        var listIdDetail = new List<long>();

        using var transaction = Db.Database.BeginTransaction();
        try
        {
            // Checking order qty is excess or not
            var checkQty = Db.SystemParameters.FirstOrDefault(x => x.Code == "DEF_SLS_ORD_CHECK_QTY")?.Value == "1";
                
            if (checkQty && IsQtyExcess(data.WarehouseCode, data.ItemDetails, null))
            {
                result.Message = "Data order penjualan tidak bisa disimpan karena qty yang dipesan lebih besar dari qty yang tersedia.";
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
                }).Where(x => x.StartDate <= data.Date && data.Date <= x.EndDate).ToList();
            var items = Db.Items.ToList();
            var uomConversions = Db.UoMConversions.ToList();
            List<decimal> totalDetail = new();
            List<decimal> totalTax = new();
            List<decimal> totalExemptTax = new();
            List<decimal> totalDpp = new();

            // Get new code
            var newCode = GetNewCode("SO_NUM_FMT", data.Date);

            data.Code = newCode;

            // Insert detail data
            short i = 0;
            List<SalesOrderDetailFreeGood> bonusPromoMulti = new();
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
                                        var tierData = detailTierPromo.FirstOrDefault(x => item.Qty >= x.FromQty && item.Qty <= x.ToQty);
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
                                            else if ((applyTo == 1 && item.UnitId == tierData.SaleUnit) || applyTo == 3)
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
                                        var tierData3 = detailTierPromo.FirstOrDefault(x => item.Qty >= x.FromQty && item.Qty <= x.ToQty);
                                        if (tierData3 != null)
                                        {
                                            var freeItem = items.FirstOrDefault(x => x.Id == tierData3.FreeGoodItemId);
                                            if (tierData3.IsMultiple && ((applyTo == 1 && item.UnitId == tierData3.SaleUnit) || applyTo == 3))
                                            {
                                                var multipleValue = Math.Floor(item.Qty / tierData3.FromQty);
                                                bonusPromo.Add(new SalesOrderDetailFreeGood
                                                {
                                                    PromoCode = dataPromo.Code,
                                                    ItemId = freeItem.Id,
                                                    UomId = (int)freeItem.UomId,
                                                    UnitId = Convert.ToInt32(tierData3.UnitFreeGood),
                                                    Qty = multipleValue * tierData3.Value,
                                                    QtyClosed = 0m,
                                                    UnitPrice = (decimal)freeItem.SellPrice,
                                                    CoaCode = dataPromo.CoaCost
                                                });
                                            }
                                            else if ((applyTo == 1 && item.UnitId == tierData3.SaleUnit) || applyTo == 3)
                                            {
                                                bonusPromo.Add(new SalesOrderDetailFreeGood
                                                {
                                                    PromoCode = dataPromo.Code,
                                                    ItemId = freeItem.Id,
                                                    UomId = (int)freeItem.UomId,
                                                    UnitId = Convert.ToInt32(tierData3.UnitFreeGood),
                                                    Qty = tierData3.Value,
                                                    QtyClosed = 0m,
                                                    UnitPrice = (decimal)freeItem.SellPrice,
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
                                            var miData2 = data.ItemDetails.Where(x => multiItem.Contains(x.ItemId));
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
                                            var miData3 = data.ItemDetails.Where(x => multiItem.Contains(x.ItemId));
                                            var tierData3 = detailTierPromo.FirstOrDefault(x => miData3.Sum(x => x.Qty) >= x.FromQty && miData3.Sum(x => x.Qty) <= x.ToQty);
                                            if (tierData3 != null)
                                            {
                                                var freeItem = items.FirstOrDefault(x => x.Id == tierData3.FreeGoodItemId);
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
                                                        UnitPrice = (decimal)freeItem.SellPrice,
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
                                                        UnitPrice = (decimal)freeItem.SellPrice,
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
                    result.Message = $"Data order penjualan tidak bisa disimpan karena nilai bersih barang {ctItem.Initial} minus.";
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
                    CoaSlsReturn = item.CoaSlsReturn,
                    CoaTransit = string.IsNullOrEmpty(item.CoaTransit) ? Db.SystemParameters.FirstOrDefault(x => x.Code == "TRANSIT_ITEM_COA")?.Value ?? "" : item.CoaTransit
                };

                Db.SalesOrderDetails.Add(orderDetail);

                Db.SaveChanges();
                listIdDetail.Add(orderDetail.Id);

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
                    }
                    Db.SaveChanges();
                }

                if (bonusPromoMulti.Any() && orderDetail.LineNo == 1)
                {
                    bonusPromo.AddRange(bonusPromoMulti);
                }

                if (bonusPromo.Any())
                {
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
            Db.SalesOrderHeaders.Add(data);

            // Update Credit Used
            if(!isOverLimit)
                UpdateCreditUsed(data.CustCode, data.Total);

            if (data.IsSoDlv && !isOverLimit)
            {
                var newDlvCode = GetNewCode("DO_NUM_FMT", data.Date);

                var newSdlvData = new SalesDeliveryHeader
                {
                    Code = newDlvCode,
                    Date = data.DlvDate,
                    SrcTrans = 1,
                    TransCode = newCode,
                    CustCode = data.CustCode,
                    WarehouseCode = data.WarehouseCode,
                    ShippedBy = data.SalesBy,
                    CurrCode = data.CurrCode,
                    Rate = data.Rate,
                    ShipmentFee = data.ShipmentFee,
                    HandlingFee = data.HandlingFee,
                    SubTotal = data.SubTotal,
                    FinalDiscPercent = data.FinalDiscPercent,
                    FinalDisc = data.FinalDisc,
                    IncludeTax = data.IncludeTax,
                    TaxAmount = data.TaxAmount,
                    ExemptTaxAmount = data.ExemptTaxAmount,
                    Total = data.Total,
                    Dpp = data.Dpp,
                    Mark = data.Mark,
                    CreatedBy = data.CreatedBy,
                    CreatedDate = data.CreatedDate,
                    UpdatedBy = data.UpdatedBy,
                    UpdatedDate = data.UpdatedDate
                };

                Db.SalesDeliveryHeaders.Add(newSdlvData);

                short j = 0;
                foreach (var item in data.ItemDetails)
                {
                    Db.SalesDeliveryDetails.Add(new SalesDeliveryDetail
                    {
                        Code = newDlvCode,
                        LineNo = ++j,
                        SoDetailId = listIdDetail[j - 1],
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
                        ExemptTaxAmount = item.ExemptTaxAmount,
                        NettPrice = item.NettPrice,
                        Total = item.Total,
                        Dpp = item.Dpp
                    });
                }
            }
            var newInvCode = "";
            if (data.IsSoInv && !isOverLimit)
            {
                // Sales Delivery
                var newDlvCode = GetNewCode("DO_NUM_FMT", data.Date);

                var newSdlvData = new SalesDeliveryHeader
                {
                    Code = newDlvCode,
                    Date = data.DlvDate,
                    SrcTrans = 1,
                    TransCode = newCode,
                    CustCode = data.CustCode,
                    WarehouseCode = data.WarehouseCode,
                    ShippedBy = data.SalesBy,
                    CurrCode = data.CurrCode,
                    Rate = data.Rate,
                    ShipmentFee = data.ShipmentFee,
                    HandlingFee = data.HandlingFee,
                    SubTotal = data.SubTotal,
                    FinalDiscPercent = data.FinalDiscPercent,
                    FinalDisc = data.FinalDisc,
                    IncludeTax = data.IncludeTax,
                    TaxAmount = data.TaxAmount,
                    ExemptTaxAmount = data.ExemptTaxAmount,
                    Total = data.Total,
                    Dpp = data.Dpp,
                    Mark = "INV",
                    CreatedBy = data.CreatedBy,
                    CreatedDate = data.CreatedDate,
                    UpdatedBy = data.UpdatedBy,
                    UpdatedDate = data.UpdatedDate
                };

                Db.SalesDeliveryHeaders.Add(newSdlvData);

                // Sales Invoice
                newInvCode = GetNewCode("SI_NUM_FMT", data.Date);
                var newSinvData = new SalesInvoiceHeader
                {
                    Code = newInvCode,
                    Date = data.InvDate,
                    DueDate = data.InvDueDate,
                    SoCode = newCode,
                    CustCode = data.CustCode,
                    CurrCode = data.CurrCode,
                    Total = data.Total,
                    Notes = data.Notes,
                    Mark = data.Mark,
                    CreatedBy = data.CreatedBy,
                    CreatedDate = data.CreatedDate,
                    UpdatedBy = data.UpdatedBy,
                    UpdatedDate = data.UpdatedDate
                };

                Db.SalesInvoiceHeaders.Add(newSinvData);

                short j = 0;
                foreach (var item in data.ItemDetails)
                {
                    Db.SalesDeliveryDetails.Add(new SalesDeliveryDetail
                    {
                        Code = newDlvCode,
                        LineNo = ++j,
                        SoDetailId = listIdDetail[j - 1],
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
                        ExemptTaxAmount = item.ExemptTaxAmount,
                        NettPrice = item.NettPrice,
                        Total = item.Total,
                        Dpp = item.Dpp
                    });
                }

                Db.SalesInvoiceDetails.Add(new SalesInvoiceDetail
                {
                    Code = newInvCode,
                    LineNo = 1,
                    DoCode = newDlvCode,
                    ShipmentFee = data.ShipmentFee,
                    HandlingFee = data.HandlingFee,
                    SubTotal = data.SubTotal,
                    FinalDisc = data.FinalDisc,
                    TaxAmount = data.TaxAmount,
                    ExemptTaxAmount = data.ExemptTaxAmount,
                    Total = data.Total,
                    Dpp = data.Dpp
                });
            }

            Db.SaveChanges();

            // Execute sp_update_stock_mutation_from_so
            Db.Database.ExecuteSqlRaw(
                "EXEC sp_update_stock_mutation_from_so {0}, {1}",
                data.Code, data.Date);

            if ((data.IsSoDlv || data.IsSoInv) && !isOverLimit)
            {
                var dlvData = Db.SalesDeliveryHeaders.FirstOrDefault(x => x.TransCode == newCode);

                // Execute sp_update_stock_mutation_from_rcv
                Db.Database.ExecuteSqlRaw(
                    "EXEC sp_update_stock_mutation_from_do {0}, {1}, {2}",
                    dlvData?.Code, data.Date, newCode);

                // Execute sp_update_po_rcv_qty
                Db.Database.ExecuteSqlRaw("EXEC sp_update_so_dlv_qty {0}", newCode);

                if (data.IsSoInv)
                {
                    // Execute sp_update_stock_mutation_from_si
                    Db.Database.ExecuteSqlRaw(
                        "EXEC sp_update_stock_mutation_from_si {0}, {1}, {2}",
                        newInvCode, data.Date, dlvData?.Code);
                }
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
        result.Message = "Data order penjualan berhasil disimpan.";
        return result;
    }

    public SaveResult Update(SalesOrderRequest data)
    {
        var result = new SaveResult(false);
        var listIdDetail = new List<long>();


        using var transaction = Db.Database.BeginTransaction();
        try
        {
            // Checking mark header data
            if (Db.SalesOrderHeaders.Any(x => x.Code == data.Code && new[] { "V", "CLS" }.Contains(x.Mark)))
            {
                result.Message = "Data order penjualan tidak bisa diubah karena sudah ditandai sebagai void atau closed.";
                return result;
            }

            // Check & assign overlimit
            var isOverLimit = !CheckCreditLimit(data.CustCode, data.Total);
            if (isOverLimit)
                data.Mark = "OL";

            // Checking order qty is excess or not
            var checkQty = Db.SystemParameters.FirstOrDefault(x => x.Code == "DEF_SLS_ORD_CHECK_QTY")?.Value == "1";

            if (checkQty && IsQtyExcess(data.WarehouseCode, data.ItemDetails, data.Code))
            {
                result.Message = "Data order penjualan tidak bisa disimpan karena qty yang dipesan lebih besar dari qty tersedia.";
                return result;
            }

            var (isDuplicate, message) = CheckDuplicateDetail(data.ItemDetails);
            if (isDuplicate)
            {
                result.Message = message;
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
                }).Where(x => x.StartDate <= data.Date && data.Date <= x.EndDate).ToList();
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

            // Get detail data that exists in order before
            var delDetails = Db.SalesOrderDetails
                .Where(d => d.Code == data.Code && !data.ItemDetails.Select(x => x.Id).Contains(d.Id))
                .ToList();

            // Delete detail data that exists in order before
            Db.SalesOrderDetails.RemoveRange(delDetails);

            // Update detail data
            short i = 0;
            List<SalesOrderDetailFreeGood> bonusPromoMulti = new();
            foreach (var item in data.ItemDetails)
            {
                //Promo
                var listPromo = promos.Where(x => x.ApplyTo == 1 ||
                                                  x.Subject.Select(y => y.CustCode).Contains(data.CustCode) ||
                                                  x.Subject.Select(y => y.CustTypeId).Contains(data.CustTypeId)).ToList();

                if (data.ListPromo != null && data.ListPromo.Any())
                {
                    listPromo = listPromo.Where(x => data.ListPromo.Where(y => y.UsePromo).Select(y => y.Code).Contains(x.Code)).ToList();
                }

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
                                        var tierData = detailTierPromo.FirstOrDefault(x => item.Qty >= x.FromQty && item.Qty <= x.ToQty);
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
                                            else if ((applyTo == 1 && item.UnitId == tierData.SaleUnit) || applyTo == 3)
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
                                        var tierData3 = detailTierPromo.FirstOrDefault(x => item.Qty >= x.FromQty && item.Qty <= x.ToQty);
                                        if (tierData3 != null)
                                        {
                                            var freeItem = items.FirstOrDefault(x => x.Id == tierData3.FreeGoodItemId);
                                            if (tierData3.IsMultiple && ((applyTo == 1 && item.UnitId == tierData3.SaleUnit) || applyTo == 3))
                                            {
                                                var multipleValue = Math.Floor(item.Qty / tierData3.FromQty);
                                                bonusPromo.Add(new SalesOrderDetailFreeGood
                                                {
                                                    PromoCode = dataPromo.Code,
                                                    ItemId = freeItem.Id,
                                                    UomId = (int)freeItem.UomId,
                                                    UnitId = Convert.ToInt32(tierData3.UnitFreeGood),
                                                    Qty = multipleValue * tierData3.Value,
                                                    QtyClosed = 0m,
                                                    UnitPrice = (decimal)freeItem.SellPrice,
                                                    CoaCode = dataPromo.CoaCost
                                                });
                                            }
                                            else if ((applyTo == 1 && item.UnitId == tierData3.SaleUnit) || applyTo == 3)
                                            {
                                                bonusPromo.Add(new SalesOrderDetailFreeGood
                                                {
                                                    PromoCode = dataPromo.Code,
                                                    ItemId = freeItem.Id,
                                                    UomId = (int)freeItem.UomId,
                                                    UnitId = Convert.ToInt32(tierData3.UnitFreeGood),
                                                    Qty = tierData3.Value,
                                                    QtyClosed = 0m,
                                                    UnitPrice = (decimal)freeItem.SellPrice,
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
                                            var miData2 = data.ItemDetails.Where(x => multiItem.Contains(x.ItemId));
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
                                            var miData3 = data.ItemDetails.Where(x => multiItem.Contains(x.ItemId));
                                            var tierData3 = detailTierPromo.FirstOrDefault(x => miData3.Sum(x => x.Qty) >= x.FromQty && miData3.Sum(x => x.Qty) <= x.ToQty);
                                            if (tierData3 != null)
                                            {
                                                var freeItem = items.FirstOrDefault(x => x.Id == tierData3.FreeGoodItemId);
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
                                                        UnitPrice = (decimal)freeItem.SellPrice,
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
                                                        UnitPrice = (decimal)freeItem.SellPrice,
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

                if (item.DiscountItemDetails != null)
                {
                    item.DiscountItemDetails = item.DiscountItemDetails.Where(x => x.PromoCode == null);
                    if (item.DiscountItemDetails.Any())
                    {
                        discPromo.AddRange(item.DiscountItemDetails);
                    }
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
                    result.Message = $"Data order penjualan tidak bisa diperbarui karena nilai bersih barang {ctItem.Initial} minus.";
                    return result;
                }

                item.FinalDiscHeader = discHeaderProrate;
                item.Total = item.Qty * item.NettPrice;
                totalDetail.Add(item.Total);
                totalTax.Add(item.Qty * item.TaxAmount);
                totalExemptTax.Add(item.Qty * item.ExemptTaxAmount);
                totalDpp.Add(item.Qty * item.Dpp);

                if (item.Id <= 0)
                {
                    var orderDetail = new SalesOrderDetail
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
                        CoaSlsReturn = item.CoaSlsReturn,
                        CoaTransit = string.IsNullOrEmpty(item.CoaTransit) ? Db.SystemParameters.FirstOrDefault(x => x.Code == "TRANSIT_ITEM_COA")?.Value ?? "" : item.CoaTransit
                    };

                    Db.SalesOrderDetails.Add(orderDetail);

                    Db.SaveChanges();
                    listIdDetail.Add(orderDetail.Id);
                }
                else
                {
                    item.LineNo = ++i;

                    Db.SalesOrderDetails.Update(item);
                    Db.Entry(item).Property(e => e.Code).IsModified = false;

                    listIdDetail.Add(item.Id);
                }

                if (discPromo != null)
                {
                    //Remove deleted detail
                    var delDiscDetails = Db.SalesOrderDetailDiscounts
                        .Where(d => d.Code == data.Code && d.OrderDetailId == item.Id && !discPromo.Select(x => x.Id).Contains(d.Id))
                        .ToList();

                    Db.SalesOrderDetailDiscounts.RemoveRange(delDiscDetails);

                    if (discPromo.Any())
                    {
                        short d = 0;
                        foreach (var discItem in discPromo)
                        {
                            if (discItem.Id <= 0)
                            {
                                Db.SalesOrderDetailDiscounts.Add(new SalesOrderDetailDiscount
                                {
                                    Code = data.Code,
                                    OrderDetailId = listIdDetail[i - 1],
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
                        }
                    }
                }

                if (bonusPromoMulti.Any() && item.LineNo == 1)
                {
                    bonusPromo.AddRange(bonusPromoMulti);
                }

                if (bonusPromo != null)
                {
                    //Remove Deleted detail
                    var delFreeDetails = Db.SalesOrderDetailFreeGoods
                        .Where(d => d.Code == data.Code && d.OrderDetailId == item.Id && !bonusPromo.Select(x => x.Id).Contains(d.Id))
                        .ToList();

                    Db.SalesOrderDetailFreeGoods.RemoveRange(delFreeDetails);

                    if (bonusPromo.Any())
                    {
                        short f = 0;
                        foreach (var freeItem in bonusPromo)
                        {
                            if (freeItem.Id <= 0)
                            {
                                Db.SalesOrderDetailFreeGoods.Add(new SalesOrderDetailFreeGood
                                {
                                    Code = data.Code,
                                    OrderDetailId = listIdDetail[i - 1],
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
                        }
                    }
                }
            }

            if (data.FinalDiscPercent > 0)
                data.FinalDisc = data.ItemDetails.Sum(x => x.FinalDiscHeader * x.Qty);
            data.SubTotal = totalDetail.Sum();
            data.TaxAmount = totalTax.Sum();
            data.ExemptTaxAmount = totalExemptTax.Sum();
            data.Dpp = totalDpp.Sum();
            data.Total = data.SubTotal;
            // Update header data
            Db.SalesOrderHeaders.Update(data);
            Db.Entry(data).Property(e => e.Code).IsModified = false;
            Db.Entry(data).Property(e => e.CreatedBy).IsModified = false;
            Db.Entry(data).Property(e => e.CreatedDate).IsModified = false;

            // Update Credit Used
            if (!isOverLimit)
                UpdateCreditUsed(data.CustCode, data.Total);

            if (data.IsSoDlv && !isOverLimit)
            {
                var newDlvCode = GetNewCode("DO_NUM_FMT", data.Date);

                var newSdlvData = new SalesDeliveryHeader
                {
                    Code = newDlvCode,
                    Date = data.DlvDate,
                    SrcTrans = 1,
                    TransCode = data.Code,
                    CustCode = data.CustCode,
                    WarehouseCode = data.WarehouseCode,
                    ShippedBy = data.SalesBy,
                    CurrCode = data.CurrCode,
                    Rate = data.Rate,
                    ShipmentFee = data.ShipmentFee,
                    HandlingFee = data.HandlingFee,
                    SubTotal = data.SubTotal,
                    FinalDiscPercent = data.FinalDiscPercent,
                    FinalDisc = data.FinalDisc,
                    IncludeTax = data.IncludeTax,
                    TaxAmount = data.TaxAmount,
                    ExemptTaxAmount = data.ExemptTaxAmount,
                    Total = data.Total,
                    Dpp = data.Dpp,
                    Mark = data.Mark,
                    CreatedBy = data.CreatedBy,
                    CreatedDate = data.CreatedDate,
                    UpdatedBy = data.UpdatedBy,
                    UpdatedDate = data.UpdatedDate
                };

                Db.SalesDeliveryHeaders.Add(newSdlvData);

                short j = 0;
                foreach (var item in data.ItemDetails)
                {
                    Db.SalesDeliveryDetails.Add(new SalesDeliveryDetail
                    {
                        Code = newDlvCode,
                        LineNo = ++j,
                        SoDetailId = listIdDetail[j - 1],
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
                        ExemptTaxAmount = item.ExemptTaxAmount,
                        NettPrice = item.NettPrice,
                        Total = item.Total,
                        Dpp = item.Dpp
                    });
                }
            }
            var newInvCode = "";
            if (data.IsSoInv && !isOverLimit)
            {
                var DlvData = Db.SalesDeliveryHeaders.Where(x => x.TransCode == data.Code).ToList();
                if(DlvData.Count == 0)
                {
                    // Sales Delivery
                    var newDlvCode = GetNewCode("DO_NUM_FMT", data.Date);

                    var newSdlvData = new SalesDeliveryHeader
                    {
                        Code = newDlvCode,
                        Date = data.DlvDate,
                        SrcTrans = 1,
                        TransCode = data.Code,
                        CustCode = data.CustCode,
                        WarehouseCode = data.WarehouseCode,
                        ShippedBy = data.SalesBy,
                        CurrCode = data.CurrCode,
                        Rate = data.Rate,
                        ShipmentFee = data.ShipmentFee,
                        HandlingFee = data.HandlingFee,
                        SubTotal = data.SubTotal,
                        FinalDiscPercent = data.FinalDiscPercent,
                        FinalDisc = data.FinalDisc,
                        IncludeTax = data.IncludeTax,
                        TaxAmount = data.TaxAmount,
                        ExemptTaxAmount = data.ExemptTaxAmount,
                        Total = data.Total,
                        Dpp = data.Dpp,
                        Mark = "INV",
                        CreatedBy = data.CreatedBy,
                        CreatedDate = data.CreatedDate,
                        UpdatedBy = data.UpdatedBy,
                        UpdatedDate = data.UpdatedDate
                    };

                    Db.SalesDeliveryHeaders.Add(newSdlvData);

                    // Sales Invoice
                    newInvCode = GetNewCode("SI_NUM_FMT", data.Date);
                    var newSinvData = new SalesInvoiceHeader
                    {
                        Code = newInvCode,
                        Date = data.InvDate,
                        DueDate = data.InvDueDate,
                        SoCode = data.Code,
                        CustCode = data.CustCode,
                        CurrCode = data.CurrCode,
                        Total = data.Total,
                        Notes = data.Notes,
                        Mark = data.Mark,
                        CreatedBy = data.CreatedBy,
                        CreatedDate = data.CreatedDate,
                        UpdatedBy = data.UpdatedBy,
                        UpdatedDate = data.UpdatedDate
                    };

                    Db.SalesInvoiceHeaders.Add(newSinvData);

                    short j = 0;
                    foreach (var item in data.ItemDetails)
                    {
                        Db.SalesDeliveryDetails.Add(new SalesDeliveryDetail
                        {
                            Code = newDlvCode,
                            LineNo = ++j,
                            SoDetailId = listIdDetail[j - 1],
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
                            ExemptTaxAmount = item.ExemptTaxAmount,
                            NettPrice = item.NettPrice,
                            Total = item.Total,
                            Dpp = item.Dpp
                        });
                    }

                    Db.SalesInvoiceDetails.Add(new SalesInvoiceDetail
                    {
                        Code = newInvCode,
                        LineNo = 1,
                        DoCode = newDlvCode,
                        ShipmentFee = data.ShipmentFee,
                        HandlingFee = data.HandlingFee,
                        SubTotal = data.SubTotal,
                        FinalDisc = data.FinalDisc,
                        TaxAmount = data.TaxAmount,
                        ExemptTaxAmount = data.ExemptTaxAmount,
                        Total = data.Total,
                        Dpp = data.Dpp
                    });
                }
                else
                {
                    // Sales Invoice
                    newInvCode = GetNewCode("SI_NUM_FMT", data.Date);
                    var newSinvData = new SalesInvoiceHeader
                    {
                        Code = newInvCode,
                        Date = data.InvDate,
                        DueDate = data.InvDueDate,
                        SoCode = data.Code,
                        CustCode = data.CustCode,
                        CurrCode = data.CurrCode,
                        Total = data.Total,
                        Notes = data.Notes,
                        Mark = data.Mark,
                        CreatedBy = data.CreatedBy,
                        CreatedDate = data.CreatedDate,
                        UpdatedBy = data.UpdatedBy,
                        UpdatedDate = data.UpdatedDate
                    };

                    Db.SalesInvoiceHeaders.Add(newSinvData);

                    short j = 0;
                    foreach (var Dlvitem in DlvData)
                    {
                        Dlvitem.Date = data.DlvDate;
                        Dlvitem.Mark = "INV";
                        Dlvitem.UpdatedBy = data.UpdatedBy;
                        Dlvitem.UpdatedDate = data.UpdatedDate;

                        Db.SalesDeliveryHeaders.Update(Dlvitem);

                        Db.SalesInvoiceDetails.Add(new SalesInvoiceDetail
                        {
                            Code = newInvCode,
                            LineNo = ++j,
                            DoCode = Dlvitem.Code,
                            ShipmentFee = Dlvitem.ShipmentFee,
                            HandlingFee = Dlvitem.HandlingFee,
                            SubTotal = Dlvitem.SubTotal,
                            FinalDisc = Dlvitem.FinalDisc,
                            TaxAmount = Dlvitem.TaxAmount,
                            ExemptTaxAmount = Dlvitem.ExemptTaxAmount,
                            Total = Dlvitem.Total,
                            Dpp = Dlvitem.Dpp
                        });
                    }
                }
            }

            Db.SaveChanges();

            // Execute sp_update_stock_mutation_from_so
            Db.Database.ExecuteSqlRaw(
                "EXEC sp_update_stock_mutation_from_so {0}, {1}",
                data.Code, data.Date);

            if ((data.IsSoDlv || data.IsSoInv) && !isOverLimit)
            {
                var dlvData = Db.SalesDeliveryHeaders.FirstOrDefault(x => x.TransCode == data.Code);

                // Execute sp_update_stock_mutation_from_rcv
                Db.Database.ExecuteSqlRaw(
                    "EXEC sp_update_stock_mutation_from_do {0}, {1}, {2}",
                    dlvData?.Code, data.Date, data.Code);

                // Execute sp_update_po_rcv_qty
                Db.Database.ExecuteSqlRaw("EXEC sp_update_so_dlv_qty {0}", data.Code);

                if (data.IsSoInv)
                {
                    // Execute sp_update_stock_mutation_from_si
                    Db.Database.ExecuteSqlRaw(
                        "EXEC sp_update_stock_mutation_from_si {0}, {1}, {2}",
                        newInvCode, data.Date, dlvData?.Code);
                }
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
        result.Message = "Data order penjualan berhasil diperbarui.";
        return result;
    }

    public SaveResult Delete(string code, int userId)
    {
        var result = new SaveResult(false);

        var data = Db.SalesOrderHeaders.Find(code);
        if (data != null)
        {
            // Checking mark header data
            if (data.Mark == "V")
            {
                result.Message = "Data order penjualan tidak bisa ditandai sebagai void karena sudah ditandai sebagai void.";
                return result;
            }

            // Update header data
            data.Mark = "V";
            data.UpdatedBy = userId;
            data.UpdatedDate = DateTime.Now;                

            Db.SaveChanges();

            // Execute sp_update_stock_mutation_from_so
            Db.Database.ExecuteSqlRaw(
                "EXEC sp_update_stock_mutation_from_so {0}, {1}, {2}",
                data.Code, data.Date, true);

            // Restore Credit Used
            RestoreCreditUsed(code, data.CustCode);
        }

        result.Success = true;
        result.Message = "Data order penjualan berhasil ditandai sebagai void.";
        return result;
    }

    public SaveResult Close(string code, int userId)
    {
        var result = new SaveResult(false);
        var data = Db.SalesOrderHeaders.Find(code);
        if (data != null)
        {
            // Checking mark header data
            if (data.Mark == "CLS")
            {
                result.Message = "Data order penjualan tidak bisa ditutup karena sudah ditutup.";
                return result;
            }
            // Update header data
            data.Mark = "CLS";
            data.UpdatedBy = userId;
            data.UpdatedDate = DateTime.Now;
            Db.SaveChanges();

            RestoreCreditUsedFromCloseAction(code, data.CustCode);
        }
        result.Success = true;
        result.Message = "Data order penjualan berhasil ditutup.";
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
                if(code == null)
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

    public IEnumerable<DetailFreeGoodData> GetFreeDetailData(string code, bool? fullDlv)
    {
        var result = (from dc in Db.SalesOrderDetailFreeGoods
            join i in Db.Items on dc.ItemId equals i.Id
            join u in Db.UoMConversions on dc.UnitId equals u.Id
            where dc.Code == code
            select new DetailFreeGoodData
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
                QtyClosed = dc.QtyClosed,
                UnitPrice = dc.UnitPrice,
                CoaCode = dc.CoaCode,
                Initial = i.Initial,
                Name = i.Name,
                UnitName = u.UnitEquivalent
            }).ToList();

        if (fullDlv.HasValue)
        {
            result = (bool)fullDlv
                ? result.Where(x => x.Qty <= x.QtyClosed).ToList()
                : result.Where(x => x.Qty > x.QtyClosed).ToList();
        }
        return result;
    }

    public IEnumerable<SalesOrderDetailDiscount> GetDiscDetailData(string code)
    {
        return Db.SalesOrderDetailDiscounts.Where(x => x.Code == code).ToList();
    }

    public SaveResult CheckOverLimit(SalesOrderRequest data)
    {
        return new SaveResult(!CheckCreditLimit(data.CustCode, data.Total));
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
        var dlvSOData = Db.StockMutations.AsNoTracking().Where(x => x.RefCode1 == code).ToList();
        foreach (var itemData in dlvSOData)
        {
            var whQtyData = new Entity.Inventory.WarehouseQuantity();
            if (itemData.Type == "OO")
            {
                whQtyData = Db.WarehouseQuantities.FirstOrDefault(x => x.WarehouseCode == itemData.WarehouseCode && x.ItemId == itemData.ItemId);
                whQtyData.QtyOnOrder = whQtyData.QtyOnOrder + itemData.BaseQty;
                Db.WarehouseQuantities.Update(whQtyData);
            }
        }
        Db.SaveChanges();
    }

    #region Credit Used - Limit
    private bool CheckCreditLimit(string custCode, decimal total) => Db.Customers.Any(c => c.Code.Equals(custCode) && (c.CreditLimit <= 0 || (c.CreditLimit - c.CreditUsed) >= total));
    private void RestoreCreditUsed(string transCode, string custCode)
    {
        var prevAmount = Db.SalesOrderHeaders.AsNoTracking().FirstOrDefault(x => x.Code.Equals(transCode))?.Total;
        string query = $"update General.Customer set CreditUsed= (CreditUsed - {prevAmount}) where code = '{custCode}'";
        Db.Database.ExecuteSqlRaw(query);
    }
    private void RestoreCreditUsedFromCloseAction(string transCode, string custCode)
    {
        var amount = Db.SalesOrderDetails.Where(x => x.Code == transCode).Sum(x => (x.Qty - x.QtyDlv) * x.NettPrice);
        string query = $"update General.Customer set CreditUsed= (CreditUsed - {amount}) where code = '{custCode}'";
        Db.Database.ExecuteSqlRaw(query);
    }
    private void UpdateCreditUsed(string custCode, decimal total)
    {
        string query = $"update General.Customer set CreditUsed= (CreditUsed + {total}) where code = '{custCode}'";
        Db.Database.ExecuteSqlRaw(query);
    }
    #endregion
}