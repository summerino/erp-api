using ERP.Common;
using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Entity.Sales;
using ERP.Web.API.Domain.Interfaces.Sales;
using ERP.Web.API.Model.Sales;

namespace ERP.Web.API.Domain.Services.Sales;

public class SalesTargetService : GeneralService<SalesTargetHeader>, ISalesTargetService
{
    public SalesTargetService(TenantContext db)
        : base(db)
    {
                
    }

    public SaveResult Clone(string code, string name, DateTime startDate, DateTime endDate, int userId)
    {
        var result = new SaveResult(false);

        using var transaction = Db.Database.BeginTransaction();
        try
        {
            var data = Db.SalesTargetHeaders.FirstOrDefault(x => x.Code == code);
            var detailData = Db.SalesTargetDetails.Where(x => x.Code == data.Code).ToList();
            var subjectData = Db.SalesTargetSubjects.Where(x => x.Code == data.Code).ToList();

            var dataRequest = new SalesTargetRequest
            {
                Code = "",
                Name = name,
                StartDate = startDate,
                EndDate = endDate,
                Mark = "A",
                CreatedBy = userId,
                CreatedDate = DateTime.Now,
                UpdatedBy = data.UpdatedBy,
                UpdatedDate = DateTime.Now,
                ItemDetails = detailData,
                ItemSubjects = subjectData
            };


            var (valid, validData) = SubjectValidation(dataRequest);
            if (!valid)
            {
                result.Message = $@"Data target penjual tidak bisa diklon karena terdapat subjek penjual
                                    yang terdaftar pada periode {validData.StartDate:dd-MMM-yyyy} - {validData.EndDate:dd-MMM-yyyy}
                                    dengan kode transaksi {validData.Code}.";
                return result;
            }

            // Insert header data
            var newCode = GetNewCode("ST_NUM_FMT", DateTime.Now);

            dataRequest.Code = newCode;
            Db.SalesTargetHeaders.Add(dataRequest);

            // Insert detail data
            short i = 0;
            foreach (var item in dataRequest.ItemDetails)
            {
                Db.SalesTargetDetails.Add(new SalesTargetDetail
                {
                    Code = dataRequest.Code,
                    LineNo = ++i,
                    ItemGroupId = item.ItemGroupId,
                    ItemSubGroupId = item.ItemSubGroupId,
                    SubGroup = item.SubGroup,
                    Amount = item.Amount
                });
            }

            short j = 0;
            foreach (var item in dataRequest.ItemSubjects)
            {

                Db.SalesTargetSubjects.Add(new SalesTargetSubject
                {
                    Code = dataRequest.Code,
                    LineNo = ++j,
                    SalesmanId = item.SalesmanId
                });
            }

            // Save changes
            Db.SaveChanges();

            transaction.Commit();
        }
        catch (Exception ex)
        {
            result.Message = ex.InnerException?.Message ?? ex.Message;
            return result;
        }

        result.Success = true;
        result.Message = "Data target penjual berhasil diklon.";
        return result;
    }

    public SaveResult Delete(string code, int userId)
    {
        var result = new SaveResult(false);

        var data = Db.SalesTargetHeaders.Find(code);
        if (data != null)
        {
            // Checking mark header data
            if (data.Mark == "V")
            {
                result.Message = "Data target penjual tidak bisa ditandai sebagai void karena sudah ditandai sebagai void.";
                return result;
            }

            // Update header data
            data.Mark = "V";
            data.UpdatedBy = userId;
            data.UpdatedDate = DateTime.Now;

            Db.SaveChanges();
        }

        result.Success = true;
        result.Message = "Data target penjual berhasil ditandai sebagai void.";
        return result;
    }

