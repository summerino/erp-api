using Microsoft.EntityFrameworkCore;
using ERP.Common;
using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.General;
using ERP.Web.API.Domain.Models.General;
using ERP.Web.API.Model.General;

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
                AND DATEDIFF(MINUTE, ViewedDate, GETDATE()) < {timeout}";

        return Convert.ToInt16(_tenantCtx.NewCodes.FromSqlRaw(query).SingleOrDefault()?.Value ?? "0") < 1;
    }

    public SaveResult LockedTransaction(ActiveTransactionRequest data, int userId)
    {
        var result = new SaveResult(false);

        if (SeenByOthers(data.Src.ToUpper(), data.Code, userId))
        {
            var query = GenerateQuery(data.Src.ToUpper(), data.Code, userId, "GETDATE()");
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
}