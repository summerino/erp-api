using ERP.Entity;
using ERP.Entity.Accounting;
using ERP.Entity.General;
using ERP.Entity.Inventory;
using ERP.Entity.SystemManagement;
using ERP.Web.API.Domain.Models;
using ERP_API.Domain.Interfaces.Accounting;
using ERP_API.Model.Accounting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ERP_API.Domain.Services.Accounting
{
    public class JournalService : IJournalService
    {
        private readonly TenantContext _db;

        public JournalService(TenantContext db)
        {
            _db = db;
        }

        public SaveResult PostingJournal(JournalRequest data)
        {
            var result = new SaveResult(false);
            var systemParam = _db.SystemParameters.ToList();
            var items = _db.Items.ToList();
            var taxes = _db.Taxes.ToList();

            using var transaction = _db.Database.BeginTransaction();
            try
            {
                var removedAP = _db.Journals.Where(x => x.Date.Month == data.Date.Month && x.Date.Year == data.Date.Year && x.SrcTrans == "RCV").ToList();
                if (removedAP != null)
                {
                    _db.RemoveRange(removedAP);
                }

                var removedAR = _db.Journals.Where(x => x.Date.Month == data.Date.Month && x.Date.Year == data.Date.Year && x.SrcTrans == "DO").ToList();
                if (removedAR != null)
                {
                    _db.RemoveRange(removedAR);
                }

                var journalAP = ProcessPurchaseJournal(data.Date, systemParam, items, taxes);
                _db.AddRange(journalAP);

                var journalAR = ProcessSaleJournal(data.Date, systemParam, items, taxes);
                _db.AddRange(journalAR);

                _db.SaveChanges();

                transaction.Commit();
            }
            catch (Exception ex)
            {
                result.Message = ex.InnerException?.Message ?? ex.Message;
                return result;
            }

            result.Success = true;
            result.Message = "Posting jurnal berhasil.";
            return result;
        }

        private IEnumerable<Journal> ProcessPurchaseJournal(DateTime dateTime, List<SystemParameter> systemParam, List<Item> items, List<Tax> taxes)
        {
            List<Journal> journals = new();

            var RcvData = (from rcvheader in _db.PurchaseReceiveHeaders
                           join supplier in _db.Suppliers on rcvheader.SupCode equals supplier.Code
                           where rcvheader.Date.Month == dateTime.Month && rcvheader.Date.Year == dateTime.Year
                           select new { RcvHeader = rcvheader, Supplier = supplier }).ToList();

            foreach (var itemData in RcvData)
            {
                var RcvDetailData = (from rcvdetail in _db.PurchaseReceiveDetails
                                     join item in _db.Items on rcvdetail.ItemId equals item.Id
                                     where rcvdetail.Code == itemData.RcvHeader.Code
                                     select new { RcvDetail = rcvdetail, Item = item }).ToList();
                short i = 0;
                short j = 0;
                foreach (var itemDetail in RcvDetailData)
                {
                    var prorateHeaderDisc = itemData.RcvHeader.FinalDisc > 0 ? (itemData.RcvHeader.FinalDisc * itemDetail.RcvDetail.NettPrice) / RcvDetailData.Sum(x => x.RcvDetail.NettPrice) : 0;
                    //Inventory
                    journals.Add(new Journal
                    {
                        Code = itemData.RcvHeader.Code,
                        LineNo = ++i,
                        Date = itemData.RcvHeader.Date,
                        CoaCode = items.FirstOrDefault(x => x.Id == itemDetail.RcvDetail.ItemId)?.CoaInventory ?? systemParam.FirstOrDefault(x => x.Code == "INVENTORY_COA")?.Value ?? "",
                        TypeCode = "RCV_DT",
                        Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_INVENTORY")?.Value ?? ""} {itemDetail.Item.Initial}").Trim(),
                        RefCode1 = itemData.RcvHeader.TransCode,
                        RefCode2 = itemDetail.Item.Initial,
                        Group = 1,
                        CurrCode = itemData.RcvHeader.CurrCode,
                        Period = itemData.RcvHeader.Date.ToString("yyyyMMdd"),
                        Type = "D",
                        Amount = itemData.RcvHeader.IncludeTax ? ((itemDetail.RcvDetail.NettPrice - prorateHeaderDisc) * itemDetail.RcvDetail.Qty) - (itemDetail.RcvDetail.TaxAmount * itemDetail.RcvDetail.Qty) : ((itemDetail.RcvDetail.NettPrice - prorateHeaderDisc) * itemDetail.RcvDetail.Qty),
                        SrcTrans = "RCV"
                    });

                    if (itemDetail.RcvDetail.TaxAmount > 0)
                    {
                        //PPN
                        if (itemData.RcvHeader.IncludeTax)
                        {
                            journals.Add(new Journal
                            {
                                Code = itemData.RcvHeader.Code,
                                LineNo = ++j,
                                Date = itemData.RcvHeader.Date,
                                CoaCode = taxes.FirstOrDefault(x => x.Id == itemDetail.RcvDetail.TaxId)?.CoaCode ?? systemParam.FirstOrDefault(x => x.Code == "TAX_IN_COA")?.Value ?? "",
                                TypeCode = "RCV_DT",
                                Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_TAX_IN")?.Value ?? ""} {itemDetail.Item.Initial}").Trim(),
                                RefCode1 = itemData.RcvHeader.TransCode,
                                RefCode2 = itemDetail.Item.Initial,
                                Group = 2,
                                CurrCode = itemData.RcvHeader.CurrCode,
                                Period = itemData.RcvHeader.Date.ToString("yyyyMMdd"),
                                Type = "C",
                                Amount = itemDetail.RcvDetail.TaxAmount * itemDetail.RcvDetail.Qty,
                                SrcTrans = "RCV"
                            });
                        }
                        else
                        {
                            journals.Add(new Journal
                            {
                                Code = itemData.RcvHeader.Code,
                                LineNo = ++j,
                                Date = itemData.RcvHeader.Date,
                                CoaCode = taxes.FirstOrDefault(x => x.Id == itemDetail.RcvDetail.TaxId)?.CoaCode ?? systemParam.FirstOrDefault(x => x.Code == "TAX_OUT_COA")?.Value ?? "",
                                TypeCode = "RCV_DT",
                                Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_TAX_OUT")?.Value ?? ""} {itemDetail.Item.Initial}").Trim(),
                                RefCode1 = itemData.RcvHeader.TransCode,
                                RefCode2 = itemDetail.Item.Initial,
                                Group = 2,
                                CurrCode = itemData.RcvHeader.CurrCode,
                                Period = itemData.RcvHeader.Date.ToString("yyyyMMdd"),
                                Type = "C",
                                Amount = itemDetail.RcvDetail.TaxAmount * itemDetail.RcvDetail.Qty,
                                SrcTrans = "RCV"
                            });
                        }
                    }
                }
                //Hutang - AP
                journals.Add(new Journal
                {
                    Code = itemData.RcvHeader.Code,
                    LineNo = 1,
                    Date = itemData.RcvHeader.Date,
                    CoaCode = systemParam.FirstOrDefault(x => x.Code == "AP_COA")?.Value ?? "",
                    TypeCode = "AP",
                    Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_AP")?.Value ?? ""} {itemData.Supplier.Initial}").Trim(),
                    RefCode1 = itemData.RcvHeader.TransCode,
                    Group = 3,
                    CurrCode = itemData.RcvHeader.CurrCode,
                    Period = itemData.RcvHeader.Date.ToString("yyyyMMdd"),
                    Type = "C",
                    Amount = itemData.RcvHeader.Total,
                    SrcTrans = "RCV"
                });
            }

            return journals;
        }

        private IEnumerable<Journal> ProcessSaleJournal(DateTime dateTime, List<SystemParameter> systemParam, List<Item> items, List<Tax> taxes)
        {
            List<Journal> journals = new();

            var DlvData = (from dlvheader in _db.SalesDeliveryHeaders
                           join customer in _db.Customers on dlvheader.CustCode equals customer.Code
                           where dlvheader.Date.Month == dateTime.Month && dlvheader.Date.Year == dateTime.Year
                           select new { Dlvheader = dlvheader, Customer = customer }).ToList();

            var stockMutations = _db.StockMutations.Where(x => new[] { "RCV", "DO" }.Contains(x.Src) && x.RefCode2 != null);

            foreach (var itemData in DlvData)
            {
                var DlvDetailData = (from dlvdetail in _db.SalesDeliveryDetails
                                     join item in _db.Items on dlvdetail.ItemId equals item.Id
                                     where dlvdetail.Code == itemData.Dlvheader.Code
                                     select new { DlvDetail = dlvdetail, Item = item }).ToList();

                var DlvDetailFreeData = (from fg in _db.SalesDeliveryDetailFreeGoods
                                         join item in _db.Items on fg.ItemId equals item.Id
                                         group new { fg,item } by new { fg.CoaCode, fg.ItemId, item.Initial } into grp
                                         select new
                                         {
                                             grp.Key.CoaCode,
                                             grp.Key.ItemId,
                                             grp.Key.Initial,
                                             UnitPrice = grp.Sum(x => x.fg.UnitPrice * x.fg.Qty)
                                         }).ToList();
                short Ndisc = 0;
                short Ninv = 0;
                short Ntax = 0;
                short Nfree = 0;
                short Nakun = 0;
                foreach (var itemDetail in DlvDetailData)
                {
                    var resultHpp = CalculateHPP(stockMutations, itemData.Dlvheader.WarehouseCode, itemDetail.DlvDetail.ItemId, itemDetail.DlvDetail.Id);
                    var prorateHeaderDisc = itemData.Dlvheader.FinalDisc > 0 ? (itemData.Dlvheader.FinalDisc * itemDetail.DlvDetail.NettPrice) / DlvDetailData.Sum(x => x.DlvDetail.NettPrice) : 0;

                    //Discount - Diskon
                    if (itemDetail.DlvDetail.Disc > 0)
                    {
                        journals.Add(new Journal
                        {
                            Code = itemData.Dlvheader.Code,
                            LineNo = ++Ndisc,
                            Date = itemData.Dlvheader.Date,
                            CoaCode = items.FirstOrDefault(x => x.Id == itemDetail.DlvDetail.ItemId)?.CoaSlsDisc ?? systemParam.FirstOrDefault(x => x.Code == "SLS_DISC_COA")?.Value ?? "",
                            TypeCode = "DLV_DT",
                            Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_SLS_DISC")?.Value ?? ""} {itemDetail.Item.Initial}").Trim(),
                            RefCode1 = itemData.Dlvheader.TransCode,
                            RefCode2 = itemDetail.Item.Initial,
                            Group = 3,
                            CurrCode = itemData.Dlvheader.CurrCode,
                            Period = itemData.Dlvheader.Date.ToString("yyyyMMdd"),
                            Type = "D",
                            Amount = (itemDetail.DlvDetail.Qty * itemDetail.DlvDetail.Disc) + prorateHeaderDisc,
                            SrcTrans = "DO"
                        });
                    } 
                    else if (prorateHeaderDisc > 0)
                    {
                        journals.Add(new Journal
                        {
                            Code = itemData.Dlvheader.Code,
                            LineNo = ++Ndisc,
                            Date = itemData.Dlvheader.Date,
                            CoaCode = items.FirstOrDefault(x => x.Id == itemDetail.DlvDetail.ItemId)?.CoaSlsDisc ?? systemParam.FirstOrDefault(x => x.Code == "SLS_DISC_COA")?.Value ?? "",
                            TypeCode = "DLV_DT",
                            Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_SLS_DISC")?.Value ?? ""} {itemDetail.Item.Initial}").Trim(),
                            RefCode1 = itemData.Dlvheader.TransCode,
                            RefCode2 = itemDetail.Item.Initial,
                            Group = 3,
                            CurrCode = itemData.Dlvheader.CurrCode,
                            Period = itemData.Dlvheader.Date.ToString("yyyyMMdd"),
                            Type = "D",
                            Amount = prorateHeaderDisc,
                            SrcTrans = "DO"
                        });
                    }

                    //Inventory using COGS for Amount
                    journals.Add(new Journal
                    {
                        Code = itemData.Dlvheader.Code,
                        LineNo = ++Ninv,
                        Date = itemData.Dlvheader.Date,
                        CoaCode = items.FirstOrDefault(x => x.Id == itemDetail.DlvDetail.ItemId)?.CoaInventory ?? systemParam.FirstOrDefault(x => x.Code == "INVENTORY_COA")?.Value ?? "",
                        TypeCode = "DLV_DT",
                        Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_INVENTORY")?.Value ?? ""} {itemDetail.Item.Initial}").Trim(),
                        RefCode1 = itemData.Dlvheader.TransCode,
                        RefCode2 = itemDetail.Item.Initial,
                        Group = 4,
                        CurrCode = itemData.Dlvheader.CurrCode,
                        Period = itemData.Dlvheader.Date.ToString("yyyyMMdd"),
                        Type = "C",
                        Amount = resultHpp,
                        SrcTrans = "DO"
                    });

                    //PPN - Pajak
                    if (itemDetail.DlvDetail.TaxAmount > 0)
                    {
                        if (itemData.Dlvheader.IncludeTax)
                        {
                            journals.Add(new Journal
                            {
                                Code = itemData.Dlvheader.Code,
                                LineNo = ++Ntax,
                                Date = itemData.Dlvheader.Date,
                                CoaCode = taxes.FirstOrDefault(x => x.Id == itemDetail.DlvDetail.TaxId)?.CoaCode ?? systemParam.FirstOrDefault(x => x.Code == "TAX_IN_COA")?.Value ?? "",
                                TypeCode = "DLV_DT",
                                Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_TAX_IN")?.Value ?? ""} {itemDetail.Item.Initial}").Trim(),
                                RefCode1 = itemData.Dlvheader.TransCode,
                                RefCode2 = itemDetail.Item.Initial,
                                Group = 5,
                                CurrCode = itemData.Dlvheader.CurrCode,
                                Period = itemData.Dlvheader.Date.ToString("yyyyMMdd"),
                                Type = "C",
                                Amount = itemDetail.DlvDetail.TaxAmount * itemDetail.DlvDetail.Qty,
                                SrcTrans = "DO"
                            });
                        }
                        else
                        {
                            journals.Add(new Journal
                            {
                                Code = itemData.Dlvheader.Code,
                                LineNo = ++Ntax,
                                Date = itemData.Dlvheader.Date,
                                CoaCode = taxes.FirstOrDefault(x => x.Id == itemDetail.DlvDetail.TaxId)?.CoaCode ?? systemParam.FirstOrDefault(x => x.Code == "TAX_OUT_COA")?.Value ?? "",
                                TypeCode = "DLV_DT",
                                Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_TAX_OUT")?.Value ?? ""} {itemDetail.Item.Initial}").Trim(),
                                RefCode1 = itemData.Dlvheader.TransCode,
                                RefCode2 = itemDetail.Item.Initial,
                                Group = 5,
                                CurrCode = itemData.Dlvheader.CurrCode,
                                Period = itemData.Dlvheader.Date.ToString("yyyyMMdd"),
                                Type = "C",
                                Amount = itemDetail.DlvDetail.TaxAmount * itemDetail.DlvDetail.Qty,
                                SrcTrans = "DO"
                            });
                        }
                    }
                }

                //Piutang - AR
                journals.Add(new Journal
                {
                    Code = itemData.Dlvheader.Code,
                    LineNo = 1,
                    Date = itemData.Dlvheader.Date,
                    CoaCode = systemParam.FirstOrDefault(x => x.Code == "AR_COA")?.Value ?? "",
                    TypeCode = "AR",
                    Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_AR")?.Value ?? ""} {itemData.Customer.Initial}").Trim(),
                    RefCode1 = itemData.Dlvheader.TransCode,
                    Group = 1,
                    CurrCode = itemData.Dlvheader.CurrCode,
                    Period = itemData.Dlvheader.Date.ToString("yyyyMMdd"),
                    Type = "D",
                    Amount = itemData.Dlvheader.Total,
                    SrcTrans = "DO"
                });

                //COGS (Cost of Goods Sold) - HPP (Harga Pokok Penjualan)
                journals.Add(new Journal
                {
                    Code = itemData.Dlvheader.Code,
                    LineNo = 1,
                    Date = itemData.Dlvheader.Date,
                    CoaCode = systemParam.FirstOrDefault(x => x.Code == "COGS_COA")?.Value ?? "",
                    TypeCode = "DLV_DT",
                    Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_COGS")?.Value ?? ""} {itemData.Customer.Initial}").Trim(),
                    RefCode1 = itemData.Dlvheader.TransCode,
                    Group = 2,
                    CurrCode = itemData.Dlvheader.CurrCode,
                    Period = itemData.Dlvheader.Date.ToString("yyyyMMdd"),
                    Type = "D",
                    Amount = journals.Where(x => x.Group == 4).Sum(x => x.Amount),
                    SrcTrans = "DO"
                });

                //Sales - Penjualan
                journals.Add(new Journal
                {
                    Code = itemData.Dlvheader.Code,
                    LineNo = 1,
                    Date = itemData.Dlvheader.Date,
                    CoaCode = systemParam.FirstOrDefault(x => x.Code == "SLS_COA")?.Value ?? "",
                    TypeCode = "DLV_DT",
                    Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_SLS")?.Value ?? ""} {itemData.Customer.Initial}").Trim(),
                    RefCode1 = itemData.Dlvheader.TransCode,
                    Group = 6,
                    CurrCode = itemData.Dlvheader.CurrCode,
                    Period = itemData.Dlvheader.Date.ToString("yyyyMMdd"),
                    Type = "C",
                    Amount = itemData.Dlvheader.IncludeTax ? itemData.Dlvheader.Total - (journals.Where(x => x.Group == 5).Sum(x => x.Amount)) : itemData.Dlvheader.Total,
                    SrcTrans = "DO"
                });


                //Promo Free Item
                if (DlvDetailFreeData.Count > 0)
                {
                    foreach (var itemFreeDetail in DlvDetailFreeData)
                    {
                        journals.Add(new Journal
                        {
                            Code = itemData.Dlvheader.Code,
                            LineNo = ++Nakun,
                            Date = itemData.Dlvheader.Date,
                            CoaCode = itemFreeDetail.CoaCode ?? "",
                            TypeCode = "DLV_FREE_DT",
                            Notes = ($"Akun Alokasi Biaya {itemFreeDetail.CoaCode}").Trim(),
                            RefCode1 = itemData.Dlvheader.TransCode,
                            Group = 7,
                            CurrCode = itemData.Dlvheader.CurrCode,
                            Period = itemData.Dlvheader.Date.ToString("yyyyMMdd"),
                            Type = "D",
                            Amount = itemFreeDetail.UnitPrice,
                            SrcTrans = "DO"
                        });

                        journals.Add(new Journal
                        {
                            Code = itemData.Dlvheader.Code,
                            LineNo = ++Nfree,
                            Date = itemData.Dlvheader.Date,
                            CoaCode = items.FirstOrDefault(x => x.Id == itemFreeDetail.ItemId)?.CoaInventory ?? systemParam.FirstOrDefault(x => x.Code == "INVENTORY_COA")?.Value ?? "",
                            TypeCode = "DLV_FREE_DT",
                            Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_INVENTORY")?.Value ?? ""} {itemFreeDetail.Initial}").Trim(),
                            RefCode1 = itemData.Dlvheader.TransCode,
                            RefCode2 = itemFreeDetail.Initial,
                            Group = 8,
                            CurrCode = itemData.Dlvheader.CurrCode,
                            Period = itemData.Dlvheader.Date.ToString("yyyyMMdd"),
                            Type = "C",
                            Amount = itemFreeDetail.UnitPrice,
                            SrcTrans = "DO"
                        });
                    }
                }
            }

            return journals;
        }

        private decimal CalculateHPP(IEnumerable<StockMutation> stockMutations, string whCode, int itemId, long dlvId)
        {
            decimal latestQty = 0;
            decimal latestStockValue = 0;
            decimal hpp = 0;

            var firstId = stockMutations.Where(x => x.WarehouseCode == whCode && x.ItemId == itemId).Min(x => x.Id);
            var firstSM = stockMutations.FirstOrDefault(x => x.Id == firstId);

            var currentSM = stockMutations.FirstOrDefault(x => x.WarehouseCode == whCode && x.ItemId == itemId && x.RefDetailId1 == dlvId);

            latestStockValue += firstSM.BaseNettPrice;
            latestQty += firstSM.BaseQty;
            hpp = latestStockValue / latestQty;

            foreach (var item in stockMutations.Where(x => x.WarehouseCode == whCode && x.ItemId == itemId && x.Id > firstId && x.Id <= currentSM.Id).OrderBy(x => x.Id))
            {
                if (item.Src == "RCV")
                {
                    latestStockValue += item.BaseNettPrice * item.BaseQty;
                    latestQty += item.BaseQty;
                }
                else if (item.Src == "DO")
                {
                    item.BaseNettPrice = hpp;
                    item.NettPrice = (hpp * item.BaseQty) / item.Qty;
                    latestStockValue -= item.BaseNettPrice * item.BaseQty;
                    latestQty -= item.BaseQty;

                    _db.StockMutations.Update(item);
                }
                hpp = latestStockValue / latestQty;
            }

            _db.SaveChanges();
            return hpp * currentSM.BaseQty;
        }
    }
}
