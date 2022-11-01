using Microsoft.EntityFrameworkCore;
using ERP.Common;
using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.General;
using ERP.Web.API.Domain.Models.General;
using ERP.Web.API.Model.General;
using ERP.Common.Models;
using ERP.Common.Extensions;

namespace ERP.Web.API.Domain.Services.General;

public class ActiveTransactionService : IActiveTransactionService
{
    private readonly TenantContext _tenantCtx;

    public ActiveTransactionService(TenantContext tenantCtx)
    {
        _tenantCtx = tenantCtx;
    }

    public bool SeenByOthers(string src, string code, int userId)
    {
        var timeout =
            _tenantCtx.SystemParameters.SingleOrDefault(x => x.Code == "ACTIVE_TRANS_TIMEOUT")?.Value ?? "0";

        var map = new MapApproval();
        var tableName = map.Approvals.SingleOrDefault(x => x.Src == src)?.TableName ?? "";

        var query = @$"
                SELECT CONVERT(varchar, COUNT(1)) AS Value
                FROM {tableName}
                WHERE Code='{code}'
                AND ViewedBy <> '{userId}'
                AND ViewedDate IS NOT NULL
                AND DATEDIFF(MINUTE, ViewedDate, dbo.udf_current_local_time()) < {timeout}";

        return Convert.ToInt16(_tenantCtx.NewCodes.FromSqlRaw(query).SingleOrDefault()?.Value ?? "0") < 1;
    }

    public SaveResult LockedTransaction(ActiveTransactionRequest data, int userId)
    {
        var result = new SaveResult(false);

        if (SeenByOthers(data.Src.ToUpper(), data.Code, userId))
        {
            var query = GenerateQuery(data.Src.ToUpper(), data.Code, userId, "dbo.udf_current_local_time()");
            try
            {
                _tenantCtx.Database.ExecuteSqlRaw(query);
            }
            catch (Exception ex) 
            {
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }

            result.Success = true;
            result.Message = "Data transaksi berhasil dikunci.";
        }
        else
        {
            result.Success = true;
            result.Message = "used";
        }

        return result;
    }
        
    public SaveResult ReleasedTransaction(ActiveTransactionRequest data, int userId)
    {
        var result = new SaveResult(false);

        var query = GenerateQuery(data.Src.ToUpper(), data.Code, null, "NULL", userId);
        try
        {
            _tenantCtx.Database.ExecuteSqlRaw(query);
        }
        catch (Exception ex)
        {
            result.Message = ex.InnerException?.Message ?? ex.Message;
        }

        result.Success = true;
        result.Message = "Data transaksi berhasil dilepaskan.";
        return result;
    }
        
    private string GenerateQuery(string src, string code, int? viewedUser, string date, int? releasedUser = null)
    {
        var map = new MapApproval();
        var tableName = map.Approvals.SingleOrDefault(x => x.Src == src)?.TableName ?? "";

        var wh = "";
        if (releasedUser.HasValue) wh = $"AND ViewedBy={releasedUser}";

        return $"UPDATE {tableName} SET ViewedBy={(viewedUser.HasValue ? viewedUser.ToString() : "NULL")}, ViewedDate={date} WHERE Code='{code}' {wh}";
    }

    public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts, string search)
    {
        List<string> listQuery = new();

        var timeout =
            _tenantCtx.SystemParameters.SingleOrDefault(x => x.Code == "ACTIVE_TRANS_TIMEOUT")?.Value ?? "0";

        var map = new MapApproval().Approvals.ToList();

        foreach (var item in map)
        {
            string srcName = "";
            string extraFilter = "";
            switch (item.Src)
            {
                case "PO":
                    srcName = "Order Pembelian";
                    break;

                case "RCV":
                    srcName = "Penerimaan Pembelian";
                    break;

                case "PI":
                    srcName = "Faktur Pembelian";
                    break;

                case "PR":
                    srcName = "Retur Pembelian";
                    break;

                case "PROMO":
                    srcName = "Promo";
                    break;

                case "SO":
                    srcName = "Order Penjualan";
                    break;

                case "DO":
                    srcName = "Surat Jalan";
                    break;

                case "SI":
                    srcName = "Faktur Penjualan";
                    extraFilter = "AND t.FromDirectInvoice = 0";
                    break;

                case "DI":
                    srcName = "Penjualan Langsung";
                    extraFilter = "AND t.FromDirectInvoice = 1";
                    break;

                case "SR":
                    srcName = "Retur Penjualan";
                    break;

                case "DP":
                    srcName = "Rencana Pengiriman";
                    break;

                case "VO":
                    srcName = "Perintah Kunjungan";
                    break;

                case "EI":
                    srcName = "Faktur Ekspedisi";
                    break;

                case "CB":
                    srcName = "Kas Bank Umum";
                    extraFilter = "AND t.IsInterCashBank = 0";
                    break;

                case "ICB":
                    srcName = "Kas Bank Pemindahan Dana";
                    extraFilter = "AND t.IsInterCashBank = 1";
                    break;

                case "GEN-JR":
                    srcName = "Jurnal Umum";
                    break;

                case "FA":
                    srcName = "Aktiva Tetap";
                    break;

                case "TS":
                    srcName = "Transfer Persediaan";
                    extraFilter = "AND t.IsConsignee = 0";
                    break;

                case "CNEE":
                    srcName = "Konsinyasi";
                    extraFilter = "AND t.IsConsignee = 1";
                    break;

                case "ADJ":
                    srcName = "Penyesuaian";
                    break;
            }

            var query = @$"
                SELECT t.Code, '{srcName}' AS SrcName, '{item.Src}' AS Src, u.[Name] AS Username, t.ViewedDate
                FROM {item.TableName} t
                LEFT JOIN SystemManagement.[User] u ON u.Id = t.ViewedBy
                WHERE t.ViewedBy IS NOT NULL
                AND t.ViewedDate IS NOT NULL
                AND DATEDIFF(MINUTE, t.ViewedDate, dbo.udf_current_local_time()) < {timeout}" + extraFilter;

            listQuery.Add(query);
        }
        var querys = string.Join(" UNION ", listQuery);
        var data = _tenantCtx.ActiveTransactionGetDataRequests.FromSqlRaw(querys).AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            data = data.Where(x => x.Code.Contains(search) || x.SrcName.Contains(search));
        }

        return data.ToDataSourceResult(skip, take, filters, sorts);
    }

    public SaveResult ReleaseTransaction(List<ActiveTransactionRequest> data)
    {
        var result = new SaveResult(false);

        try
        {
            List<string> listQuery = new();

            foreach (var item in data)
            {
                listQuery.Add(GenerateQuery(item.Src.ToUpper(), item.Code, null, "NULL"));
            }

            var query = string.Join(";", listQuery);
            _tenantCtx.Database.ExecuteSqlRaw(query);
        }
        catch (Exception ex)
        {
            result.Message = ex.InnerException?.Message ?? ex.Message;
        }

        result.Success = true;
        result.Message = "Data transaksi berhasil dilepaskan.";
        return result;
    }
}