    public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort, string search)
    {
        var data = Db.VwSalesTargetHeaders.AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            data = DateTime.TryParse(search, out var searchDate)
                ? data.Where(x => x.StartDate == searchDate || x.EndDate == searchDate)
                : data.Where(x =>
                    x.Code.Contains(search) || x.Name.Contains(search));
        }

        return data.ToDataSourceResult(skip, take, filter, sort);
    }

    public IEnumerable<SalesTargetDetail> GetDetailData(string code)
    {
        return Db.SalesTargetDetails.Where(x => x.Code == code).OrderBy(x => x.LineNo);
    }

    public IEnumerable<SalesTargetSubject> GetSubjectData(string code)
    {
        return Db.SalesTargetSubjects.Where(x => x.Code == code).OrderBy(x => x.LineNo);
    }

    public SaveResult Insert(SalesTargetRequest data)
    {
        var result = new SaveResult(false);

        using var transaction = Db.Database.BeginTransaction();
        try
        {
            var (valid, validData) = SubjectValidation(data);
            if (!valid)
            {
                result.Message = $@"Data target penjual tidak bisa disimpan karena terdapat subjek penjual
                                    yang terdaftar pada periode {validData.StartDate:dd-MMM-yyyy} - {validData.EndDate:dd-MMM-yyyy}
                                    dengan kode transaksi {validData.Code}.";
                return result;
            }

            // Insert header data
            var newCode = GetNewCode("ST_NUM_FMT", DateTime.Now);

            data.Code = newCode;
            Db.SalesTargetHeaders.Add(data);

            // Insert detail data
            short i = 0;
            foreach (var item in data.ItemDetails)
            {
                Db.SalesTargetDetails.Add(new SalesTargetDetail
                {
                    Code = data.Code,
                    LineNo = ++i,
                    ItemGroupId = item.ItemGroupId,
                    ItemSubGroupId = item.ItemSubGroupId,
                    SubGroup = item.SubGroup,
                    Amount = item.Amount
                });
            }

            short j = 0;
            foreach (var item in data.ItemSubjects)
            {

                Db.SalesTargetSubjects.Add(new SalesTargetSubject
                {
                    Code = data.Code,
                    LineNo = ++j,
                    SalesmanId = item.SalesmanId
                });
            }

            // Save changes
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
        result.Message = "Data target penjual berhasil disimpan.";
        return result;
    }

    public SaveResult Update(SalesTargetRequest data)
    {
        var result = new SaveResult(false);

        using var transaction = Db.Database.BeginTransaction();
        try
        {
            if (Db.PromoHeaders.Any(x => x.Code == data.Code && x.Mark == "V"))
            {
                result.Message = "Data target penjual tidak bisa diubah karena data sudah ditandai sebagai void.";
                return result;
            }

            var (valid, validData) = SubjectValidation(data);
            if (!valid)
            {
                result.Message = $@"Data target penjual tidak bisa diubah karena terdapat subjek penjual
                                    yang terdaftar pada periode {validData.StartDate:dd-MMM-yyyy} - {validData.EndDate:dd-MMM-yyyy}
                                    dengan kode transaksi {validData.Code}.";
                return result;
            }

            Db.SalesTargetHeaders.Update(data);
            Db.Entry(data).Property(e => e.Code).IsModified = false;
            Db.Entry(data).Property(e => e.CreatedBy).IsModified = false;
            Db.Entry(data).Property(e => e.CreatedDate).IsModified = false;

            var delDetails = Db.SalesTargetDetails
                .Where(d => d.Code == data.Code && !data.ItemDetails.Select(x => x.Id).Contains(d.Id))
                .ToList();

            Db.SalesTargetDetails.RemoveRange(delDetails);

            var delSubject = Db.SalesTargetSubjects
                .Where(d => d.Code == data.Code && !data.ItemSubjects.Select(x => x.Id).Contains(d.Id))
                .ToList();

            Db.SalesTargetSubjects.RemoveRange(delSubject);

            short i = 0;
            foreach (var item in data.ItemDetails)
            {
                if (item.Id < 0)
                {
                    var newItem = new SalesTargetDetail
                    {
                        Code = data.Code,
                        LineNo = ++i,
                        ItemGroupId = item.ItemGroupId,
                        ItemSubGroupId = item.ItemSubGroupId,
                        SubGroup = item.SubGroup,
                        Amount = item.Amount
                    };

                    Db.SalesTargetDetails.Add(newItem);
                }
                else
                {
                    item.LineNo = ++i;

                    Db.SalesTargetDetails.Update(item);
                    Db.Entry(item).Property(e => e.Code).IsModified = false;
                }
            }

            short j = 0;
            foreach (var item in data.ItemSubjects)
            {
                if (item.Id < 0)
                {
                    Db.SalesTargetSubjects.Add(new SalesTargetSubject
                    {
                        Code = data.Code,
                        LineNo = ++j,
                        SalesmanId = item.SalesmanId
                    });
                }
                else
                {
                    item.LineNo = ++j;
                    Db.SalesTargetSubjects.Update(item);
                }
            }

            // Save changes
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
        result.Message = "Data target penjual berhasil diperbarui.";
        return result;
    }

    private (bool, SalesTargetHeader) SubjectValidation(SalesTargetRequest data)
    {
        var result = true;
        var resultData = new SalesTargetHeader();
        var stData = Db.SalesTargetHeaders
            .Where(x => x.Code != data.Code && x.Mark == "A" &&
                        (data.StartDate >= x.StartDate && data.StartDate <= x.EndDate || 
                         data.EndDate >= x.StartDate && data.EndDate <= x.EndDate))
            .ToList();
        if (stData.Any())
        {
            var subjectData = Db.SalesTargetSubjects.Where(x => stData.Select(y => y.Code).Contains(x.Code)).ToList();
            var foundData = subjectData.Where(x => data.ItemSubjects.Select(y => y.SalesmanId).Contains(x.SalesmanId)).ToList();
            if (foundData.Any())
            {
                result = false;
                resultData = stData.FirstOrDefault(x => x.Code == foundData.FirstOrDefault().Code);
            }
        }
        return (result, resultData);
    }
}