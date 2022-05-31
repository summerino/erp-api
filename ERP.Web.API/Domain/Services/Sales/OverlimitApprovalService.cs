using Microsoft.EntityFrameworkCore;
using ERP.Common;
using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.Sales;
using ERP.Web.API.Model.Sales;

namespace ERP.Web.API.Domain.Services.Sales;

public class OverlimitApprovalService : IOverlimitApprovalService
{
    private readonly TenantContext _tenantCtx;

    public OverlimitApprovalService(TenantContext tenantCtx)
    {
        _tenantCtx = tenantCtx;
    }

    public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts, string search)
    {
        var data = _tenantCtx.VwSalesOrderHeaders.AsQueryable()
            .Select(x => new
            {
                x.Code,
                x.Date,
                x.SalesInitial,
                x.SalesName,
                x.CustCode,
                x.CustName,
                x.Total,
                x.Mark,
                x.UpdatedDate,
                SourceTrans = x.FromDirectInvoice ? "Penjualan Langsung" : "Order Penjualan"
            });

        if (!string.IsNullOrEmpty(search))
        {
            data = DateTime.TryParse(search, out var searchDate)
                ? data.Where(x => x.Date == searchDate)
                : data.Where(x =>
                    x.Code.Contains(search) || x.SalesInitial.Contains(search) || x.CustCode.StartsWith(search) ||
                    x.CustName.Contains(search) || x.SourceTrans.Contains(search));
        }

        return data.ToDataSourceResult(skip, take, filters, sorts);
    }

    public SaveResult SaveChanges(List<SalesOrderRequest> data, int userId)
    {
        var result = new SaveResult(false);

        if (!data.Any())
            return new SaveResult(false, "Tidak ada data yang di proses.");

        try
        {
            foreach (var item in data)
            {
                if (item.FromDirectInvoice)
                {
                    var soData = _tenantCtx.SalesOrderHeaders.FirstOrDefault(x => x.Code == item.Code);
                    if (soData == null)
                        return new SaveResult(false, $"Data dengan kode { item.Code } tidak valid.");

                    soData.Mark = "A";
                    soData.OverlimitApprovedBy = userId;
                    soData.OverlimitApprovedDate = DateTime.Now;
                    _tenantCtx.SalesOrderHeaders.Update(soData);

                    UpdateCreditUsed(soData.CustCode, soData.Total);

                    var doData = _tenantCtx.SalesDeliveryHeaders.FirstOrDefault(x => x.Code == item.Code);
                    if (doData == null)
                        return new SaveResult(false, $"Data dengan kode { item.Code } tidak valid.");

                    doData.Mark = "INV";
                    _tenantCtx.SalesDeliveryHeaders.Update(doData);

                    _tenantCtx.Database.ExecuteSqlRaw("EXEC sp_update_so_dlv_qty {0}", soData.Code);

                    var siData = _tenantCtx.SalesInvoiceHeaders.FirstOrDefault(x => x.Code == item.Code);
                    if (siData == null)
                        return new SaveResult(false, $"Data dengan kode { item.Code } tidak valid.");

                    siData.Mark = "A";
                    _tenantCtx.SalesInvoiceHeaders.Update(siData);
                }
                else
                {
                    var soData = _tenantCtx.SalesOrderHeaders.FirstOrDefault(x => x.Code == item.Code);
                    if (soData == null)
                        return new SaveResult(false, $"Data dengan kode { item.Code } tidak valid.");

                    soData.Mark = "A";
                    soData.OverlimitApprovedBy = userId;
                    soData.OverlimitApprovedDate = DateTime.Now;
                    _tenantCtx.SalesOrderHeaders.Update(soData);

                    UpdateCreditUsed(soData.CustCode, soData.Total);
                }
            }

            _tenantCtx.SaveChanges();
        }
        catch (Exception ex)
        {
            result.Message = ex.InnerException?.Message ?? ex.Message;
            return result;
        }

        result.Success = true;
        result.Message = "Data kelebihan batas kredit berhasil disetujui.";
        return result;
    }

    private void UpdateCreditUsed(string custCode, decimal total)
    {
        string query = $"update General.Customer set CreditUsed= (CreditUsed + {total}) where code = '{custCode}'";
        _tenantCtx.Database.ExecuteSqlRaw(query);
    }
}