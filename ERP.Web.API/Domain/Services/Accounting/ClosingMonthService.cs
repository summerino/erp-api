using System;
using System.Collections.Generic;
using System.Linq;
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
                        x.Period.Contains(search));
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

                if(Db.ClosingMonths.Where(x => Convert.ToInt32(x.Period) < Convert.ToInt32($"{startDate.Year}{(startDate.Month > 9 ? startDate.Month : "0" + startDate.Month)}") && x.IsClose == false).Any())
                {
                    result.Message = "Tidak bisa melakukan tutup bulan karena bulan sebelumnya ada yang belum ditutup.";
                    return result;
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

            if (Db.ClosingMonths.Where(x => Convert.ToInt32(x.Period) < Convert.ToInt32(data.Period) && x.IsClose == false).Any())
            {
                result.Message = "Tidak bisa melakukan tutup bulan karena bulan sebelumnya ada yang belum ditutup.";
                return result;
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
