using ERP.Common;
using ERP.Entity;
using ERP.Entity.Accounting;
using ERP.Entity.SystemManagement;
using ERP.Web.API.Domain.Interfaces.Accounting;
using ERP.Web.API.Model.Accounting;
using Microsoft.EntityFrameworkCore;

namespace ERP.Web.API.Domain.Services.Accounting;

public class IncomeStatementFormatService : GeneralService<IncomeStatementFormat>, IIncomeStatementFormatService
{
    public IncomeStatementFormatService(TenantContext db)
        : base(db)
    {

    }

    public override SaveResult Insert(IncomeStatementFormat data)
    {
        var result = new SaveResult(false);

        using var transaction = Db.Database.BeginTransaction();
        try
        {
            var latestCode = Db.IncomeStatementFormats.OrderBy(x => x.Code).LastOrDefault().Code;
            var nCode = Convert.ToInt32(latestCode[1..]);
            data.Code = nCode + 1 < 100 ? "I0" + Convert.ToString(nCode + 1) : "I" + Convert.ToString(nCode + 1);
            if (data.ParentCode != null)
            {
                var parentData = Db.IncomeStatementFormats.FirstOrDefault(x => x.Code == data.ParentCode);
                data.Deep = parentData.Deep + 1;
                var nSort = Db.IncomeStatementFormats
                    .Where(x => x.ParentCode == data.ParentCode);
                data.Sort = nSort.Any() ? nSort.Max(x => x.Sort) + 1 : 1;
            }
            else
            {
                var latestSort = Db.IncomeStatementFormats
                    .Where(x => x.Category == data.Category && x.Deep == 1)
                    .Max(x => x.Sort);
                data.Sort = latestSort + 1;
            }

            Db.IncomeStatementFormats.Add(data);

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
        result.Message = "Data format laba/rugi berhasil disimpan.";
        return result;
    }

    public SaveResult Update(IncomeStatementFormatRequest data)
    {
        var result = new SaveResult(false);

        var fData = Db.IncomeStatementFormats.AsNoTracking().FirstOrDefault(x => x.Code == data.Code);

        if (data.ParentCode != null)
        {
            if(fData.ParentCode != data.ParentCode)
            {
                var parentData = Db.IncomeStatementFormats.FirstOrDefault(x => x.Code == data.ParentCode);
                data.Deep = parentData.Deep + 1;
                var nSort = Db.IncomeStatementFormats
                    .Where(x => x.ParentCode == data.ParentCode);
                data.Sort = nSort.Any() ? nSort.Max(x => x.Sort) + 1 : 1;
            }
        }
        else
        {

            data.Deep = 1;
            if(fData.ParentCode != null)
            {
                var latestSort = Db.IncomeStatementFormats
                    .Where(x => x.Category == data.Category && x.Deep == 1)
                    .Max(x => x.Sort);
                data.Sort = latestSort == data.Sort ? data.Sort : latestSort + 1;
            }
        }

        Db.IncomeStatementFormats.Update(data);
        Db.Entry(data).Property(e => e.Code).IsModified = false;
        Db.Entry(data).Property(e => e.CreatedBy).IsModified = false;
        Db.Entry(data).Property(e => e.CreatedDate).IsModified = false;

        if (data.Coas.Count() > 0)
        {
            foreach (var item in data.Coas)
            {
                var coaData = Db.Coas.FirstOrDefault(x => x.Code == item.Code);

                coaData.IsSeq = item.IsSeq;
                coaData.IsDetSeq = item.IsDetSeq;

                Db.Coas.Update(coaData);
            }
        }

        Db.SaveChanges();

        result.Success = true;
        result.Data = data.Code;
        result.Message = "Data format laba/rugi berhasil diperbarui.";
        return result;
    }

    public SaveResult Move(IncomeStatementFormat data, string type)
    {
        var result = new SaveResult(false);
        if (type == "up")
        {
            if (data.Sort == 1)
                return new SaveResult(false, "Urutan teratas");

            var prevData = Db.IncomeStatementFormats
                .FirstOrDefault(x => x.Category == data.Category && 
                                     x.Deep == data.Deep && x.Sort == (data.Sort - 1));

            int value = data.Sort;
            data.Sort = prevData.Sort;
            prevData.Sort = value;
            prevData.UpdatedBy = data.UpdatedBy;
            prevData.UpdatedDate = data.UpdatedDate;

            Db.IncomeStatementFormats.Update(data);
            Db.IncomeStatementFormats.Update(prevData);
        }
        else if (type == "down")
        {
            var lastSort = Db.IncomeStatementFormats
                .Where(x => x.Category == data.Category && x.Deep == data.Deep)
                .Max(x => x.Sort);
                
            if (data.Sort == lastSort)
                return new SaveResult(false, "Urutan terendah");

            var nextData = Db.IncomeStatementFormats
                .FirstOrDefault(x => x.Category == data.Category &&
                                     x.Deep == data.Deep && x.Sort == (data.Sort + 1));

            int value = data.Sort;
            data.Sort = nextData.Sort;
            nextData.Sort = value;
            nextData.UpdatedBy = data.UpdatedBy;
            nextData.UpdatedDate = data.UpdatedDate;

            Db.IncomeStatementFormats.Update(data);
            Db.IncomeStatementFormats.Update(nextData);
        }

        Db.Entry(data).Property(e => e.Code).IsModified = false;
        Db.Entry(data).Property(e => e.CreatedBy).IsModified = false;
        Db.Entry(data).Property(e => e.CreatedDate).IsModified = false;

        Db.SaveChanges();

        result.Success = true;
        result.Data = data.Code;
        result.Message = "Urutan format laba/rugi berhasil diperbarui.";
        return result;
    }

    public SaveResult Delete(string code, int userId)
    {
        var result = new SaveResult(false);

        var data = Db.IncomeStatementFormats.FirstOrDefault(x => x.Code == code);
        if (data != null)
        {
            data.IsActive = false;
            data.UpdatedBy = userId;
            data.UpdatedDate = DateTime.Now;

            Db.IncomeStatementFormats.Update(data);

            Db.SaveChanges();
        }

        result.Success = true;
        result.Message = "Data format laba/rugi berhasil dinonaktifkan.";
        return result;
    }

    public IEnumerable<IncomeStatementFormat> GetSubFormat(string code)
    {
        var subData = Db.IncomeStatementFormatSubtotals
            .Where(x => x.Code == code)
            .Select(x => x.SubCode)
            .ToList();

        var data = Db.IncomeStatementFormats
            .Where(x => subData.Contains(x.Code))
            .ToList();

        return data;
    }

    public IEnumerable<IncomeStatementFormat> GetUnSubFormat(string code, string category)
    {
        var subData = Db.IncomeStatementFormatSubtotals
            .Where(x => x.Code == code)
            .Select(x => x.SubCode).ToList();

        var data = Db.IncomeStatementFormats
            .Where(x => x.Category == category && !subData.Contains(x.Code) && !x.Hidden && x.Type == "N")
            .ToList();

        return data;
    }

    public SaveResult InsertSub(string subCode, string code, int userId)
    {
        var result = new SaveResult(false);

        var validateData = Db.IncomeStatementFormatSubtotals
            .FirstOrDefault(x => x.Code == code && x.SubCode == subCode);
            
        if(validateData != null)
            return new SaveResult(false, "Data tidak bisa diproses");

        Db.IncomeStatementFormatSubtotals.Add(new IncomeStatementFormatSubtotal { 
            Code = code,
            SubCode = subCode,
            CreatedBy = userId,
            CreatedDate = DateTime.Now
        });

        Db.SaveChanges();

        result.Success = true;
        result.Message = "Data format subtotal berhasil diubah.";
        return result;
    }

    public SaveResult RemoveSub(string subCode, string code)
    {
        var result = new SaveResult(false);

        var subData = Db.IncomeStatementFormatSubtotals
            .FirstOrDefault(x => x.Code == code && x.SubCode == subCode);

        if(subData == null)
            return new SaveResult(false,"Data tidak bisa diproses");

        Db.IncomeStatementFormatSubtotals.Remove(subData);

        Db.SaveChanges();

        result.Success = true;
        result.Message = "Data format subtotal berhasil diubah.";
        return result;
    }

    public object GetFormatHierarchy(string category)
    {
        var data = Db.IncomeStatementFormats
            .Where(x => x.IsActive && x.Category == category);

        var users = Db.Users.ToList();

        var result = DefineChildNodes(data.ToList(), users);

        return result;
    }

    public IEnumerable<IncomeStatementFormat> GetFormatLists(string category)
    {
        var data = Db.IncomeStatementFormats
            .Where(x => x.IsActive && !x.Hidden && x.Category == category)
            .OrderBy(x => x.Sort)
            .ToList();

        return data;
    }

    private static object DefineChildNodes(List<IncomeStatementFormat> data, List<User> users, string parentId = null)
    {
        var nodes = data
            .Where(x => x.ParentCode == parentId)
            .OrderBy(x => x.Sort)
            .Select(x => new
            {
                x.Code,
                x.Name,
                x.ParentCode,
                x.Category,
                x.Type,
                x.PercentOf,
                x.PercentFrom,
                x.Position,
                x.Deep,
                x.Sort,
                x.SubtotalSort,
                x.Detail,
                x.Hidden,
                x.Bold,
                x.ByAccount,
                x.IsActive,
                x.CreatedBy,
                x.CreatedDate,
                x.UpdatedBy,
                x.UpdatedDate,
                UpdatedInitial = users.FirstOrDefault(y => y.Id == x.UpdatedBy).Initial,
                Children = DefineChildNodes(data, users, x.Code)
            });

        return nodes;
    }
}