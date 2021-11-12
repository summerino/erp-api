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

        public SaveResult PostingJournal(JournalRequest data, int userId)
        {
            var result = new SaveResult(false);
            var systemParam = _db.SystemParameters.ToList();
            var items = _db.Items.ToList();
            var taxes = _db.Taxes.ToList();

            using var transaction = _db.Database.BeginTransaction();
            try
            {
                var periodValid = CheckPrevPeriod(data.Date);
                if (!periodValid)
                {
                    result.Message = "Tidak bisa melakukan posting jurnal karena terdapat periode sebelumnya yang belum diposting.";
                    return result;
                }

                var typeBB = new[] { "BB_AP", "BB_AR", "BB_DM", "BB_CM", "BB_INVT" };
                var removedBB = _db.Journals.Where(x => typeBB.Contains(x.SrcTrans)).ToList();
                if (removedBB != null)
                    _db.RemoveRange(removedBB);

                var removed = _db.Journals.Where(x => x.Date.Month == data.Date.Month && x.Date.Year == data.Date.Year).ToList();
                if (removed != null)
                    _db.RemoveRange(removed);

                var journalRCV = ProcessPurchaseJournal(data.Date, systemParam, items, taxes);
                if (journalRCV != null)
                    _db.AddRange(journalRCV);

                var journalPR = ProcessPurchaseReturnJournal(data.Date, systemParam, items, taxes);
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

                var journalINVT = ProcessBBInventoryJournal(systemParam);
                if (journalINVT != null)
                    _db.AddRange(journalINVT);

                var journalEXP = ProcessExpeditionJournal(data.Date, systemParam);
                if (journalEXP != null)
                    _db.AddRange(journalEXP);

                var journalFA = ProcessFixedAssetJournal(data.Date, systemParam);
                if (journalFA != null)
                    _db.AddRange(journalFA);

                var journalDFA = ProcessDepreciationFixedAssetJournal(data.Date, systemParam);
                if (journalDFA != null)
                    _db.AddRange(journalDFA);

                //var journalEYAS = ProcessEndYearAssetJournal(data.Date, systemParam, journalDFA);
                //if (journalEYAS != null)
                //    _db.AddRange(journalEYAS);

                var journalADJ = ProcessAdjustmentJournal(data.Date, systemParam);
                if (journalADJ != null)
                    _db.AddRange(journalADJ);

                var journalGJ = ProcessGeneralJournal(data.Date);
                if (journalGJ != null)
                    _db.AddRange(journalGJ);

                var journalTS = ProcessTransferStockJournal(data.Date, systemParam);
                if (journalTS != null)
                    _db.AddRange(journalTS);

                if (data.Date.Month == 12)
                {
                    var removedEY = _db.Journals.Where(x => x.Code == "ENDYEAR-" + data.Date.Year.ToString()).ToList();
                    if (removedEY != null)
                        _db.RemoveRange(removedEY);

                    var journalEY = ProcessEndYearJournal(data.Date, systemParam);
                    if (journalEY != null)
                        _db.AddRange(journalEY);
                }

                //Update posting log
                var plData = _db.PostingLogs.FirstOrDefault(x => x.Period == data.Date.ToString("yyyyMM"));
                if (plData == null)
                {
                    _db.PostingLogs.Add(new PostingLog
                    {
                        Period = data.Date.ToString("yyyyMM"),
                        IsPosted = true,
                        PostedBy = userId,
                        PostedDate = DateTime.Now
                    });
                }
                else
                {
                    plData.IsPosted = true;
                    plData.PostedBy = userId;
                    plData.PostedDate = DateTime.Now;
                    _db.PostingLogs.Update(plData);
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
            result.Message = "Posting jurnal selesai.";
            return result;
        }

        private IEnumerable<Journal> ProcessPurchaseJournal(DateTime dateTime, List<SystemParameter> systemParam, List<Item> items, List<Tax> taxes)
        {
            List<Journal> journals = new();

            var RcvData = (from rcvheader in _db.PurchaseReceiveHeaders
                           join supplier in _db.Suppliers on rcvheader.SupCode equals supplier.Code
                           where rcvheader.Date.Month == dateTime.Month && rcvheader.Date.Year == dateTime.Year && rcvheader.SrcTrans == 1 && rcvheader.Mark != "V"
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
                                CoaCode = string.IsNullOrWhiteSpace(taxes.FirstOrDefault(x => x.Id == itemDetail.RcvDetail.TaxId)?.CoaCode) ? systemParam.FirstOrDefault(x => x.Code == "TAX_IN_COA")?.Value ?? "" : taxes.FirstOrDefault(x => x.Id == itemDetail.RcvDetail.TaxId)?.CoaCode,
                                TypeCode = "RCV_DT",
                                Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_TAX_IN")?.Value ?? ""} {itemDetail.Item.Initial}").Trim(),
                                RefCode1 = itemData.RcvHeader.TransCode,
                                RefCode2 = itemDetail.Item.Initial,
                                Group = 2,
                                CurrCode = itemData.RcvHeader.CurrCode,
                                Period = itemData.RcvHeader.Date.ToString("yyyyMMdd"),
                                Type = "D",
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
                                CoaCode = string.IsNullOrWhiteSpace(taxes.FirstOrDefault(x => x.Id == itemDetail.RcvDetail.TaxId)?.CoaCode) ? systemParam.FirstOrDefault(x => x.Code == "TAX_OUT_COA")?.Value ?? "" : taxes.FirstOrDefault(x => x.Id == itemDetail.RcvDetail.TaxId)?.CoaCode,
                                TypeCode = "RCV_DT",
                                Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_TAX_OUT")?.Value ?? ""} {itemDetail.Item.Initial}").Trim(),
                                RefCode1 = itemData.RcvHeader.TransCode,
                                RefCode2 = itemDetail.Item.Initial,
                                Group = 2,
                                CurrCode = itemData.RcvHeader.CurrCode,
                                Period = itemData.RcvHeader.Date.ToString("yyyyMMdd"),
                                Type = "D",
                                Amount = itemDetail.RcvDetail.TaxAmount * itemDetail.RcvDetail.Qty,
                                SrcTrans = "RCV"
                            });
                        }
                    }

                    var ivnValue = (itemDetail.RcvDetail.UnitPrice - itemDetail.RcvDetail.Disc - prorateHeaderDisc) * itemDetail.RcvDetail.Qty;
                    var taxValue = journals.Where(x => x.Code == itemData.RcvHeader.Code && x.RefCode2 == itemDetail.Item.Initial && x.Group == 2).Sum(x => x.Amount);
                    if (itemData.RcvHeader.TaxAmount > 0)
                        if (itemData.RcvHeader.IncludeTax)
                            ivnValue -= taxValue;

                    //Inventory
                    journals.Add(new Journal
                    {
                        Code = itemData.RcvHeader.Code,
                        LineNo = ++i,
                        Date = itemData.RcvHeader.Date,
                        CoaCode = string.IsNullOrWhiteSpace(items.FirstOrDefault(x => x.Id == itemDetail.RcvDetail.ItemId)?.CoaInventory) ? systemParam.FirstOrDefault(x => x.Code == "INVENTORY_COA")?.Value ?? "" : items.FirstOrDefault(x => x.Id == itemDetail.RcvDetail.ItemId)?.CoaInventory,
                        TypeCode = "RCV_DT",
                        Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_INVENTORY")?.Value ?? ""} {itemDetail.Item.Initial}").Trim(),
                        RefCode1 = itemData.RcvHeader.TransCode,
                        RefCode2 = itemDetail.Item.Initial,
                        Group = 1,
                        CurrCode = itemData.RcvHeader.CurrCode,
                        Period = itemData.RcvHeader.Date.ToString("yyyyMMdd"),
                        Type = "D",
                        Amount = ivnValue,
                        SrcTrans = "RCV"
                    });
                }

                var invDetData = _db.PurchaseInvoiceDetails.Where(x => x.RcvCode == itemData.RcvHeader.Code).ToList();
                if (invDetData.Any())
                {
                    foreach (var item in invDetData)
                    {
                        var invData = _db.PurchaseInvoiceHeaders.FirstOrDefault(x => x.Code == item.Code);
                        if (invData.Mark != "V")
                        {
                            var invMemo = _db.PurchaseInvoiceDebitMemos.Where(x => x.InvCode == item.Code).ToList();
                            if (invMemo.Any())
                            {
                                short k = 0;
                                foreach (var itemMemo in invMemo)
                                {
                                    var memoData = _db.DebitMemos.FirstOrDefault(x => x.Code == itemMemo.DebitMemoCode);
                                    if (memoData != null || memoData.Mark != "V")
                                    {
                                        journals.Add(new Journal
                                        {
                                            Code = itemData.RcvHeader.Code,
                                            LineNo = ++k,
                                            Date = itemData.RcvHeader.Date,
                                            CoaCode = systemParam.FirstOrDefault(x => x.Code == "DPS_COA")?.Value ?? "",
                                            TypeCode = "DN",
                                            Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_DPS")?.Value ?? ""} {itemData.Supplier.Initial}").Trim(),
                                            RefCode1 = itemMemo.DebitMemoCode,
                                            Group = 4,
                                            CurrCode = itemData.RcvHeader.CurrCode,
                                            Period = itemData.RcvHeader.Date.ToString("yyyyMMdd"),
                                            Type = "C",
                                            Amount = itemMemo.DebitMemoAmount,
                                            SrcTrans = "PI"
                                        });
                                    }
                                }
                            }
                        }
                    }
                }

                var apValue = itemData.RcvHeader.Total;

                var dmValue = journals.Where(x => x.Code == itemData.RcvHeader.Code && x.Group == 4).Sum(x => x.Amount);
                if (dmValue > 0)
                    apValue -= dmValue;

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
                    Amount = apValue,
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
                           where dlvheader.Date.Month == dateTime.Month && dlvheader.Date.Year == dateTime.Year && dlvheader.SrcTrans == 1 && dlvheader.Mark != "V"
                           select new { Dlvheader = dlvheader, Customer = customer }).ToList();

            foreach (var itemData in DlvData)
            {
                var DlvDetailData = (from dlvdetail in _db.SalesDeliveryDetails
                                     join item in _db.Items on dlvdetail.ItemId equals item.Id
                                     where dlvdetail.Code == itemData.Dlvheader.Code
                                     select new { DlvDetail = dlvdetail, Item = item }).ToList();

                var DlvDetailFreeData = (from fg in _db.SalesDeliveryDetailFreeGoods
                                         join item in _db.Items on fg.ItemId equals item.Id
                                         where fg.Code == itemData.Dlvheader.Code
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
                    var smData = _db.StockMutations.FirstOrDefault(x => x.RefDetailId1 == itemDetail.DlvDetail.Id && x.RefCode1 == itemDetail.DlvDetail.Code);
                    if (smData == null) continue;
                    var prorateHeaderDisc = itemData.Dlvheader.FinalDisc > 0 ? (itemData.Dlvheader.FinalDisc * itemDetail.DlvDetail.NettPrice) / DlvDetailData.Sum(x => x.DlvDetail.NettPrice) : 0;
                    var nonVoidSM = RemoveVoidSM(_db.StockMutations.ToList());
                    CalculateHPP(nonVoidSM, smData.ItemId, smData.RefDetailId1, "DO");
                    smData = _db.StockMutations.FirstOrDefault(x => x.RefDetailId1 == itemDetail.DlvDetail.Id && x.RefCode1 == itemDetail.DlvDetail.Code);
                    var resultHpp = smData.BaseNettPrice > 0 ? smData.BaseNettPrice * smData.BaseQty : 0m;

                    //Discount - Diskon
                    if (itemDetail.DlvDetail.Disc > 0)
                    {
                        journals.Add(new Journal
                        {
                            Code = itemData.Dlvheader.Code,
                            LineNo = ++Ndisc,
                            Date = itemData.Dlvheader.Date,
                            CoaCode = string.IsNullOrWhiteSpace(items.FirstOrDefault(x => x.Id == itemDetail.DlvDetail.ItemId)?.CoaSlsDisc) ? systemParam.FirstOrDefault(x => x.Code == "SLS_DISC_COA")?.Value ?? "" : items.FirstOrDefault(x => x.Id == itemDetail.DlvDetail.ItemId)?.CoaSlsDisc,
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
                            CoaCode = string.IsNullOrWhiteSpace(items.FirstOrDefault(x => x.Id == itemDetail.DlvDetail.ItemId)?.CoaSlsDisc) ? systemParam.FirstOrDefault(x => x.Code == "SLS_DISC_COA")?.Value ?? "" : items.FirstOrDefault(x => x.Id == itemDetail.DlvDetail.ItemId)?.CoaSlsDisc,
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

                    var ivnValue = smData != null ? (smData.BaseNettPrice * smData.BaseQty) : 0;
                    var discValue = journals.Where(x => x.Code == itemData.Dlvheader.Code && x.RefCode2 == itemDetail.Item.Initial && x.Group == 3).Sum(x => x.Amount);
                    if (discValue > 0)
                        ivnValue += discValue;

                    //Inventory using COGS for Amount
                    journals.Add(new Journal
                    {
                        Code = itemData.Dlvheader.Code,
                        LineNo = ++Ninv,
                        Date = itemData.Dlvheader.Date,
                        CoaCode = string.IsNullOrWhiteSpace(items.FirstOrDefault(x => x.Id == itemDetail.DlvDetail.ItemId)?.CoaInventory) ? systemParam.FirstOrDefault(x => x.Code == "INVENTORY_COA")?.Value ?? "" : items.FirstOrDefault(x => x.Id == itemDetail.DlvDetail.ItemId)?.CoaInventory,
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
                        CoaCode = string.IsNullOrWhiteSpace(items.FirstOrDefault(x => x.Id == itemDetail.DlvDetail.ItemId)?.CoaCogs) ? systemParam.FirstOrDefault(x => x.Code == "COGS_COA")?.Value ?? "" : items.FirstOrDefault(x => x.Id == itemDetail.DlvDetail.ItemId)?.CoaCogs,
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
                                CoaCode = string.IsNullOrWhiteSpace(taxes.FirstOrDefault(x => x.Id == itemDetail.DlvDetail.TaxId)?.CoaCode) ? systemParam.FirstOrDefault(x => x.Code == "TAX_IN_COA")?.Value ?? "" : taxes.FirstOrDefault(x => x.Id == itemDetail.DlvDetail.TaxId)?.CoaCode,
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
                                CoaCode = string.IsNullOrWhiteSpace(taxes.FirstOrDefault(x => x.Id == itemDetail.DlvDetail.TaxId)?.CoaCode) ? systemParam.FirstOrDefault(x => x.Code == "TAX_OUT_COA")?.Value ?? "" : taxes.FirstOrDefault(x => x.Id == itemDetail.DlvDetail.TaxId)?.CoaCode,
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

                var invDetData = _db.SalesInvoiceDetails.Where(x => x.DoCode == itemData.Dlvheader.Code).ToList();
                if (invDetData.Any())
                {
                    foreach (var item in invDetData)
                    {
                        var invData = _db.SalesInvoiceHeaders.FirstOrDefault(x => x.Code == item.Code);
                        if (invData.Mark != "V")
                        {
                            var invMemo = _db.SalesInvoiceCreditMemos.Where(x => x.InvCode == item.Code).ToList();
                            if (invMemo.Any())
                            {
                                short k = 0;
                                foreach (var itemMemo in invMemo)
                                {
                                    var memoData = _db.CreditMemos.FirstOrDefault(x => x.Code == itemMemo.CreditMemoCode);
                                    if (memoData != null || memoData.Mark != "V")
                                    {
                                        journals.Add(new Journal
                                        {
                                            Code = itemData.Dlvheader.Code,
                                            LineNo = ++k,
                                            Date = itemData.Dlvheader.Date,
                                            CoaCode = systemParam.FirstOrDefault(x => x.Code == "DPC_COA")?.Value ?? "",
                                            TypeCode = "CN",
                                            Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_DPC")?.Value ?? ""} {itemData.Customer.Initial}").Trim(),
                                            RefCode1 = itemMemo.CreditMemoCode,
                                            Group = 9,
                                            CurrCode = itemData.Dlvheader.CurrCode,
                                            Period = itemData.Dlvheader.Date.ToString("yyyyMMdd"),
                                            Type = "D",
                                            Amount = itemMemo.CreditMemoAmount,
                                            SrcTrans = "SI"
                                        });
                                    }
                                }
                            }
                        }
                    }
                }

                var arValue = itemData.Dlvheader.Total;
                var cmValue = journals.Where(x => x.Code == itemData.Dlvheader.Code && x.Group == 9).Sum(x => x.Amount);
                if (cmValue > 0)
                {
                    arValue -= cmValue;
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
                    Amount = arValue,
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
                    Amount = itemData.Dlvheader.Total - journals.Where(x => x.Code == itemData.Dlvheader.Code && new[] { 4, 5 }.Contains(x.Group)).Sum(x => x.Amount) + journals.Where(x => x.Code == itemData.Dlvheader.Code && new[] { 2, 3 }.Contains(x.Group)).Sum(x => x.Amount),
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
                            CoaCode = string.IsNullOrWhiteSpace(items.FirstOrDefault(x => x.Id == itemFreeDetail.ItemId)?.CoaInventory) ? systemParam.FirstOrDefault(x => x.Code == "INVENTORY_COA")?.Value ?? "" : items.FirstOrDefault(x => x.Id == itemFreeDetail.ItemId)?.CoaInventory,
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
            var cashBankData = _db.GeneralCashBankHeaders.Where(x => x.Date.Month == dateTime.Month && x.Date.Year == dateTime.Year && x.Mark != "V").ToList();
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
                        Date = itemData.ChequeDate.HasValue ? itemData.ChequeDate.Value : itemData.Date,
                        CoaCode = itemDetailData.CoaCode ?? systemParam.FirstOrDefault(x => x.Code == $"{itemDetailData.Type}_COA")?.Value ?? "",
                        TypeCode = $"CB_{itemDetailData.Type}",
                        Notes = ($"{systemParam.FirstOrDefault(x => x.Code == $"JR_PREFIX_{itemDetailData.Type}")?.Value ?? ""} {itemDetailData.Notes}").Trim(),
                        RefCode1 = itemDetailData.TransCode,
                        Group = (short)(itemDetailData.TypeAmount == "D" ? 1 : 2),
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
                        Date = itemData.ChequeDate.HasValue ? itemData.ChequeDate.Value : itemData.Date,
                        CoaCode = itemData.CoaCode ?? "",
                        TypeCode = "CB",
                        Notes = "Kas/Bank",
                        RefCode2 = itemData.Code,
                        Group = (short)(itemDetailData.TypeAmount == "D" ? 1 : 2),
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
                        Date = startDate,
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
                    RefCode1 = "BB-AP-" + startDate.ToString("yyyyMMdd"),
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
                        Date = startDate,
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
                    RefCode1 = "BB-AR-" + startDate.ToString("yyyyMMdd"),
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
                        Date = startDate,
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
                        Date = startDate,
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

        private IEnumerable<Journal> ProcessPurchaseReturnJournal(DateTime dateTime, List<SystemParameter> systemParam, List<Item> items, List<Tax> taxes)
        {
            List<Journal> journals = new();
            var RtnData = (from rtnheader in _db.PurchaseReturnHeaders
                           join supplier in _db.Suppliers on rtnheader.SupCode equals supplier.Code
                           where rtnheader.Date.Month == dateTime.Month && rtnheader.Date.Year == dateTime.Year && rtnheader.Mark != "V"
                           select new { RtnHeader = rtnheader, Supplier = supplier }).ToList();

            foreach (var itemData in RtnData)
            {
                if (itemData.RtnHeader.Type != 1)
                {
                    var RtnDetailData = (from rtndetail in _db.PurchaseReturnDetails
                                         join item in _db.Items on rtndetail.ItemId equals item.Id
                                         where rtndetail.Code == itemData.RtnHeader.Code
                                         select new { RtnDetail = rtndetail, Item = item }).ToList();
                    var RtnDetailExData = _db.PurchaseReturnDetailExchDiffItems.Where(x => x.Code == itemData.RtnHeader.Code).ToList();
                    
                    var hppData = new Dictionary<int,decimal>();

                    short i = 0;
                    short j = 0;
                    foreach (var itemDetail in RtnDetailData)
                    {
                        var smData = _db.StockMutations.FirstOrDefault(x => x.RefDetailId1 == itemDetail.RtnDetail.Id && x.RefCode1 == itemDetail.RtnDetail.Code);
                        if (smData == null) continue;
                        var nonVoidSM = RemoveVoidSM(_db.StockMutations.ToList());
                        CalculateHPP(nonVoidSM, smData.ItemId, smData.RefDetailId1, "PR");
                        smData = _db.StockMutations.FirstOrDefault(x => x.RefDetailId1 == itemDetail.RtnDetail.Id && x.RefCode1 == itemDetail.RtnDetail.Code);
                        var resultHpp = smData.BaseNettPrice > 0 ? smData.BaseNettPrice * smData.BaseQty : 0m;

                        hppData.Add(itemDetail.RtnDetail.ItemId, resultHpp);

                        //Inventory
                        journals.Add(new Journal
                        {
                            Code = itemData.RtnHeader.Code,
                            LineNo = ++i,
                            Date = itemData.RtnHeader.Date,
                            CoaCode = string.IsNullOrWhiteSpace(items.FirstOrDefault(x => x.Id == itemDetail.RtnDetail.ItemId)?.CoaInventory) ? systemParam.FirstOrDefault(x => x.Code == "INVENTORY_COA")?.Value ?? "" : items.FirstOrDefault(x => x.Id == itemDetail.RtnDetail.ItemId)?.CoaInventory,
                            TypeCode = "PR_DT",
                            Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_INVENTORY")?.Value ?? ""} {itemDetail.Item.Initial}").Trim(),
                            RefCode1 = itemDetail.Item.Initial,
                            Group = 2,
                            CurrCode = itemData.RtnHeader.CurrCode,
                            Period = itemData.RtnHeader.Date.ToString("yyyyMMdd"),
                            Type = "C",
                            Amount = resultHpp,
                            SrcTrans = "PR"
                        });

                        if (itemDetail.RtnDetail.TaxAmount > 0)
                        {
                            //Pajak - PPN
                            journals.Add(new Journal
                            {
                                Code = itemData.RtnHeader.Code,
                                LineNo = ++j,
                                Date = itemData.RtnHeader.Date,
                                CoaCode = string.IsNullOrWhiteSpace(taxes.FirstOrDefault(x => x.Id == itemDetail.RtnDetail.TaxId)?.CoaCode) ? systemParam.FirstOrDefault(x => x.Code == "TAX_OUT_COA")?.Value ?? "" : taxes.FirstOrDefault(x => x.Id == itemDetail.RtnDetail.TaxId)?.CoaCode,
                                TypeCode = "PR_DT",
                                Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_TAX_OUT")?.Value ?? ""} {itemDetail.Item.Initial}").Trim(),
                                RefCode1 = itemDetail.Item.Initial,
                                Group = 3,
                                CurrCode = itemData.RtnHeader.CurrCode,
                                Period = itemData.RtnHeader.Date.ToString("yyyyMMdd"),
                                Type = "C",
                                Amount = itemDetail.RtnDetail.Qty * itemDetail.RtnDetail.TaxAmount,
                                SrcTrans = "PR"
                            });
                        }
                    }
                    //Hutang Dagang
                    journals.Add(new Journal
                    {
                        Code = itemData.RtnHeader.Code,
                        LineNo = 1,
                        Date = itemData.RtnHeader.Date,
                        CoaCode = systemParam.FirstOrDefault(x => x.Code == "AP_COA")?.Value ?? "",
                        TypeCode = "AP",
                        Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_AP")?.Value ?? ""} {itemData.Supplier.Initial}").Trim(),
                        RefCode1 = "",
                        Group = 1,
                        CurrCode = itemData.RtnHeader.CurrCode,
                        Period = itemData.RtnHeader.Date.ToString("yyyyMMdd"),
                        Type = "D",
                        Amount = itemData.RtnHeader.Type == 2 ? journals.Where(x => x.Code == itemData.RtnHeader.Code && new[] { 2, 3 }.Contains(x.Group)).Sum(s => s.Amount) : itemData.RtnHeader.Total,
                        SrcTrans = "PR"
                    });

                    if (itemData.RtnHeader.Type == 3)
                    {
                        var hppValue = (itemData.RtnHeader.Total - itemData.RtnHeader.TaxAmount) - journals.Where(x => x.Code == itemData.RtnHeader.Code && x.Group == 2).Sum(x => x.Amount);
                        //COGS - HPP
                        journals.Add(new Journal
                        {
                            Code = itemData.RtnHeader.Code,
                            LineNo = 1,
                            Date = itemData.RtnHeader.Date,
                            CoaCode = systemParam.FirstOrDefault(x => x.Code == "COGS_COA")?.Value ?? "",
                            TypeCode = "PR",
                            Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_COGS")?.Value ?? ""} {itemData.Supplier.Initial}").Trim(),
                            RefCode1 = "",
                            Group = 4,
                            CurrCode = itemData.RtnHeader.CurrCode,
                            Period = itemData.RtnHeader.Date.ToString("yyyyMMdd"),
                            Type = hppValue < 0 ? "D" : "C",
                            Amount = Math.Abs(hppValue),
                            SrcTrans = "PR"
                        });
                    }

                    //Receive Process
                    var RcvData = (from rcvheader in _db.PurchaseReceiveHeaders
                                   join supplier in _db.Suppliers on rcvheader.SupCode equals supplier.Code
                                   where rcvheader.TransCode == itemData.RtnHeader.Code && rcvheader.Mark != "V"
                                   select new { RcvHeader = rcvheader, Supplier = supplier }).ToList();

                    if (RcvData.Any())
                    {
                        foreach (var itemRcvData in RcvData)
                        {
                            var RcvDetailData = (from rcvdetail in _db.PurchaseReceiveDetails
                                                 join item in _db.Items on rcvdetail.ItemId equals item.Id
                                                 where rcvdetail.Code == itemRcvData.RcvHeader.Code
                                                 select new { RcvDetail = rcvdetail, Item = item }).ToList();
                            short k = 0;
                            short l = 0;
                            foreach (var itemDetail in RcvDetailData)
                            {
                                var smData = _db.StockMutations.FirstOrDefault(x => x.RefDetailId1 == itemDetail.RcvDetail.Id && x.RefCode1 == itemDetail.RcvDetail.Code);
                                if (smData == null) continue;
                                var nonVoidSM = RemoveVoidSM(_db.StockMutations.ToList());
                                CalculateHPP(nonVoidSM, smData.ItemId, smData.RefDetailId1, "RCV");
                                smData = _db.StockMutations.FirstOrDefault(x => x.RefDetailId1 == itemDetail.RcvDetail.Id && x.RefCode1 == itemDetail.RcvDetail.Code);
                                var resultHpp = smData.BaseNettPrice > 0 ? smData.BaseNettPrice * smData.BaseQty : 0m;

                                //Inventory
                                journals.Add(new Journal
                                {
                                    Code = itemRcvData.RcvHeader.Code,
                                    LineNo = ++k,
                                    Date = itemRcvData.RcvHeader.Date,
                                    CoaCode = string.IsNullOrWhiteSpace(items.FirstOrDefault(x => x.Id == itemDetail.RcvDetail.ItemId)?.CoaInventory) ? systemParam.FirstOrDefault(x => x.Code == "INVENTORY_COA")?.Value ?? "" : items.FirstOrDefault(x => x.Id == itemDetail.RcvDetail.ItemId)?.CoaInventory,
                                    TypeCode = "RCV_DT",
                                    Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_INVENTORY")?.Value ?? ""} {itemDetail.Item.Initial}").Trim(),
                                    RefCode1 = itemRcvData.RcvHeader.TransCode,
                                    RefCode2 = itemDetail.Item.Initial,
                                    Group = 1,
                                    CurrCode = itemRcvData.RcvHeader.CurrCode,
                                    Period = itemRcvData.RcvHeader.Date.ToString("yyyyMMdd"),
                                    Type = "D",
                                    Amount = itemData.RtnHeader.Type == 2 ? hppData.ContainsKey(itemDetail.RcvDetail.ItemId) ? hppData[itemDetail.RcvDetail.ItemId] : 0 : resultHpp - (itemDetail.RcvDetail.TaxAmount > 0 ? itemDetail.RcvDetail.Qty * itemDetail.RcvDetail.TaxAmount : 0),
                                    SrcTrans = "RCV"
                                });

                                if (itemData.RtnHeader.Type == 3 && itemDetail.RcvDetail.TaxAmount > 0)
                                {
                                    //Pajak - PPN
                                    journals.Add(new Journal
                                    {
                                        Code = itemRcvData.RcvHeader.Code,
                                        LineNo = ++l,
                                        Date = itemRcvData.RcvHeader.Date,
                                        CoaCode = string.IsNullOrWhiteSpace(taxes.FirstOrDefault(x => x.Id == itemDetail.RcvDetail.TaxId)?.CoaCode) ? systemParam.FirstOrDefault(x => x.Code == "TAX_IN_COA")?.Value ?? "" : taxes.FirstOrDefault(x => x.Id == itemDetail.RcvDetail.TaxId)?.CoaCode,
                                        TypeCode = "RCV_DT",
                                        Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_IN_OUT")?.Value ?? ""} {itemDetail.Item.Initial}").Trim(),
                                        RefCode1 = itemDetail.Item.Initial,
                                        Group = 2,
                                        CurrCode = itemRcvData.RcvHeader.CurrCode,
                                        Period = itemRcvData.RcvHeader.Date.ToString("yyyyMMdd"),
                                        Type = "D",
                                        Amount = itemDetail.RcvDetail.Qty * itemDetail.RcvDetail.TaxAmount,
                                        SrcTrans = "RCV"
                                    });
                                }
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
                                Group = 4,
                                CurrCode = itemRcvData.RcvHeader.CurrCode,
                                Period = itemRcvData.RcvHeader.Date.ToString("yyyyMMdd"),
                                Type = "C",
                                Amount = itemData.RtnHeader.Type == 2 ? journals.First(x => x.Code == itemData.RtnHeader.Code && x.Group == 1).Amount : itemData.RtnHeader.Total,
                                SrcTrans = "RCV"
                            });

                            if (itemData.RtnHeader.Type == 3)
                            {
                                var hppRtn = journals.FirstOrDefault(x => x.Code == itemData.RtnHeader.Code && x.Group == 4);
                                //COGS - HPP
                                journals.Add(new Journal
                                {
                                    Code = itemRcvData.RcvHeader.Code,
                                    LineNo = 1,
                                    Date = itemRcvData.RcvHeader.Date,
                                    CoaCode = systemParam.FirstOrDefault(x => x.Code == "COGS_COA")?.Value ?? "",
                                    TypeCode = "RCV",
                                    Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_COGS")?.Value ?? ""} {itemData.Supplier.Initial}").Trim(),
                                    RefCode1 = "",
                                    Group = 3,
                                    CurrCode = itemRcvData.RcvHeader.CurrCode,
                                    Period = itemRcvData.RcvHeader.Date.ToString("yyyyMMdd"),
                                    Type = hppRtn.Type == "C" ? "D" : "C",
                                    Amount = hppRtn.Amount,
                                    SrcTrans = "RCV"
                                });

                                var exValue = journals.Where(x => x.Code == itemRcvData.RcvHeader.Code && x.Type == "D").Sum(x => x.Amount) - journals.Where(x => x.Code == itemRcvData.RcvHeader.Code && x.Type == "C").Sum(x => x.Amount);
                                journals.Add(new Journal
                                {
                                    Code = itemRcvData.RcvHeader.Code,
                                    LineNo = 1,
                                    Date = itemRcvData.RcvHeader.Date,
                                    CoaCode = systemParam.FirstOrDefault(x => x.Code == (exValue < 0 ? "OTH_INCOME_COA" : "OTH_EXPENSE_COA"))?.Value ?? "",
                                    TypeCode = "RCV",
                                    Notes = ($"{systemParam.FirstOrDefault(x => x.Code == (exValue < 0 ? "JR_PREFIX_OTH_INCOME" : "JR_PREFIX_OTH_EXPENSE"))?.Value ?? ""}").Trim(),
                                    RefCode1 = "",
                                    Group = 5,
                                    CurrCode = itemRcvData.RcvHeader.CurrCode,
                                    Period = itemRcvData.RcvHeader.Date.ToString("yyyyMMdd"),
                                    Type = (exValue < 0 ? "D" : "C"),
                                    Amount = Math.Abs(exValue),
                                    SrcTrans = "RCV"
                                });
                            }
                        }
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
                        var smData = _db.StockMutations.FirstOrDefault(x => x.RefDetailId1 == itemDetail.RtnDetail.Id && x.RefCode1 == itemDetail.RtnDetail.Code);
                        if (smData == null) continue;
                        var nonVoidSM = RemoveVoidSM(_db.StockMutations.ToList());
                        CalculateHPP(nonVoidSM, smData.ItemId, smData.RefDetailId1, "PR");
                        smData = _db.StockMutations.FirstOrDefault(x => x.RefDetailId1 == itemDetail.RtnDetail.Id && x.RefCode1 == itemDetail.RtnDetail.Code);
                        var resultHpp = smData.BaseNettPrice > 0 ? smData.BaseNettPrice * smData.BaseQty : 0m;

                        //Inventory
                        journals.Add(new Journal
                        {
                            Code = itemData.RtnHeader.Code,
                            LineNo = ++i,
                            Date = itemData.RtnHeader.Date,
                            CoaCode = string.IsNullOrWhiteSpace(items.FirstOrDefault(x => x.Id == itemDetail.RtnDetail.ItemId)?.CoaInventory) ? systemParam.FirstOrDefault(x => x.Code == "INVENTORY_COA")?.Value ?? "" : items.FirstOrDefault(x => x.Id == itemDetail.RtnDetail.ItemId)?.CoaInventory,
                            TypeCode = "PR_DT",
                            Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_INVENTORY")?.Value ?? ""} {itemDetail.Item.Initial}").Trim(),
                            RefCode1 = itemDetail.Item.Initial,
                            Group = 2,
                            CurrCode = itemData.RtnHeader.CurrCode,
                            Period = itemData.RtnHeader.Date.ToString("yyyyMMdd"),
                            Type = "C",
                            Amount = resultHpp,
                            SrcTrans = "PR"
                        });

                        if (itemDetail.RtnDetail.TaxAmount > 0)
                        {
                            //Pajak - PPN
                            journals.Add(new Journal
                            {
                                Code = itemData.RtnHeader.Code,
                                LineNo = ++i,
                                Date = itemData.RtnHeader.Date,
                                CoaCode = string.IsNullOrWhiteSpace(taxes.FirstOrDefault(x => x.Id == itemDetail.RtnDetail.TaxId)?.CoaCode) ? systemParam.FirstOrDefault(x => x.Code == "TAX_OUT_COA")?.Value ?? "" : taxes.FirstOrDefault(x => x.Id == itemDetail.RtnDetail.TaxId)?.CoaCode,
                                TypeCode = "PR_DT",
                                Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_TAX_OUT")?.Value ?? ""} {itemDetail.Item.Initial}").Trim(),
                                RefCode1 = itemDetail.Item.Initial,
                                Group = 3,
                                CurrCode = itemData.RtnHeader.CurrCode,
                                Period = itemData.RtnHeader.Date.ToString("yyyyMMdd"),
                                Type = "C",
                                Amount = itemDetail.RtnDetail.Qty * itemDetail.RtnDetail.TaxAmount,
                                SrcTrans = "PR"
                            });
                        }
                    }

                    //Uang Muka Pembelian
                    journals.Add(new Journal
                    {
                        Code = itemData.RtnHeader.Code,
                        LineNo = 1,
                        Date = itemData.RtnHeader.Date,
                        CoaCode = systemParam.FirstOrDefault(x => x.Code == "DPS_COA")?.Value ?? "",
                        TypeCode = "DM_AR",
                        Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_DPS")?.Value ?? ""} Pembelian {itemData.Supplier.Initial}").Trim(),
                        RefCode1 = "",
                        Group = 1,
                        CurrCode = itemData.RtnHeader.CurrCode,
                        Period = itemData.RtnHeader.Date.ToString("yyyyMMdd"),
                        Type = "D",
                        Amount = itemData.RtnHeader.Total,
                        SrcTrans = "PR"
                    });

                    var hppValue = (itemData.RtnHeader.Total - itemData.RtnHeader.TaxAmount) - journals.Where(x => x.Code == itemData.RtnHeader.Code && x.Group == 2).Sum(x => x.Amount);
                    //COGS - HPP
                    journals.Add(new Journal
                    {
                        Code = itemData.RtnHeader.Code,
                        LineNo = 1,
                        Date = itemData.RtnHeader.Date,
                        CoaCode = systemParam.FirstOrDefault(x => x.Code == "COGS_COA")?.Value ?? "",
                        TypeCode = "PR",
                        Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_COGS")?.Value ?? ""} {itemData.Supplier.Initial}").Trim(),
                        RefCode1 = "",
                        Group = 4,
                        CurrCode = itemData.RtnHeader.CurrCode,
                        Period = itemData.RtnHeader.Date.ToString("yyyyMMdd"),
                        Type = hppValue < 0 ? "D" : "C",
                        Amount = Math.Abs(hppValue),
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
                           where rtnheader.Date.Month == dateTime.Month && rtnheader.Date.Year == dateTime.Year && rtnheader.Mark != "V"
                           select new { RtnHeader = rtnheader, Customer = customer }).ToList();

            foreach (var itemData in RtnData)
            {
                if (itemData.RtnHeader.Type != 1)
                {
                    var RtnDetailData = (from rtndetail in _db.SalesReturnDetails
                                         join item in _db.Items on rtndetail.ItemId equals item.Id
                                         where rtndetail.Code == itemData.RtnHeader.Code
                                         select new { RtnDetail = rtndetail, Item = item }).ToList();
                    var RtnDetailExData = _db.SalesReturnDetailExchDiffItems.Where(x => x.Code == itemData.RtnHeader.Code).ToList();

                    var hppData = new Dictionary<int, decimal>();

                    //Return Detail
                    short i = 0;
                    short j = 0;
                    short k = 0;
                    foreach (var itemDetail in RtnDetailData)
                    {
                        var smData = _db.StockMutations.FirstOrDefault(x => x.RefDetailId1 == itemDetail.RtnDetail.Id && x.RefCode1 == itemDetail.RtnDetail.Code);
                        if (smData == null) continue;
                        var nonVoidSM = RemoveVoidSM(_db.StockMutations.ToList());
                        CalculateHPP(nonVoidSM, smData.ItemId, smData.RefDetailId1, "SR");
                        smData = _db.StockMutations.FirstOrDefault(x => x.RefDetailId1 == itemDetail.RtnDetail.Id && x.RefCode1 == itemDetail.RtnDetail.Code);
                        var resultHpp = smData.BaseNettPrice > 0 ? smData.BaseNettPrice * smData.BaseQty : 0m;

                        hppData.Add(itemDetail.RtnDetail.ItemId, resultHpp);

                        //Inventory using COGS for Amount
                        journals.Add(new Journal
                        {
                            Code = itemData.RtnHeader.Code,
                            LineNo = ++i,
                            Date = itemData.RtnHeader.Date,
                            CoaCode = string.IsNullOrWhiteSpace(items.FirstOrDefault(x => x.Id == itemDetail.RtnDetail.ItemId)?.CoaInventory) ? systemParam.FirstOrDefault(x => x.Code == "INVENTORY_COA")?.Value ?? "" : items.FirstOrDefault(x => x.Id == itemDetail.RtnDetail.ItemId)?.CoaInventory,
                            TypeCode = "SR_DT",
                            Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_INVENTORY")?.Value ?? ""} {itemDetail.Item.Initial}").Trim(),
                            RefCode1 = itemDetail.Item.Initial,
                            Group = 1,
                            CurrCode = itemData.RtnHeader.CurrCode,
                            Period = itemData.RtnHeader.Date.ToString("yyyyMMdd"),
                            Type = "D",
                            Amount = resultHpp,
                            SrcTrans = "SR"
                        });

                        //COGS (Cost of Goods Sold) - HPP (Harga Pokok Penjualan)
                        journals.Add(new Journal
                        {
                            Code = itemData.RtnHeader.Code,
                            LineNo = ++j,
                            Date = itemData.RtnHeader.Date,
                            CoaCode = string.IsNullOrWhiteSpace(items.FirstOrDefault(x => x.Id == itemDetail.RtnDetail.ItemId)?.CoaCogs) ? systemParam.FirstOrDefault(x => x.Code == "COGS_COA")?.Value ?? "" : items.FirstOrDefault(x => x.Id == itemDetail.RtnDetail.ItemId)?.CoaCogs,
                            TypeCode = "SR_DT",
                            Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_COGS")?.Value ?? ""} {itemDetail.Item.Initial}").Trim(),
                            RefCode1 = itemDetail.Item.Initial,
                            Group = 2,
                            CurrCode = itemData.RtnHeader.CurrCode,
                            Period = itemData.RtnHeader.Date.ToString("yyyyMMdd"),
                            Type = "C",
                            Amount = resultHpp,
                            SrcTrans = "SR"
                        });

                        //PPN - Pajak
                        if (itemDetail.RtnDetail.TaxAmount > 0)
                        {
                            journals.Add(new Journal
                            {
                                Code = itemData.RtnHeader.Code,
                                LineNo = ++k,
                                Date = itemData.RtnHeader.Date,
                                CoaCode = string.IsNullOrWhiteSpace(taxes.FirstOrDefault(x => x.Id == itemDetail.RtnDetail.TaxId)?.CoaCode) ? systemParam.FirstOrDefault(x => x.Code == "TAX_OUT_COA")?.Value ?? "" : taxes.FirstOrDefault(x => x.Id == itemDetail.RtnDetail.TaxId)?.CoaCode,
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
                        Amount = Math.Abs(itemData.RtnHeader.Total),
                        SrcTrans = "SR"
                    });

                    //Retur Penjualan
                    journals.Add(new Journal
                    {
                        Code = itemData.RtnHeader.Code,
                        LineNo = 1,
                        Date = itemData.RtnHeader.Date,
                        CoaCode = systemParam.FirstOrDefault(x => x.Code == "SLS_RTN_COA")?.Value ?? "",
                        TypeCode = "SR",
                        Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_SLS_RTN")?.Value ?? ""} {itemData.Customer.Initial}").Trim(),
                        RefCode1 = "",
                        Group = 5,
                        CurrCode = itemData.RtnHeader.CurrCode,
                        Period = itemData.RtnHeader.Date.ToString("yyyyMMdd"),
                        Type = "D",
                        Amount = Math.Abs(itemData.RtnHeader.Total) - journals.Where(x => x.Code == itemData.RtnHeader.Code && x.Group == 3).Sum(x => x.Amount),
                        SrcTrans = "SR"
                    });
                    
                    //Delivery Process
                    var DlvData = (from dlvheader in _db.SalesDeliveryHeaders
                                   join customer in _db.Customers on dlvheader.CustCode equals customer.Code
                                   where dlvheader.TransCode == itemData.RtnHeader.Code && dlvheader.Mark != "V"
                                   select new { DlvHeader = dlvheader, Customer = customer }).ToList();
                    if (DlvData.Any())
                    {
                        foreach (var itemDlvData in DlvData)
                        {
                            var DlvDetailData = (from dlvdetail in _db.SalesDeliveryDetails
                                                 join item in _db.Items on dlvdetail.ItemId equals item.Id
                                                 where dlvdetail.Code == itemDlvData.DlvHeader.Code 
                                                 select new { DlvDetail = dlvdetail, Item = item }).ToList();
                            short l = 0;
                            short m = 0;
                            short n = 0;
                            foreach (var itemDetail in DlvDetailData)
                            {
                                var smData = _db.StockMutations.FirstOrDefault(x => x.RefDetailId1 == itemDetail.DlvDetail.Id && x.RefCode1 == itemDetail.DlvDetail.Code);
                                if (smData == null) continue;
                                var nonVoidSM = RemoveVoidSM(_db.StockMutations.ToList());
                                CalculateHPP(nonVoidSM, smData.ItemId, smData.RefDetailId1, "DO");
                                smData = _db.StockMutations.FirstOrDefault(x => x.RefDetailId1 == itemDetail.DlvDetail.Id && x.RefCode1 == itemDetail.DlvDetail.Code);
                                var resultHpp = smData.BaseNettPrice > 0 ? smData.BaseNettPrice * smData.BaseQty : 0m;

                                //Inventory using COGS for Amount
                                journals.Add(new Journal
                                {
                                    Code = itemDlvData.DlvHeader.Code,
                                    LineNo = ++l,
                                    Date = itemDlvData.DlvHeader.Date,
                                    CoaCode = string.IsNullOrWhiteSpace(items.FirstOrDefault(x => x.Id == itemDetail.DlvDetail.ItemId)?.CoaInventory) ? systemParam.FirstOrDefault(x => x.Code == "INVENTORY_COA")?.Value ?? "" : items.FirstOrDefault(x => x.Id == itemDetail.DlvDetail.ItemId)?.CoaInventory,
                                    TypeCode = "DLV_DT",
                                    Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_INVENTORY")?.Value ?? ""} {itemDetail.Item.Initial}").Trim(),
                                    RefCode1 = itemData.RtnHeader.Code,
                                    RefCode2 = itemDetail.Item.Initial,
                                    Group = 4,
                                    CurrCode = itemDlvData.DlvHeader.CurrCode,
                                    Period = itemDlvData.DlvHeader.Date.ToString("yyyyMMdd"),
                                    Type = "C",
                                    Amount = resultHpp,
                                    SrcTrans = "DLV"
                                });

                                //COGS (Cost of Goods Sold) - HPP (Harga Pokok Penjualan)
                                journals.Add(new Journal
                                {
                                    Code = itemDlvData.DlvHeader.Code,
                                    LineNo = ++m,
                                    Date = itemDlvData.DlvHeader.Date,
                                    CoaCode = string.IsNullOrWhiteSpace(items.FirstOrDefault(x => x.Id == itemDetail.DlvDetail.ItemId)?.CoaCogs) ? systemParam.FirstOrDefault(x => x.Code == "COGS_COA")?.Value ?? "" : items.FirstOrDefault(x => x.Id == itemDetail.DlvDetail.ItemId)?.CoaCogs,
                                    TypeCode = "DLV_DT",
                                    Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_COGS")?.Value ?? ""} {itemDetail.Item.Initial}").Trim(),
                                    RefCode1 = itemData.RtnHeader.Code,
                                    RefCode2 = itemDetail.Item.Initial,
                                    Group = 2,
                                    CurrCode = itemDlvData.DlvHeader.CurrCode,
                                    Period = itemDlvData.DlvHeader.Date.ToString("yyyyMMdd"),
                                    Type = "D",
                                    Amount = resultHpp,
                                    SrcTrans = "DLV"
                                });

                                //PPN - Pajak
                                if (itemDetail.DlvDetail.TaxAmount > 0)
                                {
                                    journals.Add(new Journal
                                    {
                                        Code = itemDlvData.DlvHeader.Code,
                                        LineNo = ++n,
                                        Date = itemDlvData.DlvHeader.Date,
                                        CoaCode = string.IsNullOrWhiteSpace(taxes.FirstOrDefault(x => x.Id == itemDetail.DlvDetail.TaxId)?.CoaCode) ? systemParam.FirstOrDefault(x => x.Code == "TAX_IN_COA")?.Value ?? "" : taxes.FirstOrDefault(x => x.Id == itemDetail.DlvDetail.TaxId)?.CoaCode,
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
                                Amount = itemData.RtnHeader.Type == 2 ? itemDlvData.DlvHeader.Total : journals.FirstOrDefault(x => x.Code == itemData.RtnHeader.Code && x.Group == 4).Amount,
                                SrcTrans = "DLV"
                            });

                            //Retur Penjualan
                            journals.Add(new Journal
                            {
                                Code = itemDlvData.DlvHeader.Code,
                                LineNo = 1,
                                Date = itemDlvData.DlvHeader.Date,
                                CoaCode = systemParam.FirstOrDefault(x => x.Code == "SLS_RTN_COA")?.Value ?? "",
                                TypeCode = "DLV",
                                Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_SLS_RTN")?.Value ?? ""} {itemDlvData.Customer.Initial}").Trim(),
                                RefCode1 = itemData.RtnHeader.Code,
                                Group = 3,
                                CurrCode = itemDlvData.DlvHeader.CurrCode,
                                Period = itemDlvData.DlvHeader.Date.ToString("yyyyMMdd"),
                                Type = "C",
                                Amount = itemDlvData.DlvHeader.Total - journals.Where(x => x.Code == itemDlvData.DlvHeader.Code && x.Group == 5).Sum(x => x.Amount),
                                SrcTrans = "DLV"
                            });

                            if (itemData.RtnHeader.Type == 3)
                            {
                                var ciValue = journals.FirstOrDefault(x => x.Code == itemDlvData.DlvHeader.Code && x.Group == 1).Amount - (journals.FirstOrDefault(x => x.Code == itemDlvData.DlvHeader.Code && x.Group == 3).Amount + journals.Where(x => x.Code == itemDlvData.DlvHeader.Code && x.Group == 5).Sum(x => x.Amount));
                                journals.Add(new Journal
                                {
                                    Code = itemDlvData.DlvHeader.Code,
                                    LineNo = 1,
                                    Date = itemDlvData.DlvHeader.Date,
                                    CoaCode = systemParam.FirstOrDefault(x => x.Code == (ciValue < 0 ? "OTH_EXPENSE_COA" : "OTH_INCOME_COA"))?.Value ?? "",
                                    TypeCode = "DLV",
                                    Notes = ($"{systemParam.FirstOrDefault(x => x.Code == (ciValue < 0 ? "JR_PREFIX_OTH_EXPENSE" : "JR_PREFIX_OTH_INCOME"))?.Value ?? ""} {itemData.Customer.Initial}").Trim(),
                                    RefCode1 = "",
                                    Group = 6,
                                    CurrCode = itemDlvData.DlvHeader.CurrCode,
                                    Period = itemDlvData.DlvHeader.Date.ToString("yyyyMMdd"),
                                    Type = (ciValue < 0 ? "D" : "C"),
                                    Amount = Math.Abs(ciValue),
                                    SrcTrans = "DLV"
                                });
                            }
                        }
                    }
                }
                else
                {
                    var RtnDetailData = (from rtndetail in _db.SalesReturnDetails
                                         join item in _db.Items on rtndetail.ItemId equals item.Id
                                         where rtndetail.Code == itemData.RtnHeader.Code
                                         select new { RtnDetail = rtndetail, Item = item }).ToList();


                    short i = 0;
                    short j = 0;
                    short k = 0;
                    foreach (var itemDetail in RtnDetailData)
                    {
                        var smData = _db.StockMutations.FirstOrDefault(x => x.RefDetailId1 == itemDetail.RtnDetail.Id && x.RefCode1 == itemDetail.RtnDetail.Code);
                        if (smData == null) continue;

                        var nonVoidSM = RemoveVoidSM(_db.StockMutations.ToList());
                        CalculateHPP(nonVoidSM, smData.ItemId, smData.RefDetailId1, "SR");
                        smData = _db.StockMutations.FirstOrDefault(x => x.RefDetailId1 == itemDetail.RtnDetail.Id && x.RefCode1 == itemDetail.RtnDetail.Code);
                        var resultHpp = smData.BaseNettPrice > 0 ? smData.BaseNettPrice * smData.BaseQty : 0m;

                        //Persediaan Barang
                        journals.Add(new Journal
                        {
                            Code = itemData.RtnHeader.Code,
                            LineNo = ++i,
                            Date = itemData.RtnHeader.Date,
                            CoaCode = systemParam.FirstOrDefault(x => x.Code == "INVENTORY_COA")?.Value ?? "",
                            TypeCode = "SR_DT",
                            Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_INVENTORY")?.Value ?? ""} {itemDetail.Item.Initial}").Trim(),
                            RefCode1 = itemDetail.Item.Initial,
                            Group = 1,
                            CurrCode = itemData.RtnHeader.CurrCode,
                            Period = itemData.RtnHeader.Date.ToString("yyyyMMdd"),
                            Type = "D",
                            Amount = resultHpp,
                            SrcTrans = "SR"
                        });

                        //HPP
                        journals.Add(new Journal
                        {
                            Code = itemData.RtnHeader.Code,
                            LineNo = ++j,
                            Date = itemData.RtnHeader.Date,
                            CoaCode = systemParam.FirstOrDefault(x => x.Code == "COGS_COA")?.Value ?? "",
                            TypeCode = "SR_DT",
                            Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_COGS")?.Value ?? ""} {itemDetail.Item.Initial}").Trim(),
                            RefCode1 = itemDetail.Item.Initial,
                            Group = 3,
                            CurrCode = itemData.RtnHeader.CurrCode,
                            Period = itemData.RtnHeader.Date.ToString("yyyyMMdd"),
                            Type = "C",
                            Amount = resultHpp,
                            SrcTrans = "SR"
                        });

                        if (itemDetail.RtnDetail.TaxAmount > 0)
                        {
                            //PPN - Pajak
                            journals.Add(new Journal
                            {
                                Code = itemData.RtnHeader.Code,
                                LineNo = ++k,
                                Date = itemData.RtnHeader.Date,
                                CoaCode = string.IsNullOrWhiteSpace(taxes.FirstOrDefault(x => x.Id == itemDetail.RtnDetail.TaxId)?.CoaCode) ? systemParam.FirstOrDefault(x => x.Code == "TAX_OUT_COA")?.Value ?? "" : taxes.FirstOrDefault(x => x.Id == itemDetail.RtnDetail.TaxId)?.CoaCode,
                                TypeCode = "SR_DT",
                                Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_TAX_OUT")?.Value ?? ""} {itemDetail.Item.Initial}").Trim(),
                                RefCode1 = itemDetail.Item.Initial,
                                Group = 2,
                                CurrCode = itemData.RtnHeader.CurrCode,
                                Period = itemData.RtnHeader.Date.ToString("yyyyMMdd"),
                                Type = "D",
                                Amount = itemDetail.RtnDetail.TaxAmount * itemDetail.RtnDetail.Qty,
                                SrcTrans = "SR"
                            });
                        }
                    }
                    //Retur Penjualan
                    journals.Add(new Journal
                    {
                        Code = itemData.RtnHeader.Code,
                        LineNo = 1,
                        Date = itemData.RtnHeader.Date,
                        CoaCode = systemParam.FirstOrDefault(x => x.Code == "SLS_RTN_COA")?.Value ?? "",
                        TypeCode = "SR",
                        Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_SLS_RTN")?.Value ?? ""} {itemData.Customer.Initial}").Trim(),
                        RefCode1 = "",
                        Group = 4,
                        CurrCode = itemData.RtnHeader.CurrCode,
                        Period = itemData.RtnHeader.Date.ToString("yyyyMMdd"),
                        Type = "D",
                        Amount = itemData.RtnHeader.Total - itemData.RtnHeader.TaxAmount,
                        SrcTrans = "SR"
                    });

                    //Hutang Nota Kredit
                    journals.Add(new Journal
                    {
                        Code = itemData.RtnHeader.Code,
                        LineNo = 1,
                        Date = itemData.RtnHeader.Date,
                        CoaCode = systemParam.FirstOrDefault(x => x.Code == "CM_AP_COA")?.Value ?? "",
                        TypeCode = "CM_AP",
                        Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_CM_AP")?.Value ?? ""} {itemData.Customer.Initial}").Trim(),
                        RefCode1 = "",
                        Group = 5,
                        CurrCode = itemData.RtnHeader.CurrCode,
                        Period = itemData.RtnHeader.Date.ToString("yyyyMMdd"),
                        Type = "C",
                        Amount = itemData.RtnHeader.Total,
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
                                  where expheader.Date.Month == dateTime.Month && expheader.Date.Year == dateTime.Year && expheader.Mark != "V"
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
                                  where fixedAsset.PurchaseDate.Month == dateTime.Month && fixedAsset.PurchaseDate.Year == dateTime.Year && fixedAsset.Mark != "V"
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
                                  fixedAsset.StartDepreciateOn.Month + fixedAsset.EstimatedLife >= dateTime.Month && fixedAsset.PurchaseDate.Year == dateTime.Year && fixedAsset.Mark != "V"
                                  select new { FixedAsset = fixedAsset, Supplier = supplier, AssetType = assetType }).ToList();

            foreach (var itemData in fixedAssetData)
            {
                decimal depreciationValue = itemData.FixedAsset.EstimatedLife > 0 ? (itemData.FixedAsset.PurchaseValue - itemData.FixedAsset.AcquiredValue) / itemData.FixedAsset.EstimatedLife : (itemData.FixedAsset.PurchaseValue - itemData.FixedAsset.AcquiredValue);
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
                    CoaCode = "",
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
                    CoaCode = "",
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
                    CoaCode = "",
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
                    CoaCode = "",
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

        private IEnumerable<Journal> ProcessBBInventoryJournal(List<SystemParameter> systemParam)
        {
            List<Journal> journals = new();
            var startDate = Convert.ToDateTime(systemParam.FirstOrDefault(x => x.Code == "DATA_START_DATE").Value).AddDays(-1);
            var latestData = _db.VwBeginningBalanceStockHeaders.OrderByDescending(x => x.Date).FirstOrDefault();
            if (latestData?.Date.Month == startDate.Month && latestData?.Date.Year == startDate.Year)
            {
                var bbapData = _db.VwBeginningBalanceStockHeaders.ToList();
                foreach (var item in bbapData)
                {
                    var detailData = _db.VwBeginningBalanceStockDetails.Where(x => x.Code == item.Code).ToList();

                    short i = 0;
                    foreach (var itemDetail in detailData)
                    {
                        journals.Add(new Journal
                        {
                            Code = "BB-INVT-" + startDate.ToString("yyyyMMdd"),
                            LineNo = ++i,
                            Date = startDate,
                            CoaCode = systemParam.FirstOrDefault(x => x.Code == "INVENTORY_COA")?.Value ?? "",
                            TypeCode = "BB_INVT",
                            Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_INVENTORY")?.Value ?? ""} {itemDetail.ItemName}").Trim(),
                            RefCode1 = itemDetail.ItemInitial,
                            Group = 2,
                            CurrCode = "IDR",
                            Period = item.Date.ToString("yyyyMMdd"),
                            Type = "D",
                            Amount = itemDetail.UnitPrice * itemDetail.Qty,
                            SrcTrans = "BB_INVT"
                        });
                    }
                }

                journals.Add(new Journal
                {
                    Code = "BB-INVT-" + startDate.ToString("yyyyMMdd"),
                    LineNo = 1,
                    Date = startDate,
                    CoaCode = systemParam.FirstOrDefault(x => x.Code == "BB_COA")?.Value ?? "",
                    TypeCode = "BB_INVT",
                    Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_BB_INVT")?.Value ?? ""}").Trim(),
                    RefCode1 = "",
                    Group = 1,
                    CurrCode = "IDR",
                    Period = startDate.ToString("yyyyMMdd"),
                    Type = "C",
                    Amount = journals.Where(x => x.Code == "BB-INVT-" + startDate.ToString("yyyyMMdd")).Sum(x => x.Amount),
                    SrcTrans = "BB_INVT"
                });
            }
            return journals;
        }

        private IEnumerable<Journal> ProcessEndYearJournal(DateTime dateTime, List<SystemParameter> systemParam)
        {
            List<Journal> journals = new();
            if (dateTime.Month == 12)
            {
                var nonSpecialAcc = _db.Journals.Where(x => Convert.ToInt32(x.CoaCode) < 400000).ToList();
                var holdAmount = nonSpecialAcc.Where(x => x.Type == "D").Sum(x => x.Amount) - nonSpecialAcc.Where(x => x.Type == "C").Sum(x => x.Amount);

                var specialAcc = _db.Journals.Where(x => Convert.ToInt32(x.CoaCode) >= 400000).ToList();

                journals.Add(new Journal
                {
                    Code = "ENDYEAR-" + dateTime.Year.ToString(),
                    LineNo = 1,
                    Date = new DateTime(dateTime.Year, dateTime.Month, DateTime.DaysInMonth(dateTime.Year, dateTime.Month)),
                    CoaCode = systemParam.FirstOrDefault(x => x.Code == "RETAINED_EARNING_COA")?.Value ?? "",
                    TypeCode = "ADJ_END_YEAR",
                    Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_RETAINED_EARNING")?.Value ?? ""}").Trim(),
                    RefCode1 = "",
                    Group = 1,
                    CurrCode = "IDR",
                    Period = new DateTime(dateTime.Year, dateTime.Month, DateTime.DaysInMonth(dateTime.Year, dateTime.Month)).ToString("yyyyMMdd"),
                    Type = holdAmount > 0 ? "D" : "C",
                    Amount = holdAmount,
                    SrcTrans = "END_YEAR"
                });

                short i = 0;
                foreach (var item in specialAcc)
                {
                    journals.Add(new Journal
                    {
                        Code = "ENDYEAR-" + dateTime.Year.ToString(),
                        LineNo = i++,
                        Date = new DateTime(dateTime.Year, dateTime.Month, DateTime.DaysInMonth(dateTime.Year, dateTime.Month)),
                        CoaCode = item.CoaCode,
                        TypeCode = "ADJ_END_YEAR",
                        Notes = item.Notes ?? "",
                        RefCode1 = "",
                        Group = 2,
                        CurrCode = "IDR",
                        Period = new DateTime(dateTime.Year, dateTime.Month, DateTime.DaysInMonth(dateTime.Year, dateTime.Month)).ToString("yyyyMMdd"),
                        Type = item.Type,
                        Amount = item.Amount,
                        SrcTrans = "END_YEAR"
                    });
                }
            }
            return journals;
        }

        private IEnumerable<Journal> ProcessAdjustmentJournal(DateTime dateTime, List<SystemParameter> systemParam)
        {
            List<Journal> journals = new();

            var adjData = _db.AdjustmentHeaders.Where(x => x.Date.Month == dateTime.Month && x.Date.Year == dateTime.Year && x.Mark != "V").ToList();

            short i = 0;
            short j = 0;
            foreach (var itemData in adjData)
            {
                var adjDetailData = (from adjdetail in _db.AdjustmentDetails
                                     join item in _db.Items on adjdetail.ItemId equals item.Id
                                     where adjdetail.Code == itemData.Code
                                     select new { AdjDetail = adjdetail, Item = item }).ToList();

                short k = 0;
                short l = 0;
                foreach (var itemDetail in adjDetailData)
                {
                    var smData = _db.StockMutations.FirstOrDefault(x => x.RefDetailId1 == itemDetail.AdjDetail.Id && x.RefCode1 == itemDetail.AdjDetail.Code);
                    if (smData == null) continue;

                    var nonVoidSM = RemoveVoidSM(_db.StockMutations.ToList());
                    CalculateHPP(nonVoidSM, smData.ItemId, smData.RefDetailId1, "ADJ");
                    smData = _db.StockMutations.FirstOrDefault(x => x.RefDetailId1 == itemDetail.AdjDetail.Id && x.RefCode1 == itemDetail.AdjDetail.Code);
                    var resultHpp = smData.BaseNettPrice > 0 ? smData.BaseNettPrice * smData.BaseQty : 0m;

                    if (itemData.Type == 1)
                    {
                        journals.Add(new Journal
                        {
                            Code = itemData.Code,
                            LineNo = ++k,
                            Date = itemData.Date,
                            CoaCode = systemParam.FirstOrDefault(x => x.Code == "INVENTORY_COA")?.Value ?? "",
                            TypeCode = "ADJ_DT",
                            Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_ADJ_PLUS")?.Value ?? ""} {itemDetail.Item.Initial}").Trim(),
                            RefCode1 = itemDetail.Item.Initial,
                            Group = 1,
                            CurrCode = "IDR",
                            Period = itemData.Date.ToString("yyyyMMdd"),
                            Type = "D",
                            Amount = resultHpp,
                            SrcTrans = "ADJ"
                        });
                    }
                    else if (itemData.Type == 2)
                    {
                        if (itemDetail.AdjDetail.QtyAdjust > 0)
                        {
                            journals.Add(new Journal
                            {
                                Code = itemData.Code,
                                LineNo = ++k,
                                Date = itemData.Date,
                                CoaCode = systemParam.FirstOrDefault(x => x.Code == "INVENTORY_COA")?.Value ?? "",
                                TypeCode = "ADJ_DT",
                                Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_ADJ_PLUS")?.Value ?? ""} {itemDetail.Item.Initial}").Trim(),
                                RefCode1 = itemDetail.Item.Initial,
                                Group = 1,
                                CurrCode = "IDR",
                                Period = itemData.Date.ToString("yyyyMMdd"),
                                Type = "D",
                                Amount = resultHpp,
                                SrcTrans = "ADJ"
                            });
                        }
                        else if (itemDetail.AdjDetail.QtyAdjust < 0)
                        {
                            journals.Add(new Journal
                            {
                                Code = itemData.Code,
                                LineNo = ++l,
                                Date = itemData.Date,
                                CoaCode = systemParam.FirstOrDefault(x => x.Code == "INVENTORY_COA")?.Value ?? "",
                                TypeCode = "ADJ_DT",
                                Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_ADJ_MINUS")?.Value ?? ""} {itemDetail.Item.Initial}").Trim(),
                                RefCode1 = itemDetail.Item.Initial,
                                Group = 2,
                                CurrCode = "IDR",
                                Period = itemData.Date.ToString("yyyyMMdd"),
                                Type = "C",
                                Amount = Math.Abs(resultHpp),
                                SrcTrans = "ADJ"
                            });
                        }
                    }
                }

                if (journals.Where(x => x.Code == itemData.Code && x.Group == 1).Any())
                {
                    journals.Add(new Journal
                    {
                        Code = itemData.Code,
                        LineNo = ++i,
                        Date = itemData.Date,
                        CoaCode = systemParam.FirstOrDefault(x => x.Code == "OTH_INCOME_COA")?.Value ?? "",
                        TypeCode = "ADJ_INCOME",
                        Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_OTH_INCOME")?.Value ?? ""}").Trim(),
                        RefCode1 = "",
                        Group = 3,
                        CurrCode = "IDR",
                        Period = itemData.Date.ToString("yyyyMMdd"),
                        Type = "C",
                        Amount = journals.Where(x => x.Code == itemData.Code && x.Group == 1).Sum(x => x.Amount),
                        SrcTrans = "ADJ"
                    });
                }

                if (journals.Where(x => x.Code == itemData.Code && x.Group == 2).Any())
                {
                    journals.Add(new Journal
                    {
                        Code = itemData.Code,
                        LineNo = ++j,
                        Date = itemData.Date,
                        CoaCode = systemParam.FirstOrDefault(x => x.Code == "OTH_EXPENSE_COA")?.Value ?? "",
                        TypeCode = "ADJ_COST",
                        Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_OTH_EXPENSE")?.Value ?? ""}").Trim(),
                        RefCode1 = "",
                        Group = 4,
                        CurrCode = "IDR",
                        Period = itemData.Date.ToString("yyyyMMdd"),
                        Type = "D",
                        Amount = journals.Where(x => x.Code == itemData.Code && x.Group == 2).Sum(x => x.Amount),
                        SrcTrans = "ADJ"
                    });
                }
            }

            return journals;
        }
        private IEnumerable<Journal> ProcessGeneralJournal(DateTime dateTime)
        {
            List<Journal> journals = new();

            var dataHeader = _db.GeneralJournalHeaders
                .Where(x => x.Date.Month == dateTime.Month && x.Date.Year == dateTime.Year && x.Mark != "V")
                .ToList();

            foreach (var itemData in dataHeader)
            {
                var dataDetail = _db.GeneralJournalDetails.Where(x => x.Code == itemData.Code).ToList();

                short i = 0;
                short j = 0;
                foreach (var itemDetail in dataDetail)
                {
                    journals.Add(new Journal
                    {
                        Code = itemData.Code,
                        LineNo = itemDetail.Type == "D" ? ++i : ++j,
                        Date = itemData.Date,
                        CoaCode = itemDetail.CoaCode,
                        TypeCode = "GJ_DT",
                        Notes = itemDetail.Notes ?? "",
                        RefCode1 = "",
                        Group = (short)(itemDetail.Type == "D" ? 1 : 2),
                        CurrCode = itemData.CurrCode,
                        Period = itemData.Date.ToString("yyyyMMdd"),
                        Type = itemDetail.Type,
                        Amount = itemDetail.Amount,
                        SrcTrans = "GJ"
                    }); ;
                }
            }

            return journals;
        }

        private IEnumerable<Journal> ProcessTransferStockJournal(DateTime dateTime, List<SystemParameter> systemParam)
        {
            List<Journal> journals = new();

            var dataHeader = _db.TransferStockHeaders
                .Where(x => x.Date.Month == dateTime.Month && x.Date.Year == dateTime.Year && x.Mark != "V")
                .ToList();

            var dataHPP = (new[] { new { Code = "", ItemId = 0, HPP = 0m } }).ToList();

            foreach (var itemData in dataHeader)
            {
                var dataDetail = (from tsdetail in _db.TransferStockDetails
                                     join item in _db.Items on tsdetail.ItemId equals item.Id
                                     where tsdetail.Code == itemData.Code
                                     select new { TsDetail = tsdetail, Item = item }).ToList();

                short i = 0;
                short j = 0;
                foreach (var itemDetail in dataDetail)
                {
                    var smData = _db.StockMutations.FirstOrDefault(x => x.RefDetailId1 == itemDetail.TsDetail.Id && x.RefCode1 == itemDetail.TsDetail.Code);
                    if (smData == null) continue;

                    var nonVoidSM = RemoveVoidSM(_db.StockMutations.ToList());
                    var resultHpp = 0m;
                    if (itemData.Type != "IN")
                    {
                        var srcType = new[] { "OUT", "DT" }.Contains(itemData.Type) ? "TS" : "CNEE";
                        CalculateHPP(nonVoidSM, smData.ItemId, smData.RefDetailId1, srcType);
                        smData = _db.StockMutations.FirstOrDefault(x => x.RefDetailId1 == itemDetail.TsDetail.Id && x.RefCode1 == itemDetail.TsDetail.Code);
                        resultHpp = smData.BaseNettPrice > 0 ? smData.BaseNettPrice * smData.BaseQty : 0m;
                    }

                    if(itemData.Type == "OUT")
                        dataHPP.Add(new { Code = itemData.Code, ItemId = itemDetail.TsDetail.ItemId, HPP = resultHpp });

                    if(itemData.Type == "IN")
                    {
                        var valueHPP = dataHPP.FirstOrDefault(x => x.Code == itemData.OriginTransferCode && x.ItemId == itemDetail.TsDetail.ItemId);
                        if (valueHPP != null)
                            resultHpp = valueHPP.HPP;
                    }

                    //Persediaan Barang
                    journals.Add(new Journal
                    {
                        Code = itemData.Code,
                        LineNo = ++i,
                        Date = itemData.Date,
                        CoaCode = systemParam.FirstOrDefault(x => x.Code == "INVENTORY_COA")?.Value ?? "",
                        TypeCode = "TS_DT",
                        Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_INVENTORY")?.Value ?? ""} {itemDetail.Item.Initial}").Trim(),
                        RefCode1 = itemDetail.Item.Initial,
                        Group = 1,
                        CurrCode = "IDR",
                        Period = itemData.Date.ToString("yyyyMMdd"),
                        Type = new[] { "C", "RC", "OUT", "DT" }.Contains(itemData.Type) ? "C" : "D",
                        Amount = resultHpp,
                        SrcTrans = "TS"
                    });

                    if (new[] { "C", "RC", "DT" }.Contains(itemData.Type))
                    {
                        //Persediaan Barang
                        journals.Add(new Journal
                        {
                            Code = itemData.Code,
                            LineNo = ++j,
                            Date = itemData.Date,
                            CoaCode = systemParam.FirstOrDefault(x => x.Code == "INVENTORY_COA")?.Value ?? "",
                            TypeCode = "TS_DT",
                            Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_INVENTORY")?.Value ?? ""} {itemDetail.Item.Initial}").Trim(),
                            RefCode1 = itemDetail.Item.Initial,
                            Group = 2,
                            CurrCode = "IDR",
                            Period = itemData.Date.ToString("yyyyMMdd"),
                            Type = new[] { "C", "RC", "DT" }.Contains(itemData.Type) ? "D" : "C",
                            Amount = resultHpp,
                            SrcTrans = "TS"
                        });
                    }
                }

                //Barang Terkirim
                journals.Add(new Journal
                {
                    Code = itemData.Code,
                    LineNo = 1,
                    Date = itemData.Date,
                    CoaCode = systemParam.FirstOrDefault(x => x.Code == "SENT_ITEM_COA")?.Value ?? "",
                    TypeCode = "TS",
                    Notes = "Barang Terkirim",
                    RefCode1 = "",
                    Group = 3,
                    CurrCode = "IDR",
                    Period = itemData.Date.ToString("yyyyMMdd"),
                    Type = new[] { "C", "RC", "OUT", "DT" }.Contains(itemData.Type) ? "D" : "C",
                    Amount = journals.Where(x => x.Code ==  itemData.Code && x.Group == 1).Sum(x => x.Amount),
                    SrcTrans = "TS"
                });

                if (new[] { "C", "RC", "DT" }.Contains(itemData.Type))
                {
                    //Barang Terkirim
                    journals.Add(new Journal
                    {
                        Code = itemData.Code,
                        LineNo = 1,
                        Date = itemData.Date,
                        CoaCode = systemParam.FirstOrDefault(x => x.Code == "SENT_ITEM_COA")?.Value ?? "",
                        TypeCode = "TS",
                        Notes = "Barang Terkirim",
                        RefCode1 = "",
                        Group = 4,
                        CurrCode = "IDR",
                        Period = itemData.Date.ToString("yyyyMMdd"),
                        Type = new[] { "C", "RC", "DT" }.Contains(itemData.Type) ? "C" : "D",
                        Amount = journals.Where(x => x.Code == itemData.Code && x.Group == 2).Sum(x => x.Amount),
                        SrcTrans = "TS"
                    });
                }
            }

            return journals;
        }

        private void CalculateHPP(IEnumerable<StockMutation> stockMutations, int itemId, long id, string srcCode)
        {
            decimal latestQty = 0;
            decimal latestStockValue = 0;
            decimal hpp = 0;

            var firstSM = stockMutations.OrderBy(y => y.Date).FirstOrDefault(x => x.ItemId == itemId && x.Src == "BB");
            if (firstSM == null)
                firstSM = stockMutations.OrderBy(y => y.Date).FirstOrDefault(x => x.ItemId == itemId && x.Src == "RCV");


            var currentSM = stockMutations.FirstOrDefault(x => x.ItemId == itemId && x.RefDetailId1 == id && x.Src == srcCode);
            if (currentSM == null) return;

            if ((firstSM?.BaseNettPrice ?? 0m) != 0m)
            {
                latestStockValue += firstSM.BaseNettPrice * firstSM.BaseQty;
                latestQty += firstSM.BaseQty;
                hpp = latestStockValue / latestQty;

                var listSM = stockMutations.Where(x => x.Id != firstSM.Id 
                            && new[] { "RCV", "DO", "SR", "ADJ", "TS", "PR", "CNEE", "BB" }.Contains(x.Src) 
                            && x.ItemId == itemId
                            && x.BaseQty != 0
                            && x.Date >= firstSM.Date 
                            && x.Date <= currentSM.Date)
                            .OrderBy(x => x.Date)
                            .ThenBy(x => x.Id)
                            .ToList();
                foreach (var item in listSM)
                {
                    if (new[] { "RCV","BB","SR" }.Contains(item.Src))
                    {
                        if (item.Src != "BB")
                        {
                            item.BaseNettPrice = hpp;
                            item.NettPrice = hpp * item.BaseQty / item.Qty;
                            _db.StockMutations.Update(item);
                        }
                        latestStockValue += item.BaseNettPrice * item.BaseQty;
                        latestQty += item.BaseQty;
                    }
                    else if (new[] { "ADJ", "TS", "CNEE"}.Contains(item.Src))
                    {
                        item.BaseNettPrice = hpp;
                        item.NettPrice = hpp * item.BaseQty / item.Qty;
                        latestStockValue += item.BaseNettPrice * Math.Abs(item.BaseQty);
                        latestQty += item.BaseQty;
                        _db.StockMutations.Update(item);
                    }
                    else
                    {
                        item.BaseNettPrice = hpp;
                        item.NettPrice = hpp * item.BaseQty / item.Qty;
                        latestStockValue -= item.BaseNettPrice * item.BaseQty;
                        latestQty -= item.BaseQty;
                        _db.StockMutations.Update(item);
                    }
                    if (latestStockValue > 0 && latestQty > 0)
                        hpp = latestStockValue / latestQty;
                }
                _db.SaveChanges();
            }
        }

        private List<StockMutation> RemoveVoidSM(List<StockMutation> data)
        {
            var result = data;
            var listVoid = new List<string>();

            listVoid.AddRange(_db.PurchaseReceiveHeaders.Where(x => x.Mark == "V").Select(x => x.Code).ToList());
            listVoid.AddRange(_db.SalesDeliveryHeaders.Where(x => x.Mark == "V").Select(x => x.Code).ToList());
            listVoid.AddRange(_db.AdjustmentHeaders.Where(x => x.Mark == "V").Select(x => x.Code).ToList());
            listVoid.AddRange(_db.TransferStockHeaders.Where(x => x.Mark == "V").Select(x => x.Code).ToList());
            listVoid.AddRange(_db.PurchaseReturnHeaders.Where(x => x.Mark == "V").Select(x => x.Code).ToList());
            listVoid.AddRange(_db.SalesReturnHeaders.Where(x => x.Mark == "V").Select(x => x.Code).ToList());



            result = result.Where(x => !listVoid.Contains(x.RefCode1)).ToList();

            return result;
        }

        public IEnumerable<PostingLog> GetPostingHistory(JournalRequest data)
        {
            var plData = _db.PostingLogs.ToList();
            return plData.Where(x => x.Period.StartsWith(data.Date.Year.ToString()));
        }

        private bool CheckPrevPeriod(DateTime postDate)
        {
            var bbPeriod =
                Convert
                    .ToDateTime(_db.SystemParameters.FirstOrDefault(x => x.Code == "DATA_START_DATE")?.Value)
                    .AddMonths(-1);

            DateTime startDate = new(postDate.Year, 1, 1);
            startDate = startDate > bbPeriod ? startDate : bbPeriod; 
            DateTime endDate = new(postDate.Year, postDate.Month, 1);
            for (var dataMonth = startDate; dataMonth.Date < endDate.Date; dataMonth = dataMonth.AddMonths(1))
            {
                var plData = _db.PostingLogs.FirstOrDefault(x => x.Period == $"{dataMonth.Year}{(dataMonth.Month > 9 ? dataMonth.Month : "0" + dataMonth.Month)}");
                if (plData == null)
                {
                    _db.PostingLogs.Add(new PostingLog
                    {
                        Period = $"{dataMonth.Year}{(dataMonth.Month > 9 ? dataMonth.Month : "0" + dataMonth.Month)}",
                        IsPosted = false
                    });
                    _db.SaveChanges();

                    return false;
                }

                if (!plData.IsPosted)
                    return false;
            }
            return true;
        }
    }
}
