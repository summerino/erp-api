using ERP.Common;
using ERP.Entity;
using ERP.Entity.MobileSales;
using ERP.Web.API.Domain.Interfaces.Mobile.General;
using ERP.Web.API.Domain.Models.General;

namespace ERP.Web.API.Domain.Services.Mobile.General;

public class ActivityLogService : IActivityLogService
{
    protected TenantContext Db;
    public ActivityLogService(TenantContext db)
    {
        Db = db;
    }
    public SaveResult Insert(IEnumerable<LogActivityRequestModel> data, int userId)
    {
        var username = Db.Users.Where(x => x.Id.Equals(userId)).Select(y => y.Username).SingleOrDefault();
        var result = new SaveResult(false);

        using var transaction = Db.Database.BeginTransaction();
        try
        {
            foreach (var log in data)
            {
                Db.MobileActivityLogs.Add(new MobileActivityLog
                {
                    Date = log.Date,
                    Username = username,
                    TypeCode = log.TypeCode,
                    Notes = log.Notes
                });
            }

            Db.SaveChanges();

            transaction.Commit();
        }
        catch (Exception ex)
        {
            result.Message = ex.InnerException?.Message ?? ex.Message;
            return result;
        }

        result.Success = true;
        result.Data = data;
        result.Message = "Log Activity berhasil disimpan.";
        return result;
    }
}