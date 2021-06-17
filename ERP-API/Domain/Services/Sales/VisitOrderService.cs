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
                        x.Code.Contains(search) || x.VisitPlanCode.Contains(search) || x.CustomerName.Contains(search));
            }

            return data.ToDataSourceResult(skip, take, filter, sort);
        }

        public SaveResult Insert(VisitOrderRequest data)
        {
            var result = new SaveResult(false);            

            using var transaction = Db.Database.BeginTransaction();
            try
            {
                // Get new code
                var newCode = GetNewCode("VST_ORD_NUM_FMT", data.Date);

                // Insert data
                data.Code = newCode;
                Db.VisitOrders.Add(data);
                Db.SaveChanges();

                // Insert detail data CustomerDetails
                foreach (var item in data.CustomerDetails)
                {
                    Db.VisitOrderCustomers.Add(new VisitOrderCustomer
                    {
                        Code = newCode,
                        CustCode = item.CustCode,
                        ReplacingForSalesmanId = item.ReplacingForSalesmanId,
                        Visited = item.Visited
                    });
                }

                // Insert detail data Invoice
                foreach (var item in data.InvoiceDetails)
                {
                    Db.VisitOrderInvoices.Add(new VisitOrderInvoice
                    {
                        Code = newCode,
                        InvCode = item.InvCode,
                        Collecting = item.Collecting,
                        FailCollect = item.FailCollect,
                        NotesFailCollect = item.NotesFailCollect
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
            result.Data = data.Code;
            result.Message = "Data perintah kunjungan berhasil disimpan.";
            return result;
        }

        public SaveResult Update(VisitOrderRequest data)
        {
            var result = new SaveResult(false);
            
            using var transaction = Db.Database.BeginTransaction();
            try
            {
                // Disable sementara
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
    }
}
