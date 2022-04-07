using ERP.Common;
using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Entity.Accounting;
using ERP.Entity.Finance;
using ERP.Entity.MobileSales;
using ERP.Web.API.Domain.Interfaces.MobileSales;
using ERP.Web.API.Domain.Models.Mobile.Operational;
using ERP.Web.API.Model.MobileSales;

namespace ERP.Web.API.Domain.Services.MobileSales;

public class MobileCostService : GeneralService<MobileCostHeader>, IMobileCostService
{
    public MobileCostService(TenantContext db)
        :base(db)
    {
    }

    public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts, string search)
    {
        var data = Db.VwMobileCostHeaders.AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            data = DateTime.TryParse(search, out var searchDate)
                ? data.Where(x => x.Date == searchDate)
                : data.Where(x =>
                    x.Code.Contains(search) || x.SalesmanInitial.Contains(search) || x.SalesmanName.Contains(search)
                    || x.Mark.Contains(search) || x.Status.Contains(search));
        }

        return data.ToDataSourceResult(skip, take, filters, sorts);
    }

    public IEnumerable<VwMobileCostDetail> GetDetailData(string code)
    {
        var data = Db.VwMobileCostDetails.Where(x => x.Code == code);

        return data.OrderBy(x => x.LineNo);
    }

    public IEnumerable<MobileCostImage> GetImageData(string code)
    {
        var data = Db.MobileCostImages.Where(x => x.Code == code);

        return data.OrderBy(x => x.LineNo);
    }

    public bool IsCostExists(string date, int userId)
    {
        var salesmanId = Db.Users.SingleOrDefault(x => x.Id == userId)?.EmployeeId;
        return Db.MobileCostHeaders.Any(x => x.Date == DateTime.ParseExact(date, "yyyy-MM-dd", null) && x.SalesmanId == salesmanId);
    }

    public SaveResult Insert(MobileCostRequest data)
    {
        var result = new SaveResult(false);

        long? employeeId = Db.Users.SingleOrDefault(x => x.Id.Equals(data.CreatedBy))?.EmployeeId;
        using var transaction = Db.Database.BeginTransaction();
        try
        {
            var newCode = GetNewCode("MOB_SC_NUM_FMT", data.Date);

            data.Code = newCode;
            data.Total = data.ItemDetails.Sum(x => x.Amount);
            data.SalesmanId = (long)employeeId;

            Db.MobileCostHeaders.Add(data);

            short i = 0;
            foreach (var cost in data.ItemDetails)
            {
                Db.MobileCostDetails.Add(new MobileCostDetail
                {
                    Code = newCode,
                    LineNo = ++i,
                    CoaCode = cost.CoaCode,
                    Amount = cost.Amount
                });
            }

            short j = 0;
            foreach (var cost in data.ImageDetails)
            {
                Db.MobileCostImages.Add(new MobileCostImage
                {
                    Code = newCode,
                    LineNo = ++j,
                    Image = cost.Image
                });
            }

            Db.SaveChanges();
            transaction.Commit();
        }
        catch (Exception e)
        {
            result.Message = e.InnerException?.Message ?? e.Message;
            return result;
        }

        result.Success = true;
        result.Data = data.Code;
        result.Message = "Biaya sales berhasil disimpan.";
        return result;
    }

    public SaveResult Update(MobileCostRequest data)
    {
        var result = new SaveResult(false);

        // Checking mark header data
        if (Db.MobileCostHeaders.Any(x => x.Code == data.Code && x.Mark != "A"))
        {
            result.Message = "Data biaya sales mobile tidak bisa diubah karena sudah tidak aktif.";
            return result;
        }

        // Get detail data that exists in detail before
        var delDetails = Db.MobileCostDetails.Where(d => d.Code == data.Code).ToList();

        // Delete detail data that exists in detail before
        Db.MobileCostDetails.RemoveRange(delDetails);

        if (data.ItemDetails.GroupBy(x => x.CoaCode).Any(x => x.Count() > 1))
        {
            result.Message = "Terdapat akun dengan code yang sama pada bagian detail.";
            return result;
        }

        var headerData = Db.MobileCostHeaders.FirstOrDefault(x => x.Code == data.Code);
        if (headerData == null)
        {
            result.Message = "Data biaya sales mobile tidak ditemukan.";
            return result;
        }

        // Update detail data
        short i = 0;
        foreach (var item in data.ItemDetails)
        {
            Db.MobileCostDetails.Add(new MobileCostDetail
            {
                Code = data.Code,
                LineNo = ++i,
                CoaCode = item.CoaCode,
                Amount = item.Amount
            });
        }

        headerData.Total = data.ItemDetails.Sum(x => x.Amount);
        Db.MobileCostHeaders.Update(headerData);

        Db.SaveChanges();

        result.Success = true;
        result.Data = data.Code;
        result.Message = "Data biaya sales mobile berhasil diperbarui.";
        return result;
    }

    public SaveResult Approve(List<MobileCostRequest> data, int userId, string date, string coa, string notes)
    {
        var result = new SaveResult(false);

        if (!data.Any())
            return new SaveResult(false, "Tidak ada data yang di proses");

        if (data.Any(x => x.Mark != "A"))
            return new SaveResult(false, "Tidak dapat menyetujui data yang sudah disetujui atau ditolak");

        var vDate = Convert.ToDateTime(date);

        using var transaction = Db.Database.BeginTransaction();
        try
        {
            foreach (var itemCost in data)
            {
                // Get new code
                var newCode = GetNewCode("CB_NUM_FMT", vDate);

                // Insert header data
                var headData = new GeneralCashBankHeader
                {
                    Code = newCode,
                    Type = "C",
                    Date = vDate,
                    CoaCode = coa,
                    CurrCode = itemCost.CurrCode,
                    Rate = itemCost.Rate,
                    Amount = -itemCost.Total,
                    Notes = notes,
                    IsInterCashBank = false,
                    Mark = "A",
                    CreatedBy = userId,
                    CreatedDate = DateTime.Now,
                    UpdatedBy = userId,
                    UpdatedDate = DateTime.Now,
                    ApprovedBy = userId,
                    ApprovedDate = DateTime.Now
                };

                Db.GeneralCashBankHeaders.Add(headData);

                var itemData = Db.MobileCostDetails.Where(x => x.Code == itemCost.Code).ToList();
                short j = 0;
                foreach (var item in itemData)
                {
                    Db.GeneralCashBankDetails.Add(new GeneralCashBankDetail
                    {
                        Code = headData.Code,
                        LineNo = ++j,
                        Type = "TU",
                        CoaCode = item.CoaCode,
                        CurrCode = "IDR",
                        Rate = 1,
                        Amount = item.Amount,
                        TransAmount = item.Amount,
                        TypeAmount = "D",
                        Notes = notes
                    });
                }

                var mcData = Db.MobileCostHeaders.FirstOrDefault(x => x.Code == itemCost.Code);
                mcData.CashBankCode = newCode;
                mcData.Mark = "APR";
                mcData.ApprovedBy = userId;
                mcData.ApprovedDate = DateTime.Now;
                Db.MobileCostHeaders.Update(mcData);

                Db.SaveChanges();
            }
            transaction.Commit();
        }
        catch (Exception ex)
        {
            result.Message = ex.InnerException?.Message ?? ex.Message;
            return result;
        }

        result.Success = true;
        result.Message = "Data biaya sales mobile berhasil disetujui.";
        return result;
    }

    public SaveResult Reject(List<MobileCostRequest> data, int userId)
    {
        var result = new SaveResult(false);

        if (!data.Any())
            return new SaveResult(false, "Tidak ada data yang di proses");

        if (data.Any(x => x.Mark != "A"))
            return new SaveResult(false, "Tidak dapat menolak data yang sudah disetujui atau ditolak");

        foreach (var item in data)
        {
            var mirData = Db.MobileCostHeaders.FirstOrDefault(x => x.Code == item.Code);
            mirData.RejectedBy = userId;
            mirData.RejectedDate = DateTime.Now;
            mirData.Mark = "REJ";
            Db.MobileCostHeaders.Update(mirData);
        }

        Db.SaveChanges();

        result.Success = true;
        result.Message = "Data biaya sales mobile berhasil ditolak.";
        return result;
    }

    #region Mobile
    public DataSourceResult GetDataForMobile(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts, int userId, string date)
    {
        var data = (from costHeader in Db.MobileCostHeaders
            join user in Db.Users on costHeader.SalesmanId equals user.EmployeeId
            where user.Id.Equals(userId)
            select new MobileCostHeader
            {
                Code = costHeader.Code,
                Date = costHeader.Date,
                Mark = costHeader.Mark,
                SalesmanId = costHeader.SalesmanId,
                Rate = costHeader.Rate,
                ApprovedDate = costHeader.ApprovedDate,
                Total = costHeader.Total,
            }).AsQueryable();
        if (!string.IsNullOrEmpty(date))
        {
            var date1 = DateTime.ParseExact(date, "yyyy-MM-dd", null);
            data = data.Where(x => x.Date.Equals(date1));
        }
        return data.ToDataSourceResult(skip, take, filters, sorts);
    }

    public IEnumerable<CostDetailModel> GetDetailForMobile(string Code)
    {
        var data = from cost in Db.MobileCostDetails
            join coa in Db.Coas on cost.CoaCode equals coa.Code
            where cost.Code.Equals(Code)
            select new CostDetailModel
            {
                Code = cost.Code,
                LineNo = cost.LineNo,
                CoaCode = cost.CoaCode,
                CoaName = coa.Name,
                Amount = cost.Amount,
            };
        return data.ToList();
    }

    public SaveResult InsertForMobile(CostRequestModel data, int UserId)
    {
        var result = new SaveResult(false);

        long? employeeId = Db.Users.Where(x => x.Id.Equals(UserId)).Select(u => u.EmployeeId).SingleOrDefault();
        using var transaction = Db.Database.BeginTransaction();
        try
        {
            var newCode = GetNewCode("MOB_SC_NUM_FMT", data.Date);

            data.Code = newCode;
            data.Total = data.CostDetails.Sum(x => x.Amount);
            data.SalesmanId = (long)employeeId;

            Db.MobileCostHeaders.Add(data);

            short i = 0;
            foreach (var cost in data.CostDetails)
            {
                Db.MobileCostDetails.Add(new MobileCostDetail
                {
                    Code = newCode,
                    LineNo = ++i,
                    CoaCode = cost.CoaCode,
                    Amount = cost.Amount
                });
            }

            short j = 0;
            foreach (var cost in data.CostImages)
            {
                Db.MobileCostImages.Add(new MobileCostImage
                {
                    Code = newCode,
                    LineNo = ++j,
                    Image = cost.Image
                });
            }

            Db.SaveChanges();
            transaction.Commit();
        }
        catch (Exception e)
        {
            result.Message = e.InnerException?.Message ?? e.Message;
            return result;
        }

        result.Success = true;
        result.Data = data.Code;
        result.Message = "Biaya sales berhasil disimpan.";
        return result;
    }

    public IEnumerable<Coa> GetMobileCoaForMobile(string lastUpdate)
    {
        var data = Db.Coas.Where(x => x.ShowInMobile == true).AsQueryable();

        if (lastUpdate != null)
        {
            if (lastUpdate.Length == 19)
                lastUpdate += ".0000";
            else if (lastUpdate.Length == 21)
                lastUpdate += "000";
            else if (lastUpdate.Length == 22)
                lastUpdate += "00";
            else if (lastUpdate.Length == 23)
                lastUpdate += "0";
            data = data.Where(x => x.UpdatedDate > DateTime.ParseExact(lastUpdate, "yyyy-MM-ddTHH:mm:ss.ffff", null));
        }

        return data;
    }

    public IEnumerable<CostImageModel> GetImageForMobile(string Code)
    {
        var data = from image in Db.MobileCostImages
            where image.Code.Equals(Code)
            select new CostImageModel
            {
                Code = image.Code,
                LineNo = image.LineNo,
                Image = image.Image,
            };
        return data.ToList();
    }

    public CostTodayTransactionModel GetTodayTransactionForMobile(string date, int userId)
    {
        var salesmanId = Db.Users.Where(x => x.Id.Equals(userId)).Select(x => x.EmployeeId).SingleOrDefault();
        var date1 = Db.MobileCostHeaders.Any(x => x.Date == DateTime.ParseExact(date, "yyyy-MM-dd", null) && x.SalesmanId == salesmanId);
        var data = new CostTodayTransactionModel { Exist = date1 };
        return data;
    }
    #endregion
}