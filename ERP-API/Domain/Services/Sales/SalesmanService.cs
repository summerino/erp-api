using System;
using System.Collections.Generic;
using System.Linq;
using ERP_API.Domain.Entities;
using ERP_API.Domain.Entities.Sales;
using ERP_API.Domain.Extensions;
using ERP_API.Domain.Interfaces.Sales;
using ERP_API.Domain.Models;
using ERP_API.Model.Sales;

namespace ERP_API.Domain.Services.Sales
{
    public class SalesmanService : GeneralService<SalesmanGroup>, ISalesmanService
    {
        public SalesmanService(TenantContext db)
            : base(db)
        {
        }

        public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
            List<int> category, string search)
        {
            var data = Db.VwSalesmanGroups.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                data = data.Where(x =>
                        x.Initial.Contains(search) || x.Name.Contains(search) || x.SupervisorInitial.Contains(search));
            }

            return data.ToDataSourceResult(skip, take, filter, sort);
        }

        public IEnumerable<VwSalesmanSchedule> GetSalesmanSchedule(string groupId, string startDate, string recurrence, string visitDay)
        {
            var data = Db.VwSalesmanSchedules.AsQueryable();

            if (!string.IsNullOrEmpty(groupId) && !string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(recurrence) && !string.IsNullOrEmpty(visitDay))
            {
                if (DateTime.TryParse(startDate, out var outStartDate))
                {
                    data = data.Where(x => x.VisitDay == Convert.ToByte(visitDay) && x.SalesGroupId == Convert.ToInt32(groupId)).Where(x => outStartDate >= x.StartDate).Where(x => outStartDate <= x.EndDate);
                }

                return data.OrderBy(x => x.Id);
            }
            else
            {
                return Db.VwSalesmanSchedules.Where(x => x.Id == 0);
            }
        }

        public IEnumerable<VwSalesmanScheduleCustomer> GetSalesmanScheduleDetailData(List<long> id)
        {
            var data = Db.VwSalesmanScheduleCustomers.AsQueryable();

            if (id?.Any() ?? false)
            {
                data = Db.VwSalesmanScheduleCustomers.Where(x => id.Contains(x.SalesmanScheduleId));
            }

            return data.OrderBy(x => x.SalesmanScheduleId);
        }

        public override SaveResult Insert(SalesmanGroup data)
        {
            var result = new SaveResult(false);

            using var transaction = Db.Database.BeginTransaction();
            try
            {
                // Checking initial already exists or not
                if (IsInitialExists(data.Initial, 0))
                {
                    result.Message = "Inisial sudah terdaftar. Tolong gunakan inisial lain.";
                    return result;
                }

                Db.Add(data);

                Db.SaveChanges();
                transaction.Commit();
            }
            catch (Exception ex)
            {
                result.Message = ex.InnerException?.Message ?? ex.Message;
                return result;
            }

            result.Success = true;
            result.Data = data.Id;
            result.Message = "Data grup penjual berhasil disimpan.";
            return result;
        }

        public override SaveResult Update(SalesmanGroup data)
        {
            var result = new SaveResult(false);

            // Checking initial already exists or not
            if (IsInitialExists(data.Initial, data.Id))
            {
                result.Message = "Inisial sudah terdaftar. Tolong gunakan inisial lain.";
                return result;
            }

            // Update data
            Db.SalesmanGroups.Update(data);
            Db.Entry(data).Property(e => e.Id).IsModified = false;
            Db.Entry(data).Property(e => e.CreatedBy).IsModified = false;
            Db.Entry(data).Property(e => e.CreatedDate).IsModified = false;

            Db.SaveChanges();

            result.Success = true;
            result.Data = data.Id;
            result.Message = "Data grup penjual berhasil diperbarui.";
            return result;
        }

        public SaveResult Delete(int id, int userId)
        {
            var result = new SaveResult(false);

            var data = Db.SalesmanGroups.Find(id);
            if (data != null)
            {
                // Checking active
                if (data.IsActive == false)
                {
                    result.Message = "Tidak bisa menghapus data grup penjual karena data sudah dihapus.";
                    return result;
                }

                Db.SalesmanGroups.Remove(data);

                Db.SaveChanges();
            }

            result.Success = true;
            result.Message = "Data grup penjual berhasil dihapus.";
            return result;
        }

        private bool IsInitialExists(string initial, int id)
        {
            return Db.SalesmanGroups.Any(x => x.Initial == initial && x.Id != id && x.IsActive == true);
        }
    }
}
