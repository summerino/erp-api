using System;
using System.Collections.Generic;
using System.Linq;
using ERP.Common;
using ERP.Entity;
using ERP.Entity.Accounting;
using ERP.Entity.AssetManagement;
using ERP.Entity.General;
using ERP.Entity.Inventory;
using ERP.Entity.SystemManagement;
using ERP.Web.API.Domain.Interfaces.Accounting;
using ERP.Web.API.Model.Accounting;

namespace ERP.Web.API.Domain.Services.Accounting
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
                var removed = _db.Journals.Where(x => x.Date.Month == data.Date.Month && x.Date.Year == data.Date.Year).ToList();
                if (removed != null)
                    _db.RemoveRange(removed);

                var journalRCV = ProcessPurchaseJournal(data.Date, systemParam, items, taxes);
                if (journalRCV != null)
                    _db.AddRange(journalRCV);

                var journalPR = ProcessPurchaseReturnJournal(data.Date, systemParam, items);
                if (journalPR != null)
                    _db.AddRange(journalPR);

                var journalDO = ProcessSaleJournal(data.Date, systemParam, items, taxes);
                if (journalDO != null)
                    _db.AddRange(journalDO);

                var journalSR = ProcessSalesReturnJournal(data.Date, systemParam, items, taxes);
                if (journalSR != null)
                    _db.AddRange(journalSR);

                var journalCB = ProcessCashBankJournal(data.Date, systemParam);
                if (journalCB != null)
                    _db.AddRange(journalCB);

                var journalBBAP = ProcessBBAPJournal(systemParam);
                if (journalBBAP != null)
                    _db.AddRange(journalBBAP);

                var journalBBAR = ProcessBBARJournal(systemParam);
                if (journalBBAR != null)
                    _db.AddRange(journalBBAR);

                var journalBBDM = ProcessBBDebitMemoJournal(systemParam);
                if (journalBBDM != null)
                    _db.AddRange(journalBBDM);

                var journalBBCM = ProcessBBCreditMemoJournal(systemParam);
                if (journalBBCM != null)
                    _db.AddRange(journalBBCM);

                var journalEXP = ProcessExpeditionJournal(data.Date, systemParam);
                if (journalEXP != null)
                    _db.AddRange(journalEXP);

                var journalFA = ProcessFixedAssetJournal(data.Date, systemParam);
                if (journalFA != null)
                    _db.AddRange(journalFA);

                var journalDFA = ProcessDepreciationFixedAssetJournal(data.Date, systemParam);
                if (journalDFA != null)
                    _db.AddRange(journalDFA);

                var journalEY = ProcessEndYearAssetJournal(data.Date, systemParam, journalDFA);
                if (journalEY != null)
                    _db.AddRange(journalEY);

                _db.SaveChanges();

                transaction.Commit();
            }
            catch (Exception ex)
            {
                result.Message = ex.InnerException?.Message ?? ex.Message;
                return result;
            }

            result.Success = true;
            result.Message = "Posting jurnal selesai.";
            return result;
        }

        private IEnumerable<Journal> ProcessPurchaseJournal(DateTime dateTime, List<SystemParameter> systemParam, List<Item> items, List<Tax> taxes)
        {
            List<Journal> journals = new();

            var RcvData = (from rcvheader in _db.PurchaseReceiveHeaders
                           join supplier in _db.Suppliers on rcvheader.SupCode equals supplier.Code
                           where rcvheader.Date.Month == dateTime.Month && rcvheader.Date.Year == dateTime.Year && rcvheader.SrcTrans == 1
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
                           where dlvheader.Date.Month == dateTime.Month && dlvheader.Date.Year == dateTime.Year && dlvheader.SrcTrans == 1
                           select new { Dlvheader = dlvheader, Customer = customer }).ToList();

            var stockMutations = _db.StockMutations.Where(x => new[] { "RCV", "DO", "SR" }.Contains(x.Src));

            foreach (var itemData in DlvData)
            {
                var DlvDetailData = (from dlvdetail in _db.SalesDeliveryDetails
                                     join item in _db.Items on dlvdetail.ItemId equals item.Id
                                     where dlvdetail.Code == itemData.Dlvheader.Code
                                     select new { DlvDetail = dlvdetail, Item = item }).ToList();

                var DlvDetailFreeData = (from fg in _db.SalesDeliveryDetailFreeGoods
                                         join item in _db.Items on fg.ItemId equals item.Id
                                         group new { fg, item } by new { fg.CoaCode, fg.ItemId, item.Initial } into grp
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
                short Nhpp = 0;
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
                            TypeCode = "SLS_DISC",
                            Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_SLS_DISC")?.Value ?? ""} {itemDetail.Item.Initial}").Trim(),
                            RefCode1 = itemData.Dlvheader.TransCode,
                            RefCode2 = itemDetail.Item.Initial,
                            Group = 3,
                            CurrCode = itemData.Dlvheader.CurrCode,
                            Period = itemData.Dlvheader.Date.ToString("yyyyMMdd"),
                            Type = "D",
                            Amount = (itemDetail.DlvDetail.Qty * itemDetail.DlvDetail.Disc) + prorateHeaderDisc,
                            SrcTrans = "DLV"
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
                            TypeCode = "SLS_DISC",
                            Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_SLS_DISC")?.Value ?? ""} {itemDetail.Item.Initial}").Trim(),
                            RefCode1 = itemData.Dlvheader.TransCode,
                            RefCode2 = itemDetail.Item.Initial,
                            Group = 3,
                            CurrCode = itemData.Dlvheader.CurrCode,
                            Period = itemData.Dlvheader.Date.ToString("yyyyMMdd"),
                            Type = "D",
                            Amount = prorateHeaderDisc,
                            SrcTrans = "DLV"
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
                        SrcTrans = "DLV"
                    });

                    //COGS (Cost of Goods Sold) - HPP (Harga Pokok Penjualan)
                    journals.Add(new Journal
                    {
                        Code = itemData.Dlvheader.Code,
                        LineNo = ++Nhpp,
                        Date = itemData.Dlvheader.Date,
                        CoaCode = items.FirstOrDefault(x => x.Id == itemDetail.DlvDetail.ItemId)?.CoaCogs ?? systemParam.FirstOrDefault(x => x.Code == "COGS_COA")?.Value ?? "",
                        TypeCode = "DLV_DT",
                        Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_COGS")?.Value ?? ""} {itemDetail.Item.Initial}").Trim(),
                        RefCode1 = itemData.Dlvheader.TransCode,
                        RefCode2 = itemDetail.Item.Initial,
                        Group = 2,
                        CurrCode = itemData.Dlvheader.CurrCode,
                        Period = itemData.Dlvheader.Date.ToString("yyyyMMdd"),
                        Type = "D",
                        Amount = resultHpp,
                        SrcTrans = "DLV"
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
                                SrcTrans = "DLV"
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
                                SrcTrans = "DLV"
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
                    SrcTrans = "DLV"
                });

                //Sales - Penjualan
                journals.Add(new Journal
                {
                    Code = itemData.Dlvheader.Code,
                    LineNo = 1,
                    Date = itemData.Dlvheader.Date,
                    CoaCode = systemParam.FirstOrDefault(x => x.Code == "SLS_COA")?.Value ?? "",
                    TypeCode = "SLS",
                    Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_SLS")?.Value ?? ""} {itemData.Customer.Initial}").Trim(),
                    RefCode1 = itemData.Dlvheader.TransCode,
                    Group = 6,
                    CurrCode = itemData.Dlvheader.CurrCode,
                    Period = itemData.Dlvheader.Date.ToString("yyyyMMdd"),
                    Type = "C",
                    Amount = itemData.Dlvheader.IncludeTax ? itemData.Dlvheader.Total - (journals.Where(x => x.Code == itemData.Dlvheader.Code && x.Group == 5).Sum(x => x.Amount)) : itemData.Dlvheader.Total,
                    SrcTrans = "DLV"
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
                            TypeCode = "COST",
                            Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_PROMO_COST")?.Value ?? ""} {itemFreeDetail.Initial}").Trim(),
                            RefCode1 = itemData.Dlvheader.TransCode,
                            Group = 7,
                            CurrCode = itemData.Dlvheader.CurrCode,
                            Period = itemData.Dlvheader.Date.ToString("yyyyMMdd"),
                            Type = "D",
                            Amount = itemFreeDetail.UnitPrice,
                            SrcTrans = "DLV"
                        });

                        journals.Add(new Journal
                        {
                            Code = itemData.Dlvheader.Code,
                            LineNo = ++Nfree,
                            Date = itemData.Dlvheader.Date,
                            CoaCode = items.FirstOrDefault(x => x.Id == itemFreeDetail.ItemId)?.CoaInventory ?? systemParam.FirstOrDefault(x => x.Code == "INVENTORY_COA")?.Value ?? "",
                            TypeCode = "DLV_DT",
                            Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_INVENTORY")?.Value ?? ""} {itemFreeDetail.Initial}").Trim(),
                            RefCode1 = itemData.Dlvheader.TransCode,
                            RefCode2 = itemFreeDetail.Initial,
                            Group = 8,
                            CurrCode = itemData.Dlvheader.CurrCode,
                            Period = itemData.Dlvheader.Date.ToString("yyyyMMdd"),
                            Type = "C",
                            Amount = itemFreeDetail.UnitPrice,
                            SrcTrans = "DLV"
                        });
                    }
                }
            }

            return journals;
        }

        private IEnumerable<Journal> ProcessCashBankJournal(DateTime dateTime, List<SystemParameter> systemParam)
        {
            List<Journal> journals = new();
            var cashBankData = _db.GeneralCashBankHeaders.Where(x => x.Date.Month == dateTime.Month && x.Date.Year == dateTime.Year).ToList();
            foreach (var itemData in cashBankData)
            {
                var cashBankDetailData = _db.GeneralCashBankDetails.Where(x => x.Code == itemData.Code).ToList();
                short i = 0;
                short j = 0;
                foreach (var itemDetailData in cashBankDetailData)
                {
                    //Detail
                    journals.Add(new Journal
                    {
                        Code = itemData.Code,
                        LineNo = ++i,
                        Date = itemData.Date,
                        CoaCode = itemDetailData.CoaCode ?? systemParam.FirstOrDefault(x => x.Code == $"{itemDetailData.Type}_COA")?.Value ?? "",
                        TypeCode = $"CB_{itemDetailData.Type}",
                        Notes = ($"{systemParam.FirstOrDefault(x => x.Code == $"JR_PREFIX_{itemDetailData.Type}")?.Value ?? ""} {itemDetailData.Notes}").Trim(),
                        RefCode1 = itemDetailData.TransCode,
                        Group = 1,
                        CurrCode = itemDetailData.CurrCode,
                        Period = itemData.Date.ToString("yyyyMMdd"),
                        Type = itemDetailData.TypeAmount,
                        Amount = itemDetailData.Amount,
                        SrcTrans = "CB"
                    });
                    //Kas&Bank- Header
                    journals.Add(new Journal
                    {
                        Code = itemData.Code,
                        LineNo = ++j,
                        Date = itemData.Date,
                        CoaCode = itemData.CoaCode ?? "",
                        TypeCode = "CB",
                        Notes = "Kas/Bank",
                        RefCode2 = itemData.Code,
                        Group = 2,
                        CurrCode = itemData.CurrCode,
                        Period = itemData.Date.ToString("yyyyMMdd"),
                        Type = itemDetailData.TypeAmount == "C" ? "D" : "C",
                        Amount = itemDetailData.Amount,
                        SrcTrans = "CB"
                    });
                }
            }

            return journals;
        }

        private IEnumerable<Journal> ProcessBBAPJournal(List<SystemParameter> systemParam)
        {
            List<Journal> journals = new();
            var startDate = Convert.ToDateTime(systemParam.FirstOrDefault(x => x.Code == "DATA_START_DATE").Value).AddDays(-1);
            var latestData = _db.VwBeginningBalanceAPs.OrderByDescending(x => x.Date).FirstOrDefault();
            if (latestData?.Date.Month == startDate.Month && latestData?.Date.Year == startDate.Year)
            {
                var bbapData = _db.VwBeginningBalanceAPs.ToList();
                short i = 0;
                foreach (var item in bbapData)
                {
                    journals.Add(new Journal
                    {
                        Code = "BB-AP-" + startDate.ToString("yyyyMMdd"),
                        LineNo = ++i,
                        Date = item.Date,
                        CoaCode = systemParam.FirstOrDefault(x => x.Code == "AP_COA")?.Value ?? "",
                        TypeCode = "BB_AP",
                        Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_AP")?.Value ?? ""} {item.SupName}").Trim(),
                        RefCode1 = item.Code,
                        Group = 2,
                        CurrCode = "IDR",
                        Period = item.Date.ToString("yyyyMMdd"),
                        Type = "C",
                        Amount = item.Amount,
                        SrcTrans = "BB_AP"
                    });
                }

                journals.Add(new Journal
                {
                    Code = "BB-AP-" + startDate.ToString("yyyyMMdd"),
                    LineNo = 1,
                    Date = startDate,
                    CoaCode = systemParam.FirstOrDefault(x => x.Code == "BB_COA")?.Value ?? "",
                    TypeCode = "BB_AP",
                    Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_BB_AP")?.Value ?? ""}").Trim(),
                    RefCode1 = "BB-AP-" + startDate.ToString("yyMMdd"),
                    Group = 1,
                    CurrCode = "IDR",
                    Period = startDate.ToString("yyyyMMdd"),
                    Type = "D",
                    Amount = journals.Where(x => x.Code == "BB-AP-" + startDate.ToString("yyyyMMdd")).Sum(x => x.Amount),
                    SrcTrans = "BB_AP"
                });
            }
            return journals;
        }

        private IEnumerable<Journal> ProcessBBARJournal(List<SystemParameter> systemParam)
        {
            List<Journal> journals = new();
            var startDate = Convert.ToDateTime(systemParam.FirstOrDefault(x => x.Code == "DATA_START_DATE").Value).AddDays(-1);
            var latestData = _db.VwBeginningBalanceARs.OrderByDescending(x => x.Date).FirstOrDefault();
            if (latestData?.Date.Month == startDate.Month && latestData?.Date.Year == startDate.Year)
            {
                var bbapData = _db.VwBeginningBalanceARs.ToList();
                short i = 0;
                foreach (var item in bbapData)
                {
                    journals.Add(new Journal
                    {
                        Code = "BB-AR-" + startDate.ToString("yyyyMMdd"),
                        LineNo = ++i,
                        Date = item.Date,
                        CoaCode = systemParam.FirstOrDefault(x => x.Code == "AR_COA")?.Value ?? "",
                        TypeCode = "BB_AR",
                        Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_AR")?.Value ?? ""} {item.CustName}").Trim(),
                        RefCode1 = item.Code,
                        Group = 2,
                        CurrCode = "IDR",
                        Period = item.Date.ToString("yyyyMMdd"),
                        Type = "D",
                        Amount = item.Amount,
                        SrcTrans = "BB_AR"
                    });
                }

                journals.Add(new Journal
                {
                    Code = "BB-AR-" + startDate.ToString("yyyyMMdd"),
                    LineNo = 1,
                    Date = startDate,
                    CoaCode = systemParam.FirstOrDefault(x => x.Code == "BB_COA")?.Value ?? "",
                    TypeCode = "BB_AR",
                    Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_BB_AR")?.Value ?? ""}").Trim(),
                    RefCode1 = "BB-AR-" + startDate.ToString("yyMMdd"),
                    Group = 1,
                    CurrCode = "IDR",
                    Period = startDate.ToString("yyyyMMdd"),
                    Type = "C",
                    Amount = journals.Where(x => x.Code == "BB-AR-" + startDate.ToString("yyyyMMdd")).Sum(x => x.Amount),
                    SrcTrans = "BB_AR"
                });
            }
            return journals;
        }

        private IEnumerable<Journal> ProcessBBDebitMemoJournal(List<SystemParameter> systemParam)
        {
            List<Journal> journals = new();
            var startDate = Convert.ToDateTime(systemParam.FirstOrDefault(x => x.Code == "DATA_START_DATE").Value).AddDays(-1);
            var latestData = _db.VwBeginningBalanceDebitMemos.OrderByDescending(x => x.Date).FirstOrDefault();
            if (latestData?.Date.Month == startDate.Month && latestData?.Date.Year == startDate.Year)
            {
                var bbapData = _db.VwBeginningBalanceDebitMemos.ToList();
                short i = 0;
                foreach (var item in bbapData)
                {
                    journals.Add(new Journal
                    {
                        Code = $"BB-DM-{(item.Type == 1 ? "DPS" : "PR")}-" + startDate.ToString("yyyyMMdd"),
                        LineNo = ++i,
                        Date = item.Date,
                        CoaCode = systemParam.FirstOrDefault(x => x.Code == $"{(item.Type == 1 ? "DPS" : "PR")}_COA")?.Value ?? "",
                        TypeCode = $"BB_{(item.Type == 1 ? "DPS" : "PR")}",
                        Notes = ($"{systemParam.FirstOrDefault(x => x.Code == $"JR_PREFIX_BB_{(item.Type == 1 ? "DPS" : "PR")}")?.Value ?? ""} {item.SupName}").Trim(),
                        RefCode1 = item.Code,
                        Group = 2,
                        CurrCode = "IDR",
                        Period = item.Date.ToString("yyyyMMdd"),
                        Type = "D",
                        Amount = item.Amount,
                        SrcTrans = "BB_DM"
                    });
                }

                journals.Add(new Journal
                {
                    Code = "BB-DM-PR-" + startDate.ToString("yyyyMMdd"),
                    LineNo = 2,
                    Date = startDate,
                    CoaCode = systemParam.FirstOrDefault(x => x.Code == "BB_COA")?.Value ?? "",
                    TypeCode = "BB_PR",
                    Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_BB_AP")?.Value ?? ""}").Trim(),
                    RefCode1 = "",
                    Group = 1,
                    CurrCode = "IDR",
                    Period = startDate.ToString("yyyyMMdd"),
                    Type = "C",
                    Amount = journals.Where(x => x.Code == "BB-DM-PR-" + startDate.ToString("yyyyMMdd") && x.TypeCode == "BB_PR").Sum(x => x.Amount),
                    SrcTrans = "BB_DM"
                });

                journals.Add(new Journal
                {
                    Code = "BB-DM-DPS-" + startDate.ToString("yyyyMMdd"),
                    LineNo = 1,
                    Date = startDate,
                    CoaCode = systemParam.FirstOrDefault(x => x.Code == "BB_COA")?.Value ?? "",
                    TypeCode = "BB_DPS",
                    Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_BB_AP")?.Value ?? ""}").Trim(),
                    RefCode1 = "",
                    Group = 1,
                    CurrCode = "IDR",
                    Period = startDate.ToString("yyyyMMdd"),
                    Type = "C",
                    Amount = journals.Where(x => x.Code == "BB-DM-DPS-" + startDate.ToString("yyyyMMdd") && x.TypeCode == "BB_DPS").Sum(x => x.Amount),
                    SrcTrans = "BB_DM"
                });
            }
            return journals;
        }

        private IEnumerable<Journal> ProcessBBCreditMemoJournal(List<SystemParameter> systemParam)
        {
            List<Journal> journals = new();
            var startDate = Convert.ToDateTime(systemParam.FirstOrDefault(x => x.Code == "DATA_START_DATE").Value).AddDays(-1);
            var latestData = _db.VwBeginningBalanceCreditMemos.OrderByDescending(x => x.Date).FirstOrDefault();
            if (latestData?.Date.Month == startDate.Month && latestData?.Date.Year == startDate.Year)
            {
                var bbapData = _db.VwBeginningBalanceCreditMemos.ToList();
                short i = 0;
                foreach (var item in bbapData)
                {
                    journals.Add(new Journal
                    {
                        Code = $"BB-CM-{(item.Type == 1 ? "DPC" : "SR")}-" + startDate.ToString("yyyyMMdd"),
                        LineNo = ++i,
                        Date = item.Date,
                        CoaCode = systemParam.FirstOrDefault(x => x.Code == $"{(item.Type == 1 ? "DPC" : "SR")}_COA")?.Value ?? "",
                        TypeCode = $"BB_{(item.Type == 1 ? "DPC" : "SR")}",
                        Notes = ($"{systemParam.FirstOrDefault(x => x.Code == $"JR_PREFIX_BB_{(item.Type == 1 ? "DPC" : "SR")}")?.Value ?? ""} {item.CustName}").Trim(),
                        RefCode1 = item.Code,
                        Group = 2,
                        CurrCode = "IDR",
                        Period = item.Date.ToString("yyyyMMdd"),
                        Type = "C",
                        Amount = item.Amount,
                        SrcTrans = "BB_CM"
                    });
                }

                journals.Add(new Journal
                {
                    Code = "BB-CM-SR-" + startDate.ToString("yyyyMMdd"),
                    LineNo = 2,
                    Date = startDate,
                    CoaCode = systemParam.FirstOrDefault(x => x.Code == "BB_COA")?.Value ?? "",
                    TypeCode = "BB_SR",
                    Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_BB_AR")?.Value ?? ""}").Trim(),
                    RefCode1 = "",
                    Group = 1,
                    CurrCode = "IDR",
                    Period = startDate.ToString("yyyyMMdd"),
                    Type = "D",
                    Amount = journals.Where(x => x.Code == "BB-CM-SR-" + startDate.ToString("yyyyMMdd") && x.TypeCode == "BB_SR").Sum(x => x.Amount),
                    SrcTrans = "BB_CM"
                });

                journals.Add(new Journal
                {
                    Code = "BB-CM-DPC-" + startDate.ToString("yyyyMMdd"),
                    LineNo = 1,
                    Date = startDate,
                    CoaCode = systemParam.FirstOrDefault(x => x.Code == "BB_COA")?.Value ?? "",
                    TypeCode = "BB_DPC",
                    Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_BB_AR")?.Value ?? ""}").Trim(),
                    RefCode1 = "",
                    Group = 1,
                    CurrCode = "IDR",
                    Period = startDate.ToString("yyyyMMdd"),
                    Type = "D",
                    Amount = journals.Where(x => x.Code == "BB-CM-DPC-" + startDate.ToString("yyyyMMdd") && x.TypeCode == "BB_DPC").Sum(x => x.Amount),
                    SrcTrans = "BB_CM"
                });
            }
            return journals;
        }

        private IEnumerable<Journal> ProcessPurchaseReturnJournal(DateTime dateTime, List<SystemParameter> systemParam, List<Item> items)
        {
            List<Journal> journals = new();
            var RtnData = (from rtnheader in _db.PurchaseReturnHeaders
                           join supplier in _db.Suppliers on rtnheader.SupCode equals supplier.Code
                           where rtnheader.Date.Month == dateTime.Month && rtnheader.Date.Year == dateTime.Year
                           select new { RtnHeader = rtnheader, Supplier = supplier }).ToList();

            foreach (var itemData in RtnData)
            {
                if (itemData.RtnHeader.Type != 1)
                {
                    var RtnDetailData = (from rtndetail in _db.PurchaseReturnDetails
                                         join item in _db.Items on rtndetail.ItemId equals item.Id
                                         where rtndetail.Code == itemData.RtnHeader.Code
                                         select new { RtnDetail = rtndetail, Item = item }).ToList();
                    var RtnDetailExData = (from rtndetail in _db.PurchaseReturnDetailExchDiffItems
                                           join item in _db.Items on rtndetail.ItemId equals item.Id
                                           where rtndetail.Code == itemData.RtnHeader.Code
                                           select new { RtnDetail = rtndetail, Item = item }).ToList();

                    if (itemData.RtnHeader.Type == 2)
                    {
                        //Return Detail
                        short i = 0;
                        foreach (var itemDetail in RtnDetailData)
                        {
                            //Inventory
                            journals.Add(new Journal
                            {
                                Code = itemData.RtnHeader.Code,
                                LineNo = ++i,
                                Date = itemData.RtnHeader.Date,
                                CoaCode = items.FirstOrDefault(x => x.Id == itemDetail.RtnDetail.ItemId)?.CoaInventory ?? systemParam.FirstOrDefault(x => x.Code == "INVENTORY_COA")?.Value ?? "",
                                TypeCode = "PR_DT",
                                Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_INVENTORY")?.Value ?? ""} {itemDetail.Item.Initial}").Trim(),
                                RefCode1 = itemDetail.Item.Initial,
                                Group = 1,
                                CurrCode = itemData.RtnHeader.CurrCode,
                                Period = itemData.RtnHeader.Date.ToString("yyyyMMdd"),
                                Type = "C",
                                Amount = itemDetail.RtnDetail.Total,
                                SrcTrans = "PR"
                            });
                        }
                        //Hutang - AP
                        journals.Add(new Journal
                        {
                            Code = itemData.RtnHeader.Code,
                            LineNo = 1,
                            Date = itemData.RtnHeader.Date,
                            CoaCode = systemParam.FirstOrDefault(x => x.Code == "AP_COA")?.Value ?? "",
                            TypeCode = "AP",
                            Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_AP")?.Value ?? ""} {itemData.Supplier.Initial}").Trim(),
                            RefCode1 = "",
                            Group = 2,
                            CurrCode = itemData.RtnHeader.CurrCode,
                            Period = itemData.RtnHeader.Date.ToString("yyyyMMdd"),
                            Type = "D",
                            Amount = journals.Where(x => x.Code == itemData.RtnHeader.Code).Sum(s => s.Amount),
                            SrcTrans = "PR"
                        });
                    }
                    else
                    {
                        //Return Detail Exchange
                        short i = 0;
                        foreach (var itemDetail in RtnDetailExData)
                        {
                            //Inventory
                            journals.Add(new Journal
                            {
                                Code = itemData.RtnHeader.Code,
                                LineNo = ++i,
                                Date = itemData.RtnHeader.Date,
                                CoaCode = items.FirstOrDefault(x => x.Id == itemDetail.RtnDetail.ItemId)?.CoaInventory ?? systemParam.FirstOrDefault(x => x.Code == "INVENTORY_COA")?.Value ?? "",
                                TypeCode = "PR_DT",
                                Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_INVENTORY")?.Value ?? ""} {itemDetail.Item.Initial}").Trim(),
                                RefCode1 = itemDetail.Item.Initial,
                                Group = 1,
                                CurrCode = itemData.RtnHeader.CurrCode,
                                Period = itemData.RtnHeader.Date.ToString("yyyyMMdd"),
                                Type = "C",
                                Amount = itemDetail.RtnDetail.Total,
                                SrcTrans = "PR"
                            });
                        }
                        //Hutang - AP
                        journals.Add(new Journal
                        {
                            Code = itemData.RtnHeader.Code,
                            LineNo = 1,
                            Date = itemData.RtnHeader.Date,
                            CoaCode = systemParam.FirstOrDefault(x => x.Code == "AP_COA")?.Value ?? "",
                            TypeCode = "AP",
                            Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_AP")?.Value ?? ""} {itemData.Supplier.Initial}").Trim(),
                            RefCode1 = "",
                            Group = 2,
                            CurrCode = itemData.RtnHeader.CurrCode,
                            Period = itemData.RtnHeader.Date.ToString("yyyyMMdd"),
                            Type = "D",
                            Amount = journals.Where(x => x.Code == itemData.RtnHeader.Code).Sum(s => s.Amount),
                            SrcTrans = "PR"
                        });
                    }
                    //Receive Process
                    var RcvData = (from rcvheader in _db.PurchaseReceiveHeaders
                                   join supplier in _db.Suppliers on rcvheader.SupCode equals supplier.Code
                                   where rcvheader.TransCode == itemData.RtnHeader.Code
                                   select new { RcvHeader = rcvheader, Supplier = supplier }).ToList();

                    foreach (var itemRcvData in RcvData)
                    {
                        var RcvDetailData = (from rcvdetail in _db.PurchaseReceiveDetails
                                             join item in _db.Items on rcvdetail.ItemId equals item.Id
                                             where rcvdetail.Code == itemRcvData.RcvHeader.Code
                                             select new { RcvDetail = rcvdetail, Item = item }).ToList();
                        short i = 0;
                        foreach (var itemDetail in RcvDetailData)
                        {
                            //Inventory
                            journals.Add(new Journal
                            {
                                Code = itemRcvData.RcvHeader.Code,
                                LineNo = ++i,
                                Date = itemRcvData.RcvHeader.Date,
                                CoaCode = items.FirstOrDefault(x => x.Id == itemDetail.RcvDetail.ItemId)?.CoaInventory ?? systemParam.FirstOrDefault(x => x.Code == "INVENTORY_COA")?.Value ?? "",
                                TypeCode = "RCV_DT",
                                Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_INVENTORY")?.Value ?? ""} {itemDetail.Item.Initial}").Trim(),
                                RefCode1 = itemRcvData.RcvHeader.TransCode,
                                RefCode2 = itemDetail.Item.Initial,
                                Group = 1,
                                CurrCode = itemRcvData.RcvHeader.CurrCode,
                                Period = itemRcvData.RcvHeader.Date.ToString("yyyyMMdd"),
                                Type = "D",
                                Amount = itemDetail.RcvDetail.Total,
                                SrcTrans = "RCV"
                            });
                        }
                        //Hutang - AP
                        journals.Add(new Journal
                        {
                            Code = itemRcvData.RcvHeader.Code,
                            LineNo = 1,
                            Date = itemRcvData.RcvHeader.Date,
                            CoaCode = systemParam.FirstOrDefault(x => x.Code == "AP_COA")?.Value ?? "",
                            TypeCode = "RCV",
                            Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_AP")?.Value ?? ""} {itemData.Supplier.Initial}").Trim(),
                            RefCode1 = itemRcvData.RcvHeader.TransCode,
                            Group = 2,
                            CurrCode = itemRcvData.RcvHeader.CurrCode,
                            Period = itemRcvData.RcvHeader.Date.ToString("yyyyMMdd"),
                            Type = "C",
                            Amount = itemRcvData.RcvHeader.Total,
                            SrcTrans = "RCV"
                        });
                    }
                }
                else
                {
                    var RtnDetailData = (from rtndetail in _db.PurchaseReturnDetails
                                         join item in _db.Items on rtndetail.ItemId equals item.Id
                                         where rtndetail.Code == itemData.RtnHeader.Code
                                         select new { RtnDetail = rtndetail, Item = item }).ToList();
                    short i = 0;
                    foreach (var itemDetail in RtnDetailData)
                    {
                        //Inventory
                        journals.Add(new Journal
                        {
                            Code = itemData.RtnHeader.Code,
                            LineNo = ++i,
                            Date = itemData.RtnHeader.Date,
                            CoaCode = items.FirstOrDefault(x => x.Id == itemDetail.RtnDetail.ItemId)?.CoaInventory ?? systemParam.FirstOrDefault(x => x.Code == "INVENTORY_COA")?.Value ?? "",
                            TypeCode = "PR_DT",
                            Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_INVENTORY")?.Value ?? ""} {itemDetail.Item.Initial}").Trim(),
                            RefCode1 = itemDetail.Item.Initial,
                            Group = 1,
                            CurrCode = itemData.RtnHeader.CurrCode,
                            Period = itemData.RtnHeader.Date.ToString("yyyyMMdd"),
                            Type = "C",
                            Amount = itemDetail.RtnDetail.Total,
                            SrcTrans = "PR"
                        });
                    }
                    //Piutang Nota Debit
                    journals.Add(new Journal
                    {
                        Code = itemData.RtnHeader.Code,
                        LineNo = 1,
                        Date = itemData.RtnHeader.Date,
                        CoaCode = systemParam.FirstOrDefault(x => x.Code == "DM_AR_COA")?.Value ?? "",
                        TypeCode = "DM_AR",
                        Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_PR")?.Value ?? ""} {itemData.Supplier.Initial}").Trim(),
                        RefCode1 = "",
                        Group = 1,
                        CurrCode = itemData.RtnHeader.CurrCode,
                        Period = itemData.RtnHeader.Date.ToString("yyyyMMdd"),
                        Type = "D",
                        Amount = journals.Where(x => x.Code == itemData.RtnHeader.Code).Sum(s => s.Amount),
                        SrcTrans = "PR"
                    });
                }

            }
            return journals;
        }

        private IEnumerable<Journal> ProcessSalesReturnJournal(DateTime dateTime, List<SystemParameter> systemParam, List<Item> items, List<Tax> taxes)
        {
            List<Journal> journals = new();
            var RtnData = (from rtnheader in _db.SalesReturnHeaders
                           join customer in _db.Customers on rtnheader.CustCode equals customer.Code
                           where rtnheader.Date.Month == dateTime.Month && rtnheader.Date.Year == dateTime.Year
                           select new { RtnHeader = rtnheader, Customer = customer }).ToList();

            var stockMutations = _db.StockMutations.Where(x => new[] { "RCV", "DO", "SR" }.Contains(x.Src));

            foreach (var itemData in RtnData)
            {
                if (itemData.RtnHeader.Type != 1)
                {
                    var RtnDetailData = (from rtndetail in _db.SalesReturnDetails
                                         join item in _db.Items on rtndetail.ItemId equals item.Id
                                         where rtndetail.Code == itemData.RtnHeader.Code
                                         select new { RtnDetail = rtndetail, Item = item }).ToList();
                    var RtnDetailExData = (from rtndetail in _db.SalesReturnDetailExchDiffItems
                                           join item in _db.Items on rtndetail.ItemId equals item.Id
                                           where rtndetail.Code == itemData.RtnHeader.Code
                                           select new { RtnDetail = rtndetail, Item = item }).ToList();

                    decimal totalHeader = 0;

                    if (itemData.RtnHeader.Type == 2)
                    {
                        //Return Detail
                        short i = 0;
                        short j = 0;
                        short k = 0;
                        foreach (var itemDetail in RtnDetailData)
                        {
                            //Inventory using COGS for Amount
                            journals.Add(new Journal
                            {
                                Code = itemData.RtnHeader.Code,
                                LineNo = ++i,
                                Date = itemData.RtnHeader.Date,
                                CoaCode = items.FirstOrDefault(x => x.Id == itemDetail.RtnDetail.ItemId)?.CoaInventory ?? systemParam.FirstOrDefault(x => x.Code == "INVENTORY_COA")?.Value ?? "",
                                TypeCode = "SR_DT",
                                Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_INVENTORY")?.Value ?? ""} {itemDetail.Item.Initial}").Trim(),
                                RefCode1 = itemDetail.Item.Initial,
                                Group = 2,
                                CurrCode = itemData.RtnHeader.CurrCode,
                                Period = itemData.RtnHeader.Date.ToString("yyyyMMdd"),
                                Type = "D",
                                Amount = itemDetail.RtnDetail.NettPrice * itemDetail.RtnDetail.Qty,
                                SrcTrans = "SR"
                            });

                            //COGS (Cost of Goods Sold) - HPP (Harga Pokok Penjualan)
                            journals.Add(new Journal
                            {
                                Code = itemData.RtnHeader.Code,
                                LineNo = ++k,
                                Date = itemData.RtnHeader.Date,
                                CoaCode = items.FirstOrDefault(x => x.Id == itemDetail.RtnDetail.ItemId)?.CoaCogs ?? systemParam.FirstOrDefault(x => x.Code == "COGS_COA")?.Value ?? "",
                                TypeCode = "SR_DT",
                                Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_COGS")?.Value ?? ""} {itemDetail.Item.Initial}").Trim(),
                                RefCode1 = itemDetail.Item.Initial,
                                Group = 5,
                                CurrCode = itemData.RtnHeader.CurrCode,
                                Period = itemData.RtnHeader.Date.ToString("yyyyMMdd"),
                                Type = "C",
                                Amount = itemDetail.RtnDetail.NettPrice * itemDetail.RtnDetail.Qty,
                                SrcTrans = "SR"
                            });

                            //PPN - Pajak
                            if (itemDetail.RtnDetail.TaxAmount > 0)
                            {
                                if (itemData.RtnHeader.IncludeTax)
                                {
                                    journals.Add(new Journal
                                    {
                                        Code = itemData.RtnHeader.Code,
                                        LineNo = ++j,
                                        Date = itemData.RtnHeader.Date,
                                        CoaCode = taxes.FirstOrDefault(x => x.Id == itemDetail.RtnDetail.TaxId)?.CoaCode ?? systemParam.FirstOrDefault(x => x.Code == "TAX_IN_COA")?.Value ?? "",
                                        TypeCode = "SR_DT",
                                        Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_TAX_IN")?.Value ?? ""} {itemDetail.Item.Initial}").Trim(),
                                        RefCode1 = itemDetail.Item.Initial,
                                        Group = 3,
                                        CurrCode = itemData.RtnHeader.CurrCode,
                                        Period = itemData.RtnHeader.Date.ToString("yyyyMMdd"),
                                        Type = "D",
                                        Amount = itemDetail.RtnDetail.TaxAmount * itemDetail.RtnDetail.Qty,
                                        SrcTrans = "SR"
                                    });
                                }
                                else
                                {
                                    journals.Add(new Journal
                                    {
                                        Code = itemData.RtnHeader.Code,
                                        LineNo = ++j,
                                        Date = itemData.RtnHeader.Date,
                                        CoaCode = taxes.FirstOrDefault(x => x.Id == itemDetail.RtnDetail.TaxId)?.CoaCode ?? systemParam.FirstOrDefault(x => x.Code == "TAX_OUT_COA")?.Value ?? "",
                                        TypeCode = "SR_DT",
                                        Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_TAX_OUT")?.Value ?? ""} {itemDetail.Item.Initial}").Trim(),
                                        RefCode1 = itemDetail.Item.Initial,
                                        Group = 3,
                                        CurrCode = itemData.RtnHeader.CurrCode,
                                        Period = itemData.RtnHeader.Date.ToString("yyyyMMdd"),
                                        Type = "D",
                                        Amount = itemDetail.RtnDetail.TaxAmount * itemDetail.RtnDetail.Qty,
                                        SrcTrans = "SR"
                                    });
                                }
                            }

                            totalHeader += itemDetail.RtnDetail.Total;
                        }
                        //Piutang - AR
                        journals.Add(new Journal
                        {
                            Code = itemData.RtnHeader.Code,
                            LineNo = 1,
                            Date = itemData.RtnHeader.Date,
                            CoaCode = systemParam.FirstOrDefault(x => x.Code == "AR_COA")?.Value ?? "",
                            TypeCode = "AR",
                            Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_AR")?.Value ?? ""} {itemData.Customer.Initial}").Trim(),
                            RefCode1 = "",
                            Group = 4,
                            CurrCode = itemData.RtnHeader.CurrCode,
                            Period = itemData.RtnHeader.Date.ToString("yyyyMMdd"),
                            Type = "C",
                            Amount = totalHeader,
                            SrcTrans = "SR"
                        });

                        //Retur Penjualan
                        journals.Add(new Journal
                        {
                            Code = itemData.RtnHeader.Code,
                            LineNo = 1,
                            Date = itemData.RtnHeader.Date,
                            CoaCode = systemParam.FirstOrDefault(x => x.Code == "SR_COA")?.Value ?? "",
                            TypeCode = "SR",
                            Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_SR")?.Value ?? ""} {itemData.Customer.Initial}").Trim(),
                            RefCode1 = "",
                            Group = 1,
                            CurrCode = itemData.RtnHeader.CurrCode,
                            Period = itemData.RtnHeader.Date.ToString("yyyyMMdd"),
                            Type = "D",
                            Amount = itemData.RtnHeader.IncludeTax ? totalHeader - (journals.Where(x => x.Code == itemData.RtnHeader.Code && x.Group == 3).Sum(x => x.Amount)) : totalHeader,
                            SrcTrans = "SR"
                        });
                    }
                    else
                    {
                        //Return Detail Exchange
                        short i = 0;
                        short j = 0;
                        short k = 0;
                        foreach (var itemDetail in RtnDetailExData)
                        {
                            //Inventory using COGS for Amount
                            journals.Add(new Journal
                            {
                                Code = itemData.RtnHeader.Code,
                                LineNo = ++i,
                                Date = itemData.RtnHeader.Date,
                                CoaCode = items.FirstOrDefault(x => x.Id == itemDetail.RtnDetail.ItemId)?.CoaInventory ?? systemParam.FirstOrDefault(x => x.Code == "INVENTORY_COA")?.Value ?? "",
                                TypeCode = "SR_DT",
                                Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_INVENTORY")?.Value ?? ""} {itemDetail.Item.Initial}").Trim(),
                                RefCode1 = itemDetail.Item.Initial,
                                Group = 2,
                                CurrCode = itemData.RtnHeader.CurrCode,
                                Period = itemData.RtnHeader.Date.ToString("yyyyMMdd"),
                                Type = "D",
                                Amount = itemDetail.RtnDetail.NettPrice * itemDetail.RtnDetail.Qty,
                                SrcTrans = "SR"
                            });

                            //COGS (Cost of Goods Sold) - HPP (Harga Pokok Penjualan)
                            journals.Add(new Journal
                            {
                                Code = itemData.RtnHeader.Code,
                                LineNo = ++k,
                                Date = itemData.RtnHeader.Date,
                                CoaCode = items.FirstOrDefault(x => x.Id == itemDetail.RtnDetail.ItemId)?.CoaCogs ?? systemParam.FirstOrDefault(x => x.Code == "COGS_COA")?.Value ?? "",
                                TypeCode = "SR_DT",
                                Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_COGS")?.Value ?? ""} {itemDetail.Item.Initial}").Trim(),
                                RefCode1 = itemDetail.Item.Initial,
                                Group = 5,
                                CurrCode = itemData.RtnHeader.CurrCode,
                                Period = itemData.RtnHeader.Date.ToString("yyyyMMdd"),
                                Type = "C",
                                Amount = itemDetail.RtnDetail.NettPrice * itemDetail.RtnDetail.Qty,
                                SrcTrans = "SR"
                            });

                            //PPN - Pajak
                            if (itemDetail.RtnDetail.TaxAmount > 0)
                            {
                                if (itemData.RtnHeader.IncludeTax)
                                {
                                    journals.Add(new Journal
                                    {
                                        Code = itemData.RtnHeader.Code,
                                        LineNo = ++j,
                                        Date = itemData.RtnHeader.Date,
                                        CoaCode = taxes.FirstOrDefault(x => x.Id == itemDetail.RtnDetail.TaxId)?.CoaCode ?? systemParam.FirstOrDefault(x => x.Code == "TAX_IN_COA")?.Value ?? "",
                                        TypeCode = "SR_DT",
                                        Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_TAX_IN")?.Value ?? ""} {itemDetail.Item.Initial}").Trim(),
                                        RefCode1 = itemDetail.Item.Initial,
                                        Group = 3,
                                        CurrCode = itemData.RtnHeader.CurrCode,
                                        Period = itemData.RtnHeader.Date.ToString("yyyyMMdd"),
                                        Type = "D",
                                        Amount = itemDetail.RtnDetail.TaxAmount * itemDetail.RtnDetail.Qty,
                                        SrcTrans = "SR"
                                    });
                                }
                                else
                                {
                                    journals.Add(new Journal
                                    {
                                        Code = itemData.RtnHeader.Code,
                                        LineNo = ++j,
                                        Date = itemData.RtnHeader.Date,
                                        CoaCode = taxes.FirstOrDefault(x => x.Id == itemDetail.RtnDetail.TaxId)?.CoaCode ?? systemParam.FirstOrDefault(x => x.Code == "TAX_OUT_COA")?.Value ?? "",
                                        TypeCode = "SR_DT",
                                        Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_TAX_OUT")?.Value ?? ""} {itemDetail.Item.Initial}").Trim(),
                                        RefCode1 = itemDetail.Item.Initial,
                                        Group = 3,
                                        CurrCode = itemData.RtnHeader.CurrCode,
                                        Period = itemData.RtnHeader.Date.ToString("yyyyMMdd"),
                                        Type = "D",
                                        Amount = itemDetail.RtnDetail.TaxAmount * itemDetail.RtnDetail.Qty,
                                        SrcTrans = "SR"
                                    });
                                }
                            }

                            totalHeader += itemDetail.RtnDetail.Total;
                        }
                        //Piutang - AR
                        journals.Add(new Journal
                        {
                            Code = itemData.RtnHeader.Code,
                            LineNo = 1,
                            Date = itemData.RtnHeader.Date,
                            CoaCode = systemParam.FirstOrDefault(x => x.Code == "AR_COA")?.Value ?? "",
                            TypeCode = "AR",
                            Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_AR")?.Value ?? ""} {itemData.Customer.Initial}").Trim(),
                            RefCode1 = "",
                            Group = 4,
                            CurrCode = itemData.RtnHeader.CurrCode,
                            Period = itemData.RtnHeader.Date.ToString("yyyyMMdd"),
                            Type = "C",
                            Amount = totalHeader,
                            SrcTrans = "SR"
                        });

                        //Retur Penjualan
                        journals.Add(new Journal
                        {
                            Code = itemData.RtnHeader.Code,
                            LineNo = 1,
                            Date = itemData.RtnHeader.Date,
                            CoaCode = systemParam.FirstOrDefault(x => x.Code == "SR_COA")?.Value ?? "",
                            TypeCode = "SR",
                            Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_SR")?.Value ?? ""} {itemData.Customer.Initial}").Trim(),
                            RefCode1 = "",
                            Group = 1,
                            CurrCode = itemData.RtnHeader.CurrCode,
                            Period = itemData.RtnHeader.Date.ToString("yyyyMMdd"),
                            Type = "D",
                            Amount = itemData.RtnHeader.IncludeTax ? totalHeader - (journals.Where(x => x.Code == itemData.RtnHeader.Code && x.Group == 3).Sum(x => x.Amount)) : totalHeader,
                            SrcTrans = "SR"
                        });
                    }
                    //Delivery Process
                    var DlvData = (from dlvheader in _db.SalesDeliveryHeaders
                                   join customer in _db.Customers on dlvheader.CustCode equals customer.Code
                                   where dlvheader.TransCode == itemData.RtnHeader.Code
                                   select new { DlvHeader = dlvheader, Customer = customer }).ToList();

                    foreach (var itemDlvData in DlvData)
                    {
                        var DlvDetailData = (from dlvdetail in _db.SalesDeliveryDetails
                                             join item in _db.Items on dlvdetail.ItemId equals item.Id
                                             where dlvdetail.Code == itemDlvData.DlvHeader.Code
                                             select new { DlvDetail = dlvdetail, Item = item }).ToList();
                        short i = 0;
                        short j = 0;
                        short k = 0;
                        foreach (var itemDetail in DlvDetailData)
                        {
                            var resultHpp = CalculateHPP(stockMutations, itemDlvData.DlvHeader.WarehouseCode, itemDetail.DlvDetail.ItemId, itemDetail.DlvDetail.Id);
                            //Inventory using COGS for Amount
                            journals.Add(new Journal
                            {
                                Code = itemDlvData.DlvHeader.Code,
                                LineNo = ++i,
                                Date = itemDlvData.DlvHeader.Date,
                                CoaCode = items.FirstOrDefault(x => x.Id == itemDetail.DlvDetail.ItemId)?.CoaInventory ?? systemParam.FirstOrDefault(x => x.Code == "INVENTORY_COA")?.Value ?? "",
                                TypeCode = "DLV_DT",
                                Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_INVENTORY")?.Value ?? ""} {itemDetail.Item.Initial}").Trim(),
                                RefCode1 = itemData.RtnHeader.Code,
                                RefCode2 = itemDetail.Item.Initial,
                                Group = 4,
                                CurrCode = itemDlvData.DlvHeader.CurrCode,
                                Period = itemDlvData.DlvHeader.Date.ToString("yyyyMMdd"),
                                Type = "C",
                                Amount = itemDetail.DlvDetail.NettPrice * itemDetail.DlvDetail.Qty,
                                SrcTrans = "DLV"
                            });

                            //COGS (Cost of Goods Sold) - HPP (Harga Pokok Penjualan)
                            journals.Add(new Journal
                            {
                                Code = itemDlvData.DlvHeader.Code,
                                LineNo = ++k,
                                Date = itemDlvData.DlvHeader.Date,
                                CoaCode = items.FirstOrDefault(x => x.Id == itemDetail.DlvDetail.ItemId)?.CoaCogs ?? systemParam.FirstOrDefault(x => x.Code == "COGS_COA")?.Value ?? "",
                                TypeCode = "DLV_DT",
                                Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_COGS")?.Value ?? ""} {itemDetail.Item.Initial}").Trim(),
                                RefCode1 = itemData.RtnHeader.Code,
                                RefCode2 = itemDetail.Item.Initial,
                                Group = 2,
                                CurrCode = itemDlvData.DlvHeader.CurrCode,
                                Period = itemDlvData.DlvHeader.Date.ToString("yyyyMMdd"),
                                Type = "D",
                                Amount = itemDetail.DlvDetail.NettPrice * itemDetail.DlvDetail.Qty,
                                SrcTrans = "DLV"
                            });

                            //PPN - Pajak
                            if (itemDetail.DlvDetail.TaxAmount > 0)
                            {
                                if (itemDlvData.DlvHeader.IncludeTax)
                                {
                                    journals.Add(new Journal
                                    {
                                        Code = itemDlvData.DlvHeader.Code,
                                        LineNo = ++j,
                                        Date = itemDlvData.DlvHeader.Date,
                                        CoaCode = taxes.FirstOrDefault(x => x.Id == itemDetail.DlvDetail.TaxId)?.CoaCode ?? systemParam.FirstOrDefault(x => x.Code == "TAX_IN_COA")?.Value ?? "",
                                        TypeCode = "DLV_DT",
                                        Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_TAX_IN")?.Value ?? ""} {itemDetail.Item.Initial}").Trim(),
                                        RefCode1 = itemData.RtnHeader.Code,
                                        RefCode2 = itemDetail.Item.Initial,
                                        Group = 5,
                                        CurrCode = itemDlvData.DlvHeader.CurrCode,
                                        Period = itemDlvData.DlvHeader.Date.ToString("yyyyMMdd"),
                                        Type = "C",
                                        Amount = itemDetail.DlvDetail.TaxAmount * itemDetail.DlvDetail.Qty,
                                        SrcTrans = "DLV"
                                    });
                                }
                                else
                                {
                                    journals.Add(new Journal
                                    {
                                        Code = itemDlvData.DlvHeader.Code,
                                        LineNo = ++j,
                                        Date = itemDlvData.DlvHeader.Date,
                                        CoaCode = taxes.FirstOrDefault(x => x.Id == itemDetail.DlvDetail.TaxId)?.CoaCode ?? systemParam.FirstOrDefault(x => x.Code == "TAX_OUT_COA")?.Value ?? "",
                                        TypeCode = "DLV_DT",
                                        Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_TAX_OUT")?.Value ?? ""} {itemDetail.Item.Initial}").Trim(),
                                        RefCode1 = itemData.RtnHeader.Code,
                                        RefCode2 = itemDetail.Item.Initial,
                                        Group = 5,
                                        CurrCode = itemDlvData.DlvHeader.CurrCode,
                                        Period = itemDlvData.DlvHeader.Date.ToString("yyyyMMdd"),
                                        Type = "C",
                                        Amount = itemDetail.DlvDetail.TaxAmount * itemDetail.DlvDetail.Qty,
                                        SrcTrans = "DLV"
                                    });
                                }
                            }
                        }
                        if (itemData.RtnHeader.Type == 3)
                        {
                            if (itemDlvData.DlvHeader.Total > totalHeader)
                            {
                                //Beban Lain-lain
                                journals.Add(new Journal
                                {
                                    Code = itemDlvData.DlvHeader.Code,
                                    LineNo = 1,
                                    Date = itemDlvData.DlvHeader.Date,
                                    CoaCode = systemParam.FirstOrDefault(x => x.Code == "OTH_EXPENSE_COA")?.Value ?? "",
                                    TypeCode = "OTH_EXPENSE",
                                    Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_OTH_EXPENSE")?.Value ?? ""} {itemDlvData.Customer.Initial}").Trim(),
                                    RefCode1 = itemData.RtnHeader.Code,
                                    Group = 6,
                                    CurrCode = itemDlvData.DlvHeader.CurrCode,
                                    Period = itemDlvData.DlvHeader.Date.ToString("yyyyMMdd"),
                                    Type = "D",
                                    Amount = itemDlvData.DlvHeader.Total - totalHeader,
                                    SrcTrans = "DLV"
                                });
                            }
                            else if (totalHeader > itemDlvData.DlvHeader.Total)
                            {
                                //Pendapatan Lain-Lain
                                journals.Add(new Journal
                                {
                                    Code = itemDlvData.DlvHeader.Code,
                                    LineNo = 1,
                                    Date = itemDlvData.DlvHeader.Date,
                                    CoaCode = systemParam.FirstOrDefault(x => x.Code == "OTH_INCOME_COA")?.Value ?? "",
                                    TypeCode = "OTH_INCOME",
                                    Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_OTH_INCOME")?.Value ?? ""} {itemDlvData.Customer.Initial}").Trim(),
                                    RefCode1 = itemData.RtnHeader.Code,
                                    Group = 6,
                                    CurrCode = itemDlvData.DlvHeader.CurrCode,
                                    Period = itemDlvData.DlvHeader.Date.ToString("yyyyMMdd"),
                                    Type = "D",
                                    Amount = Math.Abs(totalHeader - itemDlvData.DlvHeader.Total),
                                    SrcTrans = "DLV"
                                });
                            }
                        }
                        //Piutang - AR
                        journals.Add(new Journal
                        {
                            Code = itemDlvData.DlvHeader.Code,
                            LineNo = 1,
                            Date = itemDlvData.DlvHeader.Date,
                            CoaCode = systemParam.FirstOrDefault(x => x.Code == "AR_COA")?.Value ?? "",
                            TypeCode = "AR",
                            Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_AR")?.Value ?? ""} {itemDlvData.Customer.Initial}").Trim(),
                            RefCode1 = itemData.RtnHeader.Code,
                            Group = 1,
                            CurrCode = itemDlvData.DlvHeader.CurrCode,
                            Period = itemDlvData.DlvHeader.Date.ToString("yyyyMMdd"),
                            Type = "D",
                            Amount = itemDlvData.DlvHeader.Total,
                            SrcTrans = "DLV"
                        });

                        //Retur Penjualan
                        journals.Add(new Journal
                        {
                            Code = itemDlvData.DlvHeader.Code,
                            LineNo = 1,
                            Date = itemDlvData.DlvHeader.Date,
                            CoaCode = systemParam.FirstOrDefault(x => x.Code == "SR_COA")?.Value ?? "",
                            TypeCode = "DLV",
                            Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_SR")?.Value ?? ""} {itemDlvData.Customer.Initial}").Trim(),
                            RefCode1 = itemData.RtnHeader.Code,
                            Group = 3,
                            CurrCode = itemDlvData.DlvHeader.CurrCode,
                            Period = itemDlvData.DlvHeader.Date.ToString("yyyyMMdd"),
                            Type = "C",
                            Amount = itemDlvData.DlvHeader.IncludeTax ? itemDlvData.DlvHeader.Total - (journals.Where(x => x.Code == itemDlvData.DlvHeader.Code && x.Group == 5).Sum(x => x.Amount)) : itemDlvData.DlvHeader.Total,
                            SrcTrans = "DLV"
                        });
                    }
                }
                else
                {
                    var RtnDetailData = (from rtndetail in _db.SalesReturnDetails
                                         join item in _db.Items on rtndetail.ItemId equals item.Id
                                         where rtndetail.Code == itemData.RtnHeader.Code
                                         select new { RtnDetail = rtndetail, Item = item }).ToList();
                    decimal totalHeader = 0;

                    short i = 0;
                    short j = 0;
                    short k = 0;
                    foreach (var itemDetail in RtnDetailData)
                    {
                        //Inventory using COGS for Amount
                        journals.Add(new Journal
                        {
                            Code = itemData.RtnHeader.Code,
                            LineNo = ++i,
                            Date = itemData.RtnHeader.Date,
                            CoaCode = items.FirstOrDefault(x => x.Id == itemDetail.RtnDetail.ItemId)?.CoaInventory ?? systemParam.FirstOrDefault(x => x.Code == "INVENTORY_COA")?.Value ?? "",
                            TypeCode = "SR_DT",
                            Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_INVENTORY")?.Value ?? ""} {itemDetail.Item.Initial}").Trim(),
                            RefCode1 = itemDetail.Item.Initial,
                            Group = 2,
                            CurrCode = itemData.RtnHeader.CurrCode,
                            Period = itemData.RtnHeader.Date.ToString("yyyyMMdd"),
                            Type = "D",
                            Amount = itemDetail.RtnDetail.NettPrice * itemDetail.RtnDetail.Qty,
                            SrcTrans = "SR"
                        });

                        //COGS (Cost of Goods Sold) - HPP (Harga Pokok Penjualan)
                        journals.Add(new Journal
                        {
                            Code = itemData.RtnHeader.Code,
                            LineNo = ++k,
                            Date = itemData.RtnHeader.Date,
                            CoaCode = items.FirstOrDefault(x => x.Id == itemDetail.RtnDetail.ItemId)?.CoaCogs ?? systemParam.FirstOrDefault(x => x.Code == "COGS_COA")?.Value ?? "",
                            TypeCode = "SR_DT",
                            Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_COGS")?.Value ?? ""} {itemDetail.Item.Initial}").Trim(),
                            RefCode1 = itemDetail.Item.Initial,
                            Group = 5,
                            CurrCode = itemData.RtnHeader.CurrCode,
                            Period = itemData.RtnHeader.Date.ToString("yyyyMMdd"),
                            Type = "C",
                            Amount = itemDetail.RtnDetail.NettPrice * itemDetail.RtnDetail.Qty,
                            SrcTrans = "SR"
                        });

                        //PPN - Pajak
                        if (itemDetail.RtnDetail.TaxAmount > 0)
                        {
                            if (itemData.RtnHeader.IncludeTax)
                            {
                                journals.Add(new Journal
                                {
                                    Code = itemData.RtnHeader.Code,
                                    LineNo = ++j,
                                    Date = itemData.RtnHeader.Date,
                                    CoaCode = taxes.FirstOrDefault(x => x.Id == itemDetail.RtnDetail.TaxId)?.CoaCode ?? systemParam.FirstOrDefault(x => x.Code == "TAX_IN_COA")?.Value ?? "",
                                    TypeCode = "SR_DT",
                                    Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_TAX_IN")?.Value ?? ""} {itemDetail.Item.Initial}").Trim(),
                                    RefCode1 = itemDetail.Item.Initial,
                                    Group = 3,
                                    CurrCode = itemData.RtnHeader.CurrCode,
                                    Period = itemData.RtnHeader.Date.ToString("yyyyMMdd"),
                                    Type = "D",
                                    Amount = itemDetail.RtnDetail.TaxAmount * itemDetail.RtnDetail.Qty,
                                    SrcTrans = "SR"
                                });
                            }
                            else
                            {
                                journals.Add(new Journal
                                {
                                    Code = itemData.RtnHeader.Code,
                                    LineNo = ++j,
                                    Date = itemData.RtnHeader.Date,
                                    CoaCode = taxes.FirstOrDefault(x => x.Id == itemDetail.RtnDetail.TaxId)?.CoaCode ?? systemParam.FirstOrDefault(x => x.Code == "TAX_OUT_COA")?.Value ?? "",
                                    TypeCode = "SR_DT",
                                    Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_TAX_OUT")?.Value ?? ""} {itemDetail.Item.Initial}").Trim(),
                                    RefCode1 = itemDetail.Item.Initial,
                                    Group = 3,
                                    CurrCode = itemData.RtnHeader.CurrCode,
                                    Period = itemData.RtnHeader.Date.ToString("yyyyMMdd"),
                                    Type = "D",
                                    Amount = itemDetail.RtnDetail.TaxAmount * itemDetail.RtnDetail.Qty,
                                    SrcTrans = "SR"
                                });
                            }
                        }

                        totalHeader += itemDetail.RtnDetail.Total;
                    }
                    //Hutang Nota Kredit
                    journals.Add(new Journal
                    {
                        Code = itemData.RtnHeader.Code,
                        LineNo = 1,
                        Date = itemData.RtnHeader.Date,
                        CoaCode = systemParam.FirstOrDefault(x => x.Code == "CM_AP_COA")?.Value ?? "",
                        TypeCode = "CM_AP",
                        Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_SR")?.Value ?? ""} {itemData.Customer.Initial}").Trim(),
                        RefCode1 = "",
                        Group = 4,
                        CurrCode = itemData.RtnHeader.CurrCode,
                        Period = itemData.RtnHeader.Date.ToString("yyyyMMdd"),
                        Type = "C",
                        Amount = totalHeader,
                        SrcTrans = "SR"
                    });

                    //Retur Penjualan
                    journals.Add(new Journal
                    {
                        Code = itemData.RtnHeader.Code,
                        LineNo = 1,
                        Date = itemData.RtnHeader.Date,
                        CoaCode = systemParam.FirstOrDefault(x => x.Code == "SR_COA")?.Value ?? "",
                        TypeCode = "SR",
                        Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_SR")?.Value ?? ""} {itemData.Customer.Initial}").Trim(),
                        RefCode1 = "",
                        Group = 1,
                        CurrCode = itemData.RtnHeader.CurrCode,
                        Period = itemData.RtnHeader.Date.ToString("yyyyMMdd"),
                        Type = "D",
                        Amount = itemData.RtnHeader.IncludeTax ? totalHeader - (journals.Where(x => x.Code == itemData.RtnHeader.Code && x.Group == 3).Sum(x => x.Amount)) : totalHeader,
                        SrcTrans = "SR"
                    });
                }
            }
            return journals;
        }

        private IEnumerable<Journal> ProcessExpeditionJournal(DateTime dateTime, List<SystemParameter> systemParam)
        {
            List<Journal> journals = new();
            var expeditionData = (from expheader in _db.ExpeditionInvoiceHeaders
                                  join supplier in _db.Suppliers on expheader.SupCode equals supplier.Code
                                  where expheader.Date.Month == dateTime.Month && expheader.Date.Year == dateTime.Year
                                  select new { ExpHeader = expheader, Supplier = supplier }).ToList();
            short i = 0;
            short j = 0;
            foreach (var itemData in expeditionData)
            {
                //Biaya Ekspedisi
                journals.Add(new Journal
                {
                    Code = itemData.ExpHeader.Code,
                    LineNo = ++i,
                    Date = itemData.ExpHeader.Date,
                    CoaCode = systemParam.FirstOrDefault(x => x.Code == "EP_COST_COA")?.Value ?? "",
                    TypeCode = "EP_COST",
                    Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_EP_COST")?.Value ?? ""} {itemData.Supplier.Initial}").Trim(),
                    RefCode1 = "",
                    Group = 1,
                    CurrCode = itemData.ExpHeader.CurrCode,
                    Period = itemData.ExpHeader.Date.ToString("yyyyMMdd"),
                    Type = "D",
                    Amount = itemData.ExpHeader.Amount,
                    SrcTrans = "EP"
                });
                //Hutang Ekspedisi
                journals.Add(new Journal
                {
                    Code = itemData.ExpHeader.Code,
                    LineNo = ++j,
                    Date = itemData.ExpHeader.Date,
                    CoaCode = systemParam.FirstOrDefault(x => x.Code == "EP_AP_COA")?.Value ?? "",
                    TypeCode = "EP_AP",
                    Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_EP_AP")?.Value ?? ""} {itemData.Supplier.Initial}").Trim(),
                    RefCode1 = "",
                    Group = 2,
                    CurrCode = itemData.ExpHeader.CurrCode,
                    Period = itemData.ExpHeader.Date.ToString("yyyyMMdd"),
                    Type = "C",
                    Amount = itemData.ExpHeader.Amount,
                    SrcTrans = "EP"
                });
            }

            return journals;
        }

        private IEnumerable<Journal> ProcessFixedAssetJournal(DateTime dateTime, List<SystemParameter> systemParam)
        {
            List<Journal> journals = new();
            var fixedAssetData = (from fixedAsset in _db.FixedAssets
                                  join supplier in _db.Suppliers on fixedAsset.SupCode equals supplier.Code
                                  join assetType in _db.AssetTypes on fixedAsset.TypeId equals assetType.Id
                                  where fixedAsset.PurchaseDate.Month == dateTime.Month && fixedAsset.PurchaseDate.Year == dateTime.Year
                                  select new { FixedAsset = fixedAsset, Supplier = supplier, AssetType = assetType }).ToList();
            short i = 0;
            foreach (var itemData in fixedAssetData)
            {
                //Aktiva
                journals.Add(new Journal
                {
                    Code = itemData.FixedAsset.Code,
                    LineNo = ++i,
                    Date = itemData.FixedAsset.PurchaseDate,
                    CoaCode = itemData.AssetType.CoaAsset ?? "",
                    TypeCode = "ASSET",
                    Notes = ($"Aktiva {itemData.Supplier.Initial}").Trim(),
                    RefCode1 = "",
                    Group = 1,
                    CurrCode = "IDR",
                    Period = itemData.FixedAsset.PurchaseDate.ToString("yyyyMMdd"),
                    Type = "D",
                    Amount = itemData.FixedAsset.PurchaseValue,
                    SrcTrans = "ASSET"
                });
                //Biaya
                journals.Add(new Journal
                {
                    Code = itemData.FixedAsset.Code,
                    LineNo = i,
                    Date = itemData.FixedAsset.PurchaseDate,
                    CoaCode = itemData.AssetType.CoaExpense ?? "",
                    TypeCode = "ASSET",
                    Notes = ($"Biaya {itemData.Supplier.Initial}").Trim(),
                    RefCode1 = "",
                    Group = 2,
                    CurrCode = "IDR",
                    Period = itemData.FixedAsset.PurchaseDate.ToString("yyyyMMdd"),
                    Type = "C",
                    Amount = itemData.FixedAsset.PurchaseValue,
                    SrcTrans = "ASSET"
                });
            }

            return journals;
        }
        private IEnumerable<Journal> ProcessDepreciationFixedAssetJournal(DateTime dateTime, List<SystemParameter> systemParam)
        {
            List<Journal> journals = new();
            var fixedAssetData = (from fixedAsset in _db.FixedAssets
                                  join supplier in _db.Suppliers on fixedAsset.SupCode equals supplier.Code
                                  join assetType in _db.AssetTypes on fixedAsset.TypeId equals assetType.Id
                                  where fixedAsset.DepreciationMethod == 2 && fixedAsset.StartDepreciateOn.Month <= dateTime.Month &&
                                  fixedAsset.StartDepreciateOn.Month + fixedAsset.EstimatedLife >= dateTime.Month && fixedAsset.PurchaseDate.Year == dateTime.Year
                                  select new { FixedAsset = fixedAsset, Supplier = supplier, AssetType = assetType }).ToList();

            foreach (var itemData in fixedAssetData)
            {
                decimal depreciationValue = (itemData.FixedAsset.PurchaseValue - itemData.FixedAsset.AcquiredValue) / itemData.FixedAsset.EstimatedLife;
                decimal bookValue = itemData.FixedAsset.PurchaseValue - depreciationValue;
                var startDate = itemData.FixedAsset.StartDepreciateOn;
                var endDate = startDate.AddMonths(itemData.FixedAsset.EstimatedLife);
                short j = 0;

                for (var dataMonth = startDate; dataMonth.Date < endDate.Date; dataMonth = dataMonth.AddMonths(1))
                {
                    if (dataMonth.Month == dateTime.Month)
                    {
                        var hData = _db.FixedAssetHistories.FirstOrDefault(x => x.Code == itemData.FixedAsset.Code && x.NumberOfMonth == dataMonth.Month);
                        if (hData == null)
                        {
                            _db.FixedAssetHistories.Add(new FixedAssetHistory
                            {
                                Code = itemData.FixedAsset.Code,
                                JournalCode = itemData.FixedAsset.Code,
                                FiscalYear = dataMonth.Year.ToString(),
                                NumberOfMonth = (short)dataMonth.Month,
                                Period = (short)(dataMonth.Year % 100),
                                DepreciateDate = new DateTime(dataMonth.Year, dataMonth.Month, dataMonth.Day),
                                DepreciateValue = depreciationValue,
                                BookValue = bookValue
                            });
                        }
                        else
                        {
                            hData.DepreciateValue = depreciationValue;
                            hData.BookValue = bookValue;

                            _db.FixedAssetHistories.Update(hData);
                        }
                        _db.SaveChanges();

                        //Beban Depresiasi
                        journals.Add(new Journal
                        {
                            Code = itemData.FixedAsset.Code,
                            LineNo = ++j,
                            Date = itemData.FixedAsset.PurchaseDate,
                            CoaCode = itemData.AssetType.CoaDeprecExpense ?? "",
                            TypeCode = "ASSET",
                            Notes = ($"Beban Depresiasi {itemData.Supplier.Initial}").Trim(),
                            RefCode1 = "",
                            Group = 3,
                            CurrCode = "IDR",
                            Period = itemData.FixedAsset.PurchaseDate.ToString("yyyyMMdd"),
                            Type = "D",
                            Amount = bookValue,
                            SrcTrans = "ASSET"
                        });
                        //Akumulasi Depresiasi
                        journals.Add(new Journal
                        {
                            Code = itemData.FixedAsset.Code,
                            LineNo = j,
                            Date = itemData.FixedAsset.PurchaseDate,
                            CoaCode = itemData.AssetType.CoaAccumDeprec ?? "",
                            TypeCode = "ASSET",
                            Notes = ($"Akumulasi Depresiasi {itemData.Supplier.Initial}").Trim(),
                            RefCode1 = "",
                            Group = 4,
                            CurrCode = "IDR",
                            Period = itemData.FixedAsset.PurchaseDate.ToString("yyyyMMdd"),
                            Type = "C",
                            Amount = bookValue,
                            SrcTrans = "ASSET"
                        });
                    }
                    bookValue -= depreciationValue;
                }
            }
            return journals;
        }

        private IEnumerable<Journal> ProcessEndYearAssetJournal(DateTime dateTime, List<SystemParameter> systemParam, IEnumerable<Journal> depreJournals)
        {
            List<Journal> journals = new();

            if (dateTime.Month == 12)
            {
                var historyJournal = _db.Journals.Where(x => x.TypeCode == "ASSET" && new[] { 1, 4 }.Contains(x.Group) && x.Date.Year == dateTime.Year).ToList();
                historyJournal.AddRange(depreJournals.Where(x => new[] { 1, 4 }.Contains(x.Group)).ToList());

                //Akumulasi Depresiasi
                journals.Add(new Journal
                {
                    Code = "ASSET-END-YEAR-" + dateTime.Year.ToString(),
                    LineNo = 1,
                    Date = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day),
                    CoaCode = "CoaAccumDeprec",
                    TypeCode = "ASSET",
                    Notes = "Akumulasi Depresiasi Tahun " + dateTime.Year.ToString(),
                    RefCode1 = "",
                    Group = 1,
                    CurrCode = "IDR",
                    Period = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day).ToString("yyyyMMdd"),
                    Type = "D",
                    Amount = historyJournal.Where(x => x.Group == 4).Sum(x => x.Amount),
                    SrcTrans = "END_YEAR"
                });
                //Aktiva
                journals.Add(new Journal
                {
                    Code = "ASSET-END-YEAR-" + dateTime.Year.ToString(),
                    LineNo = 1,
                    Date = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day),
                    CoaCode = "CoaAsset",
                    TypeCode = "ASSET",
                    Notes = "Aktiva Tahun " + dateTime.Year.ToString(),
                    RefCode1 = "",
                    Group = 2,
                    CurrCode = "IDR",
                    Period = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day).ToString("yyyyMMdd"),
                    Type = "C",
                    Amount = historyJournal.Where(x => x.Group == 1).Sum(x => x.Amount),
                    SrcTrans = "END_YEAR"
                });

                //Last Year Akumulasi Depresiasi
                journals.Add(new Journal
                {
                    Code = "ASSET-LAST-YEAR-" + dateTime.Year.ToString(),
                    LineNo = 1,
                    Date = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day),
                    CoaCode = "CoaAccumDeprec",
                    TypeCode = "ASSET",
                    Notes = "Akumulasi Depresiasi Tahun " + dateTime.Year.ToString(),
                    RefCode1 = "",
                    Group = 3,
                    CurrCode = "IDR",
                    Period = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day).ToString("yyyyMMdd"),
                    Type = "D",
                    Amount = historyJournal.Where(x => x.Group == 4).Sum(x => x.Amount),
                    SrcTrans = "END_YEAR"
                });
                //Last Year Aktiva
                journals.Add(new Journal
                {
                    Code = "ASSET-LAST-YEAR-" + dateTime.Year.ToString(),
                    LineNo = 1,
                    Date = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day),
                    CoaCode = "CoaAsset",
                    TypeCode = "ASSET",
                    Notes = "Aktiva Tahun " + dateTime.Year.ToString(),
                    RefCode1 = "",
                    Group = 4,
                    CurrCode = "IDR",
                    Period = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day).ToString("yyyyMMdd"),
                    Type = "C",
                    Amount = historyJournal.Where(x => x.Group == 1).Sum(x => x.Amount),
                    SrcTrans = "END_YEAR"
                });
            }

            return journals;
        }

        private decimal CalculateHPP(IEnumerable<StockMutation> stockMutations, string whCode, int itemId, long id)
        {
            decimal latestQty = 0;
            decimal latestStockValue = 0;
            decimal hpp = 0;

            var firstId = stockMutations.Where(x => x.WarehouseCode == whCode && x.ItemId == itemId).Min(x => x.Id);
            var firstSM = stockMutations.FirstOrDefault(x => x.Id == firstId);

            var currentSM = stockMutations.FirstOrDefault(x => x.WarehouseCode == whCode && x.ItemId == itemId && x.RefDetailId1 == id);

            latestStockValue += firstSM.BaseNettPrice * firstSM.BaseQty;
            latestQty += firstSM.BaseQty;
            hpp = latestStockValue / latestQty;

            var listSM = stockMutations.Where(x => x.WarehouseCode == whCode && x.ItemId == itemId && x.Id > firstId && x.Id <= currentSM.Id).OrderBy(x => x.Id).ToList();
            foreach (var item in listSM)
            {
                if (item.Src == "RCV" && item.Src == "SR")
                {
                    latestStockValue += item.BaseNettPrice * item.BaseQty;
                    latestQty += item.BaseQty;
                }
                else if (item.Src == "DO")
                {
                    var srcTrans = stockMutations.FirstOrDefault(x => x.ItemId == item.ItemId && x.RefCode1 == item.RefCode2);                    
                    if (srcTrans?.Src == "SR")
                    {
                        item.BaseNettPrice = srcTrans.BaseNettPrice;
                        item.NettPrice = srcTrans.NettPrice;
                        latestStockValue -= item.BaseNettPrice * item.BaseQty;
                        latestQty -= item.BaseQty;
                    }
                    else
                    {
                        item.BaseNettPrice = hpp;
                        item.NettPrice = (hpp * item.BaseQty) / item.Qty;
                        latestStockValue -= item.BaseNettPrice * item.BaseQty;
                        latestQty -= item.BaseQty;
                    }
                    _db.StockMutations.Update(item);
                }
                hpp = latestStockValue / latestQty;
            }

            _db.SaveChanges();
            return hpp * currentSM.BaseQty;
        }
    }
}
