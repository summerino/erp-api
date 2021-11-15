using ERP.Common;
using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Entity.Accounting;
using ERP.Web.API.Domain.Interfaces.Accounting;
using ERP.Web.API.Model.Accounting;

namespace ERP.Web.API.Domain.Services.Accounting
{
    public class ClosingMonthService : GeneralService<ClosingMonth>, IClosingMonthService
    {
        public ClosingMonthService(TenantContext db)
            :base(db)
        {
        }

        public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts, string search)
        {
            var data = Db.VwClosingMonths.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                data = data.Where(x =>
                        x.PeriodName.Contains(search));
            }

            return data.ToDataSourceResult(skip, take, filters, sorts);
        }

        public SaveResult Insert(ClosingMonthRequest data)
        {
            var result = new SaveResult(false);

            try
            {
                DateTime startDate = new(data.StartDate.Value.Year, data.StartDate.Value.Month, 1);
                DateTime endDate = new(data.EndDate.Value.Year, data.EndDate.Value.Month, 1);
                //Check if EndDate greater than StartDate
                if (endDate < startDate)
                {
                    result.Message = "Tanggal akhir tidak boleh lebih kecil dari tanggal mulai.";
                    return result;
                }

                // Check if dataStartDate & previous month is valid
                var bbPeriod = Convert.ToDateTime(Db.SystemParameters.FirstOrDefault(x => x.Code == "DATA_START_DATE")?.Value).AddMonths(-1).ToString("yyyyMM");
                if (startDate.Month == 1)
                {
                    if (bbPeriod == null)
                    {
                        result.Message = "Tidak bisa melakukan tutup bulan karena data periode sebelumnya tidak ada.";
                        return result;
                    }
                }
                else
                {
                    var prevDataMonth = startDate.AddMonths(-1);
                    var prevData = Db.ClosingMonths.FirstOrDefault(x => x.Period == $"{startDate.Year}{(prevDataMonth.Month > 9 ? prevDataMonth.Month : "0" + prevDataMonth.Month)}");
                    if (prevData == null)
                    {
                        result.Message = "Tidak bisa melakukan tutup bulan karena data periode sebelumnya tidak ada.";
                        return result;
                    }
                }

                if (data.IsClose)
                {
                    // Check if previous month still open
                    if (Db.ClosingMonths.Where(x => Convert.ToInt32(x.Period) < Convert.ToInt32($"{startDate.Year}{(startDate.Month > 9 ? startDate.Month : "0" + startDate.Month)}") && x.IsClose == false).Any())
                    {
                        result.Message = "Tidak bisa melakukan tutup bulan karena terdapat periode sebelumnya yang belum ditutup.";
                        return result;
                    }
                }

                // Insert data
                for (var dataMonth = startDate; dataMonth.Date <= endDate.Date; dataMonth = dataMonth.AddMonths(1))
                {
                    if (Db.ClosingMonths.FirstOrDefault(x => x.Period == $"{dataMonth.Year}{(dataMonth.Month > 9 ? dataMonth.Month : "0" + dataMonth.Month)}") == null)
                    {
                        Db.ClosingMonths.Add(new ClosingMonth
                        {
                            Period = $"{dataMonth.Year}{(dataMonth.Month > 9 ? dataMonth.Month : "0" + dataMonth.Month)}",
                            IsClose = data.IsClose,
                            CreatedBy = data.CreatedBy,
                            CreatedDate = data.CreatedDate,
                            UpdatedBy = data.UpdatedBy,
                            UpdatedDate = data.UpdatedDate
                        });

                        var plData = Db.PostingLogs.FirstOrDefault(x => x.Period == $"{dataMonth.Year}{(dataMonth.Month > 9 ? dataMonth.Month : "0" + dataMonth.Month)}");
                        if (plData != null)
                        {
                            plData.IsPosted = false;
                            plData.PostedBy = null;
                            plData.PostedDate = null;
                            Db.PostingLogs.Update(plData);
                        } 
                        else
                        {
                            Db.PostingLogs.Add(new PostingLog
                            {
                                Period = $"{dataMonth.Year}{(dataMonth.Month > 9 ? dataMonth.Month : "0" + dataMonth.Month)}",
                                IsPosted = false
                            });
                        }
                    }
                }

                Db.SaveChanges();
            }
            catch (Exception ex)
            {
                result.Message = ex.InnerException?.Message ?? ex.Message;
                return result;
            }

            result.Success = true;
            result.Message = "Data tutup bulan berhasil disimpan.";
            return result;
        }

        public bool IsMonthClosed(List<string> periods)
        {
            return Db.ClosingMonths.Any(x => x.IsClose && periods.Contains(x.Period));
        }

        public override SaveResult Update(ClosingMonth data)
        {
            var result = new SaveResult(false);

            var bbPeriod =
                Convert.ToInt32(
                    Convert
                        .ToDateTime(Db.SystemParameters.FirstOrDefault(x => x.Code == "DATA_START_DATE")?.Value).AddMonths(-1)
                        .ToString("yyyyMM"));

            if (data.IsClose)
            {
                if (
                    Db.ClosingMonths.Any(x =>
                        Convert.ToInt32(x.Period) >= bbPeriod &&
                        Convert.ToInt32(x.Period) < Convert.ToInt32(data.Period) &&
                        x.IsClose == false))
                {
                    result.Message = "Tidak bisa melakukan tutup bulan karena terdapat periode sebelumnya yang belum ditutup.";
                    return result;
                }

                if (
                    Db.PostingLogs.Any(x =>
                        Convert.ToInt32(x.Period) >= bbPeriod &&
                        Convert.ToInt32(x.Period) <= Convert.ToInt32(data.Period) &&
                        x.IsPosted == false))
                {
                    result.Message = "Tidak bisa melakukan tutup bulan karena terdapat periode sekarang atau sebelumnya yang belum diposting.";
                    return result;
                }
            }
            else
            {
                var currData = Db.PostingLogs.FirstOrDefault(x => x.Period == data.Period);
                if (currData != null)
                {
                    currData.IsPosted = false;
                    currData.PostedBy = null;
                    currData.PostedDate = null;
                    Db.PostingLogs.Update(currData);
                }
                else
                {
                    Db.PostingLogs.Add(new PostingLog
                    {
                        Period = data.Period,
                        IsPosted = false
                    });
                }

                var cmData = Db.ClosingMonths.Where(x => Convert.ToInt32(x.Period) > Convert.ToInt32(data.Period))
                    .ToList()
                    .Select(x =>
                    {
                        x.IsClose = false;
                        return x;
                    });
                Db.ClosingMonths.UpdateRange(cmData);
                
                var plData = Db.PostingLogs.Where(x => Convert.ToInt32(x.Period) > Convert.ToInt32(data.Period))
                    .ToList()
                    .Select(x =>
                    {
                        x.IsPosted = false;
                        return x;
                    });
                Db.PostingLogs.UpdateRange(plData);
            }


            // Update data
            Db.ClosingMonths.Update(data);
            Db.Entry(data).Property(e => e.Period).IsModified = false;
            Db.Entry(data).Property(e => e.CreatedBy).IsModified = false;
            Db.Entry(data).Property(e => e.CreatedDate).IsModified = false;

            Db.SaveChanges();

            result.Success = true;
            result.Data = data.Period;
            result.Message = "Data tutup bulan berhasil diperbarui.";
            return result;
        }
    }
}
