using Microsoft.EntityFrameworkCore;
using ERP.Entity;
using ERP.Entity.Accounting;
using ERP.Entity.AssetManagement;
using ERP.Entity.General;
using ERP.Entity.Inventory;
using ERP.Entity.SystemManagement;
using ERP.Web.API.Domain.Interfaces.Accounting;
using ERP.Web.API.Model.Accounting;

namespace ERP.Web.API.Domain.Services.Accounting;

public class JournalService : IJournalService
{
    private readonly TenantContext _ctx;
    private readonly IClaimService _claim;
    private readonly IConfiguration _configuration;
    private readonly ILogger<JournalService> _logger;

    public JournalService(TenantContext db, IClaimService claim,
        IConfiguration configuration, ILogger<JournalService> logger)
    {
        _ctx = db;
        _claim = claim;
        _configuration = configuration;
        _logger = logger;
    }
    
    public async ValueTask DoQueueWork(JournalRequest data, int userId, int tenantId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Posting Journal Queued Background Task {data.Date:yyyy-MM-dd} is starting.");

        await PostingJournal(data, userId, tenantId, cancellationToken);
    }

    private async Task PostingJournal(JournalRequest data, int userId, int tenantId,
        CancellationToken cancellationToken)
    {
        // Re configure catalog context
        var contextOptions = new DbContextOptionsBuilder<CatalogContext>()
            .UseSqlServer(_configuration.GetConnectionString("CatalogConnection"))
            .Options;
        
        await using var catalogCtx = new CatalogContext(contextOptions);
        
        // Get tenant server information
        var tenant = await catalogCtx.Tenants.FindAsync(tenantId, cancellationToken);
        if (tenant == null)
        {
            throw new Exception("Tenant doesn't exists (called from JournalService).");
        }

        var optionsBuilder = new DbContextOptionsBuilder<TenantContext>();
        optionsBuilder.UseSqlServer(
            $"Server={tenant.ServerName};Database={tenant.DatabaseName};User Id={tenant.ServerUserId};Password={tenant.ServerPassword};MultipleActiveResultSets=True;TrustServerCertificate=True;Command Timeout=600;Application Name=ERP");

        await using var tenantCtx = new TenantContext(optionsBuilder.Options, catalogCtx, _claim);

        var systemParam = await tenantCtx.SystemParameters.AsNoTracking().ToListAsync(cancellationToken);
        var items = await tenantCtx.Items.AsNoTracking().ToListAsync(cancellationToken);
        var taxes = await tenantCtx.Taxes.AsNoTracking().ToListAsync(cancellationToken);

        try
        {
            var stateData = new PostingState
            {
                Date = DateTime.Now,
                ProcessDate = data.Date,
                UserId = userId,
                Step = 1,
                Status = "ONGOING",
                Notes = ""
            };

            await tenantCtx.PostingStates.AddAsync(stateData, cancellationToken);
            await tenantCtx.SaveChangesAsync(cancellationToken);
            
            await tenantCtx.Database.ExecuteSqlRawAsync(
                "DELETE Accounting.Journal WHERE YEAR([Date]) = {0} AND MONTH([Date]) = {1}",
                data.Date.Year, data.Date.Month);
            
            await NewCalculateHPPAsync(tenantCtx, data.Date, stateData, cancellationToken);
            
            stateData.Notes = $"Done calculate HPP at {DateTime.Now:yyyy-MM-dd HH:mm:ss}.";
            var startDate = Convert.ToDateTime(systemParam.FirstOrDefault(x => x.Code == "DATA_START_DATE")?.Value).AddDays(-1);
            if (data.Date.Year == startDate.Year && data.Date.Month == startDate.Month)
            {
                stateData.Step++; //2
                await tenantCtx.SaveChangesAsync(cancellationToken);
            
                var journalBBAP = ProcessBBAPJournal(tenantCtx, systemParam);
                if (journalBBAP != null)
                    tenantCtx.AddRange(journalBBAP);
            
                stateData.Step++; //3
                await tenantCtx.SaveChangesAsync(cancellationToken);
            
                var journalBBAR = ProcessBBARJournal(tenantCtx, systemParam);
                if (journalBBAR != null)
                    tenantCtx.AddRange(journalBBAR);
            
                stateData.Step++; //4
                await tenantCtx.SaveChangesAsync(cancellationToken);
            
                var journalBBDM = ProcessBBDebitMemoJournal(tenantCtx, systemParam);
                if (journalBBDM != null)
                    tenantCtx.AddRange(journalBBDM);
            
                stateData.Step++; //5
                await tenantCtx.SaveChangesAsync(cancellationToken);
            
                var journalBBCM = ProcessBBCreditMemoJournal(tenantCtx, systemParam);
                if (journalBBCM != null)
                    tenantCtx.AddRange(journalBBCM);
            
                stateData.Step++; //6
                await tenantCtx.SaveChangesAsync(cancellationToken);
            
                var journalINVT = ProcessBBInventoryJournal(tenantCtx, systemParam);
                if (journalINVT != null)
                    tenantCtx.AddRange(journalINVT);
            }
            
            stateData.Step++; //2 /7
            await tenantCtx.SaveChangesAsync(cancellationToken);
            
            var journalRCV = ProcessPurchaseRcvJournal(tenantCtx, data.Date, systemParam, items, taxes);
            if (journalRCV != null)
                tenantCtx.AddRange(journalRCV);
            
            stateData.Step++; //3 /8
            await tenantCtx.SaveChangesAsync(cancellationToken);
            
            var journalPR = ProcessPurchaseReturnJournal(tenantCtx, data.Date, systemParam, items, taxes);
            if (journalPR != null)
                tenantCtx.AddRange(journalPR);
            
            stateData.Step++; //4 /9
            await tenantCtx.SaveChangesAsync(cancellationToken);
            
            var journalPI = ProcessPurchaseInvJournal(tenantCtx, data.Date, systemParam, items, taxes);
            if (journalPI != null)
                tenantCtx.AddRange(journalPI);
            
            stateData.Step++; //5 /10
            await tenantCtx.SaveChangesAsync(cancellationToken);
            
            // Posting do journals
            _logger.LogInformation($"Start posting journal DO at {DateTime.Now:yyyy-MM-dd HH:mm:ss}.");
            var journalDO = await ProcessSaleDlvJournalAsync(tenantCtx, data.Date, systemParam, items, taxes, cancellationToken);
            if (journalDO != null)
                await tenantCtx.AddRangeAsync(journalDO, cancellationToken);
            
            _logger.LogInformation($"Insert posting journal DO at {DateTime.Now:yyyy-MM-dd HH:mm:ss}.");
            stateData.Step++; //6 /11
            await tenantCtx.SaveChangesAsync(cancellationToken);
            _logger.LogInformation($"Done posting journal DO at {DateTime.Now:yyyy-MM-dd HH:mm:ss}.");
            
            // Posting si journals
            _logger.LogInformation($"Start posting journal SI at {DateTime.Now:yyyy-MM-dd HH:mm:ss}.");
            var journalSI = await ProcessSaleInvJournalAsync(tenantCtx, data.Date, systemParam, taxes, cancellationToken);
            if (journalSI != null)
                await tenantCtx.AddRangeAsync(journalSI, cancellationToken);
            
            _logger.LogInformation($"Insert posting journal SI at {DateTime.Now:yyyy-MM-dd HH:mm:ss}.");
            stateData.Step++; //7 /12
            await tenantCtx.SaveChangesAsync(cancellationToken);
            _logger.LogInformation($"Done posting journal SI at {DateTime.Now:yyyy-MM-dd HH:mm:ss}.");
            
            // Posting sr journals
            _logger.LogInformation($"Start posting journal SR at {DateTime.Now:yyyy-MM-dd HH:mm:ss}.");
            var journalSR = await ProcessSalesReturnJournalAsync(tenantCtx, data.Date, systemParam, items, taxes, cancellationToken);
            if (journalSR != null)
                await tenantCtx.AddRangeAsync(journalSR, cancellationToken);
            
            _logger.LogInformation($"Insert posting journal SR at {DateTime.Now:yyyy-MM-dd HH:mm:ss}.");
            stateData.Step++; //8 /13
            await tenantCtx.SaveChangesAsync(cancellationToken);
            _logger.LogInformation($"Done posting journal SR at {DateTime.Now:yyyy-MM-dd HH:mm:ss}.");
            
            // Posting do from sr journals
            _logger.LogInformation($"Start posting journal DO from SR at {DateTime.Now:yyyy-MM-dd HH:mm:ss}.");
            var journalDoSr = await ProcessSalesDlvReturnJournalAsync(tenantCtx, data.Date, systemParam, items, taxes, cancellationToken);
            if (journalDoSr != null)
                await tenantCtx.AddRangeAsync(journalDoSr, cancellationToken);
            
            _logger.LogInformation($"Insert posting journal DO from SR at {DateTime.Now:yyyy-MM-dd HH:mm:ss}.");
            stateData.Step++; //9 /14
            await tenantCtx.SaveChangesAsync(cancellationToken);
            _logger.LogInformation($"Done posting journal DO from SR at {DateTime.Now:yyyy-MM-dd HH:mm:ss}.");
            
            // Posting cb journals
            _logger.LogInformation($"Start posting journal CB at {DateTime.Now:yyyy-MM-dd HH:mm:ss}.");
            var journalCB = await ProcessCashBankJournalAsync(tenantCtx, data.Date, systemParam, cancellationToken);
            if (journalCB != null)
                await tenantCtx.AddRangeAsync(journalCB, cancellationToken);
            
            _logger.LogInformation($"Insert posting journal CB at {DateTime.Now:yyyy-MM-dd HH:mm:ss}.");
            stateData.Step++; //10 /15
            await tenantCtx.SaveChangesAsync(cancellationToken);
            _logger.LogInformation($"Done posting journal CB at {DateTime.Now:yyyy-MM-dd HH:mm:ss}.");
            
            
            var journalEXP = ProcessExpeditionJournal(tenantCtx, data.Date, systemParam);
            if (journalEXP != null)
                tenantCtx.AddRange(journalEXP);
            
            stateData.Step++; //11 /16
            await tenantCtx.SaveChangesAsync(cancellationToken);
            
            var journalFA = ProcessFixedAssetJournal(tenantCtx, data.Date, systemParam);
            if (journalFA != null)
                tenantCtx.AddRange(journalFA);
            
            stateData.Step++; //11 /17
            await tenantCtx.SaveChangesAsync(cancellationToken);
            
            var journalDFA = ProcessDepreciationFixedAssetJournal(tenantCtx, data.Date, systemParam);
            if (journalDFA != null)
                tenantCtx.AddRange(journalDFA);
            
            stateData.Step++; //13 /18
            await tenantCtx.SaveChangesAsync(cancellationToken);
            
            //var journalEYAS = ProcessEndYearAssetJournal(data.Date, systemParam, journalDFA);
            //if (journalEYAS != null)
            //    tenantCtx.AddRange(journalEYAS);
            
            var journalADJ = ProcessAdjustmentJournal(tenantCtx, data.Date, systemParam);
            if (journalADJ != null)
                tenantCtx.AddRange(journalADJ);
            
            stateData.Step++; //14 /19
            await tenantCtx.SaveChangesAsync(cancellationToken);
            
            var journalGJ = ProcessGeneralJournal(tenantCtx, data.Date);
            if (journalGJ != null)
                tenantCtx.AddRange(journalGJ);
            
            stateData.Step++; //15 /20
            await tenantCtx.SaveChangesAsync(cancellationToken);
            
            var journalTS = ProcessTransferStockJournal(tenantCtx, data.Date, systemParam);
            if (journalTS != null)
                tenantCtx.AddRange(journalTS);
            
            stateData.Step++; //16 /21
            await tenantCtx.SaveChangesAsync(cancellationToken);
            
            var journalSDP = ProcessSalesDownPaymentJournal(tenantCtx, systemParam);
            if (journalSDP != null)
                tenantCtx.AddRange(journalSDP);
            
            if (data.Date.Month == 12)
            {
                stateData.Step++; //17 /22
                await tenantCtx.SaveChangesAsync(cancellationToken);
            
                tenantCtx.Database.ExecuteSqlRaw(
                    "DELETE Accounting.Journal WHERE Code = {0}",
                    "ENDYEAR-" + data.Date.Year.ToString());
            
                stateData.Step++; //18 /23
                await tenantCtx.SaveChangesAsync(cancellationToken);
            
                ProcessEndYearJournal(tenantCtx, data.Date);
            }
            
            stateData.Step++; //17 /24 /22
            await tenantCtx.SaveChangesAsync(cancellationToken);
            
            //Update posting log
            var plData = tenantCtx.PostingLogs.FirstOrDefault(x => x.Period == data.Date.ToString("yyyyMM"));
            if (plData == null)
            {
                tenantCtx.PostingLogs.Add(new PostingLog
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
                tenantCtx.PostingLogs.Update(plData);
            }
            
            stateData.Status = "FINISH";
            stateData.Notes = $"Finish Time at {DateTime.Now:yyyy-MM-dd HH:mm:ss}.";
            await tenantCtx.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            var stateData = tenantCtx.PostingStates.OrderByDescending(x => x.Id).FirstOrDefault(x => x.UserId == userId);
            stateData.Status = "FAILED";
            stateData.Notes = ex.InnerException?.Message ?? ex.Message;
            await tenantCtx.SaveChangesAsync(cancellationToken);
        }
    }

    private IEnumerable<Journal> ProcessPurchaseRcvJournal(TenantContext db, DateTime dateTime, List<SystemParameter> systemParam, List<Item> items, List<Tax> taxes)
    {
        List<Journal> journals = new();
        var apRecog = systemParam.FirstOrDefault(x => x.Code == "AP_RECOG_TIME").Value;

        var RcvData = (from rcvheader in db.PurchaseReceiveHeaders
                       join supplier in db.Suppliers on rcvheader.SupCode equals supplier.Code
                       where rcvheader.Date.Month == dateTime.Month && rcvheader.Date.Year == dateTime.Year && rcvheader.SrcTrans == 1 && rcvheader.Mark != "V"
                       select new { RcvHeader = rcvheader, Supplier = supplier }).AsNoTracking().ToList();

        foreach (var itemData in RcvData)
        {
            var RcvDetailData = (from rcvdetail in db.PurchaseReceiveDetails
                                 join item in db.Items on rcvdetail.ItemId equals item.Id
                                 where rcvdetail.Code == itemData.RcvHeader.Code
                                 select new { RcvDetail = rcvdetail, Item = item }).AsNoTracking().ToList();
            short i = 0;
            short j = 0;
            short k = 0;
            if (apRecog == "PI")
            {
                foreach (var itemDetail in RcvDetailData)
                {
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
                        Amount = itemDetail.RcvDetail.Total - (itemDetail.RcvDetail.TaxAmount * itemDetail.RcvDetail.Qty) + ((itemDetail.RcvDetail.ExemptTaxAmount * itemDetail.RcvDetail.Qty)),
                        SrcTrans = "RCV"
                    });
                }

                //Hutang belum difakturkan
                journals.Add(new Journal
                {
                    Code = itemData.RcvHeader.Code,
                    LineNo = 1,
                    Date = itemData.RcvHeader.Date,
                    CoaCode = systemParam.FirstOrDefault(x => x.Code == "AP_DFR_COA")?.Value ?? "",
                    TypeCode = "AP_DFR",
                    Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "AP_DFR_COA")?.Description ?? ""} {itemData.Supplier.Initial}").Trim(),
                    RefCode1 = itemData.RcvHeader.TransCode,
                    RefCode2 = "",
                    Group = 2,
                    CurrCode = itemData.RcvHeader.CurrCode,
                    Period = itemData.RcvHeader.Date.ToString("yyyyMMdd"),
                    Type = "C",
                    Amount = itemData.RcvHeader.Total - itemData.RcvHeader.TaxAmount + itemData.RcvHeader.ExemptTaxAmount,
                    SrcTrans = "RCV"
                });
            }
            else
            {
                foreach (var itemDetail in RcvDetailData)
                {
                    var prorateHeaderDisc = itemData.RcvHeader.FinalDisc > 0 ? (itemData.RcvHeader.FinalDisc * itemDetail.RcvDetail.NettPrice) / RcvDetailData.Sum(x => x.RcvDetail.NettPrice) : 0;

                    if (itemDetail.RcvDetail.TaxAmount > 0)
                    {
                        //PPN
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
                            Amount = (itemDetail.RcvDetail.TaxAmount - itemDetail.RcvDetail.ExemptTaxAmount) != 0 ? (itemDetail.RcvDetail.TaxAmount - itemDetail.RcvDetail.ExemptTaxAmount) * itemDetail.RcvDetail.Qty : 0,
                            SrcTrans = "RCV"
                        });
                    }

                    if (itemDetail.RcvDetail.ExemptTaxAmount > 0)
                    {
                        //PPN Yang Dibebaskan
                        journals.Add(new Journal
                        {
                            Code = itemData.RcvHeader.Code,
                            LineNo = ++k,
                            Date = itemData.RcvHeader.Date,
                            CoaCode = string.IsNullOrWhiteSpace(taxes.FirstOrDefault(x => x.Id == itemDetail.RcvDetail.TaxId)?.ExemptCoaCode) ? "" : taxes.FirstOrDefault(x => x.Id == itemDetail.RcvDetail.TaxId)?.ExemptCoaCode,
                            TypeCode = "RCV_DT",
                            Notes = "PPN Yang Dibebaskan",
                            RefCode1 = itemData.RcvHeader.TransCode,
                            RefCode2 = itemDetail.Item.Initial,
                            Group = 3,
                            CurrCode = itemData.RcvHeader.CurrCode,
                            Period = itemData.RcvHeader.Date.ToString("yyyyMMdd"),
                            Type = "C",
                            Amount = itemDetail.RcvDetail.ExemptTaxAmount * itemDetail.RcvDetail.Qty,
                            SrcTrans = "RCV"
                        });

                        //PPN Yang Dibebaskan
                        journals.Add(new Journal
                        {
                            Code = itemData.RcvHeader.Code,
                            LineNo = k,
                            Date = itemData.RcvHeader.Date,
                            CoaCode = string.IsNullOrWhiteSpace(taxes.FirstOrDefault(x => x.Id == itemDetail.RcvDetail.TaxId)?.ExemptCoaCode) ? "" : taxes.FirstOrDefault(x => x.Id == itemDetail.RcvDetail.TaxId)?.ExemptCoaCode,
                            TypeCode = "RCV_DT",
                            Notes = "PPN Yang Dibebaskan",
                            RefCode1 = itemData.RcvHeader.TransCode,
                            RefCode2 = itemDetail.Item.Initial,
                            Group = 5,
                            CurrCode = itemData.RcvHeader.CurrCode,
                            Period = itemData.RcvHeader.Date.ToString("yyyyMMdd"),
                            Type = "D",
                            Amount = itemDetail.RcvDetail.ExemptTaxAmount * itemDetail.RcvDetail.Qty,
                            SrcTrans = "RCV"
                        });
                    }

                    var ivnValue = (itemDetail.RcvDetail.UnitPrice - itemDetail.RcvDetail.Disc - prorateHeaderDisc) * itemDetail.RcvDetail.Qty;
                    var taxValue = journals.Where(x => x.Code == itemData.RcvHeader.Code && x.RefCode2 == itemDetail.Item.Initial && x.Group == 2).Sum(x => x.Amount);
                    //var extTaxValue = journals.Where(x => x.Code == itemData.RcvHeader.Code && x.RefCode2 == itemDetail.Item.Initial && x.Group == 3).Sum(x => x.Amount);
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
                        Amount = itemDetail.RcvDetail.Type == 0 ? ivnValue : 0m,
                        SrcTrans = "RCV"
                    });
                }

                var invDetData = db.PurchaseInvoiceDetails.AsNoTracking().Where(x => x.RcvCode == itemData.RcvHeader.Code).ToList();
                if (invDetData.Any())
                {
                    foreach (var item in invDetData)
                    {
                        var invData = db.PurchaseInvoiceHeaders.AsNoTracking().FirstOrDefault(x => x.Code == item.Code);
                        if (invData.Mark != "V")
                        {
                            var invMemo = db.PurchaseInvoiceDebitMemos.AsNoTracking().Where(x => x.InvCode == item.Code).ToList();
                            if (invMemo.Any())
                            {
                                short l = 0;
                                foreach (var itemMemo in invMemo)
                                {
                                    var memoData = db.DebitMemos.AsNoTracking().FirstOrDefault(x => x.Code == itemMemo.DebitMemoCode);
                                    if (memoData != null || memoData.Mark != "V")
                                    {
                                        journals.Add(new Journal
                                        {
                                            Code = itemData.RcvHeader.Code,
                                            LineNo = ++l,
                                            Date = itemData.RcvHeader.Date,
                                            CoaCode = systemParam.FirstOrDefault(x => x.Code == "DM_AR_COA")?.Value ?? "",
                                            TypeCode = "DM_AP",
                                            Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_DM_AR")?.Value ?? ""} {itemData.Supplier.Initial}").Trim(),
                                            RefCode1 = itemMemo.DebitMemoCode,
                                            Group = 6,
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

                var dmValue = journals.Where(x => x.Code == itemData.RcvHeader.Code && x.Group == 5).Sum(x => x.Amount);
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
                    Group = 4,
                    CurrCode = itemData.RcvHeader.CurrCode,
                    Period = itemData.RcvHeader.Date.ToString("yyyyMMdd"),
                    Type = "C",
                    Amount = apValue,
                    SrcTrans = "RCV"
                });
            }
        }

        return journals;
    }

    private IEnumerable<Journal> ProcessPurchaseInvJournal(TenantContext db, DateTime dateTime, List<SystemParameter> systemParam, List<Item> items, List<Tax> taxes)
    {
        List<Journal> journals = new();
        var apRecog = systemParam.FirstOrDefault(x => x.Code == "AP_RECOG_TIME").Value;

        if (apRecog == "PI")
        {
            var InvData = (from invheader in db.PurchaseInvoiceHeaders
                           join supplier in db.Suppliers on invheader.SupCode equals supplier.Code
                           where invheader.Date.Month == dateTime.Month && invheader.Date.Year == dateTime.Year && invheader.Mark != "V"
                           select new { InvHeader = invheader, Supplier = supplier }).ToList();

            foreach (var itemData in InvData)
            {
                var InvDetailData = (from invdetail in db.PurchaseInvoiceDetails
                                     join rcvdata in db.PurchaseReceiveHeaders on invdetail.RcvCode equals rcvdata.Code
                                     where invdetail.Code == itemData.InvHeader.Code
                                     select new { InvDetail = invdetail, RcvData = rcvdata }).ToList();

                decimal apAmount = 0m;
                short i = 0;

                foreach (var itemDetail in InvDetailData)
                {
                    //Hutang belum difakturkan
                    journals.Add(new Journal
                    {
                        Code = itemData.InvHeader.Code,
                        LineNo = ++i,
                        Date = itemData.InvHeader.Date,
                        CoaCode = systemParam.FirstOrDefault(x => x.Code == "AP_DFR_COA")?.Value ?? "",
                        TypeCode = "AP_DFR",
                        Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "AP_DFR_COA")?.Description ?? ""} {itemData.Supplier.Initial}").Trim(),
                        RefCode1 = itemDetail.InvDetail.RcvCode,
                        RefCode2 = itemDetail.RcvData.TransCode,
                        Group = 1,
                        CurrCode = itemData.InvHeader.CurrCode,
                        Period = itemData.InvHeader.Date.ToString("yyyyMMdd"),
                        Type = "D",
                        Amount = itemDetail.RcvData.Total - itemDetail.RcvData.TaxAmount + itemDetail.RcvData.ExemptTaxAmount,
                        SrcTrans = "PI"
                    });

                    //HUTANG
                    apAmount += itemDetail.RcvData.Total;

                    //PPN
                    journals.Add(new Journal
                    {
                        Code = itemData.InvHeader.Code,
                        LineNo = i,
                        Date = itemData.InvHeader.Date,
                        CoaCode = systemParam.FirstOrDefault(x => x.Code == "TAX_IN_COA")?.Value ?? "",
                        TypeCode = "PPN",
                        Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_TAX_IN")?.Value ?? ""} {itemData.Supplier.Initial}").Trim(),
                        RefCode1 = itemData.InvHeader.PoCode,
                        Group = 2,
                        CurrCode = itemData.InvHeader.CurrCode,
                        Period = itemData.InvHeader.Date.ToString("yyyyMMdd"),
                        Type = "D",
                        Amount = itemDetail.RcvData.TaxAmount,
                        SrcTrans = "PI"
                    });

                    //PPN Yang Dibebaskan
                    var RcvDetailData = (from rcvdetail in db.PurchaseReceiveDetails
                                         join item in db.Items on rcvdetail.ItemId equals item.Id
                                         where rcvdetail.Code == itemDetail.RcvData.Code
                                         select new { RcvDetail = rcvdetail, Item = item }).ToList();

                    short ix = 0;
                    foreach (var itemRcvDetail in RcvDetailData)
                    {
                        if (itemRcvDetail.RcvDetail.ExemptTaxAmount > 0)
                        {
                            //PPN Yang Dibebaskan
                            journals.Add(new Journal
                            {
                                Code = itemData.InvHeader.Code,
                                LineNo = ++ix,
                                Date = itemData.InvHeader.Date,
                                CoaCode = string.IsNullOrWhiteSpace(taxes.FirstOrDefault(x => x.Id == itemRcvDetail.RcvDetail.TaxId)?.ExemptCoaCode) ? "" : taxes.FirstOrDefault(x => x.Id == itemRcvDetail.RcvDetail.TaxId)?.ExemptCoaCode,
                                TypeCode = "EPPN",
                                Notes = "PPN Yang Dibebaskan",
                                RefCode1 = itemData.InvHeader.PoCode,
                                //RefCode2 = itemRcvDetail.Item.Initial,
                                Group = 3,
                                CurrCode = itemData.InvHeader.CurrCode,
                                Period = itemData.InvHeader.Date.ToString("yyyyMMdd"),
                                Type = "D",
                                Amount = itemRcvDetail.RcvDetail.ExemptTaxAmount * itemRcvDetail.RcvDetail.Qty,
                                SrcTrans = "PI"
                            });

                            journals.Add(new Journal
                            {
                                Code = itemData.InvHeader.Code,
                                LineNo = ix,
                                Date = itemData.InvHeader.Date,
                                CoaCode = systemParam.FirstOrDefault(x => x.Code == "TAX_IN_COA")?.Value ?? "",
                                TypeCode = "PPN",
                                Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_TAX_IN")?.Value ?? ""} {itemData.Supplier.Initial}").Trim(),
                                RefCode1 = itemData.InvHeader.PoCode,
                                Group = 2,
                                CurrCode = itemData.InvHeader.CurrCode,
                                Period = itemData.InvHeader.Date.ToString("yyyyMMdd"),
                                Type = "C",
                                Amount = itemRcvDetail.RcvDetail.ExemptTaxAmount * itemRcvDetail.RcvDetail.Qty,
                                SrcTrans = "PI"
                            });

                            //PPN Yang Dibebaskan
                            journals.Add(new Journal
                            {
                                Code = itemData.InvHeader.Code,
                                LineNo = ix,
                                Date = itemData.InvHeader.Date,
                                CoaCode = string.IsNullOrWhiteSpace(taxes.FirstOrDefault(x => x.Id == itemRcvDetail.RcvDetail.TaxId)?.ExemptCoaCode) ? "" : taxes.FirstOrDefault(x => x.Id == itemRcvDetail.RcvDetail.TaxId)?.ExemptCoaCode,
                                TypeCode = "EPPN",
                                Notes = "PPN Yang Dibebaskan",
                                RefCode1 = itemData.InvHeader.PoCode,
                                //RefCode2 = itemRcvDetail.Item.Initial,
                                Group = 5,
                                CurrCode = itemData.InvHeader.CurrCode,
                                Period = itemData.InvHeader.Date.ToString("yyyyMMdd"),
                                Type = "C",
                                Amount = itemRcvDetail.RcvDetail.ExemptTaxAmount * itemRcvDetail.RcvDetail.Qty,
                                SrcTrans = "PI"
                            });
                        }
                    }
                }

                var invMemo = db.PurchaseInvoiceDebitMemos.Where(x => x.InvCode == itemData.InvHeader.Code).ToList();
                if (invMemo.Any())
                {
                    short k = 0;
                    foreach (var itemMemo in invMemo)
                    {
                        var memoData = db.DebitMemos.FirstOrDefault(x => x.Code == itemMemo.DebitMemoCode);
                        var bbmemoData = db.BeginningBalanceDebitMemos.FirstOrDefault(x => x.Code == itemMemo.DebitMemoCode);
                        if (memoData != null)
                        {
                            journals.Add(new Journal
                            {
                                Code = itemData.InvHeader.Code + "-DM",
                                LineNo = ++k,
                                Date = itemData.InvHeader.Date,
                                CoaCode = memoData.SrcTrans == 1 ? systemParam.FirstOrDefault(x => x.Code == "DEP_SUP_COA")?.Value ?? "" : systemParam.FirstOrDefault(x => x.Code == "DM_AR_COA")?.Value ?? "",
                                TypeCode = "DM_AR",
                                Notes = ($"{(memoData.SrcTrans == 1 ? systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_DEP_SUP")?.Value ?? "" : systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_DM_AR")?.Value ?? "")} {itemData.Supplier.Initial}").Trim(),
                                RefCode1 = itemData.InvHeader.Code,
                                RefCode2 = itemData.InvHeader.PoCode,
                                RefCode3 = itemMemo.DebitMemoCode,
                                Group = 2,
                                CurrCode = itemData.InvHeader.CurrCode,
                                Period = itemData.InvHeader.Date.ToString("yyyyMMdd"),
                                Type = "C",
                                Amount = itemMemo.DebitMemoAmount,
                                SrcTrans = "PI"
                            });

                            journals.Add(new Journal
                            {
                                Code = itemData.InvHeader.Code + "-DM",
                                LineNo = k,
                                Date = itemData.InvHeader.Date,
                                CoaCode = systemParam.FirstOrDefault(x => x.Code == "AP_COA")?.Value ?? "",
                                TypeCode = "AP",
                                Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_AP")?.Value ?? ""} {itemData.Supplier.Initial}").Trim(),
                                RefCode1 = itemMemo.DebitMemoCode,
                                RefCode2 = itemData.InvHeader.Code,
                                RefCode3 = itemData.InvHeader.PoCode,
                                Group = 1,
                                CurrCode = itemData.InvHeader.CurrCode,
                                Period = itemData.InvHeader.Date.ToString("yyyyMMdd"),
                                Type = "D",
                                Amount = itemMemo.DebitMemoAmount,
                                SrcTrans = "PI"
                            });
                        }
                        else if (bbmemoData != null)
                        {
                            journals.Add(new Journal
                            {
                                Code = itemData.InvHeader.Code + "-DM",
                                LineNo = ++k,
                                Date = itemData.InvHeader.Date,
                                CoaCode = bbmemoData.Type == 1 ? systemParam.FirstOrDefault(x => x.Code == "DEP_SUP_COA")?.Value ?? "" : systemParam.FirstOrDefault(x => x.Code == "DM_AR_COA")?.Value ?? "",
                                TypeCode = "DM_AR",
                                Notes = ($"{(bbmemoData.Type == 1 ? systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_DEP_SUP")?.Value ?? "" : systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_DM_AR")?.Value ?? "")} {itemData.Supplier.Initial}").Trim(),
                                RefCode1 = itemData.InvHeader.Code,
                                RefCode2 = itemData.InvHeader.PoCode,
                                RefCode3 = itemMemo.DebitMemoCode,
                                Group = 2,
                                CurrCode = itemData.InvHeader.CurrCode,
                                Period = itemData.InvHeader.Date.ToString("yyyyMMdd"),
                                Type = "C",
                                Amount = itemMemo.DebitMemoAmount,
                                SrcTrans = "PI"
                            });

                            journals.Add(new Journal
                            {
                                Code = itemData.InvHeader.Code + "-DM",
                                LineNo = k,
                                Date = itemData.InvHeader.Date,
                                CoaCode = systemParam.FirstOrDefault(x => x.Code == "AP_COA")?.Value ?? "",
                                TypeCode = "AP",
                                Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_AP")?.Value ?? ""} {itemData.Supplier.Initial}").Trim(),
                                RefCode1 = itemMemo.DebitMemoCode,
                                RefCode2 = itemData.InvHeader.Code,
                                RefCode3 = itemData.InvHeader.PoCode,
                                Group = 1,
                                CurrCode = itemData.InvHeader.CurrCode,
                                Period = itemData.InvHeader.Date.ToString("yyyyMMdd"),
                                Type = "D",
                                Amount = itemMemo.DebitMemoAmount,
                                SrcTrans = "PI"
                            });
                        }
                    }
                }

                //Hutang - AP
                journals.Add(new Journal
                {
                    Code = itemData.InvHeader.Code,
                    LineNo = 1,
                    Date = itemData.InvHeader.Date,
                    CoaCode = systemParam.FirstOrDefault(x => x.Code == "AP_COA")?.Value ?? "",
                    TypeCode = "AP",
                    Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_AP")?.Value ?? ""} {itemData.Supplier.Initial}").Trim(),
                    RefCode1 = itemData.InvHeader.PoCode,
                    Group = 4,
                    CurrCode = itemData.InvHeader.CurrCode,
                    Period = itemData.InvHeader.Date.ToString("yyyyMMdd"),
                    Type = "C",
                    Amount = apAmount,
                    SrcTrans = "PI"
                });
            }
        }

        return journals;
    }

    private async Task<IEnumerable<Journal>> ProcessSaleDlvJournalAsync(TenantContext db, DateTime date,
        List<SystemParameter> systemParam, List<Item> items, List<Tax> taxes, CancellationToken cancellationToken)
    {
        List<Journal> journals = new();
        var arRecog = systemParam.FirstOrDefault(x => x.Code == "AR_RECOG_TIME")?.Value;

        _logger.LogInformation("Posting journal DO: get do data.");
        var doData =
            await (from dlvheader in db.SalesDeliveryHeaders
            join customer in db.Customers on dlvheader.CustCode equals customer.Code
            where dlvheader.Date.Month == date.Month && dlvheader.Date.Year == date.Year && dlvheader.SrcTrans == 1 &&
                  !new[] { "OL", "V" }.Contains(dlvheader.Mark)
            select new
            {
               Dlvheader = dlvheader, Customer = customer
            }).AsNoTracking().ToListAsync(cancellationToken);

        _logger.LogInformation("Posting journal DO: get do detail data.");
        var doDetailData =
            await (from dlvdetail in db.SalesDeliveryDetails
            join item in db.Items on dlvdetail.ItemId equals item.Id
            where doData.Select(i => i.Dlvheader.Code).Contains(dlvdetail.Code)
            select new
            {
                DlvDetail = dlvdetail, Item = item
            }).AsNoTracking().ToListAsync(cancellationToken);
        
        _logger.LogInformation("Posting journal DO: get do detail free data.");
        var doDetailFreeData =
            await (from fg in db.SalesDeliveryDetailFreeGoods
            join item in db.Items on fg.ItemId equals item.Id
            where doData.Select(i => i.Dlvheader.Code).Contains(fg.Code)
            select new
            {
                DlvDetail = fg, Item = item
            }).AsNoTracking().ToListAsync(cancellationToken);
        
        _logger.LogInformation("Posting journal DO: get stock mutation data.");
        var smData = await db.StockMutations.FromSql(@$"
            WITH cte_do_h_src AS (
                SELECT *
                FROM Sales.SalesDeliveryHeader
                WHERE Mark NOT IN ('OL', 'V')
                AND MONTH([Date]) = {date.Month}
                AND YEAR([Date]) = {date.Year}
                AND SrcTrans = 1
            ), cte_do_d_src AS (
                SELECT do_d.Id, do_d.Code, 'DO' AS Src
                FROM Sales.SalesDeliveryDetail do_d
                INNER JOIN cte_do_h_src cte_do_h
                    ON cte_do_h.Code = do_d.Code
                UNION ALL
                SELECT do_d_fg.Id, do_d_fg.Code, 'DOF' AS Src
                FROM Sales.SalesDeliveryDetailFreeGood do_d_fg
                INNER JOIN cte_do_h_src cte_do_h
                    ON cte_do_h.Code = do_d_fg.Code
            )
            SELECT *
            FROM Inventory.StockMutation sm
            WHERE sm.[Type] = 'OH'
            AND sm.Src IN ('DO', 'DOF')
            AND EXISTS (
                SELECT 1
                FROM cte_do_d_src cte_do_d
                WHERE cte_do_d.Id = sm.RefDetailId1
                AND cte_do_d.Code = sm.RefCode1
                AND cte_do_d.Src = sm.Src
            )").AsNoTracking().ToListAsync(cancellationToken);

        // var smData =
        //     await db.StockMutations.AsNoTracking()
        //         .Where(x => doDetailData.Select(d => d.DlvDetail.Id.ToString() + '~' + d.DlvDetail.Code).ToList().Contains(x.RefDetailId1.ToString() + '~' + x.RefCode1) &&
        //                     x.Type == "OH" && x.Src == "DO")
        //         .ToListAsync(cancellationToken);
        
        // _logger.LogInformation("Posting journal DO: get stock mutation free data.");
        // var smFreeData =
        //     await db.StockMutations.AsNoTracking()
        //         .Where(x => doDetailData.Select(d => d.DlvDetail.Id.ToString() + '~' + d.DlvDetail.Code).ToList().Contains(x.RefDetailId1.ToString() + '~' + x.RefCode1) &&
        //                     x.Type == "OH" && x.Src == "DOF")
        //         .ToListAsync(cancellationToken);

        if (arRecog == "SI")
        {
            foreach (var itemData in doData)
            {
                short Ninv = 0;
                short Nakun = 0;
                
                foreach (var itemDetail in doDetailData.Where(x => x.DlvDetail.Code == itemData.Dlvheader.Code))
                {
                    _logger.LogInformation($"Posting journal DO: {itemData.Dlvheader.Code} - {itemDetail.Item.Initial} ({itemDetail.Item.Id}).");
                    var smItemData = smData.FirstOrDefault(x => x.RefDetailId1 == itemDetail.DlvDetail.Id && x.RefCode1 == itemDetail.DlvDetail.Code && x.Src == "DO");
                    if (smItemData == null) continue;
                    //var nonVoidSM = RemoveVoidSM(db, db.StockMutations.ToList());
                    //CalculateHPP(db, nonVoidSM, smData.ItemId, smData.RefDetailId1, "DO");
                    //smData = db.StockMutations.FirstOrDefault(x => x.RefDetailId1 == itemDetail.DlvDetail.Id && x.RefCode1 == itemDetail.DlvDetail.Code);
                    var resultHpp = smItemData.BaseNettPrice * smItemData.BaseQty;

                    //Inventory using COGS for Amount
                    journals.Add(new Journal
                    {
                        Code = itemData.Dlvheader.Code,
                        LineNo = ++Ninv,
                        Date = itemData.Dlvheader.Date,
                        CoaCode = string.IsNullOrWhiteSpace(items.FirstOrDefault(x => x.Id == itemDetail.DlvDetail.ItemId)?.CoaInventory) ? systemParam.FirstOrDefault(x => x.Code == "INVENTORY_COA")?.Value ?? "" : items.FirstOrDefault(x => x.Id == itemDetail.DlvDetail.ItemId)?.CoaInventory,
                        TypeCode = "DLV_DT",
                        Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_INVENTORY")?.Value ?? ""}").Trim(),
                        RefCode1 = itemData.Dlvheader.TransCode,
                        //RefCode2 = itemDetail.Item.Initial,
                        Group = 1,
                        CurrCode = itemData.Dlvheader.CurrCode,
                        Period = itemData.Dlvheader.Date.ToString("yyyyMMdd"),
                        Type = "C",
                        Amount = resultHpp,
                        SrcTrans = "DLV"
                    });

                    if (!itemData.Dlvheader.FromDirectInvoice)
                    {
                        //Barang Terkirim
                        journals.Add(new Journal
                        {
                            Code = itemData.Dlvheader.Code,
                            LineNo = Ninv,
                            Date = itemData.Dlvheader.Date,
                            CoaCode = systemParam.FirstOrDefault(x => x.Code == "SENT_ITEM_COA")?.Value ?? "",
                            TypeCode = "DLV_DT",
                            Notes = ($"Barang Terkirim {itemDetail.Item.Initial}").Trim(),
                            RefCode1 = itemData.Dlvheader.TransCode,
                            RefCode2 = itemDetail.Item.Initial,
                            Group = 2,
                            CurrCode = itemData.Dlvheader.CurrCode,
                            Period = itemData.Dlvheader.Date.ToString("yyyyMMdd"),
                            Type = "D",
                            Amount = resultHpp,
                            SrcTrans = "DLV"
                        });
                    }
                }

                //Promo Free Item
                foreach (var itemFreeDetail in doDetailFreeData.Where(x => x.DlvDetail.Code == itemData.Dlvheader.Code))
                {
                    _logger.LogInformation($"Posting journal DO: {itemData.Dlvheader.Code} - {itemFreeDetail.Item.Initial} ({itemFreeDetail.Item.Id}) free.");
                    var smItemData = smData.FirstOrDefault(x => x.RefDetailId1 == itemFreeDetail.DlvDetail.Id && x.RefCode1 == itemFreeDetail.DlvDetail.Code && x.Src == "DOF");
                    if (smItemData == null) continue;
                    //var nonVoidSM = RemoveVoidSM(db, db.StockMutations.ToList());
                    //CalculateHPP(db, nonVoidSM, smData.ItemId, smData.RefDetailId1, "DOF");
                    //smData = db.StockMutations.FirstOrDefault(x => x.RefDetailId1 == itemFreeDetail.DlvDetail.Id && x.RefCode1 == itemFreeDetail.DlvDetail.Code);
                    var resultHpp = smItemData.BaseNettPrice * smItemData.BaseQty;

                    journals.Add(new Journal
                    {
                        Code = itemData.Dlvheader.Code,
                        LineNo = ++Nakun,
                        Date = itemData.Dlvheader.Date,
                        CoaCode = string.IsNullOrWhiteSpace(items.FirstOrDefault(x => x.Id == itemFreeDetail.DlvDetail.ItemId)?.CoaInventory) ? systemParam.FirstOrDefault(x => x.Code == "INVENTORY_COA")?.Value ?? "" : items.FirstOrDefault(x => x.Id == itemFreeDetail.DlvDetail.ItemId)?.CoaInventory,
                        TypeCode = "DLV_DT",
                        Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_INVENTORY")?.Value ?? ""}").Trim(),
                        RefCode1 = itemData.Dlvheader.TransCode,
                        //RefCode2 = itemFreeDetail.Item.Initial,
                        Group = 1,
                        CurrCode = itemData.Dlvheader.CurrCode,
                        Period = itemData.Dlvheader.Date.ToString("yyyyMMdd"),
                        Type = "C",
                        Amount = resultHpp,
                        SrcTrans = "DLV"
                    });

                    if (!itemData.Dlvheader.FromDirectInvoice)
                    {
                        //Barang Terkirim
                        journals.Add(new Journal
                        {
                            Code = itemData.Dlvheader.Code,
                            LineNo = Nakun,
                            Date = itemData.Dlvheader.Date,
                            CoaCode = systemParam.FirstOrDefault(x => x.Code == "SENT_ITEM_COA")?.Value ?? "",
                            TypeCode = "DLV_DT",
                            Notes = ($"Barang Terkirim {itemFreeDetail.Item.Initial}").Trim(),
                            RefCode1 = itemData.Dlvheader.TransCode,
                            RefCode2 = itemFreeDetail.Item.Initial,
                            Group = 2,
                            CurrCode = itemData.Dlvheader.CurrCode,
                            Period = itemData.Dlvheader.Date.ToString("yyyyMMdd"),
                            Type = "D",
                            Amount = resultHpp,
                            SrcTrans = "DLV"
                        });
                    }
                }
            }
        }
        else
        {
            _logger.LogInformation("Posting journal DO: get invoice detail data.");
            var invDetData =
                await (from d in db.SalesInvoiceDetails
                    join h in db.SalesInvoiceHeaders on d.Code equals h.Code
                    join cm in db.SalesInvoiceCreditMemos on d.Code equals cm.InvCode
                    where doData.Select(i => i.Dlvheader.Code).Contains(d.DoCode) &&
                          h.Mark != "V"
                    select new
                    {
                        InvCode = h.Code, InvTotal = h.Total, DoCode = d.DoCode,
                        cm.CreditMemoCode, cm.CreditMemoAmount
                    }).AsNoTracking().ToListAsync(cancellationToken);
            
            foreach (var itemData in doData)
            {
                var discAmount = 0m;
                var taxAmount = 0m;
                short Ninv = 0;
                short Nfree = 0;
                short Nakun = 0;
                short Nhpp = 0;
                short Next = 0;
                
                foreach (var itemDetail in doDetailData.Where(x => x.DlvDetail.Code == itemData.Dlvheader.Code))
                {
                    _logger.LogInformation($"Posting journal DO: {itemData.Dlvheader.Code} - {itemDetail.Item.Initial} ({itemDetail.Item.Id}).");
                    var smItemData = smData.FirstOrDefault(x => x.RefDetailId1 == itemDetail.DlvDetail.Id && x.RefCode1 == itemDetail.DlvDetail.Code && x.Src == "DO");
                    if (smItemData == null) continue;
                    //var nonVoidSM = RemoveVoidSM(db, db.StockMutations.ToList());
                    //CalculateHPP(db, nonVoidSM, smData.ItemId, smData.RefDetailId1, "DO");
                    //smData = db.StockMutations.FirstOrDefault(x => x.RefDetailId1 == itemDetail.DlvDetail.Id && x.RefCode1 == itemDetail.DlvDetail.Code);
                    var resultHpp = smItemData.BaseNettPrice * smItemData.BaseQty;

                    //Discount - Diskon
                    discAmount += (itemDetail.DlvDetail.Disc > 0 ? itemDetail.DlvDetail.Disc * itemDetail.DlvDetail.Qty : 0m) + (itemDetail.DlvDetail.FinalDiscHeader > 0 ? itemDetail.DlvDetail.FinalDiscHeader * itemDetail.DlvDetail.Qty : 0m);

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
                        taxAmount += itemDetail.DlvDetail.TaxAmount * itemDetail.DlvDetail.Qty;
                    //Pajak Yang Dibebaskan
                    if (itemDetail.DlvDetail.ExemptTaxAmount > 0)
                    {
                        taxAmount -= itemDetail.DlvDetail.ExemptTaxAmount * itemDetail.DlvDetail.Qty;
                        journals.Add(new Journal
                        {
                            Code = itemData.Dlvheader.Code,
                            LineNo = ++Next,
                            Date = itemData.Dlvheader.Date,
                            CoaCode = string.IsNullOrWhiteSpace(taxes.FirstOrDefault(x => x.Id == itemDetail.DlvDetail.TaxId)?.ExemptCoaCode) ? "" : taxes.FirstOrDefault(x => x.Id == itemDetail.DlvDetail.TaxId)?.ExemptCoaCode,
                            TypeCode = "DLV_DT",
                            Notes = "PPN Yang Dibebaskan",
                            RefCode1 = itemData.Dlvheader.TransCode,
                            RefCode2 = itemDetail.Item.Initial,
                            Group = 10,
                            CurrCode = itemData.Dlvheader.CurrCode,
                            Period = itemData.Dlvheader.Date.ToString("yyyyMMdd"),
                            Type = "C",
                            Amount = itemDetail.DlvDetail.ExemptTaxAmount * itemDetail.DlvDetail.Qty,
                            SrcTrans = "DLV"
                        });

                        journals.Add(new Journal
                        {
                            Code = itemData.Dlvheader.Code,
                            LineNo = Next,
                            Date = itemData.Dlvheader.Date,
                            CoaCode = string.IsNullOrWhiteSpace(taxes.FirstOrDefault(x => x.Id == itemDetail.DlvDetail.TaxId)?.ExemptCoaCode) ? "" : taxes.FirstOrDefault(x => x.Id == itemDetail.DlvDetail.TaxId)?.ExemptCoaCode,
                            TypeCode = "DLV_DT",
                            Notes = "PPN Yang Dibebaskan",
                            RefCode1 = itemData.Dlvheader.TransCode,
                            RefCode2 = itemDetail.Item.Initial,
                            Group = 6,
                            CurrCode = itemData.Dlvheader.CurrCode,
                            Period = itemData.Dlvheader.Date.ToString("yyyyMMdd"),
                            Type = "D",
                            Amount = itemDetail.DlvDetail.ExemptTaxAmount * itemDetail.DlvDetail.Qty,
                            SrcTrans = "DLV"
                        });
                    }
                }

                //Discount - Diskon
                if (discAmount > 0)
                {
                    journals.Add(new Journal
                    {
                        Code = itemData.Dlvheader.Code,
                        LineNo = 1,
                        Date = itemData.Dlvheader.Date,
                        CoaCode = systemParam.FirstOrDefault(x => x.Code == "SLS_DISC_COA")?.Value ?? "",
                        TypeCode = "SLS_DISC",
                        Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_SLS_DISC")?.Value ?? ""} {itemData.Customer.Initial}").Trim(),
                        RefCode1 = itemData.Dlvheader.TransCode,
                        RefCode2 = "",
                        Group = 3,
                        CurrCode = itemData.Dlvheader.CurrCode,
                        Period = itemData.Dlvheader.Date.ToString("yyyyMMdd"),
                        Type = "D",
                        Amount = discAmount,
                        SrcTrans = "DLV"
                    });
                }

                //PPN - Pajak
                if (taxAmount > 0)
                {
                    journals.Add(new Journal
                    {
                        Code = itemData.Dlvheader.Code,
                        LineNo = 1,
                        Date = itemData.Dlvheader.Date,
                        CoaCode = systemParam.FirstOrDefault(x => x.Code == "TAX_OUT_COA")?.Value ?? "",
                        TypeCode = "PPN",
                        Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_TAX_OUT")?.Value ?? ""} {itemData.Customer.Initial}").Trim(),
                        RefCode1 = itemData.Dlvheader.TransCode,
                        RefCode2 = "",
                        Group = 5,
                        CurrCode = itemData.Dlvheader.CurrCode,
                        Period = itemData.Dlvheader.Date.ToString("yyyyMMdd"),
                        Type = "C",
                        Amount = taxAmount,
                        SrcTrans = "DLV"
                    });
                }

                //Pajak Yang Dibebaskan
                //if (extTaxAmount > 0)
                //{
                //    journals.Add(new Journal
                //    {
                //        Code = itemData.Dlvheader.Code,
                //        LineNo = 1,
                //        Date = itemData.Dlvheader.Date,
                //        CoaCode = systemParam.FirstOrDefault(x => x.Code == "TAX_IN_COA")?.Value ?? "",
                //        TypeCode = "EPPN",
                //        Notes = ($"Pajak Yang Dibebaskan {itemData.Customer.Initial}").Trim(),
                //        RefCode1 = itemData.Dlvheader.TransCode,
                //        RefCode2 = "",
                //        Group = 6,
                //        CurrCode = itemData.Dlvheader.CurrCode,
                //        Period = itemData.Dlvheader.Date.ToString("yyyyMMdd"),
                //        Type = "D",
                //        Amount = extTaxAmount,
                //        SrcTrans = "DLV"
                //    });
                //}

                foreach (var item in invDetData.Where(x => x.DoCode == itemData.Dlvheader.Code).DistinctBy(x => new { x.InvCode, x.InvTotal, x.DoCode }))
                {
                    short k = 0;
                    foreach (var itemMemo in invDetData.Where(x => x.InvCode == item.InvCode))
                    {
                        journals.Add(new Journal
                        {
                            Code = itemData.Dlvheader.Code,
                            LineNo = ++k,
                            Date = itemData.Dlvheader.Date,
                            CoaCode = systemParam.FirstOrDefault(x => x.Code == "DEP_CUST_COA")?.Value ?? "",
                            TypeCode = "CM_AP",
                            Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_DEP_CUST")?.Value ?? ""} {itemData.Customer.Initial}").Trim(),
                            RefCode1 = itemMemo.CreditMemoCode,
                            Group = 11,
                            CurrCode = itemData.Dlvheader.CurrCode,
                            Period = itemData.Dlvheader.Date.ToString("yyyyMMdd"),
                            Type = "D",
                            Amount = (itemMemo.CreditMemoAmount * itemData.Dlvheader.Total) / item.InvTotal,
                            SrcTrans = "SI"
                        });
                    }
                }

                var arValue = itemData.Dlvheader.Total;
                var cmValue = journals.Where(x => x.Code == itemData.Dlvheader.Code && x.Group == 10).Sum(x => x.Amount);
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
                    Group = 7,
                    CurrCode = itemData.Dlvheader.CurrCode,
                    Period = itemData.Dlvheader.Date.ToString("yyyyMMdd"),
                    Type = "C",
                    Amount = itemData.Dlvheader.Total - journals.Where(x => x.Code == itemData.Dlvheader.Code && new[] { 4, 5, 6 }.Contains(x.Group)).Sum(x => x.Amount) + journals.Where(x => x.Code == itemData.Dlvheader.Code && new[] { 2, 3 }.Contains(x.Group)).Sum(x => x.Amount),
                    SrcTrans = "DLV"
                });


                //Promo Free Item
                foreach (var itemFreeDetail in doDetailFreeData.Where(x => x.DlvDetail.Code == itemData.Dlvheader.Code))
                {
                    _logger.LogInformation($"Posting journal DO: {itemData.Dlvheader.Code} - {itemFreeDetail.Item.Initial} ({itemFreeDetail.Item.Id}) free.");
                    var smItemData = smData.FirstOrDefault(x => x.RefDetailId1 == itemFreeDetail.DlvDetail.Id && x.RefCode1 == itemFreeDetail.DlvDetail.Code && x.Src == "DOF");
                    if (smItemData == null) continue;
                    //var nonVoidSM = RemoveVoidSM(db, db.StockMutations.ToList());
                    //CalculateHPP(db, nonVoidSM, smData.ItemId, smData.RefDetailId1, "DOF");
                    //smData = db.StockMutations.FirstOrDefault(x => x.RefDetailId1 == itemFreeDetail.DlvDetail.Id && x.RefCode1 == itemFreeDetail.DlvDetail.Code);
                    var resultHpp = smItemData.BaseNettPrice * smItemData.BaseQty;

                    journals.Add(new Journal
                    {
                        Code = itemData.Dlvheader.Code,
                        LineNo = ++Nakun,
                        Date = itemData.Dlvheader.Date,
                        CoaCode = itemFreeDetail.DlvDetail.CoaCode ?? "",
                        TypeCode = "COST",
                        Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_PROMO_COST")?.Value ?? ""} {itemFreeDetail.Item.Initial}").Trim(),
                        RefCode1 = itemData.Dlvheader.TransCode,
                        Group = 8,
                        CurrCode = itemData.Dlvheader.CurrCode,
                        Period = itemData.Dlvheader.Date.ToString("yyyyMMdd"),
                        Type = "D",
                        Amount = resultHpp,
                        SrcTrans = "DLV"
                    });

                    journals.Add(new Journal
                    {
                        Code = itemData.Dlvheader.Code,
                        LineNo = ++Nfree,
                        Date = itemData.Dlvheader.Date,
                        CoaCode = string.IsNullOrWhiteSpace(items.FirstOrDefault(x => x.Id == itemFreeDetail.DlvDetail.ItemId)?.CoaInventory) ? systemParam.FirstOrDefault(x => x.Code == "INVENTORY_COA")?.Value ?? "" : items.FirstOrDefault(x => x.Id == itemFreeDetail.DlvDetail.ItemId)?.CoaInventory,
                        TypeCode = "DLV_DT",
                        Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_INVENTORY")?.Value ?? ""} {itemFreeDetail.Item.Initial}").Trim(),
                        RefCode1 = itemData.Dlvheader.TransCode,
                        RefCode2 = itemFreeDetail.Item.Initial,
                        Group = 9,
                        CurrCode = itemData.Dlvheader.CurrCode,
                        Period = itemData.Dlvheader.Date.ToString("yyyyMMdd"),
                        Type = "C",
                        Amount = resultHpp,
                        SrcTrans = "DLV"
                    });
                }
            }
        }

        return journals;
    }

    private async Task<IEnumerable<Journal>> ProcessSaleInvJournalAsync(TenantContext db, DateTime date,
        List<SystemParameter> systemParam, List<Tax> taxes, CancellationToken cancellationToken)
    {
        List<Journal> journals = new();
        var arRecog = systemParam.FirstOrDefault(x => x.Code == "AR_RECOG_TIME")?.Value;

        if (arRecog == "SI")
        {
            _logger.LogInformation("Posting journal SI: get invoice data.");
            var invData =
                await (from invheader in db.SalesInvoiceHeaders
                join customer in db.Customers on invheader.CustCode equals customer.Code
                where invheader.Date.Month == date.Month && invheader.Date.Year == date.Year &&
                      !new[] {"OL", "V"}.Contains(invheader.Mark)
                select new
                {
                    InvHeader = invheader, Customer = customer
                }).AsNoTracking().ToListAsync(cancellationToken);

            _logger.LogInformation("Posting journal SI: get invoice detail data.");
            var invDetailData =
                await (from invdetail in db.SalesInvoiceDetails
                join dodata in db.SalesDeliveryHeaders on invdetail.DoCode equals dodata.Code
                where invData.Select(i => i.InvHeader.Code).Contains(invdetail.Code)
                select new
                {
                    InvDetail = invdetail, DoData = dodata
                }).AsNoTracking().ToListAsync(cancellationToken);
            
            _logger.LogInformation("Posting journal SI: get do data.");
            var dlvDetailData =
                await (from dlvdetail in db.SalesDeliveryDetails
                join item in db.Items on dlvdetail.ItemId equals item.Id
                where invDetailData.Select(i => i.DoData.Code).Contains(dlvdetail.Code)
                select new
                {
                    DlvDetail = dlvdetail, Item = item
                }).AsNoTracking().ToListAsync(cancellationToken);
            
            _logger.LogInformation("Posting journal SI: get do free data.");
            var dlvDetailFreeData =
                await (from dlvdetail in db.SalesDeliveryDetailFreeGoods
                join item in db.Items on dlvdetail.ItemId equals item.Id
                where invDetailData.Select(i => i.DoData.Code).Contains(dlvdetail.Code)
                select new
                {
                    DlvDetail = dlvdetail, Item = item
                }).AsNoTracking().ToListAsync(cancellationToken);
            
            _logger.LogInformation("Posting journal SI: get stock mutation data.");
            var smData =
                await db.StockMutations.AsNoTracking()
                    .Where(x => dlvDetailFreeData.Select(dof => dof.DlvDetail.Id.ToString() + '~' + dof.DlvDetail.Code).ToList().Contains(x.RefDetailId1.ToString() + '~' + x.RefCode1) &&
                                x.Type == "OH" && x.Src == "DOF")
                    .ToListAsync(cancellationToken);

            // var sdpData =
            //     await db.SalesInvoiceCreditMemos.AsNoTracking()
            //         .Where(x => invData.Select(i => i.InvHeader.Code).Contains(x.InvCode) && x.Src == "DP")
            //         .ToListAsync(cancellationToken);

            _logger.LogInformation("Posting journal SI: get invoice memo data.");
            var invMemo =
                await db.SalesInvoiceCreditMemos.AsNoTracking()
                    .Where(x => invData.Select(i => i.InvHeader.Code).Contains(x.InvCode) &&
                                new[] { "DP", "CM" }.Contains(x.Src))
                    .ToListAsync(cancellationToken);
            
            _logger.LogInformation("Posting journal SI: get memo data.");
            var invMemoCm = invMemo.Where(im => im.Src == "CM").ToList();
            var memoData =
                await db.CreditMemos.AsNoTracking()
                    .Where(x => invMemoCm.Select(im => im.CreditMemoCode).Contains(x.Code))
                    .ToListAsync(cancellationToken);

            _logger.LogInformation("Posting journal SI: get do journal data for hpp or barang terkirim.");
            var dlvJournal =
                await db.Journals.AsNoTracking()
                    .Where(x => x.SrcTrans == "DLV" && invDetailData.Select(id => id.DoData.Code).Contains(x.Code))
                    .ToListAsync(cancellationToken);

            foreach (var itemData in invData)
            {
                // var InvDetailData = (from invdetail in db.SalesInvoiceDetails
                //                      join dodata in db.SalesDeliveryHeaders on invdetail.DoCode equals dodata.Code
                //                      where invdetail.Code == itemData.InvHeader.Code
                //                      select new { InvDetail = invdetail, DoData = dodata }).AsNoTracking().ToList();

                decimal taxAmount = 0m;
                decimal discAmount = 0m;
                decimal arAmount = 0m;

                foreach (var itemDetail in invDetailData.Where(x => x.InvDetail.Code == itemData.InvHeader.Code))
                {
                    _logger.LogInformation($"Posting journal SI: {itemData.InvHeader.Code} - {itemDetail.InvDetail.DoCode} (HPP or Barang Terkirim).");
                    var doJournal = dlvJournal.Where(x => x.Code == itemDetail.InvDetail.DoCode).ToList();
                    short id = 0;
                    short ic = 0;
                    foreach (var itemDo in doJournal)
                    {
                        if (new[] { 1, 2 }.Contains(itemDo.Group))
                        {
                            //HPP or Barang Terkirim
                            journals.Add(new Journal
                            {
                                Code = itemData.InvHeader.Code,
                                LineNo = itemDo.Group == 2 ? ++id : ++ic,
                                Date = itemData.InvHeader.Date,
                                CoaCode = itemDo.Group == 2 ? itemDo.CoaCode : systemParam.FirstOrDefault(x => x.Code == "COGS_COA")?.Value ?? "",
                                TypeCode = "AR_DT",
                                Notes = itemDo.Group == 2 ? itemDo.Notes : ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_COGS")?.Value ?? ""} {itemData.Customer.Initial}").Trim(),
                                RefCode1 = itemData.InvHeader.SoCode,
                                //RefCode2 = itemDo.RefCode2,
                                Group = itemDo.Group == 2 ? (short)5 : (short)2,
                                CurrCode = itemData.InvHeader.CurrCode,
                                Period = itemData.InvHeader.Date.ToString("yyyyMMdd"),
                                Type = itemDo.Group == 2 ? "C" : "D",
                                Amount = itemDo.Amount,
                                SrcTrans = "SI"
                            });
                        }
                    }

                    //ar
                    arAmount += itemDetail.DoData.Total;
                    //PPN
                    taxAmount += itemDetail.DoData.TaxAmount - itemDetail.DoData.ExemptTaxAmount;
                    //PPN Yang Dibebaskan
                    //extTaxAmount += itemDetail.RcvData.ExemptTaxAmount;
                    // var DlvDetailData =
                    //     (from dlvdetail in db.SalesDeliveryDetails
                    //     join item in db.Items on dlvdetail.ItemId equals item.Id
                    //     where dlvdetail.Code == itemDetail.DoData.Code
                    //     select new
                    //     {
                    //         DlvDetail = dlvdetail, Item = item
                    //     }).AsNoTracking().ToList();

                    // var DlvDetailFreeData =
                    //     (from dlvdetail in db.SalesDeliveryDetailFreeGoods
                    //     join item in db.Items on dlvdetail.ItemId equals item.Id
                    //     where dlvdetail.Code == itemDetail.DoData.Code
                    //     select new
                    //     {
                    //         DlvDetail = dlvdetail, Item = item
                    //     }).AsNoTracking().ToList();

                    _logger.LogInformation($"Posting journal SI: {itemData.InvHeader.Code} - {itemDetail.DoData.Code} (PPN).");
                    short ix = 0;
                    foreach (var itemDlvDetail in dlvDetailData.Where(x => x.DlvDetail.Code == itemDetail.DoData.Code))
                    {
                        discAmount += itemDlvDetail.DlvDetail.Disc * itemDlvDetail.DlvDetail.Qty;
                        if (itemDlvDetail.DlvDetail.ExemptTaxAmount > 0)
                        {
                            //PPN Yang Dibebaskan
                            journals.Add(new Journal
                            {
                                Code = itemData.InvHeader.Code,
                                LineNo = ++ix,
                                Date = itemData.InvHeader.Date,
                                CoaCode = string.IsNullOrWhiteSpace(taxes.FirstOrDefault(x => x.Id == itemDlvDetail.DlvDetail.TaxId)?.ExemptCoaCode) ? "" : taxes.FirstOrDefault(x => x.Id == itemDlvDetail.DlvDetail.TaxId)?.ExemptCoaCode,
                                TypeCode = "PPN",
                                Notes = "PPN Yang Dibebaskan",
                                RefCode1 = itemData.InvHeader.SoCode,
                                //RefCode2 = itemDlvDetail.Item.Initial,
                                Group = 7,
                                CurrCode = itemDetail.DoData.CurrCode,
                                Period = itemData.InvHeader.Date.ToString("yyyyMMdd"),
                                Type = "C",
                                Amount = itemDlvDetail.DlvDetail.ExemptTaxAmount * itemDlvDetail.DlvDetail.Qty,
                                SrcTrans = "SI"
                            });

                            //PPN Yang Dibebaskan
                            journals.Add(new Journal
                            {
                                Code = itemData.InvHeader.Code,
                                LineNo = ix,
                                Date = itemData.InvHeader.Date,
                                CoaCode = string.IsNullOrWhiteSpace(taxes.FirstOrDefault(x => x.Id == itemDlvDetail.DlvDetail.TaxId)?.ExemptCoaCode) ? "" : taxes.FirstOrDefault(x => x.Id == itemDlvDetail.DlvDetail.TaxId)?.ExemptCoaCode,
                                TypeCode = "PPN",
                                Notes = "PPN Yang Dibebaskan",
                                RefCode1 = itemData.InvHeader.SoCode,
                                //RefCode2 = itemDlvDetail.Item.Initial,
                                Group = 4,
                                CurrCode = itemDetail.DoData.CurrCode,
                                Period = itemData.InvHeader.Date.ToString("yyyyMMdd"),
                                Type = "D",
                                Amount = itemDlvDetail.DlvDetail.ExemptTaxAmount * itemDlvDetail.DlvDetail.Qty,
                                SrcTrans = "SI"
                            });
                        }
                    }

                    _logger.LogInformation($"Posting journal SI: {itemData.InvHeader.Code} - {itemDetail.DoData.Code} (DOF).");
                    short idf = 0;
                    foreach (var itemFreeDetail in dlvDetailFreeData.Where(x => x.DlvDetail.Code == itemDetail.DoData.Code))
                    {
                        // var smData = db.StockMutations.AsNoTracking().FirstOrDefault(x => x.RefDetailId1 == itemFreeDetail.DlvDetail.Id && x.RefCode1 == itemFreeDetail.DlvDetail.Code && x.Type == "OH" && x.Src == "DOF");
                        var smItemData = smData.FirstOrDefault(x => x.RefDetailId1 == itemFreeDetail.DlvDetail.Id && x.RefCode1 == itemFreeDetail.DlvDetail.Code && x.Type == "OH" && x.Src == "DOF");
                        if (smItemData == null) continue;
                        var itemFreeHPP = smItemData.BaseNettPrice * smItemData.BaseQty;
                        journals.Add(new Journal
                        {
                            Code = itemData.InvHeader.Code,
                            LineNo = ++idf,
                            Date = itemData.InvHeader.Date,
                            CoaCode = itemFreeDetail.DlvDetail.CoaCode ?? "",
                            TypeCode = "COST",
                            Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_PROMO_COST")?.Value ?? ""} {itemFreeDetail.Item.Initial}").Trim(),
                            RefCode1 = itemData.InvHeader.SoCode,
                            Group = 3,
                            CurrCode = itemData.InvHeader.CurrCode,
                            Period = itemData.InvHeader.Date.ToString("yyyyMMdd"),
                            Type = "D",
                            Amount = itemFreeHPP,
                            SrcTrans = "SI"
                        });

                        journals.Add(new Journal
                        {
                            Code = itemData.InvHeader.Code,
                            LineNo = idf,
                            Date = itemData.InvHeader.Date,
                            CoaCode = systemParam.FirstOrDefault(x => x.Code == "COGS_COA")?.Value ?? "",
                            TypeCode = "AR_DT",
                            Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_COGS")?.Value ?? ""} {itemData.Customer.Initial}").Trim(),
                            RefCode1 = itemData.InvHeader.SoCode,
                            //RefCode2 = itemFreeDetail.Item.Initial,
                            Group = 2,
                            CurrCode = itemData.InvHeader.CurrCode,
                            Period = itemData.InvHeader.Date.ToString("yyyyMMdd"),
                            Type = "C",
                            Amount = itemFreeHPP,
                            SrcTrans = "SI"
                        });
                        //var removedFreeItem = journals.FirstOrDefault(x => x.Code == itemData.InvHeader.Code && x.Group == 2 && x.Amount == itemFreeHPP);
                        //journals.Remove(removedFreeItem);
                    }
                }

                _logger.LogInformation($"Posting journal SI: {itemData.InvHeader.Code} (DP).");
                // var SDPData = db.SalesInvoiceCreditMemos.Where(x => x.InvCode == itemData.InvHeader.Code && x.Src == "DP").ToList();
                foreach (var itemSDP in invMemo.Where(x => x.InvCode == itemData.InvHeader.Code && x.Src == "DP"))
                {
                    //Uang Muka               
                    journals.Add(new Journal
                    {
                        Code = itemData.InvHeader.Code,
                        LineNo = 1,
                        Date = itemData.InvHeader.Date,
                        CoaCode = systemParam.FirstOrDefault(x => x.Code == "SLS_DP_COA")?.Value ?? "",
                        TypeCode = "SDP",
                        Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_SLS_DP")?.Value ?? ""} {itemData.Customer.Initial}").Trim(),
                        RefCode1 = itemSDP.CreditMemoCode,
                        RefCode2 = itemData.InvHeader.Code,
                        RefCode3 = itemData.InvHeader.SoCode,
                        Group = 9,
                        CurrCode = itemData.InvHeader.CurrCode,
                        Period = itemData.InvHeader.Date.ToString("yyyyMMdd"),
                        Type = "D",
                        Amount = itemSDP.CreditMemoAmount,
                        SrcTrans = "SI"
                    });

                    //Piutang - AR
                    journals.Add(new Journal
                    {
                        Code = itemData.InvHeader.Code,
                        LineNo = 1,
                        Date = itemData.InvHeader.Date,
                        CoaCode = systemParam.FirstOrDefault(x => x.Code == "AR_COA")?.Value ?? "",
                        TypeCode = "AR",
                        Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_AR")?.Value ?? ""} {itemData.Customer.Initial}").Trim(),
                        RefCode1 = itemData.InvHeader.SoCode,
                        Group = 1,
                        CurrCode = itemData.InvHeader.CurrCode,
                        Period = itemData.InvHeader.Date.ToString("yyyyMMdd"),
                        Type = "C",
                        Amount = itemSDP.CreditMemoAmount + itemSDP.CreditMemoTaxAmount,
                        SrcTrans = "SI"
                    });

                    //PPN Uang Muka
                    journals.Add(new Journal
                    {
                        Code = itemData.InvHeader.Code,
                        LineNo = 1,
                        Date = itemData.InvHeader.Date,
                        CoaCode = systemParam.FirstOrDefault(x => x.Code == "TAX_OUT_COA")?.Value ?? "",
                        TypeCode = "PPN",
                        Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_TAX_OUT")?.Value ?? ""} {itemData.Customer.Initial}").Trim(),
                        RefCode1 = itemData.InvHeader.SoCode,
                        Group = 6,
                        CurrCode = itemData.InvHeader.CurrCode,
                        Period = itemData.InvHeader.Date.ToString("yyyyMMdd"),
                        Type = "D",
                        Amount = itemSDP.CreditMemoTaxAmount,
                        SrcTrans = "SI"
                    });
                }

                //discount
                journals.Add(new Journal
                {
                    Code = itemData.InvHeader.Code,
                    LineNo = 1,
                    Date = itemData.InvHeader.Date,
                    CoaCode = systemParam.FirstOrDefault(x => x.Code == "SLS_DISC_COA")?.Value ?? "",
                    TypeCode = "SLS_DISC",
                    Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_SLS_DISC")?.Value ?? ""} {itemData.Customer.Initial}").Trim(),
                    RefCode1 = itemData.InvHeader.SoCode,
                    RefCode2 = "",
                    Group = 3,
                    CurrCode = itemData.InvHeader.CurrCode,
                    Period = itemData.InvHeader.Date.ToString("yyyyMMdd"),
                    Type = "D",
                    Amount = discAmount,
                    SrcTrans = "SI"
                });

                //PPN
                journals.Add(new Journal
                {
                    Code = itemData.InvHeader.Code,
                    LineNo = 1,
                    Date = itemData.InvHeader.Date,
                    CoaCode = systemParam.FirstOrDefault(x => x.Code == "TAX_OUT_COA")?.Value ?? "",
                    TypeCode = "PPN",
                    Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_TAX_OUT")?.Value ?? ""} {itemData.Customer.Initial}").Trim(),
                    RefCode1 = itemData.InvHeader.SoCode,
                    Group = 6,
                    CurrCode = itemData.InvHeader.CurrCode,
                    Period = itemData.InvHeader.Date.ToString("yyyyMMdd"),
                    Type = "C",
                    Amount = taxAmount,
                    SrcTrans = "SI"
                });

                //Piutang - AR
                journals.Add(new Journal
                {
                    Code = itemData.InvHeader.Code,
                    LineNo = 1,
                    Date = itemData.InvHeader.Date,
                    CoaCode = systemParam.FirstOrDefault(x => x.Code == "AR_COA")?.Value ?? "",
                    TypeCode = "AR",
                    Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_AR")?.Value ?? ""} {itemData.Customer.Initial}").Trim(),
                    RefCode1 = itemData.InvHeader.SoCode,
                    Group = 1,
                    CurrCode = itemData.InvHeader.CurrCode,
                    Period = itemData.InvHeader.Date.ToString("yyyyMMdd"),
                    Type = "D",
                    Amount = arAmount,
                    SrcTrans = "SI"
                });

                //Sales - Penjualan
                journals.Add(new Journal
                {
                    Code = itemData.InvHeader.Code,
                    LineNo = 1,
                    Date = itemData.InvHeader.Date,
                    CoaCode = systemParam.FirstOrDefault(x => x.Code == "SLS_COA")?.Value ?? "",
                    TypeCode = "SLS",
                    Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_SLS")?.Value ?? ""} {itemData.Customer.Initial}").Trim(),
                    RefCode1 = itemData.InvHeader.SoCode,
                    Group = 8,
                    CurrCode = itemData.InvHeader.CurrCode,
                    Period = itemData.InvHeader.Date.ToString("yyyyMMdd"),
                    Type = "C",
                    Amount = arAmount - taxAmount + discAmount,
                    SrcTrans = "SI"
                });

                _logger.LogInformation($"Posting journal SI: {itemData.InvHeader.Code} (CM).");
                // var invMemo = db.SalesInvoiceCreditMemos.Where(x => x.InvCode == itemData.InvHeader.Code && x.Src == "CM").ToList();
                short k = 0;
                foreach (var itemMemo in invMemoCm.Where(x => x.InvCode == itemData.InvHeader.Code))
                {
                    // var memoData = db.CreditMemos.AsNoTracking().FirstOrDefault(x => x.Code == itemMemo.CreditMemoCode);
                    var memoItemData = memoData.FirstOrDefault(x => x.Code == itemMemo.CreditMemoCode);
                    if (memoItemData != null && memoItemData.Mark != "V")
                    {
                        journals.Add(new Journal
                        {
                            Code = itemData.InvHeader.Code + "-CN",
                            LineNo = ++k,
                            Date = itemData.InvHeader.Date,
                            CoaCode = memoItemData.SrcTrans == 1 ? systemParam.FirstOrDefault(x => x.Code == "DEP_CUST_COA")?.Value ?? "" : systemParam.FirstOrDefault(x => x.Code == "CM_AP_COA")?.Value ?? "",
                            TypeCode = "CM_AR",
                            Notes = ($"{(memoItemData.SrcTrans == 1 ? systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_DEP_CUST")?.Value ?? "" : systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_CM_AP")?.Value ?? "")} {itemData.Customer.Initial}").Trim(),
                            RefCode1 = itemMemo.CreditMemoCode,
                            RefCode2 = itemData.InvHeader.Code,
                            RefCode3 = itemData.InvHeader.SoCode,
                            Group = 1,
                            CurrCode = itemData.InvHeader.CurrCode,
                            Period = itemData.InvHeader.Date.ToString("yyyyMMdd"),
                            Type = "D",
                            Amount = itemMemo.CreditMemoAmount,
                            SrcTrans = "SI"
                        });

                        journals.Add(new Journal
                        {
                            Code = itemData.InvHeader.Code + "-CN",
                            LineNo = k,
                            Date = itemData.InvHeader.Date,
                            CoaCode = systemParam.FirstOrDefault(x => x.Code == "AR_COA")?.Value ?? "",
                            TypeCode = "AR",
                            Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_AR")?.Value ?? ""} {itemData.Customer.Initial}").Trim(),
                            RefCode1 = itemData.InvHeader.Code,
                            RefCode2 = itemData.InvHeader.SoCode,
                            RefCode3 = itemMemo.CreditMemoCode,
                            Group = 2,
                            CurrCode = itemData.InvHeader.CurrCode,
                            Period = itemData.InvHeader.Date.ToString("yyyyMMdd"),
                            Type = "C",
                            Amount = itemMemo.CreditMemoAmount,
                            SrcTrans = "SI"
                        });
                    }
                }
            }
        }

        return journals;
    }

    private async Task<IEnumerable<Journal>> ProcessCashBankJournalAsync(TenantContext db, DateTime date,
        List<SystemParameter> systemParam, CancellationToken cancellationToken)
    {
        List<Journal> journals = new();
        
        _logger.LogInformation("Posting journal CB: get cash bank data.");
        var cbData =
            await db.GeneralCashBankHeaders.AsNoTracking()
                .Where(x => ((x.Date.Month == date.Month && x.Date.Year == date.Year) ||
                             (x.ChequeDate.Value.Month == date.Month && x.ChequeDate.Value.Year == date.Year)) &&
                            x.Mark != "V")
                .ToListAsync(cancellationToken);
        
        _logger.LogInformation("Posting journal CB: get cash bank detail data.");
        var cbDetailData =
            await db.GeneralCashBankDetails.AsNoTracking()
                .Where(x => cbData.Select(i => i.Code).Contains(x.Code))
                .ToListAsync(cancellationToken);
        
        _logger.LogInformation("Posting journal CB: delete cash bank journal data.");
        var journalData = db.Journals.AsNoTracking().Where(x => cbData.Select(i => i.Code).Contains(x.Code));
        if (await journalData.AnyAsync(cancellationToken))
            db.Journals.RemoveRange(journalData);
        
        foreach (var itemData in cbData)
        {
            // var cbDetailData = db.GeneralCashBankDetails.Where(x => x.Code == itemData.Code).ToList();
            short i = 0;
            short j = 0;
            short k = 0;
            short l = 0;
            foreach (var itemDetailData in cbDetailData.Where(x => x.Code == itemData.Code))
            {
                _logger.LogInformation($"Posting journal CB: {itemData.Code} - {itemDetailData.Type} - {itemDetailData.CoaCode} - {itemDetailData.TransCode}.");
                
                //Detail
                journals.Add(new Journal
                {
                    Code = itemData.Code,
                    LineNo = ++i,
                    Date = itemData.ChequeDate ?? itemData.Date,
                    CoaCode = itemDetailData.CoaCode ?? systemParam.FirstOrDefault(x => x.Code == $"{itemDetailData.Type}_COA")?.Value ?? "",
                    TypeCode = $"CB_{itemDetailData.Type}",
                    Notes = ($"{systemParam.FirstOrDefault(x => x.Code == $"JR_PREFIX_{itemDetailData.Type}")?.Value ?? ""} {itemDetailData.Notes}").Trim(),
                    RefCode1 = itemDetailData.TransCode,
                    RefCode2 = itemData.Code,
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
                    Date = itemData.ChequeDate ?? itemData.Date,
                    CoaCode = itemData.CoaCode ?? "",
                    TypeCode = "CB",
                    Notes = "Kas/Bank",
                    RefCode2 = itemData.Code,
                    Group = 3,
                    CurrCode = itemData.CurrCode,
                    Period = itemData.Date.ToString("yyyyMMdd"),
                    Type = itemDetailData.TypeAmount == "C" ? "D" : "C",
                    Amount = itemDetailData.Amount,
                    SrcTrans = "CB"
                });

                if (itemData.ChequeDate.HasValue)
                {
                    //Check D
                    journals.Add(new Journal
                    {
                        Code = itemData.Code,
                        LineNo = 1,
                        Date = itemData.ChequeDate ?? itemData.Date,
                        CoaCode = systemParam.FirstOrDefault(x => x.Code == $"CHQ_{(itemData.Type == "D" ? "AR" : "AP")}_COA")?.Value ?? "",
                        TypeCode = "CB",
                        Notes = $"Terima Cek / Giro, Kode Cek: {(string.IsNullOrEmpty(itemData.ChequeNo) ? "-" : $"{itemData.ChequeNo}")}",
                        RefCode2 = itemData.Code,
                        Group = 4,
                        CurrCode = itemData.CurrCode,
                        Period = itemData.Date.ToString("yyyyMMdd"),
                        Type = itemDetailData.Type == "AR" ? "D" : "C",
                        Amount = itemDetailData.Amount,
                        SrcTrans = "CB"
                    });
                    //Check C
                    journals.Add(new Journal
                    {
                        Code = itemData.Code,
                        LineNo = 1,
                        Date = itemData.ChequeDate ?? itemData.Date,
                        CoaCode = systemParam.FirstOrDefault(x => x.Code == $"CHQ_{(itemData.Type == "D" ? "AR" : "AP")}_COA")?.Value ?? "",
                        TypeCode = "CB",
                        Notes = $"{(itemData.Mark == "A" ? "Kliring" : "Penolakan")} Cek / Giro, Kode Cek: {(string.IsNullOrEmpty(itemData.ChequeNo) ? "-" : $"{itemData.ChequeNo}")}",
                        RefCode2 = itemData.Code,
                        Group = 5,
                        CurrCode = itemData.CurrCode,
                        Period = itemData.Date.ToString("yyyyMMdd"),
                        Type = itemDetailData.Type == "AR" ? "C" : "D",
                        Amount = itemDetailData.Amount,
                        SrcTrans = "CB"
                    });
                    if (itemData.Mark == "REJ")
                    {
                        journals.Add(new Journal
                        {
                            Code = itemData.Code,
                            LineNo = ++k,
                            Date = itemData.ChequeDate ?? itemData.Date,
                            CoaCode = itemDetailData.CoaCode ?? systemParam.FirstOrDefault(x => x.Code == $"{itemDetailData.Type}_COA")?.Value ?? "",
                            TypeCode = $"CB_{itemDetailData.Type}",
                            Notes = $"Penolakan Cek / Giro, Kode Cek: {(string.IsNullOrEmpty(itemData.ChequeNo) ? "-" : $"{itemData.ChequeNo}")}",
                            RefCode1 = itemDetailData.TransCode,
                            Group = (short)(itemDetailData.TypeAmount == "D" ? 2 : 1),
                            CurrCode = itemDetailData.CurrCode,
                            Period = itemData.Date.ToString("yyyyMMdd"),
                            Type = itemDetailData.TypeAmount == "C" ? "D" : "C",
                            Amount = itemDetailData.Amount,
                            SrcTrans = "CB"
                        });
                        journals.Add(new Journal
                        {
                            Code = itemData.Code,
                            LineNo = ++l,
                            Date = itemData.ChequeDate ?? itemData.Date,
                            CoaCode = itemData.CoaCode ?? "",
                            TypeCode = "CB",
                            Notes = $"Penolakan Cek / Giro, Kode Cek: {(string.IsNullOrEmpty(itemData.ChequeNo) ? "-" : $"{itemData.ChequeNo}")}",
                            RefCode2 = itemData.Code,
                            Group = 6,
                            CurrCode = itemData.CurrCode,
                            Period = itemData.Date.ToString("yyyyMMdd"),
                            Type = itemDetailData.TypeAmount,
                            Amount = itemDetailData.Amount,
                            SrcTrans = "CB"
                        });
                    }
                }
            }
        }

        return journals;
    }

    private IEnumerable<Journal> ProcessBBAPJournal(TenantContext db, List<SystemParameter> systemParam)
    {
        List<Journal> journals = new();
        var startDate = Convert.ToDateTime(systemParam.FirstOrDefault(x => x.Code == "DATA_START_DATE").Value).AddDays(-1);
        var latestData = db.VwBeginningBalanceAPs.AsNoTracking().OrderByDescending(x => x.Date).FirstOrDefault();
        if (latestData?.Date.Month == startDate.Month && latestData?.Date.Year == startDate.Year)
        {
            var bbapData = db.VwBeginningBalanceAPs.AsNoTracking().ToList();
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

    private IEnumerable<Journal> ProcessBBARJournal(TenantContext db, List<SystemParameter> systemParam)
    {
        List<Journal> journals = new();
        var startDate = Convert.ToDateTime(systemParam.FirstOrDefault(x => x.Code == "DATA_START_DATE").Value).AddDays(-1);
        var latestData = db.VwBeginningBalanceARs.AsNoTracking().OrderByDescending(x => x.Date).FirstOrDefault();
        if (latestData?.Date.Month == startDate.Month && latestData?.Date.Year == startDate.Year)
        {
            var bbapData = db.VwBeginningBalanceARs.AsNoTracking().ToList();
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

    private IEnumerable<Journal> ProcessBBDebitMemoJournal(TenantContext db, List<SystemParameter> systemParam)
    {
        List<Journal> journals = new();
        var startDate = Convert.ToDateTime(systemParam.FirstOrDefault(x => x.Code == "DATA_START_DATE").Value).AddDays(-1);
        var latestData = db.VwBeginningBalanceDebitMemos.AsNoTracking().OrderByDescending(x => x.Date).FirstOrDefault();
        if (latestData?.Date.Month == startDate.Month && latestData?.Date.Year == startDate.Year)
        {
            var bbapData = db.VwBeginningBalanceDebitMemos.AsNoTracking().ToList();
            short i = 0;
            foreach (var item in bbapData)
            {
                journals.Add(new Journal
                {
                    Code = $"BB-DM-{(item.Type == 1 ? "DEP-SUP" : "AR")}-" + startDate.ToString("yyyyMMdd"),
                    LineNo = ++i,
                    Date = startDate,
                    CoaCode = systemParam.FirstOrDefault(x => x.Code == $"{(item.Type == 1 ? "DEP_SUP" : "DM_AR")}_COA")?.Value ?? "",
                    TypeCode = $"BB_{(item.Type == 1 ? "DEP_SUP" : "DM_AR")}",
                    Notes = ($"{systemParam.FirstOrDefault(x => x.Code == $"JR_PREFIX_BB_{(item.Type == 1 ? "DEP_SUP" : "DM_AR")}")?.Value ?? ""} {item.SupName}").Trim(),
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
                Code = "BB-DM-AR-" + startDate.ToString("yyyyMMdd"),
                LineNo = 2,
                Date = startDate,
                CoaCode = systemParam.FirstOrDefault(x => x.Code == "BB_COA")?.Value ?? "",
                TypeCode = "BB_DM_AR",
                Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_BB_DM_AR")?.Value ?? ""}").Trim(),
                RefCode1 = "",
                Group = 1,
                CurrCode = "IDR",
                Period = startDate.ToString("yyyyMMdd"),
                Type = "C",
                Amount = journals.Where(x => x.Code == "BB-DM-AR-" + startDate.ToString("yyyyMMdd") && x.TypeCode == "BB_DM_AR").Sum(x => x.Amount),
                SrcTrans = "BB_DM"
            });

            journals.Add(new Journal
            {
                Code = "BB-DM-DEP-SUP-" + startDate.ToString("yyyyMMdd"),
                LineNo = 1,
                Date = startDate,
                CoaCode = systemParam.FirstOrDefault(x => x.Code == "BB_COA")?.Value ?? "",
                TypeCode = "BB_DEP_SUP",
                Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_BB_DEP_SUP")?.Value ?? ""}").Trim(),
                RefCode1 = "",
                Group = 1,
                CurrCode = "IDR",
                Period = startDate.ToString("yyyyMMdd"),
                Type = "C",
                Amount = journals.Where(x => x.Code == "BB-DM-DEP-SUP-" + startDate.ToString("yyyyMMdd") && x.TypeCode == "BB_DEP_SUP").Sum(x => x.Amount),
                SrcTrans = "BB_DM"
            });
        }
        return journals;
    }

    private IEnumerable<Journal> ProcessBBCreditMemoJournal(TenantContext db, List<SystemParameter> systemParam)
    {
        List<Journal> journals = new();
        var startDate = Convert.ToDateTime(systemParam.FirstOrDefault(x => x.Code == "DATA_START_DATE").Value).AddDays(-1);
        var latestData = db.VwBeginningBalanceCreditMemos.AsNoTracking().OrderByDescending(x => x.Date).FirstOrDefault();
        if (latestData?.Date.Month == startDate.Month && latestData?.Date.Year == startDate.Year)
        {
            var bbapData = db.VwBeginningBalanceCreditMemos.AsNoTracking().ToList();
            short i = 0;
            foreach (var item in bbapData)
            {
                journals.Add(new Journal
                {
                    Code = $"BB-CM-{(item.Type == 1 ? "DEP-CUST" : "AP")}-" + startDate.ToString("yyyyMMdd"),
                    LineNo = ++i,
                    Date = startDate,
                    CoaCode = systemParam.FirstOrDefault(x => x.Code == $"{(item.Type == 1 ? "DEP_CUST" : "CM_AP")}_COA")?.Value ?? "",
                    TypeCode = $"BB_{(item.Type == 1 ? "DEP_CUST" : "CM_AP")}",
                    Notes = ($"{systemParam.FirstOrDefault(x => x.Code == $"JR_PREFIX_BB_{(item.Type == 1 ? "DEP_CUST" : "CM_AP")}")?.Value ?? ""} {item.CustName}").Trim(),
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
                Code = "BB-CM-AP-" + startDate.ToString("yyyyMMdd"),
                LineNo = 2,
                Date = startDate,
                CoaCode = systemParam.FirstOrDefault(x => x.Code == "BB_COA")?.Value ?? "",
                TypeCode = "BB_CM_AP",
                Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_BB_CM_AP")?.Value ?? ""}").Trim(),
                RefCode1 = "",
                Group = 1,
                CurrCode = "IDR",
                Period = startDate.ToString("yyyyMMdd"),
                Type = "D",
                Amount = journals.Where(x => x.Code == "BB-CM-AP-" + startDate.ToString("yyyyMMdd") && x.TypeCode == "BB_CM_AP").Sum(x => x.Amount),
                SrcTrans = "BB_CM"
            });

            journals.Add(new Journal
            {
                Code = "BB-CM-DEP-CUST-" + startDate.ToString("yyyyMMdd"),
                LineNo = 1,
                Date = startDate,
                CoaCode = systemParam.FirstOrDefault(x => x.Code == "BB_COA")?.Value ?? "",
                TypeCode = "BB_DEP_CUST",
                Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_BB_DEP_CUST")?.Value ?? ""}").Trim(),
                RefCode1 = "",
                Group = 1,
                CurrCode = "IDR",
                Period = startDate.ToString("yyyyMMdd"),
                Type = "D",
                Amount = journals.Where(x => x.Code == "BB-CM-DEP-CUST-" + startDate.ToString("yyyyMMdd") && x.TypeCode == "BB_DEP_CUST").Sum(x => x.Amount),
                SrcTrans = "BB_CM"
            });
        }
        return journals;
    }

    private IEnumerable<Journal> ProcessPurchaseReturnJournal(TenantContext db, DateTime dateTime, List<SystemParameter> systemParam, List<Item> items, List<Tax> taxes)
    {
        List<Journal> journals = new();
        var RtnData = (from rtnheader in db.PurchaseReturnHeaders
                       join supplier in db.Suppliers on rtnheader.SupCode equals supplier.Code
                       where rtnheader.Date.Month == dateTime.Month && rtnheader.Date.Year == dateTime.Year && rtnheader.Mark != "V"
                       select new { RtnHeader = rtnheader, Supplier = supplier }).AsNoTracking().ToList();

        foreach (var itemData in RtnData)
        {
            if (itemData.RtnHeader.Type != 1)
            {
                var RtnDetailData = (from rtndetail in db.PurchaseReturnDetails
                                     join item in db.Items on rtndetail.ItemId equals item.Id
                                     where rtndetail.Code == itemData.RtnHeader.Code
                                     select new { RtnDetail = rtndetail, Item = item }).AsNoTracking().ToList();
                var RtnDetailExData = db.PurchaseReturnDetailExchDiffItems.Where(x => x.Code == itemData.RtnHeader.Code).AsNoTracking().ToList();

                var taxAmount = 0m;
                //var extTaxAmount = 0m;
                short i = 0;
                short ix = 0;
                foreach (var itemDetail in RtnDetailData)
                {
                    var smData = db.StockMutations.AsNoTracking().FirstOrDefault(x => x.RefDetailId1 == itemDetail.RtnDetail.Id && x.RefCode1 == itemDetail.RtnDetail.Code);
                    if (smData == null) continue;
                    //var nonVoidSM = RemoveVoidSM(db, db.StockMutations.ToList());
                    //CalculateHPP(db, nonVoidSM, smData.ItemId, smData.RefDetailId1, "PR");
                    //smData = db.StockMutations.AsNoTracking().FirstOrDefault(x => x.RefDetailId1 == itemDetail.RtnDetail.Id && x.RefCode1 == itemDetail.RtnDetail.Code);
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
                        taxAmount += itemDetail.RtnDetail.Qty * itemDetail.RtnDetail.TaxAmount;
                    if (itemDetail.RtnDetail.ExemptTaxAmount > 0)
                    {
                        taxAmount -= itemDetail.RtnDetail.Qty * itemDetail.RtnDetail.ExemptTaxAmount;
                        journals.Add(new Journal
                        {
                            Code = itemData.RtnHeader.Code,
                            LineNo = ++ix,
                            Date = itemData.RtnHeader.Date,
                            CoaCode = string.IsNullOrWhiteSpace(taxes.FirstOrDefault(x => x.Id == itemDetail.RtnDetail.TaxId)?.ExemptCoaCode) ? "" : taxes.FirstOrDefault(x => x.Id == itemDetail.RtnDetail.TaxId)?.ExemptCoaCode,
                            TypeCode = "PR_DT",
                            Notes = "PPN Yang Dibebaskan",
                            RefCode1 = itemDetail.Item.Initial,
                            Group = 6,
                            CurrCode = itemData.RtnHeader.CurrCode,
                            Period = itemData.RtnHeader.Date.ToString("yyyyMMdd"),
                            Type = "C",
                            Amount = itemDetail.RtnDetail.ExemptTaxAmount * itemDetail.RtnDetail.Qty,
                            SrcTrans = "PR"
                        });

                        journals.Add(new Journal
                        {
                            Code = itemData.RtnHeader.Code,
                            LineNo = ix,
                            Date = itemData.RtnHeader.Date,
                            CoaCode = string.IsNullOrWhiteSpace(taxes.FirstOrDefault(x => x.Id == itemDetail.RtnDetail.TaxId)?.ExemptCoaCode) ? "" : taxes.FirstOrDefault(x => x.Id == itemDetail.RtnDetail.TaxId)?.ExemptCoaCode,
                            TypeCode = "PR_DT",
                            Notes = "PPN Yang Dibebaskan",
                            RefCode1 = itemDetail.Item.Initial,
                            Group = 4,
                            CurrCode = itemData.RtnHeader.CurrCode,
                            Period = itemData.RtnHeader.Date.ToString("yyyyMMdd"),
                            Type = "D",
                            Amount = itemDetail.RtnDetail.ExemptTaxAmount * itemDetail.RtnDetail.Qty,
                            SrcTrans = "PR"
                        });
                    }
                }

                if (taxAmount > 0)
                {
                    //Pajak - PPN
                    journals.Add(new Journal
                    {
                        Code = itemData.RtnHeader.Code,
                        LineNo = 1,
                        Date = itemData.RtnHeader.Date,
                        CoaCode = systemParam.FirstOrDefault(x => x.Code == "TAX_IN_COA")?.Value ?? "",
                        TypeCode = "PPN",
                        Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_TAX_IN")?.Value ?? ""} {itemData.Supplier.Initial}").Trim(),
                        RefCode1 = "",
                        Group = 3,
                        CurrCode = itemData.RtnHeader.CurrCode,
                        Period = itemData.RtnHeader.Date.ToString("yyyyMMdd"),
                        Type = "C",
                        Amount = taxAmount,
                        SrcTrans = "PR"
                    });
                }

                //if (extTaxAmount > 0)
                //{
                //    //Pajak Yang Dibebaskan
                //    journals.Add(new Journal
                //    {
                //        Code = itemData.RtnHeader.Code,
                //        LineNo = 1,
                //        Date = itemData.RtnHeader.Date,
                //        CoaCode = systemParam.FirstOrDefault(x => x.Code == "TAX_IN_COA")?.Value ?? "",
                //        TypeCode = "EPPN",
                //        Notes = ($"Pajak Yang Dibebaskan {itemData.Supplier.Initial}").Trim(),
                //        RefCode1 = "",
                //        Group = 4,
                //        CurrCode = itemData.RtnHeader.CurrCode,
                //        Period = itemData.RtnHeader.Date.ToString("yyyyMMdd"),
                //        Type = "D",
                //        Amount = extTaxAmount,
                //        SrcTrans = "PR"
                //    });
                //}

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
                        Group = 5,
                        CurrCode = itemData.RtnHeader.CurrCode,
                        Period = itemData.RtnHeader.Date.ToString("yyyyMMdd"),
                        Type = hppValue < 0 ? "D" : "C",
                        Amount = Math.Abs(hppValue),
                        SrcTrans = "PR"
                    });
                }

                //Receive Process
                var RcvData = (from rcvheader in db.PurchaseReceiveHeaders
                               join supplier in db.Suppliers on rcvheader.SupCode equals supplier.Code
                               where rcvheader.TransCode == itemData.RtnHeader.Code && rcvheader.Mark != "V"
                               select new { RcvHeader = rcvheader, Supplier = supplier }).AsNoTracking().ToList();

                if (RcvData.Any())
                {
                    foreach (var itemRcvData in RcvData)
                    {
                        var RcvDetailData = (from rcvdetail in db.PurchaseReceiveDetails
                                             join item in db.Items on rcvdetail.ItemId equals item.Id
                                             where rcvdetail.Code == itemRcvData.RcvHeader.Code
                                             select new { RcvDetail = rcvdetail, Item = item }).AsNoTracking().ToList();
                        short k = 0;
                        short l = 0;
                        foreach (var itemDetail in RcvDetailData)
                        {
                            var smData = db.StockMutations.AsNoTracking().FirstOrDefault(x => x.RefDetailId1 == itemDetail.RcvDetail.Id && x.RefCode1 == itemDetail.RcvDetail.Code);
                            if (smData == null) continue;
                            //var nonVoidSM = RemoveVoidSM(db, db.StockMutations.ToList());
                            //CalculateHPP(db, nonVoidSM, smData.ItemId, smData.RefDetailId1, "RCV");
                            //smData = db.StockMutations.AsNoTracking().FirstOrDefault(x => x.RefDetailId1 == itemDetail.RcvDetail.Id && x.RefCode1 == itemDetail.RcvDetail.Code);
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
                                Amount = resultHpp,
                                SrcTrans = "RCV"
                            });

                            if (itemDetail.RcvDetail.ExemptTaxAmount > 0)
                            {
                                journals.Add(new Journal
                                {
                                    Code = itemRcvData.RcvHeader.Code,
                                    LineNo = ++l,
                                    Date = itemRcvData.RcvHeader.Date,
                                    CoaCode = string.IsNullOrWhiteSpace(taxes.FirstOrDefault(x => x.Id == itemDetail.RcvDetail.TaxId)?.ExemptCoaCode) ? "" : taxes.FirstOrDefault(x => x.Id == itemDetail.RcvDetail.TaxId)?.ExemptCoaCode,
                                    TypeCode = "RCV_DT",
                                    Notes = "PPN Yang Dibebaskan",
                                    RefCode1 = itemRcvData.RcvHeader.TransCode,
                                    RefCode2 = itemDetail.Item.Initial,
                                    Group = 3,
                                    CurrCode = itemRcvData.RcvHeader.CurrCode,
                                    Period = itemRcvData.RcvHeader.Date.ToString("yyyyMMdd"),
                                    Type = "C",
                                    Amount = itemDetail.RcvDetail.ExemptTaxAmount * itemDetail.RcvDetail.Qty,
                                    SrcTrans = "RCV"
                                });

                                journals.Add(new Journal
                                {
                                    Code = itemRcvData.RcvHeader.Code,
                                    LineNo = l,
                                    Date = itemRcvData.RcvHeader.Date,
                                    CoaCode = string.IsNullOrWhiteSpace(taxes.FirstOrDefault(x => x.Id == itemDetail.RcvDetail.TaxId)?.ExemptCoaCode) ? "" : taxes.FirstOrDefault(x => x.Id == itemDetail.RcvDetail.TaxId)?.ExemptCoaCode,
                                    TypeCode = "RCV_DT",
                                    Notes = "PPN Yang Dibebaskan",
                                    RefCode1 = itemRcvData.RcvHeader.TransCode,
                                    RefCode2 = itemDetail.Item.Initial,
                                    Group = 7,
                                    CurrCode = itemRcvData.RcvHeader.CurrCode,
                                    Period = itemRcvData.RcvHeader.Date.ToString("yyyyMMdd"),
                                    Type = "D",
                                    Amount = itemDetail.RcvDetail.ExemptTaxAmount * itemDetail.RcvDetail.Qty,
                                    SrcTrans = "RCV"
                                });
                            }
                        }

                        if (journals.Where(x => x.Code == itemData.RtnHeader.Code && x.Group == 3).Any())
                        {
                            //Pajak - PPN
                            journals.Add(new Journal
                            {
                                Code = itemRcvData.RcvHeader.Code,
                                LineNo = 1,
                                Date = itemRcvData.RcvHeader.Date,
                                CoaCode = systemParam.FirstOrDefault(x => x.Code == "TAX_IN_COA")?.Value ?? "",
                                TypeCode = "PPN",
                                Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_TAX_IN")?.Value ?? ""} {itemData.Supplier.Initial}").Trim(),
                                RefCode1 = "",
                                Group = 2,
                                CurrCode = itemRcvData.RcvHeader.CurrCode,
                                Period = itemRcvData.RcvHeader.Date.ToString("yyyyMMdd"),
                                Type = "D",
                                Amount = journals.Where(x => x.Code == itemData.RtnHeader.Code && x.Group == 3).Sum(x => x.Amount),
                                SrcTrans = "RCV"
                            });
                        }

                        //if (journals.Where(x => x.Code == itemData.RtnHeader.Code && x.Group == 4).Any())
                        //{
                        //    //Pajak - PPN
                        //    journals.Add(new Journal
                        //    {
                        //        Code = itemRcvData.RcvHeader.Code,
                        //        LineNo = 1,
                        //        Date = itemRcvData.RcvHeader.Date,
                        //        CoaCode = systemParam.FirstOrDefault(x => x.Code == "TAX_IN_COA")?.Value ?? "",
                        //        TypeCode = "EPPN",
                        //        Notes = ($"Pajak Yang Dibebaskan {itemData.Supplier.Initial}").Trim(),
                        //        RefCode1 = "",
                        //        Group = 3,
                        //        CurrCode = itemRcvData.RcvHeader.CurrCode,
                        //        Period = itemRcvData.RcvHeader.Date.ToString("yyyyMMdd"),
                        //        Type = "C",
                        //        Amount = journals.Where(x => x.Code == itemData.RtnHeader.Code && x.Group == 4).Sum(x => x.Amount),
                        //        SrcTrans = "RCV"
                        //    });
                        //}

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
                            Group = 5,
                            CurrCode = itemRcvData.RcvHeader.CurrCode,
                            Period = itemRcvData.RcvHeader.Date.ToString("yyyyMMdd"),
                            Type = "C",
                            Amount = itemData.RtnHeader.Type == 2 ? journals.First(x => x.Code == itemData.RtnHeader.Code && x.Group == 1).Amount : itemData.RtnHeader.Total,
                            SrcTrans = "RCV"
                        });

                        if (itemData.RtnHeader.Type == 3)
                        {
                            var hppRtn = journals.FirstOrDefault(x => x.Code == itemData.RtnHeader.Code && x.Group == 5);
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
                                Group = 4,
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
                                Group = 6,
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
                var RtnDetailData = (from rtndetail in db.PurchaseReturnDetails
                                     join item in db.Items on rtndetail.ItemId equals item.Id
                                     where rtndetail.Code == itemData.RtnHeader.Code
                                     select new { RtnDetail = rtndetail, Item = item }).AsNoTracking().ToList();

                var taxAmount = 0m;
                //var extTaxAmount = 0m;
                short i = 0;
                short ix = 0;
                foreach (var itemDetail in RtnDetailData)
                {
                    var smData = db.StockMutations.AsNoTracking().FirstOrDefault(x => x.RefDetailId1 == itemDetail.RtnDetail.Id && x.RefCode1 == itemDetail.RtnDetail.Code);
                    if (smData == null) continue;
                    //var nonVoidSM = RemoveVoidSM(db, db.StockMutations.ToList());
                    //CalculateHPP(db, nonVoidSM, smData.ItemId, smData.RefDetailId1, "PR");
                    //smData = db.StockMutations.AsNoTracking().FirstOrDefault(x => x.RefDetailId1 == itemDetail.RtnDetail.Id && x.RefCode1 == itemDetail.RtnDetail.Code);
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
                        taxAmount += itemDetail.RtnDetail.Qty * itemDetail.RtnDetail.TaxAmount;
                    if (itemDetail.RtnDetail.ExemptTaxAmount > 0)
                    {
                        taxAmount -= itemDetail.RtnDetail.Qty * itemDetail.RtnDetail.ExemptTaxAmount;
                        journals.Add(new Journal
                        {
                            Code = itemData.RtnHeader.Code,
                            LineNo = ++ix,
                            Date = itemData.RtnHeader.Date,
                            CoaCode = string.IsNullOrWhiteSpace(taxes.FirstOrDefault(x => x.Id == itemDetail.RtnDetail.TaxId)?.ExemptCoaCode) ? "" : taxes.FirstOrDefault(x => x.Id == itemDetail.RtnDetail.TaxId)?.ExemptCoaCode,
                            TypeCode = "PR_DT",
                            Notes = "PPN Yang Dibebaskan",
                            RefCode1 = itemDetail.Item.Initial,
                            Group = 6,
                            CurrCode = itemData.RtnHeader.CurrCode,
                            Period = itemData.RtnHeader.Date.ToString("yyyyMMdd"),
                            Type = "C",
                            Amount = itemDetail.RtnDetail.ExemptTaxAmount * itemDetail.RtnDetail.Qty,
                            SrcTrans = "PR"
                        });

                        journals.Add(new Journal
                        {
                            Code = itemData.RtnHeader.Code,
                            LineNo = ix,
                            Date = itemData.RtnHeader.Date,
                            CoaCode = string.IsNullOrWhiteSpace(taxes.FirstOrDefault(x => x.Id == itemDetail.RtnDetail.TaxId)?.ExemptCoaCode) ? "" : taxes.FirstOrDefault(x => x.Id == itemDetail.RtnDetail.TaxId)?.ExemptCoaCode,
                            TypeCode = "PR_DT",
                            Notes = "PPN Yang Dibebaskan",
                            RefCode1 = itemDetail.Item.Initial,
                            Group = 4,
                            CurrCode = itemData.RtnHeader.CurrCode,
                            Period = itemData.RtnHeader.Date.ToString("yyyyMMdd"),
                            Type = "D",
                            Amount = itemDetail.RtnDetail.ExemptTaxAmount * itemDetail.RtnDetail.Qty,
                            SrcTrans = "PR"
                        });
                    }
                }

                //Pajak - PPN
                journals.Add(new Journal
                {
                    Code = itemData.RtnHeader.Code,
                    LineNo = 1,
                    Date = itemData.RtnHeader.Date,
                    CoaCode = systemParam.FirstOrDefault(x => x.Code == "TAX_IN_COA")?.Value ?? "",
                    TypeCode = "PPN",
                    Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_TAX_IN")?.Value ?? ""} {itemData.Supplier.Initial}").Trim(),
                    RefCode1 = "",
                    Group = 3,
                    CurrCode = itemData.RtnHeader.CurrCode,
                    Period = itemData.RtnHeader.Date.ToString("yyyyMMdd"),
                    Type = "C",
                    Amount = taxAmount,
                    SrcTrans = "PR"
                });

                ////Pajak Yang Dibebaskan
                //journals.Add(new Journal
                //{
                //    Code = itemData.RtnHeader.Code,
                //    LineNo = 1,
                //    Date = itemData.RtnHeader.Date,
                //    CoaCode = systemParam.FirstOrDefault(x => x.Code == "TAX_IN_COA")?.Value ?? "",
                //    TypeCode = "EPPN",
                //    Notes = ($"Pajak Yang Dibebaskan {itemData.Supplier.Initial}").Trim(),
                //    RefCode1 = "",
                //    Group = 4,
                //    CurrCode = itemData.RtnHeader.CurrCode,
                //    Period = itemData.RtnHeader.Date.ToString("yyyyMMdd"),
                //    Type = "D",
                //    Amount = extTaxAmount,
                //    SrcTrans = "PR"
                //});

                //Piutang Nota Debit
                journals.Add(new Journal
                {
                    Code = itemData.RtnHeader.Code,
                    LineNo = 1,
                    Date = itemData.RtnHeader.Date,
                    CoaCode = systemParam.FirstOrDefault(x => x.Code == "DM_AR_COA")?.Value ?? "",
                    TypeCode = "DM_AR",
                    Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_DM_AR")?.Value ?? ""} {itemData.Supplier.Initial}").Trim(),
                    RefCode1 = "",
                    Group = 1,
                    CurrCode = itemData.RtnHeader.CurrCode,
                    Period = itemData.RtnHeader.Date.ToString("yyyyMMdd"),
                    Type = "D",
                    Amount = itemData.RtnHeader.Total,
                    SrcTrans = "PR"
                });

                var hppValue = (itemData.RtnHeader.Total - itemData.RtnHeader.TaxAmount - itemData.RtnHeader.ExemptTaxAmount) - journals.Where(x => x.Code == itemData.RtnHeader.Code && x.Group == 2).Sum(x => x.Amount);
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
                    Group = 5,
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

    private async Task<IEnumerable<Journal>> ProcessSalesReturnJournalAsync(TenantContext db, DateTime date,
        List<SystemParameter> systemParam, List<Item> items, List<Tax> taxes, CancellationToken cancellationToken)
    {
        List<Journal> journals = new();
        
        _logger.LogInformation("Posting journal SR: get return data.");
        var rtnData =
            await (from rtnheader in db.SalesReturnHeaders
            join customer in db.Customers on rtnheader.CustCode equals customer.Code
            where rtnheader.Date.Month == date.Month && rtnheader.Date.Year == date.Year && rtnheader.Mark != "V"
            select new
            {
                RtnHeader = rtnheader, Customer = customer
            }).AsNoTracking().ToListAsync(cancellationToken);
        
        _logger.LogInformation("Posting journal SR: get return detail data.");
        var rtnDetailData = 
            await (from rtndetail in db.SalesReturnDetails
            join item in db.Items on rtndetail.ItemId equals item.Id
            where rtnData.Select(i => i.RtnHeader.Code).Contains(rtndetail.Code)
            select new
            {
                RtnDetail = rtndetail, Item = item
            }).AsNoTracking().ToListAsync(cancellationToken);
        
        _logger.LogInformation("Posting journal SR: get stock mutation data.");
        var smData = await db.StockMutations.FromSql(@$";
            SELECT *
            FROM Inventory.StockMutation sm
            WHERE sm.[Type] = 'OH'
            AND sm.Src = 'SR'
            AND EXISTS (
                SELECT 1
                FROM Sales.SalesReturnDetail sr_d
                INNER JOIN Sales.SalesReturnHeader sr_h
                    ON sr_h.Code = sr_d.Code
                WHERE sr_h.Mark <> 'V'
                AND MONTH(sr_h.[Date]) = {date.Month}
                AND YEAR(sr_h.[Date]) = {date.Year}
                AND sr_d.Id = sm.RefDetailId1
                AND sr_d.Code = sm.RefCode1
            )").AsNoTracking().ToListAsync(cancellationToken);

        foreach (var itemData in rtnData)
        {
            if (itemData.RtnHeader.Type != 1)
            {
                var taxAmount = 0m;
                //var extTaxAmount = 0m;
                //Return Detail
                short i = 0;
                short j = 0;
                short ix = 0;
                foreach (var itemDetail in rtnDetailData.Where(x => x.RtnDetail.Code == itemData.RtnHeader.Code))
                {
                    _logger.LogInformation($"Posting journal SR: {itemData.RtnHeader.Code} - {itemDetail.Item.Initial} ({itemDetail.Item.Id}).");
                  
                    var smItemData = smData.FirstOrDefault(x => x.RefDetailId1 == itemDetail.RtnDetail.Id && x.RefCode1 == itemDetail.RtnDetail.Code);
                    if (smItemData == null) continue;
                    //var nonVoidSM = RemoveVoidSM(db, db.StockMutations.ToList());
                    //CalculateHPP(db, nonVoidSM, smData.ItemId, smData.RefDetailId1, "SR");
                    //smData = db.StockMutations.AsNoTracking().FirstOrDefault(x => x.RefDetailId1 == itemDetail.RtnDetail.Id && x.RefCode1 == itemDetail.RtnDetail.Code);
                    var resultHpp = smItemData.BaseNettPrice * smItemData.BaseQty;

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
                        taxAmount += itemDetail.RtnDetail.TaxAmount * itemDetail.RtnDetail.Qty;
                    
                    //PPN Yang Dibebaskan
                    if (itemDetail.RtnDetail.ExemptTaxAmount > 0)
                    {
                        taxAmount -= itemDetail.RtnDetail.Qty * itemDetail.RtnDetail.ExemptTaxAmount;
                        journals.Add(new Journal
                        {
                            Code = itemData.RtnHeader.Code,
                            LineNo = ++ix,
                            Date = itemData.RtnHeader.Date,
                            CoaCode = string.IsNullOrWhiteSpace(taxes.FirstOrDefault(x => x.Id == itemDetail.RtnDetail.TaxId)?.ExemptCoaCode) ? "" : taxes.FirstOrDefault(x => x.Id == itemDetail.RtnDetail.TaxId)?.ExemptCoaCode,
                            TypeCode = "SR_DT",
                            Notes = "PPN Yang Dibebaskan",
                            RefCode1 = itemDetail.Item.Initial,
                            Group = 4,
                            CurrCode = itemData.RtnHeader.CurrCode,
                            Period = itemData.RtnHeader.Date.ToString("yyyyMMdd"),
                            Type = "C",
                            Amount = itemDetail.RtnDetail.ExemptTaxAmount * itemDetail.RtnDetail.Qty,
                            SrcTrans = "SR"
                        });

                        journals.Add(new Journal
                        {
                            Code = itemData.RtnHeader.Code,
                            LineNo = ix,
                            Date = itemData.RtnHeader.Date,
                            CoaCode = string.IsNullOrWhiteSpace(taxes.FirstOrDefault(x => x.Id == itemDetail.RtnDetail.TaxId)?.ExemptCoaCode) ? "" : taxes.FirstOrDefault(x => x.Id == itemDetail.RtnDetail.TaxId)?.ExemptCoaCode,
                            TypeCode = "SR_DT",
                            Notes = "PPN Yang Dibebaskan",
                            RefCode1 = itemDetail.Item.Initial,
                            Group = 7,
                            CurrCode = itemData.RtnHeader.CurrCode,
                            Period = itemData.RtnHeader.Date.ToString("yyyyMMdd"),
                            Type = "D",
                            Amount = itemDetail.RtnDetail.ExemptTaxAmount * itemDetail.RtnDetail.Qty,
                            SrcTrans = "SR"
                        });
                    }
                }

                //PPN - Pajak
                if (taxAmount > 0)
                {
                    journals.Add(new Journal
                    {
                        Code = itemData.RtnHeader.Code,
                        LineNo = 1,
                        Date = itemData.RtnHeader.Date,
                        CoaCode = systemParam.FirstOrDefault(x => x.Code == "TAX_OUT_COA")?.Value ?? "",
                        TypeCode = "PPN",
                        Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_TAX_OUT")?.Value ?? ""} {itemData.Customer.Initial}").Trim(),
                        RefCode1 = "",
                        Group = 3,
                        CurrCode = itemData.RtnHeader.CurrCode,
                        Period = itemData.RtnHeader.Date.ToString("yyyyMMdd"),
                        Type = "D",
                        Amount = taxAmount,
                        SrcTrans = "SR"
                    });
                }

                ////PPN Yang Dibebaskan
                //if (extTaxAmount > 0)
                //{
                //    journals.Add(new Journal
                //    {
                //        Code = itemData.RtnHeader.Code,
                //        LineNo = 1,
                //        Date = itemData.RtnHeader.Date,
                //        CoaCode = systemParam.FirstOrDefault(x => x.Code == "TAX_IN_COA")?.Value ?? "",
                //        TypeCode = "EPPN",
                //        Notes = ($"PPN Yang Dibebaskan {itemData.Customer.Initial}").Trim(),
                //        RefCode1 = "",
                //        Group = 4,
                //        CurrCode = itemData.RtnHeader.CurrCode,
                //        Period = itemData.RtnHeader.Date.ToString("yyyyMMdd"),
                //        Type = "C",
                //        Amount = taxAmount,
                //        SrcTrans = "SR"
                //    });
                //}

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
                    Group = 5,
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
                    Group = 6,
                    CurrCode = itemData.RtnHeader.CurrCode,
                    Period = itemData.RtnHeader.Date.ToString("yyyyMMdd"),
                    Type = "D",
                    Amount = Math.Abs(itemData.RtnHeader.Total) - journals.Where(x => x.Code == itemData.RtnHeader.Code && x.Group == 3).Sum(x => x.Amount) + journals.Where(x => x.Code == itemData.RtnHeader.Code && x.Group == 4).Sum(x => x.Amount),
                    SrcTrans = "SR"
                });
            }
            else
            {
                var taxAmount = 0m;
                //var extTaxAmount = 0m;
                short i = 0;
                short j = 0;
                short ix = 0;
                foreach (var itemDetail in rtnDetailData.Where(x => x.RtnDetail.Code == itemData.RtnHeader.Code))
                {
                    _logger.LogInformation($"Posting journal SR: {itemData.RtnHeader.Code} - {itemDetail.Item.Initial} ({itemDetail.Item.Id}).");

                    var smItemData = smData.FirstOrDefault(x => x.RefDetailId1 == itemDetail.RtnDetail.Id && x.RefCode1 == itemDetail.RtnDetail.Code);
                    if (smItemData == null) continue;

                    //var nonVoidSM = RemoveVoidSM(db, db.StockMutations.ToList());
                    //CalculateHPP(db, nonVoidSM, smData.ItemId, smData.RefDetailId1, "SR");
                    //smData = db.StockMutations.AsNoTracking().FirstOrDefault(x => x.RefDetailId1 == itemDetail.RtnDetail.Id && x.RefCode1 == itemDetail.RtnDetail.Code);
                    var resultHpp = smItemData.BaseNettPrice * smItemData.BaseQty;

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
                        Group = 4,
                        CurrCode = itemData.RtnHeader.CurrCode,
                        Period = itemData.RtnHeader.Date.ToString("yyyyMMdd"),
                        Type = "C",
                        Amount = resultHpp,
                        SrcTrans = "SR"
                    });

                    if (itemDetail.RtnDetail.TaxAmount > 0)
                        taxAmount += itemDetail.RtnDetail.TaxAmount * itemDetail.RtnDetail.Qty;
                    
                    if (itemDetail.RtnDetail.ExemptTaxAmount > 0)
                    {
                        taxAmount -= itemDetail.RtnDetail.Qty * itemDetail.RtnDetail.ExemptTaxAmount;
                        journals.Add(new Journal
                        {
                            Code = itemData.RtnHeader.Code,
                            LineNo = ++ix,
                            Date = itemData.RtnHeader.Date,
                            CoaCode = string.IsNullOrWhiteSpace(taxes.FirstOrDefault(x => x.Id == itemDetail.RtnDetail.TaxId)?.ExemptCoaCode) ? "" : taxes.FirstOrDefault(x => x.Id == itemDetail.RtnDetail.TaxId)?.ExemptCoaCode,
                            TypeCode = "SR_DT",
                            Notes = "PPN Yang Dibebaskan",
                            RefCode1 = itemDetail.Item.Initial,
                            Group = 3,
                            CurrCode = itemData.RtnHeader.CurrCode,
                            Period = itemData.RtnHeader.Date.ToString("yyyyMMdd"),
                            Type = "C",
                            Amount = itemDetail.RtnDetail.ExemptTaxAmount * itemDetail.RtnDetail.Qty,
                            SrcTrans = "SR"
                        });

                        journals.Add(new Journal
                        {
                            Code = itemData.RtnHeader.Code,
                            LineNo = ix,
                            Date = itemData.RtnHeader.Date,
                            CoaCode = string.IsNullOrWhiteSpace(taxes.FirstOrDefault(x => x.Id == itemDetail.RtnDetail.TaxId)?.ExemptCoaCode) ? "" : taxes.FirstOrDefault(x => x.Id == itemDetail.RtnDetail.TaxId)?.ExemptCoaCode,
                            TypeCode = "SR_DT",
                            Notes = "PPN Yang Dibebaskan",
                            RefCode1 = itemDetail.Item.Initial,
                            Group = 7,
                            CurrCode = itemData.RtnHeader.CurrCode,
                            Period = itemData.RtnHeader.Date.ToString("yyyyMMdd"),
                            Type = "D",
                            Amount = itemDetail.RtnDetail.ExemptTaxAmount * itemDetail.RtnDetail.Qty,
                            SrcTrans = "SR"
                        });
                    }
                }

                if (taxAmount > 0)
                {
                    //PPN - Pajak
                    journals.Add(new Journal
                    {
                        Code = itemData.RtnHeader.Code,
                        LineNo = 1,
                        Date = itemData.RtnHeader.Date,
                        CoaCode = systemParam.FirstOrDefault(x => x.Code == "TAX_OUT_COA")?.Value ?? "",
                        TypeCode = "PPN",
                        Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_TAX_OUT")?.Value ?? ""} {itemData.Customer.Initial}").Trim(),
                        RefCode1 = "",
                        Group = 2,
                        CurrCode = itemData.RtnHeader.CurrCode,
                        Period = itemData.RtnHeader.Date.ToString("yyyyMMdd"),
                        Type = "D",
                        Amount = taxAmount,
                        SrcTrans = "SR"
                    });
                }

                //if (extTaxAmount > 0)
                //{
                //    //PPN Yang Dibebaskan
                //    journals.Add(new Journal
                //    {
                //        Code = itemData.RtnHeader.Code,
                //        LineNo = 1,
                //        Date = itemData.RtnHeader.Date,
                //        CoaCode = systemParam.FirstOrDefault(x => x.Code == "TAX_IN_COA")?.Value ?? "",
                //        TypeCode = "EPPN",
                //        Notes = ($"Pajak Yang Dibebaskan {itemData.Customer.Initial}").Trim(),
                //        RefCode1 = "",
                //        Group = 3,
                //        CurrCode = itemData.RtnHeader.CurrCode,
                //        Period = itemData.RtnHeader.Date.ToString("yyyyMMdd"),
                //        Type = "C",
                //        Amount = extTaxAmount,
                //        SrcTrans = "SR"
                //    });
                //}

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
                    Amount = itemData.RtnHeader.Total - itemData.RtnHeader.TaxAmount - itemData.RtnHeader.ExemptTaxAmount,
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
                    Group = 6,
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

    private async Task<IEnumerable<Journal>> ProcessSalesDlvReturnJournalAsync(TenantContext db, DateTime date,
        List<SystemParameter> systemParam, List<Item> items, List<Tax> taxes, CancellationToken cancellationToken)
    {
        List<Journal> journals = new();

        _logger.LogInformation("Posting journal DO from SR: get data.");
        var doData =
            await (from dlvheader in db.SalesDeliveryHeaders
            join returnHeader in db.SalesReturnHeaders on dlvheader.TransCode equals returnHeader.Code
            join customer in db.Customers on dlvheader.CustCode equals customer.Code
            where dlvheader.Date.Month == date.Month && dlvheader.Date.Year == date.Year && dlvheader.SrcTrans == 2 &&
                  dlvheader.Mark != "V"
            select new
            {
                DlvHeader = dlvheader, ReturnType = returnHeader.Type, Customer = customer
            }).AsNoTracking().ToListAsync(cancellationToken);
            
        _logger.LogInformation("Posting journal DO from SR: get detail data.");
        var doDetailData = 
            await (from dlvdetail in db.SalesDeliveryDetails
            join item in db.Items on dlvdetail.ItemId equals item.Id
            where doData.Select(d => d.DlvHeader.Code).Contains(dlvdetail.Code)
            select new
            {
                DlvDetail = dlvdetail, Item = item
            }).AsNoTracking().ToListAsync(cancellationToken);
        
        _logger.LogInformation("Posting journal DO from SR: get stock mutation for do data.");
        var smDoData = await db.StockMutations.FromSql(@$";
            SELECT *
            FROM Inventory.StockMutation sm
            WHERE sm.[Type] = 'OH'
            AND sm.Src = 'DO'
            AND EXISTS (
                SELECT 1
                FROM Sales.SalesDeliveryDetail do_d
                INNER JOIN Sales.SalesDeliveryHeader do_h
                    ON do_h.Code = do_d.Code
                WHERE do_h.Mark <> 'V'
                AND MONTH(do_h.[Date]) = {date.Month}
                AND YEAR(do_h.[Date]) = {date.Year}
                AND do_h.SrcTrans = 2
                AND do_d.Id = sm.RefDetailId1
                AND do_d.Code = sm.RefCode1
            )").AsNoTracking().ToListAsync(cancellationToken);
        
        _logger.LogInformation("Posting journal SI: get do journal data for hpp or barang terkirim.");
        var srJournal =
            await db.Journals.AsNoTracking()
                .Where(x => x.SrcTrans == "SR" && doData.Select(d => d.DlvHeader.TransCode).Contains(x.Code))
                .ToListAsync(cancellationToken);
        
        foreach (var itemDlvData in doData)
        {
            short l = 0;
            short m = 0;
            short lx = 0;
            foreach (var itemDetail in doDetailData.Where(x => x.DlvDetail.Code == itemDlvData.DlvHeader.Code))
            {
                _logger.LogInformation($"Posting journal DO from SR: {itemDlvData.DlvHeader.Code} - {itemDlvData.DlvHeader.TransCode} - {itemDetail.Item.Initial} ({itemDetail.Item.Id}).");
                
                var smItemData = smDoData.FirstOrDefault(x => x.RefDetailId1 == itemDetail.DlvDetail.Id && x.RefCode1 == itemDetail.DlvDetail.Code);
                if (smItemData == null) continue;
                
                var resultHpp = smItemData.BaseNettPrice * smItemData.BaseQty;

                //Inventory using COGS for Amount
                journals.Add(new Journal
                {
                    Code = itemDlvData.DlvHeader.Code,
                    LineNo = ++l,
                    Date = itemDlvData.DlvHeader.Date,
                    CoaCode = string.IsNullOrWhiteSpace(items.FirstOrDefault(x => x.Id == itemDetail.DlvDetail.ItemId)?.CoaInventory) ? systemParam.FirstOrDefault(x => x.Code == "INVENTORY_COA")?.Value ?? "" : items.FirstOrDefault(x => x.Id == itemDetail.DlvDetail.ItemId)?.CoaInventory,
                    TypeCode = "DLV_DT",
                    Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_INVENTORY")?.Value ?? ""} {itemDetail.Item.Initial}").Trim(),
                    RefCode1 = itemDlvData.DlvHeader.TransCode,
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
                    RefCode1 = itemDlvData.DlvHeader.TransCode,
                    RefCode2 = itemDetail.Item.Initial,
                    Group = 2,
                    CurrCode = itemDlvData.DlvHeader.CurrCode,
                    Period = itemDlvData.DlvHeader.Date.ToString("yyyyMMdd"),
                    Type = "D",
                    Amount = resultHpp,
                    SrcTrans = "DLV"
                });
                
                //PPN Yang Dibebaskan
                if (itemDetail.DlvDetail.ExemptTaxAmount > 0)
                {
                    journals.Add(new Journal
                    {
                        Code = itemDlvData.DlvHeader.Code,
                        LineNo = ++lx,
                        Date = itemDlvData.DlvHeader.Date,
                        CoaCode = string.IsNullOrWhiteSpace(taxes.FirstOrDefault(x => x.Id == itemDetail.DlvDetail.TaxId)?.ExemptCoaCode) ? "" : taxes.FirstOrDefault(x => x.Id == itemDetail.DlvDetail.TaxId)?.ExemptCoaCode,
                        TypeCode = "DLV_DT",
                        Notes = "PPN Yang Dibebaskan",
                        RefCode1 = itemDlvData.DlvHeader.TransCode,
                        RefCode2 = itemDetail.Item.Initial,
                        Group = 8,
                        CurrCode = itemDlvData.DlvHeader.CurrCode,
                        Period = itemDlvData.DlvHeader.Date.ToString("yyyyMMdd"),
                        Type = "C",
                        Amount = itemDetail.DlvDetail.ExemptTaxAmount * itemDetail.DlvDetail.Qty,
                        SrcTrans = "DLV"
                    });

                    journals.Add(new Journal
                    {
                        Code = itemDlvData.DlvHeader.Code,
                        LineNo = lx,
                        Date = itemDlvData.DlvHeader.Date,
                        CoaCode = string.IsNullOrWhiteSpace(taxes.FirstOrDefault(x => x.Id == itemDetail.DlvDetail.TaxId)?.ExemptCoaCode) ? "" : taxes.FirstOrDefault(x => x.Id == itemDetail.DlvDetail.TaxId)?.ExemptCoaCode,
                        TypeCode = "DLV_DT",
                        Notes = "PPN Yang Dibebaskan",
                        RefCode1 = itemDlvData.DlvHeader.TransCode,
                        RefCode2 = itemDetail.Item.Initial,
                        Group = 6,
                        CurrCode = itemDlvData.DlvHeader.CurrCode,
                        Period = itemDlvData.DlvHeader.Date.ToString("yyyyMMdd"),
                        Type = "D",
                        Amount = itemDetail.DlvDetail.ExemptTaxAmount * itemDetail.DlvDetail.Qty,
                        SrcTrans = "DLV"
                    });
                }
            }
            
            //PPN - Pajak
            if (itemDlvData.DlvHeader.TaxAmount > 0)
            {
                journals.Add(new Journal
                {
                    Code = itemDlvData.DlvHeader.Code,
                    LineNo = 1,
                    Date = itemDlvData.DlvHeader.Date,
                    CoaCode = systemParam.FirstOrDefault(x => x.Code == "TAX_IN_COA")?.Value ?? "",
                    TypeCode = "PPN",
                    Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_TAX_IN")?.Value ?? ""} {itemDlvData.Customer.Initial}").Trim(),
                    RefCode1 = itemDlvData.DlvHeader.TransCode,
                    RefCode2 = "",
                    Group = 5,
                    CurrCode = itemDlvData.DlvHeader.CurrCode,
                    Period = itemDlvData.DlvHeader.Date.ToString("yyyyMMdd"),
                    Type = "C",
                    Amount = srJournal.FirstOrDefault(x => x.Code == itemDlvData.DlvHeader.TransCode && x.Group == 3)?.Amount ?? 0m,
                    SrcTrans = "DLV"
                });
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
                RefCode1 = itemDlvData.DlvHeader.TransCode,
                Group = 1,
                CurrCode = itemDlvData.DlvHeader.CurrCode,
                Period = itemDlvData.DlvHeader.Date.ToString("yyyyMMdd"),
                Type = "D",
                Amount = itemDlvData.ReturnType == 2
                    ? itemDlvData.DlvHeader.Total
                    : srJournal.FirstOrDefault(x => x.Code == itemDlvData.DlvHeader.TransCode && x.Group == 5)?.Amount ?? 0m,
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
                RefCode1 = itemDlvData.DlvHeader.TransCode,
                Group = 3,
                CurrCode = itemDlvData.DlvHeader.CurrCode,
                Period = itemDlvData.DlvHeader.Date.ToString("yyyyMMdd"),
                Type = "C",
                Amount = itemDlvData.DlvHeader.Total - journals.Where(x => x.Code == itemDlvData.DlvHeader.Code && x.Group == 5).Sum(x => x.Amount),
                SrcTrans = "DLV"
            });
            
            if (itemDlvData.ReturnType == 3)
            {
                var ciValue = (journals.FirstOrDefault(x => x.Code == itemDlvData.DlvHeader.Code && x.Group == 1)?.Amount ?? 0m) - (journals.FirstOrDefault(x => x.Code == itemDlvData.DlvHeader.Code && x.Group == 3)?.Amount ?? 0m);
                journals.Add(new Journal
                {
                    Code = itemDlvData.DlvHeader.Code,
                    LineNo = 1,
                    Date = itemDlvData.DlvHeader.Date,
                    CoaCode = systemParam.FirstOrDefault(x => x.Code == (ciValue < 0 ? "OTH_EXPENSE_COA" : "OTH_INCOME_COA"))?.Value ?? "",
                    TypeCode = "DLV",
                    Notes = ($"{systemParam.FirstOrDefault(x => x.Code == (ciValue < 0 ? "JR_PREFIX_OTH_EXPENSE" : "JR_PREFIX_OTH_INCOME"))?.Value ?? ""} {itemDlvData.Customer.Initial}").Trim(),
                    RefCode1 = "",
                    Group = 7,
                    CurrCode = itemDlvData.DlvHeader.CurrCode,
                    Period = itemDlvData.DlvHeader.Date.ToString("yyyyMMdd"),
                    Type = (ciValue < 0 ? "D" : "C"),
                    Amount = Math.Abs(ciValue),
                    SrcTrans = "DLV"
                });
            }
        }
        
        return journals;
    }
    
    private IEnumerable<Journal> ProcessExpeditionJournal(TenantContext db, DateTime dateTime, List<SystemParameter> systemParam)
    {
        List<Journal> journals = new();
        var expeditionData = (from expheader in db.ExpeditionInvoiceHeaders
                              join supplier in db.Suppliers on expheader.SupCode equals supplier.Code
                              where expheader.Date.Month == dateTime.Month && expheader.Date.Year == dateTime.Year && expheader.Mark != "V"
                              select new { ExpHeader = expheader, Supplier = supplier }).AsNoTracking().ToList();
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

    private IEnumerable<Journal> ProcessFixedAssetJournal(TenantContext db, DateTime dateTime, List<SystemParameter> systemParam)
    {
        List<Journal> journals = new();
        var fixedAssetData = (from fixedAsset in db.FixedAssets
                              join supplier in db.Suppliers on fixedAsset.SupCode equals supplier.Code
                              join assetType in db.AssetTypes on fixedAsset.TypeId equals assetType.Id
                              where fixedAsset.PurchaseDate.Month == dateTime.Month && fixedAsset.PurchaseDate.Year == dateTime.Year && fixedAsset.Mark != "V"
                              select new { FixedAsset = fixedAsset, Supplier = supplier, AssetType = assetType }).AsNoTracking().ToList();
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

    private IEnumerable<Journal> ProcessDepreciationFixedAssetJournal(TenantContext db, DateTime dateTime, List<SystemParameter> systemParam)
    {
        List<Journal> journals = new();
        var fixedAssetData = (from fixedAsset in db.FixedAssets
                              join supplier in db.Suppliers on fixedAsset.SupCode equals supplier.Code
                              join assetType in db.AssetTypes on fixedAsset.TypeId equals assetType.Id
                              where fixedAsset.DepreciationMethod == 2 && fixedAsset.StartDepreciateOn.Month <= dateTime.Month &&
                                    fixedAsset.StartDepreciateOn.Month + fixedAsset.EstimatedLife >= dateTime.Month && fixedAsset.PurchaseDate.Year == dateTime.Year && fixedAsset.Mark != "V"
                              select new { FixedAsset = fixedAsset, Supplier = supplier, AssetType = assetType }).AsNoTracking().ToList();

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
                    var hData = db.FixedAssetHistories.FirstOrDefault(x => x.Code == itemData.FixedAsset.Code && x.NumberOfMonth == dataMonth.Month);
                    if (hData == null)
                    {
                        db.FixedAssetHistories.Add(new FixedAssetHistory
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

                        db.FixedAssetHistories.Update(hData);
                    }
                    db.SaveChanges();

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

    private IEnumerable<Journal> ProcessEndYearAssetJournal(TenantContext db, DateTime dateTime, List<SystemParameter> systemParam, IEnumerable<Journal> depreJournals)
    {
        List<Journal> journals = new();

        if (dateTime.Month == 12)
        {
            var historyJournal = db.Journals.Where(x => x.TypeCode == "ASSET" && new[] { 1, 4 }.Contains(x.Group) && x.Date.Year == dateTime.Year).ToList();
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

    private IEnumerable<Journal> ProcessBBInventoryJournal(TenantContext db, List<SystemParameter> systemParam)
    {
        List<Journal> journals = new();
        var startDate = Convert.ToDateTime(systemParam.FirstOrDefault(x => x.Code == "DATA_START_DATE").Value).AddDays(-1);
        var latestData = db.VwBeginningBalanceStockHeaders.AsNoTracking().OrderByDescending(x => x.Date).FirstOrDefault();
        if (latestData?.Date.Month == startDate.Month && latestData?.Date.Year == startDate.Year)
        {
            var bbapData = db.VwBeginningBalanceStockHeaders.AsNoTracking().ToList();
            foreach (var item in bbapData)
            {
                var detailData = db.VwBeginningBalanceStockDetails.AsNoTracking().Where(x => x.Code == item.Code).ToList();

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

    private void ProcessEndYearJournal(TenantContext db, DateTime dateTime)
    {
        db.Database.ExecuteSqlRaw(@"CREATE TABLE #tmp_jur (
	                    [jur_code] [varchar](40) NOT NULL,
	                    [jur_code_dt] [varchar](50) NOT NULL,
	                    [jur_date] [date] NOT NULL,
	                    [jur_coa] [varchar](6) NOT NULL,
	                    [jur_type] [varchar](20) NOT NULL,
	                    [jur_notes] [varchar](256) NOT NULL,
	                    [jur_ref_1] [varchar](40) NOT NULL,
	                    [jur_ref_2] [varchar](40) NOT NULL,
	                    [jur_ref_3] [varchar](40) NOT NULL,
	                    [jur_ref_4] [varchar](40) NOT NULL,
	                    [jur_group] [int] NOT NULL,
	                    [jur_curr] [varchar](3) NOT NULL,
	                    [jur_period] [varchar](8) NOT NULL,
	                    [jur_custom_rate] [decimal](18, 6) NOT NULL,
	                    [jur_dc] [varchar](1) NOT NULL,
	                    [jur_amount] [decimal](18, 6) NOT NULL,
	                    [jur_src] [varchar](10) NOT NULL
                    ) ON [PRIMARY];

                    DECLARE @RefDate DATE, @StartDate DATE, @EndDate DATE
                        SET @RefDate = {0}
                        SET @StartDate = DATEADD(yy, DATEDIFF(yy, 0, @RefDate), 0)
                        SET @EndDate = DATEADD(yy, 1, DATEADD(d, -1, @StartDate))
                   
                    ;WITH cte_coa AS (
                        SELECT Code
                        FROM Accounting.COA
                        WHERE Code >= '400000'
                    )
                    ,cte_endyear_1 AS (
                        SELECT TBA.*
                        ,CASE WHEN TBA.CurrCode = 'IDR'
                            THEN 1
                            ELSE CASE WHEN TBA.Period = 'CUSTOM' THEN CustomRate ELSE ISNULL(TBC.Amount,0) END
                        END AS currRate
                        FROM (
                            SELECT CoaCode
                            ,Period,CurrCode,CustomRate
                            ,CASE WHEN [Type] = 'D' THEN Amount ELSE -Amount END AS amountOc
                            FROM Accounting.Journal
                            WHERE YEAR([Date]) = YEAR(@RefDate)
                            AND Code <> 'ENDYEAR' + FORMAT(@RefDate,'yyyy')
                        ) TBA
                        INNER JOIN cte_coa TBB
                            ON TBB.Code = TBA.CoaCode
                        LEFT JOIN Accounting.CurrencyRate TBC
                            ON TBC.CurrCode = TBA.CurrCode
                    )
                    ,cte_endyear_2 AS (
                        SELECT CoaCode
                        ,SUM(amountOc * currRate) AS amountIdr
                        FROM cte_endyear_1
                        GROUP BY CoaCode
                    )
                    SELECT *
                    ,'ENDYEAR' + FORMAT(@RefDate,'yyyy') AS jur_code
                    ,@EndDate AS jur_date,'ADJ_ENDYEAR' AS jur_type
                    ,'' AS jur_ref1,'' AS jur_ref2,'' AS jur_ref3,'' AS jur_ref4
                    ,1 AS jur_group,'IDR' AS jur_curr,'CUSTOM' AS jur_period,0 AS jur_custom_rate
                    ,CASE WHEN amountIdr < 0 THEN 'C' ELSE 'D' END AS jur_dc
                    ,CASE WHEN amountIdr < 0 THEN -amountIdr ELSE amountIdr END AS jur_amount
                    INTO #TmpJurEndYear
                    FROM cte_endyear_2

                    INSERT INTO #tmp_jur (jur_code,jur_code_dt,jur_date,jur_coa,jur_type
                        ,jur_notes,jur_ref_1,jur_ref_2,jur_ref_3,jur_ref_4,jur_group,jur_curr,jur_period,jur_custom_rate
                        ,jur_dc,jur_amount,jur_src
                    )
                    SELECT jur_code,'',jur_date
                    ,'320001'
                    , jur_type
                    , 'Laba Ditahan'
                    ,jur_ref1,jur_ref2,jur_ref3,jur_ref4,jur_group,jur_curr,jur_period,jur_custom_rate
                    ,jur_dc,jur_amount,'ENDYEAR'
                    FROM #TmpJurEndYear;
                
                    INSERT INTO #tmp_jur (jur_code,jur_code_dt,jur_date,jur_coa,jur_type
                        ,jur_notes,jur_ref_1,jur_ref_2,jur_ref_3,jur_ref_4,jur_group,jur_curr,jur_period,jur_custom_rate
                        ,jur_dc,jur_amount,jur_src
                    )
                    SELECT jur_code,'',jur_date
                    ,CoaCode,jur_type,'Laba Ditahan'
                    ,jur_ref1,jur_ref2,jur_ref3,jur_ref4,jur_group,jur_curr,jur_period,jur_custom_rate
                    ,CASE WHEN jur_dc = 'D' THEN 'C' ELSE 'D' END
                    ,jur_amount,'ENDYEAR'
                    FROM #TmpJurEndYear;
                
                    DROP TABLE #TmpJurEndYear;

                    WITH cte_tmp_jur_group AS(
                        SELECT jur_code, jur_date
                        , jur_coa, jur_type, jur_notes
                        , jur_ref_1, jur_ref_2, jur_ref_3, jur_ref_4
                        , jur_group
                        , jur_curr, jur_period, jur_custom_rate
                        , SUM (
                            CASE WHEN jur_dc = 'D' THEN jur_amount
                                ELSE -jur_amount END
                        ) AS jur_amount
                        , jur_src
                        FROM #tmp_jur
                        GROUP BY jur_code, jur_date
                        , jur_coa, jur_type, jur_notes
                        , jur_ref_1, jur_ref_2, jur_ref_3, jur_ref_4
                        , jur_group
                        , jur_curr, jur_period, jur_custom_rate
                        , jur_src
                    )
                    SELECT jur_code, jur_date
                    , jur_coa, jur_type, jur_notes
                    , jur_ref_1, jur_ref_2, jur_ref_3, jur_ref_4
                    , jur_group
                    , jur_curr, jur_period, jur_custom_rate
                    , CASE WHEN jur_amount > 0 THEN 'D'
                        ELSE 'C' END AS jur_dc
                    ,CASE WHEN jur_amount > 0 THEN jur_amount
                        ELSE - jur_amount END AS jur_amount
                    ,jur_src
                    ,ROW_NUMBER() OVER(PARTITION BY jur_code ORDER BY jur_code, jur_group, jur_type, jur_amount) AS jur_lineno
                    INTO #tmp_jur_final
                    FROM cte_tmp_jur_group;


                DELETE FROM Accounting.Journal
                WHERE EXISTS(
                    SELECT jur_code

                    FROM #tmp_jur_final
                        WHERE jur_code = Code
                );

                INSERT INTO Accounting.Journal
                SELECT jur_code,jur_lineno,jur_date
                    ,jur_coa,jur_type,jur_notes
                    ,jur_ref_1,jur_ref_2,jur_ref_3,jur_ref_4
                    ,jur_group
                    ,jur_curr,jur_period,jur_custom_rate,jur_dc,jur_amount
                    ,jur_src
                    FROM #tmp_jur_final
                    WHERE jur_amount > 0;

                DROP TABLE #tmp_jur_final;
                    DELETE FROM #tmp_jur;


                    DROP TABLE #tmp_jur;", dateTime);
    }

    private IEnumerable<Journal> ProcessAdjustmentJournal(TenantContext db, DateTime dateTime, List<SystemParameter> systemParam)
    {
        List<Journal> journals = new();

        var adjData = db.AdjustmentHeaders.AsNoTracking().Where(x => x.Date.Month == dateTime.Month && x.Date.Year == dateTime.Year && x.Mark != "V").ToList();

        short i = 0;
        short j = 0;
        foreach (var itemData in adjData)
        {
            var adjDetailData = (from adjdetail in db.AdjustmentDetails
                                 join item in db.Items on adjdetail.ItemId equals item.Id
                                 where adjdetail.Code == itemData.Code
                                 select new { AdjDetail = adjdetail, Item = item }).AsNoTracking().ToList();

            short k = 0;
            short l = 0;
            foreach (var itemDetail in adjDetailData)
            {
                var smData = db.StockMutations.AsNoTracking().FirstOrDefault(x => x.RefDetailId1 == itemDetail.AdjDetail.Id && x.RefCode1 == itemDetail.AdjDetail.Code);
                if (smData == null) continue;

                //var nonVoidSM = RemoveVoidSM(db, db.StockMutations.ToList());
                //CalculateHPP(db, nonVoidSM, smData.ItemId, smData.RefDetailId1, "ADJ");
                //smData = db.StockMutations.AsNoTracking().FirstOrDefault(x => x.RefDetailId1 == itemDetail.AdjDetail.Id && x.RefCode1 == itemDetail.AdjDetail.Code);
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

    private IEnumerable<Journal> ProcessGeneralJournal(TenantContext db, DateTime dateTime)
    {
        List<Journal> journals = new();

        var dataHeader = db.GeneralJournalHeaders.AsNoTracking()
            .Where(x => x.Date.Month == dateTime.Month && x.Date.Year == dateTime.Year && x.Mark != "V")
            .ToList();

        foreach (var itemData in dataHeader)
        {
            var dataDetail = db.GeneralJournalDetails.AsNoTracking().Where(x => x.Code == itemData.Code).ToList();

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

    private IEnumerable<Journal> ProcessTransferStockJournal(TenantContext db, DateTime dateTime, List<SystemParameter> systemParam)
    {
        List<Journal> journals = new();

        var dataHeader = db.TransferStockHeaders.AsNoTracking()
            .Where(x => x.Date.Month == dateTime.Month && x.Date.Year == dateTime.Year && x.Mark != "V")
            .ToList();

        var dataHPP = (new[] { new { Code = "", ItemId = 0, UnitId = 0, HPP = 0m } }).ToList();

        foreach (var itemData in dataHeader)
        {
            var dataDetail = (from tsdetail in db.TransferStockDetails
                              join item in db.Items on tsdetail.ItemId equals item.Id
                              where tsdetail.Code == itemData.Code
                              select new { TsDetail = tsdetail, Item = item }).AsNoTracking().ToList();

            short i = 0;
            short j = 0;
            foreach (var itemDetail in dataDetail)
            {
                var smData = db.StockMutations.AsNoTracking().FirstOrDefault(x => x.RefDetailId1 == itemDetail.TsDetail.Id && x.RefCode1 == itemDetail.TsDetail.Code);
                if (smData == null) continue;

                //var nonVoidSM = RemoveVoidSM(db, db.StockMutations.ToList());
                var resultHpp = 0m;
                if (itemData.Type != "IN")
                {
                    //var srcType = new[] { "OUT", "DT" }.Contains(itemData.Type) ? "TS" : "CNEE";
                    //CalculateHPP(db, nonVoidSM, smData.ItemId, smData.RefDetailId1, srcType);
                    //smData = db.StockMutations.AsNoTracking().FirstOrDefault(x => x.RefDetailId1 == itemDetail.TsDetail.Id && x.RefCode1 == itemDetail.TsDetail.Code);
                    resultHpp = smData.BaseNettPrice > 0 ? smData.BaseNettPrice * smData.BaseQty : 0m;
                }

                if (itemData.Type == "OUT")
                    dataHPP.Add(new { Code = itemData.Code, ItemId = itemDetail.TsDetail.ItemId, UnitId = itemDetail.TsDetail.UnitId, HPP = resultHpp });

                if (itemData.Type == "IN")
                {
                    var valueHPP = dataHPP.FirstOrDefault(x => x.Code == itemData.OriginTransferCode && x.ItemId == itemDetail.TsDetail.ItemId && x.UnitId == itemDetail.TsDetail.UnitId);
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
                    Amount = Math.Abs(resultHpp),
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
                        Amount = Math.Abs(resultHpp),
                        SrcTrans = "TS"
                    });
                }
            }

            if (!new[] { "C", "RC", "DT" }.Contains(itemData.Type))
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
                    Group = 3,
                    CurrCode = "IDR",
                    Period = itemData.Date.ToString("yyyyMMdd"),
                    Type = new[] { "C", "RC", "OUT", "DT" }.Contains(itemData.Type) ? "D" : "C",
                    Amount = journals.Where(x => x.Code == itemData.Code && x.Group == 1).Sum(x => x.Amount),
                    SrcTrans = "TS"
                });

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

    private IEnumerable<Journal> ProcessSalesDownPaymentJournal(TenantContext db, List<SystemParameter> systemParam)
    {
        List<Journal> journals = new();
        var SdpData = db.VwCreditMemos.AsNoTracking().Where(x => new[] { 3, 4 }.Contains(x.SrcTrans) && x.Mark != "V").ToList();

        foreach (var itemData in SdpData)
        {
            if (itemData.SrcTrans == 3)
            {
                //Piutang Dagang
                journals.Add(new Journal
                {
                    Code = itemData.Code,
                    LineNo = 1,
                    Date = itemData.Date,
                    CoaCode = systemParam.FirstOrDefault(x => x.Code == "AR_COA")?.Value ?? "",
                    TypeCode = "AR",
                    Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_AR")?.Value ?? ""} {itemData.CustInitial}").Trim(),
                    RefCode1 = itemData.TransCode,
                    Group = 1,
                    CurrCode = itemData.CurrCode,
                    Period = itemData.Date.ToString("yyyyMMdd"),
                    Type = "D",
                    Amount = itemData.Amount,
                    SrcTrans = "DP"
                });

                //Pajak
                if (itemData.TaxAmount > 0)
                {
                    journals.Add(new Journal
                    {
                        Code = itemData.Code,
                        LineNo = 1,
                        Date = itemData.Date,
                        CoaCode = systemParam.FirstOrDefault(x => x.Code == "TAX_OUT_COA")?.Value ?? "",
                        TypeCode = "PPN",
                        Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_TAX_OUT")?.Value ?? ""} {itemData.CustInitial}").Trim(),
                        RefCode1 = itemData.TransCode,
                        Group = 2,
                        CurrCode = itemData.CurrCode,
                        Period = itemData.Date.ToString("yyyyMMdd"),
                        Type = "C",
                        Amount = itemData.TaxAmount,
                        SrcTrans = "DP"
                    });
                }

                //Uang Muka
                journals.Add(new Journal
                {
                    Code = itemData.Code,
                    LineNo = 1,
                    Date = itemData.Date,
                    CoaCode = systemParam.FirstOrDefault(x => x.Code == "SLS_DP_COA")?.Value ?? "",
                    TypeCode = "SDP",
                    Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_SLS_DP")?.Value ?? ""} {itemData.CustInitial}").Trim(),
                    RefCode1 = itemData.TransCode,
                    Group = 3,
                    CurrCode = itemData.CurrCode,
                    Period = itemData.Date.ToString("yyyyMMdd"),
                    Type = "C",
                    Amount = itemData.Total - itemData.TaxAmount,
                    SrcTrans = "DP"
                });
            }
            else
            {
                var srcData = SdpData.FirstOrDefault(x => x.Code == itemData.TransCode);

                //Piutang Dagang
                journals.Add(new Journal
                {
                    Code = itemData.Code,
                    LineNo = 1,
                    Date = itemData.Date,
                    CoaCode = systemParam.FirstOrDefault(x => x.Code == "AR_COA")?.Value ?? "",
                    TypeCode = "AR",
                    Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_AR")?.Value ?? ""} {itemData.CustInitial}").Trim(),
                    RefCode1 = itemData.TransCode,
                    RefCode2 = srcData.TransCode,
                    Group = 3,
                    CurrCode = itemData.CurrCode,
                    Period = itemData.Date.ToString("yyyyMMdd"),
                    Type = "C",
                    Amount = itemData.Amount,
                    SrcTrans = "DP"
                });

                //Pajak
                if (itemData.TaxAmount > 0)
                {
                    journals.Add(new Journal
                    {
                        Code = itemData.Code,
                        LineNo = 1,
                        Date = itemData.Date,
                        CoaCode = systemParam.FirstOrDefault(x => x.Code == "TAX_OUT_COA")?.Value ?? "",
                        TypeCode = "PPN",
                        Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_TAX_OUT")?.Value ?? ""} {itemData.CustInitial}").Trim(),
                        RefCode1 = itemData.TransCode,
                        RefCode2 = srcData.TransCode,
                        Group = 1,
                        CurrCode = itemData.CurrCode,
                        Period = itemData.Date.ToString("yyyyMMdd"),
                        Type = "D",
                        Amount = itemData.TaxAmount,
                        SrcTrans = "DP"
                    });
                }

                //Uang Muka
                journals.Add(new Journal
                {
                    Code = itemData.Code,
                    LineNo = 1,
                    Date = itemData.Date,
                    CoaCode = systemParam.FirstOrDefault(x => x.Code == "SLS_DP_COA")?.Value ?? "",
                    TypeCode = "SDP",
                    Notes = ($"{systemParam.FirstOrDefault(x => x.Code == "JR_PREFIX_SLS_DP")?.Value ?? ""} {itemData.CustInitial}").Trim(),
                    RefCode1 = itemData.TransCode,
                    RefCode2 = srcData.TransCode,
                    Group = 2,
                    CurrCode = itemData.CurrCode,
                    Period = itemData.Date.ToString("yyyyMMdd"),
                    Type = "D",
                    Amount = itemData.Amount - itemData.TaxAmount,
                    SrcTrans = "DP"
                });
            }

        }

        return journals;
    }


    private void CalculateHPP(TenantContext db, IEnumerable<StockMutation> stockMutations, int itemId, long id, string srcCode)
    {
        DateTime latestDate;
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
            latestDate = firstSM.Date;
            latestStockValue += firstSM.BaseNettPrice * firstSM.BaseQty;
            latestQty += firstSM.BaseQty;
            hpp = latestStockValue / latestQty;

            List<string> srcType = new() { "BB", "RCV", "DO", "DOF", "SR", "ADJ", "TS", "PR", "CNEE" };

            var doData = db.SalesDeliveryHeaders.ToList();

            var srData = db.SalesReturnHeaders.ToList();

            var rcvData = db.PurchaseReceiveHeaders.ToList();

            var tsData = db.TransferStockHeaders.ToList();

            var prData = db.PurchaseReturnHeaders.ToList();

            var orderQuery = @" ORDER BY sm.Date, CASE
					WHEN sm.Src = 'BB' THEN 1
					WHEN sm.Src = 'RCV' AND rcv.SrcTrans = 1 THEN 2
					WHEN sm.Src = 'RCV' AND rcv.SrcTrans = 2 THEN 3
					WHEN sm.Src = 'ADJ' AND sm.BaseQty > 0 THEN 3
					WHEN sm.Src = 'SR' THEN 3
					WHEN sm.Src IN('TS', 'CNEE') AND sm.[Type] = 'OH' AND sm.BaseQty > 0 THEN 3
					WHEN sm.Src = 'DO' AND sr.[Type] = 2 THEN 4
					WHEN sm.Src = 'ADJ' AND sm.BaseQty < 0 THEN 5
					WHEN sm.Src IN('DO', 'DOF', 'PR') THEN 5
					WHEN sm.Src IN('TS', 'CNEE') AND sm.[Type] = 'OH' AND sm.BaseQty < 0 THEN 5
					ELSE 6
					END, sm.Id";

            var listSM = db.StockMutations.FromSqlRaw(@"SELECT sm.*
				FROM Inventory.StockMutation sm
				LEFT JOIN Inventory.Item im on im.Id = sm.ItemId
				LEFT JOIN Sales.SalesDeliveryHeader do ON do.Code = sm.RefCode1
				LEFT JOIN Sales.SalesReturnHeader sr ON sr.Code = do.TransCode
                LEFT JOIN Purchasing.PurchaseReceiveHeader rcv ON rcv.Code = sm.RefCode1
				WHERE sm.Src IN ('RCV','DO','TS','ADJ','BB','PR','CNEE','DOF','SR') AND sm.[Type] = 'OH' " +
                                                      $"AND sm.ItemId = {itemId} AND sm.BaseQty != 0 AND sm.Date >= '{firstSM.Date}' " +
                                                      $"AND sm.Date <= '{currentSM.Date}' AND sm.Id != {firstSM.Id}" + orderQuery).ToList();

            foreach (var item in listSM)
            {
                if (new[] { "RCV", "BB", "SR" }.Contains(item.Src))
                {
                    var rcvFromPRSI =
                        item.Src == "RCV" &&
                        (rcvData.FirstOrDefault(x => x.Code == item.RefCode1)?.SrcTrans ?? 0) == 2 &&
                        (prData.FirstOrDefault(x => x.Code == item.RefCode2)?.Type ?? 0) == 2;
                    if (rcvFromPRSI) // same item
                    {
                        item.BaseNettPrice = listSM.FirstOrDefault(x => x.RefCode1 == item.RefCode2)?.BaseNettPrice ?? 0m;
                        item.NettPrice = listSM.FirstOrDefault(x => x.RefCode1 == item.RefCode2)?.NettPrice ?? 0m;
                        latestStockValue += item.BaseNettPrice * item.BaseQty;
                        latestQty += item.BaseQty;
                        db.StockMutations.Update(item);
                    }
                    else
                    {
                        var rcvFromPRDI = (item.Src == "RCV" && (prData.FirstOrDefault(z => z.Code == (rcvData.FirstOrDefault(y => y.Code == item.RefCode1)?.TransCode ?? ""))?.Type ?? 0) == 3);
                        if (item.Src == "SR" || rcvFromPRDI)
                        {
                            item.BaseNettPrice = hpp;
                            item.NettPrice = hpp * item.BaseQty / item.Qty;
                            db.StockMutations.Update(item);
                        }
                        latestStockValue += item.BaseNettPrice * item.BaseQty;
                        latestQty += item.BaseQty;
                        if (item.Src == "BB" || item.Src == "RCV" && (rcvData.FirstOrDefault(x => x.Code == item.RefCode1)?.SrcTrans ?? 0) == 1)
                        {
                            if (latestStockValue > 0 && latestQty > 0)
                                hpp = latestStockValue / latestQty;
                        }
                    }
                }
                else if (new[] { "ADJ", "TS", "CNEE" }.Contains(item.Src))
                {
                    if (item.Src == "TS" && (tsData.FirstOrDefault(x => x.Code == item.RefCode1)?.Type ?? "") == "OUT")
                    {
                        if (latestDate != item.Date)
                        {
                            if (latestStockValue > 0 && latestQty > 0)
                                hpp = latestStockValue / latestQty;
                        }

                        item.BaseNettPrice = hpp;
                        item.NettPrice = hpp * item.BaseQty / item.Qty;
                    }
                    else if (item.Src == "TS" && (tsData.FirstOrDefault(x => x.Code == item.RefCode1)?.Type ?? "") == "IN")
                    {
                        item.BaseNettPrice = listSM.FirstOrDefault(x => x.RefCode1 == item.RefCode2)?.BaseNettPrice ?? 0m;
                        item.NettPrice = listSM.FirstOrDefault(x => x.RefCode1 == item.RefCode2)?.NettPrice ?? 0m;
                    }
                    else
                    {
                        if (latestDate != item.Date)
                        {
                            if (latestStockValue > 0 && latestQty > 0)
                                hpp = latestStockValue / latestQty;
                        }

                        item.BaseNettPrice = hpp;
                        item.NettPrice = hpp * item.BaseQty / item.Qty;
                    }

                    latestStockValue += item.BaseNettPrice * item.BaseQty;
                    latestQty += item.BaseQty;
                    db.StockMutations.Update(item);
                }
                else
                {
                    if (latestDate != item.Date)
                    {
                        if (latestStockValue > 0 && latestQty > 0)
                            hpp = latestStockValue / latestQty;
                    }

                    var dofromPR = item.Src == "DO" && (srData.FirstOrDefault(z => z.Code == (doData.FirstOrDefault(y => y.Code == item.RefCode1)?.TransCode ?? ""))?.Type ?? 0) == 2;
                    if (dofromPR) // same item
                    {
                        item.BaseNettPrice = listSM.FirstOrDefault(x => x.RefCode1 == item.RefCode2)?.BaseNettPrice ?? 0m;
                        item.NettPrice = listSM.FirstOrDefault(x => x.RefCode1 == item.RefCode2)?.NettPrice ?? 0m;
                        latestStockValue -= item.BaseNettPrice * item.BaseQty;
                        latestQty -= item.BaseQty;
                        db.StockMutations.Update(item);
                    }
                    else
                    {
                        if (item.Src == "DO" && (srData.FirstOrDefault(z => z.Code == (doData.FirstOrDefault(y => y.Code == item.RefCode1)?.TransCode ?? ""))?.Type ?? 0) == 3)
                        {
                            item.BaseNettPrice = hpp;
                            item.NettPrice = hpp * item.BaseQty / item.Qty;
                        }
                        else
                        {
                            item.BaseNettPrice = hpp;
                            item.NettPrice = hpp * item.BaseQty / item.Qty;
                        }
                        latestStockValue -= item.BaseNettPrice * item.BaseQty;
                        latestQty -= item.BaseQty;
                        db.StockMutations.Update(item);
                    }
                }

                latestDate = item.Date;
            }

            db.SaveChanges();
        }
    }

    private static List<StockMutation> RemoveVoidSM(TenantContext db, List<StockMutation> data)
    {
        var result = data;
        var listVoid = new List<string>();

        listVoid.AddRange(db.PurchaseReceiveHeaders.Where(x => x.Mark == "V").Select(x => x.Code).ToList());
        listVoid.AddRange(db.SalesDeliveryHeaders.Where(x => x.Mark == "V").Select(x => x.Code).ToList());
        listVoid.AddRange(db.AdjustmentHeaders.Where(x => x.Mark == "V").Select(x => x.Code).ToList());
        listVoid.AddRange(db.TransferStockHeaders.Where(x => x.Mark == "V").Select(x => x.Code).ToList());
        listVoid.AddRange(db.PurchaseReturnHeaders.Where(x => x.Mark == "V").Select(x => x.Code).ToList());
        listVoid.AddRange(db.SalesReturnHeaders.Where(x => x.Mark == "V").Select(x => x.Code).ToList());

        result = result.Where(x => !listVoid.Contains(x.RefCode1)).ToList();

        return result;
    }

    public IEnumerable<PostingLog> GetPostingHistory(JournalRequest data)
    {
        var plData = _ctx.PostingLogs.ToList();
        return plData.Where(x => x.Period.StartsWith(data.Date.Year.ToString()));
    }

    public PostingState GetPostingState(int userId)
    {
        var data = _ctx.PostingStates.OrderByDescending(x => x.Id).FirstOrDefault(x => x.UserId == userId);
        return data;
    }

    public bool CheckPrevPeriod(DateTime postDate)
    {
        var bbPeriod =
            Convert
                .ToDateTime(_ctx.SystemParameters.FirstOrDefault(x => x.Code == "DATA_START_DATE")?.Value)
                .AddMonths(-1);

        DateTime startDate = new(postDate.Year, 1, 1);
        startDate = startDate > bbPeriod ? startDate : bbPeriod;
        DateTime endDate = new(postDate.Year, postDate.Month, 1);
        for (var dataMonth = startDate; dataMonth.Date < endDate.Date; dataMonth = dataMonth.AddMonths(1))
        {
            var plData = _ctx.PostingLogs.FirstOrDefault(x => x.Period == $"{dataMonth.Year}{(dataMonth.Month > 9 ? dataMonth.Month : "0" + dataMonth.Month)}");
            if (plData == null)
            {
                _ctx.PostingLogs.Add(new PostingLog
                {
                    Period = $"{dataMonth.Year}{(dataMonth.Month > 9 ? dataMonth.Month : "0" + dataMonth.Month)}",
                    IsPosted = false
                });
                _ctx.SaveChanges();

                return false;
            }

            if (!plData.IsPosted)
                return false;
        }
        return true;
    }

    private async Task NewCalculateHPPAsync(TenantContext db, DateTime date, PostingState stateData,
        CancellationToken cancellationToken)
    {
        var doData =
            await db.SalesDeliveryHeaders.AsNoTracking()
                .Where(x => x.Date.Year == date.Year && x.Date.Month == date.Month && x.Mark != "V")
                .ToListAsync(cancellationToken);

        var doTransCodeLists = doData.Where(x => x.SrcTrans == 2).Select(x => x.TransCode).ToList();
        var srData =
            await db.SalesReturnHeaders.AsNoTracking()
                .Where(x => ((x.Date.Year == date.Year && x.Date.Month == date.Month) || doTransCodeLists.Contains(x.Code))
                            && x.Mark != "V")
                .ToListAsync(cancellationToken);

        var rcvData =
            await db.PurchaseReceiveHeaders.AsNoTracking()
                .Where(x => x.Date.Year == date.Year && x.Date.Month == date.Month && x.Mark != "V")
                .ToListAsync(cancellationToken);

        var tsData =
            await db.TransferStockHeaders.AsNoTracking()
                .Where(x => x.Date.Year == date.Year && x.Date.Month == date.Month && x.Mark != "V")
                .ToListAsync(cancellationToken);

        var rcvTransCodeLists = rcvData.Where(x => x.SrcTrans == 2).Select(x => x.TransCode).ToList();
        var prData =
            await db.PurchaseReturnHeaders.AsNoTracking()
                .Where(x => ((x.Date.Year == date.Year && x.Date.Month == date.Month) || rcvTransCodeLists.Contains(x.Code))
                            && x.Mark != "V")
                .ToListAsync(cancellationToken);

        var curMonthItem =
            await db.StockMutations.AsNoTracking()
                .Where(x => x.Date.Year == date.Year && x.Date.Month == date.Month)
                .GroupBy(x => x.ItemId).Select(x => x.First())
                .ToListAsync(cancellationToken);

        var latestItemCogsHistoryList =
            await db.ItemCogsHistories.FromSqlRaw(@$"WITH cte_max_date_item_cogs_history AS (
                    SELECT ItemId, MAX([Date]) AS max_date 
                    FROM Inventory.ItemCogsHistory
                    WHERE [Date] < '{new DateTime(date.Year, date.Month, 1)}'
                    GROUP BY ItemId
                )
                SELECT ich.*
                FROM Inventory.ItemCogsHistory ich
                RIGHT JOIN cte_max_date_item_cogs_history cte_ich
                    ON cte_ich.ItemId = ich.ItemId
                    AND cte_ich.max_date = ich.[Date]")
                .ToListAsync(cancellationToken);

        // Update posting state notes 
        var countMonthItem = curMonthItem.Count;
        stateData.Notes = $"Calculate HPP {countMonthItem} items.";
        await db.SaveChangesAsync(cancellationToken);
        _logger.LogInformation(stateData.Notes);

        foreach (var (curItem, curIdx) in curMonthItem.Select((item, index) => (item, index)))
        {
            // Update posting state notes 
            stateData.Notes = $"Calculate HPP item {curIdx} of {countMonthItem}, ItemId: {curItem.ItemId}.";
            await db.SaveChangesAsync(cancellationToken);
            _logger.LogInformation(stateData.Notes);
            
            // Get latest stock cogs
            DateTime latestDate = new();
            decimal latestQty = 0;
            decimal latestStockValue = 0;
            decimal hpp = 0;
            var latestItemCogsHistory = latestItemCogsHistoryList.FirstOrDefault(x => x.ItemId == curItem.ItemId);
            if (latestItemCogsHistory != null)
            {
                latestQty = latestItemCogsHistory.TotalBaseQty;
                latestStockValue = latestItemCogsHistory.TotalBaseNettPrice;
                if (latestStockValue > 0 && latestQty > 0)
                    hpp = latestStockValue / latestQty;
            }

            var orderQuery = @"ORDER BY sm.[Date], CASE
				WHEN sm.Src = 'BB' THEN 1
				WHEN sm.Src = 'RCV' AND rcv.SrcTrans = 1 THEN 2
				WHEN sm.Src = 'RCV' AND rcv.SrcTrans = 2 THEN 3
				WHEN sm.Src = 'ADJ' AND sm.BaseQty > 0 THEN 3
				WHEN sm.Src = 'SR' THEN 3
				WHEN sm.Src IN('TS', 'CNEE') AND sm.[Type] = 'OH' AND sm.BaseQty > 0 THEN 3
				WHEN sm.Src = 'DO' AND sr.[Type] = 2 THEN 4
				WHEN sm.Src = 'ADJ' AND sm.BaseQty < 0 THEN 5
				WHEN sm.Src IN('DO', 'DOF', 'PR') THEN 5
				WHEN sm.Src IN('TS', 'CNEE') AND sm.[Type] = 'OH' AND sm.BaseQty < 0 THEN 5
				ELSE 6
				END, sm.Id";

            // Get stock mutation records for current month
            var listSM = await db.StockMutations.FromSqlRaw(@$"SELECT sm.*
			    FROM Inventory.StockMutation sm
			    LEFT JOIN Inventory.Item im ON im.Id = sm.ItemId
			    LEFT JOIN Sales.SalesDeliveryHeader do ON do.Code = sm.RefCode1
			    LEFT JOIN Sales.SalesReturnHeader sr ON sr.Code = do.TransCode
                LEFT JOIN Purchasing.PurchaseReceiveHeader rcv ON rcv.Code = sm.RefCode1
			    WHERE sm.Src IN ('RCV','DO','TS','ADJ','BB','PR','CNEE','DOF','SR') AND sm.[Type] = 'OH' 
                AND sm.RefCode1 NOT IN (
                    SELECT Code FROM Purchasing.PurchaseReceiveHeader WHERE Mark = 'V' AND YEAR([Date]) = {date.Year} AND MONTH([Date]) = {date.Month}
                    UNION
                    SELECT Code FROM Sales.SalesDeliveryHeader WHERE Mark = 'V' AND YEAR([Date]) = {date.Year} AND MONTH([Date]) = {date.Month}
                    UNION
                    SELECT Code FROM Inventory.AdjustmentHeader WHERE Mark = 'V' AND YEAR([Date]) = {date.Year} AND MONTH([Date]) = {date.Month}
                    UNION
                    SELECT Code FROM Inventory.TransferStockHeader WHERE Mark = 'V' AND YEAR([Date]) = {date.Year} AND MONTH([Date]) = {date.Month}
                    UNION
                    SELECT Code FROM Purchasing.PurchaseReturnHeader WHERE Mark = 'V' AND YEAR([Date]) = {date.Year} AND MONTH([Date]) = {date.Month}
                    UNION
                    SELECT Code FROM Sales.SalesReturnHeader WHERE Mark = 'V' AND YEAR([Date]) = {date.Year} AND MONTH([Date]) = {date.Month}
                )
                AND sm.ItemId = {curItem.ItemId}
                AND YEAR(sm.[Date]) = {date.Year} AND MONTH(sm.[Date]) = {date.Month}
                {orderQuery}").ToListAsync(cancellationToken);

            // Get RefCode2 for rcv, ts, do
            var refCode2FromRcv =
                $"'{string.Join("', '",
                    listSM.Where(x => x.Src == "RCV" && !string.IsNullOrWhiteSpace(x.RefCode2))
                        .Select(x => x.RefCode2))}'";
            var refCode2FromTs =
                $"'{string.Join("', '",
                    listSM.Where(x => x.Src == "TS" && !string.IsNullOrWhiteSpace(x.RefCode2))
                        .Select(x => x.RefCode2))}'";
            var refCode2FromDo =
                $"'{string.Join("', '",
                    listSM.Where(x => x.Src == "DO" && !string.IsNullOrWhiteSpace(x.RefCode2) &&
                                      x.RefCode1 != x.RefCode2)
                        .Select(x => x.RefCode2))}'";

            // Get stock mutation records for previous month / other month
            var listPrevSM = await db.StockMutations.FromSqlRaw(@$"
                SELECT *
                FROM Inventory.StockMutation
                WHERE RefCode1 IN ({refCode2FromRcv})
                AND ItemId = {curItem.ItemId}
                AND Src = 'PR'
                UNION ALL
                SELECT *
                FROM Inventory.StockMutation sm_ts
                WHERE RefCode1 IN ({refCode2FromTs})
                AND ItemId = {curItem.ItemId}
                AND Src = 'TS'
                AND EXISTS (
                    SELECT *
                    FROM Inventory.TransferStockHeader ts
                    WHERE Code IN ({refCode2FromTs})
                    AND [Type] = 'OUT'
                    AND ts.Code = sm_ts.RefCode1
                )
                UNION ALL
                SELECT *
                FROM Inventory.StockMutation
                WHERE RefCode1 IN ({refCode2FromDo})
                AND ItemId = {curItem.ItemId}
                AND Src = 'SR'")
                .AsNoTracking().ToListAsync(cancellationToken);

            // Calculate HPP
            foreach (var item in listSM)
            {
                _logger.LogInformation($"StockMutation Id: {item.Id}, Date: {item.Date:yyyy-MM-dd}");

                if (item.Qty == 0 || item.BaseQty == 0)
                    continue;

                if (new[] { "RCV", "BB", "SR" }.Contains(item.Src))
                {
                    short rcvSrcTrans = 0;
                    short prType = 0;
                    if (item.Src == "RCV")
                    {
                        var itemRcvData = rcvData.FirstOrDefault(y => y.Code == item.RefCode1);
                        rcvSrcTrans = itemRcvData?.SrcTrans ?? 0;
                        if (rcvSrcTrans == 2)
                            prType = prData.FirstOrDefault(x => x.Code == (itemRcvData?.TransCode ?? ""))?.Type ?? 0;
                    }

                    if (item.Src == "RCV" && prType == 2) // same item
                    {
                        var refSM = listSM.FirstOrDefault(x => x.ItemId == item.ItemId && x.UnitId == item.UnitId &&
                                                               x.RefCode1 == item.RefCode2);
                        if (refSM != null)
                        {
                            item.BaseNettPrice = refSM.BaseNettPrice;
                            item.NettPrice = refSM.NettPrice;
                        }
                        else
                        {
                            refSM = listPrevSM.FirstOrDefault(x => x.ItemId == item.ItemId && x.UnitId == item.UnitId &&
                                                                       x.RefCode1 == item.RefCode2);
                            item.BaseNettPrice = refSM?.BaseNettPrice ?? 0m;
                            item.NettPrice = refSM?.NettPrice ?? 0m;
                        }
                        
                        latestStockValue += item.BaseNettPrice * item.BaseQty;
                        latestQty += item.BaseQty;
                        // db.StockMutations.Update(item);
                    }
                    else
                    {
                        if (item.Src == "SR" || (item.Src == "RCV" && prType == 3)) // diff item
                        {
                            item.BaseNettPrice = hpp;
                            item.NettPrice = hpp * item.BaseQty / item.Qty;
                            // db.StockMutations.Update(item);
                        }
                        latestStockValue += item.BaseNettPrice * item.BaseQty;
                        latestQty += item.BaseQty;
                        if (item.Src == "BB" || (item.Src == "RCV" && rcvSrcTrans == 1))
                        {
                            if (latestStockValue > 0 && latestQty > 0)
                                hpp = latestStockValue / latestQty;
                        }
                    }
                }
                else if (new[] { "ADJ", "TS", "CNEE" }.Contains(item.Src))
                {
                    var tsType = "";
                    if (item.Src == "TS")
                        tsType = tsData.FirstOrDefault(x => x.Code == item.RefCode1)?.Type ?? "";
                    
                    if (item.Src == "TS" && tsType == "OUT")
                    {
                        if (latestDate != item.Date)
                        {
                            if (latestStockValue > 0 && latestQty > 0)
                                hpp = latestStockValue / latestQty;
                        }

                        item.BaseNettPrice = hpp;
                        item.NettPrice = hpp * item.BaseQty / item.Qty;
                    }
                    else if (item.Src == "TS" && tsType == "IN")
                    {
                        var refSM = listSM.FirstOrDefault(x => x.ItemId == item.ItemId && x.UnitId == item.UnitId &&
                                                               x.RefCode1 == item.RefCode2);
                        if (refSM != null)
                        {
                            item.BaseNettPrice = refSM.BaseNettPrice;
                            item.NettPrice = refSM.NettPrice;
                        }
                        else
                        {
                            refSM = listPrevSM.FirstOrDefault(x => x.ItemId == item.ItemId && x.UnitId == item.UnitId &&
                                                                   x.RefCode1 == item.RefCode2);
                            item.BaseNettPrice = refSM?.BaseNettPrice ?? 0m;
                            item.NettPrice = refSM?.NettPrice ?? 0m;
                        }
                    }
                    else
                    {
                        if (latestDate != item.Date)
                        {
                            if (latestStockValue > 0 && latestQty > 0)
                                hpp = latestStockValue / latestQty;
                        }

                        item.BaseNettPrice = hpp;
                        item.NettPrice = hpp * item.BaseQty / item.Qty;
                    }

                    latestStockValue += item.BaseNettPrice * item.BaseQty;
                    latestQty += item.BaseQty;
                    // db.StockMutations.Update(item);
                }
                else
                {
                    if (latestDate != item.Date)
                    {
                        if (latestStockValue > 0 && latestQty > 0)
                            hpp = latestStockValue / latestQty;
                    }

                    short doSrcTrans = 0;
                    short srType = 0;
                    if (item.Src == "DO")
                    {
                        var itemDoData = doData.FirstOrDefault(y => y.Code == item.RefCode1);
                        doSrcTrans = itemDoData?.SrcTrans ?? 0;
                        if (doSrcTrans == 2)
                            srType = srData.FirstOrDefault(x => x.Code == (itemDoData?.TransCode ?? ""))?.Type ?? 0;
                    }

                    if (item.Src == "DO" && srType == 2) // same item
                    {
                        var refSM = listSM.FirstOrDefault(x => x.ItemId == item.ItemId && x.UnitId == item.UnitId &&
                                                               x.RefCode1 == item.RefCode2);
                        if (refSM != null)
                        {
                            item.BaseNettPrice = refSM.BaseNettPrice;
                            item.NettPrice = refSM.NettPrice;
                        }
                        else
                        {
                            refSM = listPrevSM.FirstOrDefault(x => x.ItemId == item.ItemId && x.UnitId == item.UnitId &&
                                                                   x.RefCode1 == item.RefCode2);
                            item.BaseNettPrice = refSM?.BaseNettPrice ?? 0m;
                            item.NettPrice = refSM?.NettPrice ?? 0m;
                        }
                        
                        latestStockValue -= item.BaseNettPrice * item.BaseQty;
                        latestQty -= item.BaseQty;
                        // db.StockMutations.Update(item);
                    }
                    else
                    {
                        if (item.Src == "DO" && srType == 3) // diff item
                        {
                            item.BaseNettPrice = hpp;
                            item.NettPrice = hpp * item.BaseQty / item.Qty;
                        }
                        else
                        {
                            item.BaseNettPrice = hpp;
                            item.NettPrice = hpp * item.BaseQty / item.Qty;
                        }
                        latestStockValue -= item.BaseNettPrice * item.BaseQty;
                        latestQty -= item.BaseQty;
                        // db.StockMutations.Update(item);
                    }
                }

                latestDate = item.Date;
            }

            if (listSM.Where(x => x.BaseNettPrice == 0).Any())
            {
                foreach (var item in listSM.Where(x => x.BaseNettPrice == 0))
                {
                    if (item.Src == "RCV")
                    {
                        short prType = 0;
                        var itemRcvData = rcvData.FirstOrDefault(y => y.Code == item.RefCode1);
                        short rcvSrcTrans = itemRcvData?.SrcTrans ?? 0;
                        if (rcvSrcTrans == 2)
                            prType = prData.FirstOrDefault(x => x.Code == (itemRcvData?.TransCode ?? ""))?.Type ?? 0;

                        if (prType == 2) // same item
                        {
                            var refSM = listSM.FirstOrDefault(x => x.ItemId == item.ItemId && x.UnitId == item.UnitId &&
                                                                   x.RefCode1 == item.RefCode2);
                            if (refSM != null)
                            {
                                item.BaseNettPrice = refSM.BaseNettPrice;
                                item.NettPrice = refSM.NettPrice;
                            }

                            latestStockValue += item.BaseNettPrice * item.BaseQty;
                            latestQty += item.BaseQty;
                        }
                    }
                    else if (item.Src == "TS")
                    {
                        var tsType = tsData.FirstOrDefault(x => x.Code == item.RefCode1)?.Type ?? "";

                        if (tsType == "IN")
                        {
                            var refSM = listSM.FirstOrDefault(x => x.ItemId == item.ItemId && x.UnitId == item.UnitId &&
                                                                   x.RefCode1 == item.RefCode2);
                            if (refSM != null)
                            {
                                item.BaseNettPrice = refSM.BaseNettPrice;
                                item.NettPrice = refSM.NettPrice;
                            }

                            latestStockValue += item.BaseNettPrice * item.BaseQty;
                            latestQty += item.BaseQty;
                        }
                    }
                }
            }

            var saved = false;
            while (!saved)
            {
                try
                {
                    // Save HPP to DB
                    await db.SaveChangesAsync(cancellationToken);
                    saved = true;
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    foreach (var entry in ex.Entries)
                    {
                        // var proposedValues = entry.CurrentValues;
                        var databaseValues = await entry.GetDatabaseValuesAsync(cancellationToken);
                    
                        // foreach (var property in proposedValues.Properties)
                        // {
                        //     var proposedValue = proposedValues[property];
                        //     var databaseValue = databaseValues[property];
                        //
                        //     // TODO: decide which value should be written to database
                        //     // proposedValues[property] = <value to be saved>;
                        // }
                    
                        // Refresh original values to bypass next concurrency check
                        entry.OriginalValues.SetValues(databaseValues);
                    }
                }
            }

            var eomDate = new DateTime(date.Year, date.Month, 1).AddMonths(1).AddDays(-1);
            var itemCogsHistory =
                await db.ItemCogsHistories
                    .FirstOrDefaultAsync(x => x.Date == eomDate && x.ItemId == curItem.ItemId, cancellationToken);
            if (itemCogsHistory != null)
            {
                itemCogsHistory.UomId = curItem.UomId;
                itemCogsHistory.BaseUnit = curItem.BaseUnit;
                itemCogsHistory.TotalBaseNettPrice = latestStockValue;
                itemCogsHistory.TotalBaseQty = latestQty;
            }
            else
            {
                await db.ItemCogsHistories.AddAsync(new ItemCogsHistory
                {
                    Date = eomDate,
                    ItemId = curItem.ItemId,
                    BaseUnit = curItem.BaseUnit,
                    UomId = curItem.UomId,
                    TotalBaseNettPrice = latestStockValue,
                    TotalBaseQty = latestQty
                }, cancellationToken);
            }

            await db.SaveChangesAsync(cancellationToken);
        }
    }
}