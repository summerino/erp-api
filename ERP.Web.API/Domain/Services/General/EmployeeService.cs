using ERP.Common;
using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Entity.General;
using ERP.Entity.Sales;
using ERP.Web.API.Domain.Interfaces.General;
using ERP.Web.API.Model.General;

namespace ERP.Web.API.Domain.Services.General;

public class EmployeeService : GeneralService<Employee>, IEmployeeService
{
    public EmployeeService(TenantContext db)
        : base(db)
    {
    }

    public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
        string search)
    {
        var data = Db.VwEmployees.AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            data = search.ToLower() switch
            {
                "male" => data.Where(x => x.Sex),
                "female" => data.Where(x => !x.Sex),
                _ => data.Where(x =>
                    x.Initial.Contains(search) || x.FirstName.Contains(search) || x.LastName.Contains(search) ||
                    x.Address1.Contains(search) || x.Phone.Contains(search))
            };
        }

        return data.ToDataSourceResult(skip, take, filter, sort);
    }

    public DataSourceResult GetLists(IEnumerable<Filter> filters, IEnumerable<Sort> sorts)
    {
        var data = Db.Employees.Where(x => x.IsActive);

        return data.ToDataSourceResult(0, -1, filters, sorts);
    }

    public IEnumerable<Employee> GetUnUsedList(long? empId)
    {
        var userData = Db.Users.Where(x => x.EmployeeId != empId).ToList();
        return Db.Employees.Where(x => x.IsActive && !userData.Select(d => d.EmployeeId).Contains(x.Id));
    }

    public SaveResult Insert(EmployeeRequest data)
    {
        var result = new SaveResult(false);
        var listIdDetail = new List<long>();
        var tempBeforeId = new List<long>();

        using var transaction = Db.Database.BeginTransaction();
        try
        {
            // Checking initial already exists or not
            if (IsInitialExists(data.Initial, 0))
            {
                result.Message = "Inisial sudah terdaftar. Tolong gunakan inisial lain.";
                return result;
            }

            // Insert data
            Db.Employees.Add(data);
            Db.SaveChanges();

            // Insert schedule for penjual
            if (data.ScheduleDetails?.Any() ?? false)
            {
                foreach (var item in data.ScheduleDetails)
                {
                    var itemSchedule = new SalesmanSchedule
                    {
                        SalesmanId = data.Id,
                        AreaId1 = item.AreaId1,
                        AreaId2 = item.AreaId2,
                        AreaId3 = item.AreaId3,
                        AreaId4 = item.AreaId4,
                        AreaId5 = item.AreaId5,
                        StartDate = item.StartDate,
                        EndDate = item.EndDate,
                        Recurrence = item.Recurrence,
                        VisitDay = item.VisitDay
                    };
                    tempBeforeId.Add(item.Id);
                    Db.SalesmanSchedules.Add(itemSchedule);
                    Db.SaveChanges();

                    listIdDetail.Add(itemSchedule.Id);
                }

                if (data.CustomerListDetails?.Any() ?? false)
                {
                    foreach (var itemDetail in data.CustomerListDetails)
                    {
                        for (var i = 0; i < tempBeforeId.Count; i++)
                        {
                            if (itemDetail.SalesmanScheduleId == tempBeforeId[i])
                            {
                                Db.SalesmanScheduleCustomers.Add(new SalesmanScheduleCustomer
                                {
                                    SalesmanScheduleId = listIdDetail[i],
                                    CustCode = itemDetail.CustCode
                                });
                            }
                        }
                    }
                }
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
        result.Data = data.Initial;
        result.Message = "Data karyawan berhasil disimpan.";
        return result;
    }

    public SaveResult Update(EmployeeRequest data)
    {
        var result = new SaveResult(false);
        var listIdDetail = new List<long>();
        var tempBeforeId = new List<long>();

        using var transaction = Db.Database.BeginTransaction();
        try
        {
            // Checking initial already exists or not
            if (IsInitialExists(data.Initial, data.Id))
            {
                result.Message = "Inisial sudah terdaftar. Tolong gunakan inisial lain.";
                return result;
            }

            // Update data
            Db.Employees.Update(data);
            Db.Entry(data).Property(e => e.Id).IsModified = false;
            Db.Entry(data).Property(e => e.CreatedBy).IsModified = false;
            Db.Entry(data).Property(e => e.CreatedDate).IsModified = false;

            // Delete existing item detail SalesmanSchedules
            var delScheduleDetails = Db.SalesmanSchedules
                .Where(d => d.SalesmanId == data.Id).ToList();

            if (delScheduleDetails.Count > 0)
            {
                for (var i = 0; i < delScheduleDetails.Count; i++)
                {
                    // Delete existing item detail SalesmanScheduleCustomers
                    var delCustomerDetails = Db.SalesmanScheduleCustomers
                        .Where(d => d.SalesmanScheduleId == delScheduleDetails[i].Id).ToList();

                    Db.SalesmanScheduleCustomers.RemoveRange(delCustomerDetails);
                }

                Db.SalesmanSchedules.RemoveRange(delScheduleDetails);
            }

            // Insert schedule for penjual
            if (data.ScheduleDetails?.Any() ?? false)
            {
                foreach (var item in data.ScheduleDetails)
                {
                    var itemSchedule = new SalesmanSchedule
                    {
                        SalesmanId = data.Id,
                        AreaId1 = item.AreaId1,
                        AreaId2 = item.AreaId2,
                        AreaId3 = item.AreaId3,
                        AreaId4 = item.AreaId4,
                        AreaId5 = item.AreaId5,
                        StartDate = item.StartDate,
                        EndDate = item.EndDate,
                        Recurrence = item.Recurrence,
                        VisitDay = item.VisitDay
                    };
                    tempBeforeId.Add(item.Id);
                    Db.SalesmanSchedules.Add(itemSchedule);
                    Db.SaveChanges();

                    listIdDetail.Add(itemSchedule.Id);
                }

                if (data.CustomerListDetails?.Any() ?? false)
                {
                    foreach (var itemDetail in data.CustomerListDetails)
                    {
                        for (var i = 0; i < tempBeforeId.Count; i++)
                        {
                            if (itemDetail.SalesmanScheduleId == tempBeforeId[i])
                            {
                                Db.SalesmanScheduleCustomers.Add(new SalesmanScheduleCustomer
                                {
                                    SalesmanScheduleId = listIdDetail[i],
                                    CustCode = itemDetail.CustCode
                                });
                            }
                        }
                    }
                }
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
        result.Data = data.Initial;
        result.Message = "Data karyawan berhasil diperbarui.";
        return result;
    }

    public SaveResult Delete(long id, int userId)
    {
        var result = new SaveResult(false);

        var data = Db.Employees.Find(id);
        if (data != null)
        {
            // Checking active
            if (data.IsActive == false)
            {
                result.Message = "Tidak bisa menonaktifkan data karyawan karena data sudah nonaktif.";
                return result;
            }

            // Update data
            data.IsActive = false;
            data.UpdatedBy = userId;
            data.UpdatedDate = DateTime.Now;

            Db.SaveChanges();
        }

        result.Success = true;
        result.Message = "Data karyawan berhasil dinonaktifkan.";
        return result;
    }

    private bool IsInitialExists(string initial, long id)
    {
        return Db.Employees.Any(x => x.Initial == initial && x.Id != id);
    }
}