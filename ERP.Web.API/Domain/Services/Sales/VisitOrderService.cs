using ERP.Common;
using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Entity.Sales;
using ERP.Web.API.Domain.Interfaces.Sales;
using ERP.Web.API.Model.Sales;

namespace ERP.Web.API.Domain.Services.Sales;

public class VisitOrderService : GeneralService<VisitOrder>, IVisitOrderService
{
    public VisitOrderService(TenantContext db)
        : base(db)
    {
    }

    public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
        List<int> category, string search)
    {
        var data = Db.VwVisitOrders.AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            data = DateTime.TryParse(search, out var searchDate)
                ? data.Where(x => x.Date == searchDate)
                : data.Where(x =>
                    x.Code.Contains(search) || x.VisitPlanCode.Contains(search));
        }

        return data.ToDataSourceResult(skip, take, filter, sort);
    }

    public IEnumerable<VwVisitOrderCustomer> GetVisitOrderCustomer(string code)
    {
        var data = Db.VwVisitOrderCustomers.Where(x => x.Code == code);

        return data.OrderBy(x => x.Id);
    }

    public IEnumerable<VwVisitOrderInvoice> GetVisitOrderInvoice(string code)
    {
        var data = Db.VwVisitOrderInvoices.Where(x => x.Code == code);

        return data.OrderBy(x => x.Id);
    }

    public SaveResult Insert(VisitOrderRequest data)
    {
        var result = new SaveResult(false);
        var arrCustCode = new List<string>();
        var arrInvCode = new List<string>();

        using var transaction = Db.Database.BeginTransaction();
        try
        {
            if (data.VisitPlanCode != null)
            {
                // Checking if employee is assigned in same date
                if (IsEmployeeAssignedInSameDate(data.Date, data.SalesmanId))
                {
                    result.Message = "Karyawan tidak dapat dipilih pada tanggal yang sama.";
                    return result;
                }
            }

            // Get new code
            var newCode = GetNewCode("VST_ORD_NUM_FMT", data.Date);

            // Insert data
            data.Code = newCode;
            Db.VisitOrders.Add(data);
            Db.SaveChanges();

            if (data.CustomerDetails.Any())
            {
                // Insert detail data CustomerDetails
                foreach (var item in data.CustomerDetails)
                {
                    // Prevent multiple insert details
                    if (!arrCustCode.Contains(item.CustCode))
                    {
                        arrCustCode.Add(item.CustCode);
                        Db.VisitOrderCustomers.Add(new VisitOrderCustomer
                        {
                            Code = newCode,
                            CustCode = item.CustCode,
                            ReplacingForSalesmanId = item.ReplacingForSalesmanId,
                            Visited = item.Visited
                        });
                    }
                }
            }

            if (data.InvoiceDetails.Any())
            {
                // Insert detail data Invoice
                foreach (var item in data.InvoiceDetails)
                {
                    // Prevent multiple insert details
                    if (!arrInvCode.Contains(item.InvCode))
                    {
                        arrInvCode.Add(item.InvCode);
                        Db.VisitOrderInvoices.Add(new VisitOrderInvoice
                        {
                            Code = newCode,
                            InvCode = item.InvCode,
                            Collecting = item.Collecting,
                            FailCollect = item.FailCollect,
                            NotesFailCollect = item.NotesFailCollect
                        });
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
        result.Data = data.Code;
        result.Message = "Data perintah kunjungan berhasil disimpan.";
        return result;
    }

    public SaveResult Update(VisitOrderRequest data)
    {
        var result = new SaveResult(false);
        var arrCustCode = new List<string>();
        var arrInvCode = new List<string>();

        using var transaction = Db.Database.BeginTransaction();
        try
        {
            // Checking mark header data
            if (Db.VisitOrders.Any(x => x.Code == data.Code && x.Mark == "V"))
            {
                result.Message = "Data perintah kunjungan tidak bisa diubah karena sudah ditandai sebagai void.";
                return result;
            }

            if (Db.Attendances.Any(x => x.EmployeeId == data.SalesmanId && x.Date == data.Date))
            {
                result.Message = $"Data perintah kunjungan tidak bisa diubah karena penjual sudah absen masuk pada tanggal {data.Date:dd-MMM-yyyy}.";
                return result;
            }

            // Update header data
            data.ApprovedBy = null;
            data.ApprovedDate = null;
            Db.VisitOrders.Update(data);
            Db.Entry(data).Property(e => e.Code).IsModified = false;
            Db.Entry(data).Property(e => e.CreatedBy).IsModified = false;
            Db.Entry(data).Property(e => e.CreatedDate).IsModified = false;

            // Get detail data that exists in order before
            var delCustomerDetails = Db.VisitOrderCustomers
                .Where(d => d.Code == data.Code).ToList();

            // Delete detail data that exists in order before
            Db.VisitOrderCustomers.RemoveRange(delCustomerDetails);

            // Get detail data that exists in order before
            var delInvoiceDetails = Db.VisitOrderInvoices
                .Where(d => d.Code == data.Code).ToList();

            // Delete detail data that exists in order before
            Db.VisitOrderInvoices.RemoveRange(delInvoiceDetails);

            if (data.CustomerDetails.Any())
            {
                // Insert detail data CustomerDetails
                foreach (var item in data.CustomerDetails)
                {
                    // Prevent multiple insert details
                    if (!arrCustCode.Contains(item.CustCode))
                    {
                        arrCustCode.Add(item.CustCode);
                        Db.VisitOrderCustomers.Add(new VisitOrderCustomer
                        {
                            Code = data.Code,
                            CustCode = item.CustCode,
                            ReplacingForSalesmanId = item.ReplacingForSalesmanId,
                            Visited = item.Visited
                        });
                    }
                }
            }

            if (data.InvoiceDetails.Any())
            {
                // Insert detail data Invoice
                foreach (var item in data.InvoiceDetails)
                {
                    // Prevent multiple insert details
                    if (!arrInvCode.Contains(item.InvCode))
                    {
                        arrInvCode.Add(item.InvCode);
                        Db.VisitOrderInvoices.Add(new VisitOrderInvoice
                        {
                            Code = data.Code,
                            InvCode = item.InvCode,
                            Collecting = item.Collecting,
                            FailCollect = item.FailCollect,
                            NotesFailCollect = item.NotesFailCollect
                        });
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
        result.Data = data.Code;
        result.Message = "Data perintah kunjungan berhasil diperbarui.";
        return result;
    }

    public SaveResult Delete(string code, int userId)
    {
        var result = new SaveResult(false);

        using var transaction = Db.Database.BeginTransaction();
        try
        {
            var data = Db.VisitOrders.Find(code);
            if (data != null)
            {
                // Checking mark header data
                if (data.Mark == "V")
                {
                    result.Message = "Data perintah kunjungan tidak bisa dihapus karena data sudah tidak ada.";
                    return result;
                }

                // Update data
                data.Mark = "V";
                data.UpdatedBy = userId;
                data.UpdatedDate = DateTime.Now;
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
        result.Message = "Data perintah kunjungan berhasil dihapus.";
        return result;
    }
    public bool IsSalesHasScheduledVisitOrder(int salesId, string date)
    {
        return IsEmployeeAssignedInSameDate(Convert.ToDateTime(date), salesId);
    }

    private bool IsEmployeeAssignedInSameDate(DateTime date, long id)
    {
        return Db.VwVisitOrders.Any(x => x.SalesmanId == id && x.Date == date && x.Mark == "A" && x.VisitPlanCode != null);
    }


}