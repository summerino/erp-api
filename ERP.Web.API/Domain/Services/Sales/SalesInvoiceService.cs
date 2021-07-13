using System;
using System.Collections.Generic;
using System.Linq;
using ERP.Common;
using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using Microsoft.EntityFrameworkCore;
using ERP.Entity.Sales;
using ERP.Web.API.Domain.Interfaces.Sales;
using ERP.Web.API.Domain.Models;
using ERP.Web.API.Model.Sales;

namespace ERP.Web.API.Domain.Services.Sales
{
    public class SalesInvoiceService : GeneralService<SalesInvoiceHeader>, ISalesInvoiceService
    {
        public SalesInvoiceService(TenantContext db)
            : base(db)
        {
        }

        public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
            string search)
        {
            var data = Db.VwSalesInvoiceHeaders.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                data = DateTime.TryParse(search, out var searchDate)
                    ? data.Where(x => x.Date == searchDate)
                    : data.Where(x =>
                        x.Code.Contains(search) || x.CustName.Contains(search) || x.SoCode == search ||
                        x.IssuedInitial.Contains(search));
            }

            return data.ToDataSourceResult(skip, take, filter, sort);
        }

        public IEnumerable<SalesInvoiceDetail> GetDetailData(string code)
        {
            return Db.SalesInvoiceDetails.Where(x => x.Code == code).OrderBy(x => x.LineNo);
        }

        public SaveResult Insert(SalesInvoiceRequest data)
        {
            var result = new SaveResult(false);

            using var transaction = Db.Database.BeginTransaction();
            try
            {
                // Checking sales order mark
                if (IsSalesOrderInvalid(data.SoCode))
                {
                    result.Message = "Data faktur penjualan tidak bisa diubah karena status order penjualan bukan diterima sebagian atau selesai.";
                    return result;
                }
                
                // Get new code
                var newCode = GetNewCode("SI_NUM_FMT", data.Date);
                    
                // Insert header data
                data.Code = newCode;
                Db.SalesInvoiceHeaders.Add(data);

                // Insert detail data
                short i = 0;
                foreach (var item in data.Details)
                {
                    Db.SalesInvoiceDetails.Add(new SalesInvoiceDetail
                    {
                        Code = newCode,
                        LineNo = ++i,
                        DoCode = item.DoCode,
                        ShipmentFee = item.ShipmentFee,
                        HandlingFee = item.HandlingFee,
                        SubTotal = item.SubTotal,
                        FinalDisc = item.FinalDisc,
                        TaxAmount = item.TaxAmount,
                        Total = item.Total,
                        Dpp = item.Dpp
                    });
                }

                // Save changes
                Db.SaveChanges();

                // Update sales delivery to invoiced
                Db.Database.ExecuteSqlRaw(
                    $@"UPDATE Sales.SalesDeliveryHeader
                    SET Mark='INV'
                    WHERE Code IN ('{string.Join("','", data.Details.Select(x => x.DoCode.Replace("'", "''")))}')");

                // Update sales order to closed if all sales delivery are invoiced
                if (
                    !Db.SalesDeliveryHeaders
                        .Any(x => x.TransCode == data.SoCode && x.Mark != "INV"))
                {
                    Db.Database.ExecuteSqlRaw(
                        "UPDATE Sales.SalesOrderHeader SET Mark='CLS' WHERE Code={0} AND Mark='CMP'", data.SoCode);
                }

                transaction.Commit();
            }
            catch (Exception ex)
            {
                result.Message = ex.InnerException?.Message ?? ex.Message;
                return result;
            }

            result.Success = true;
            result.Data = data.Code;
            result.Message = "Data faktur penjualan berhasil disimpan.";
            return result;
        }

        public SaveResult Update(SalesInvoiceRequest data)
        {
            var result = new SaveResult(false);

            using var transaction = Db.Database.BeginTransaction();
            try
            {
                // Checking mark header data
                if (Db.SalesInvoiceHeaders.Any(x => x.Code == data.Code && x.Mark == "V"))
                {
                    result.Message = "Data faktur penjualan tidak bisa diubah karena sudah ditandai sebagai void.";
                    return result;
                }

                // Checking sales order mark
                if (IsSalesOrderInvalid(data.SoCode))
                {
                    result.Message = "Data faktur penjualan tidak bisa diubah karena status order penjualan bukan diterima sebagian atau selesai.";
                    return result;
                }

                data.ApprovedBy = null;
                data.ApprovedDate = null;

                // Update header data
                Db.SalesInvoiceHeaders.Update(data);
                Db.Entry(data).Property(e => e.Code).IsModified = false;
                Db.Entry(data).Property(e => e.CreatedBy).IsModified = false;
                Db.Entry(data).Property(e => e.CreatedDate).IsModified = false;

                // Get delivery code that exists in invoice before
                var doCodeList = Db.SalesInvoiceDetails
                    .Where(d => d.Code == data.Code && !data.Details.Select(x => x.DoCode).Contains(d.DoCode))
                    .Select(x => x.DoCode)
                    .ToList();

                // Update sales delivery mark that exists in invoice before
                Db.Database.ExecuteSqlRaw(
                    $@"UPDATE Sales.SalesDeliveryHeader
                    SET Mark='A'
                    WHERE Code IN ('{string.Join("','", doCodeList)}')");

                // Get detail data that exists in invoice before
                var delDetails = Db.SalesInvoiceDetails
                    .Where(d => d.Code == data.Code && !data.Details.Select(x => x.Id).Contains(d.Id))
                    .ToList();

                // Delete detail data that exists in invoice before
                Db.SalesInvoiceDetails.RemoveRange(delDetails);

                // Update detail data
                short i = 0;
                var newInvDetails = new List<SalesInvoiceDetail>();
                foreach (var item in data.Details)
                {
                    if (item.Id <= 0)
                    {
                        newInvDetails.Add(new SalesInvoiceDetail
                        {
                            Code = data.Code,
                            LineNo = ++i,
                            DoCode = item.DoCode,
                            ShipmentFee = item.ShipmentFee,
                            HandlingFee = item.HandlingFee,
                            SubTotal = item.SubTotal,
                            FinalDisc = item.FinalDisc,
                            TaxAmount = item.TaxAmount,
                            Total = item.Total,
                            Dpp = item.Dpp
                        });
                    }
                    else
                    {
                        item.LineNo = ++i;

                        Db.SalesInvoiceDetails.Update(item);
                        Db.Entry(item).Property(e => e.Code).IsModified = false;
                    }
                }

                // Insert detail if new data exists
                if (newInvDetails.Any())
                    Db.SalesInvoiceDetails.AddRange(newInvDetails);

                // Save changes
                Db.SaveChanges();

                // Update sales delivery to invoiced
                Db.Database.ExecuteSqlRaw(
                    $@"UPDATE Sales.SalesDeliveryHeader
                    SET Mark='INV'
                    WHERE Code IN ('{string.Join("','", data.Details.Select(x => x.DoCode.Replace("'", "''")))}')");

                // Check all sales delivery are invoiced
                if (
                    !Db.SalesDeliveryHeaders
                        .Any(x => x.TransCode == data.SoCode && x.Mark != "INV"))
                {
                    // Update sales order to closed
                    Db.Database.ExecuteSqlRaw(
                        "UPDATE Sales.SalesOrderHeader SET Mark='CLS' WHERE Code={0} AND Mark='CMP'", data.SoCode);
                }
                else
                {
                    // Update sales order to partial receive or completed
                    var soMark = Db.SalesOrderDetails.Any(x => x.Code == data.SoCode && x.Qty > x.QtyDlv)
                        ? "PS"
                        : "CMP";

                    Db.Database.ExecuteSqlRaw(
                        "UPDATE Sales.SalesOrderHeader SET Mark={0} WHERE Code={1}", soMark, data.SoCode);
                }

                transaction.Commit();
            }
            catch (Exception ex)
            {
                result.Message = ex.InnerException?.Message ?? ex.Message;
                return result;
            }

            result.Success = true;
            result.Data = data.Code;
            result.Message = "Data faktur penjualan berhasil diperbarui.";
            return result;
        }

        public SaveResult Delete(string code, int userId)
        {
            var result = new SaveResult(false);

            var data = Db.SalesInvoiceHeaders.Find(code);
            if (data != null)
            {
                // Checking mark header data
                if (data.Mark == "V")
                {
                    result.Message = "Data faktur penjualan tidak bisa ditandai sebagai void karena sudah ditandai sebagai void.";
                    return result;
                }

                using var transaction = Db.Database.BeginTransaction();
                try
                {
                    // Update header data
                    data.Mark = "V";
                    data.UpdatedBy = userId;
                    data.UpdatedDate = DateTime.Now;

                    // Save changes
                    Db.SaveChanges();

                    // Get sales delivery code and join
                    var doCodeJoin = string.Join("','",
                        Db.SalesInvoiceDetails
                            .Where(x => x.Code == code)
                            .Select(x => x.DoCode.Replace("'", "''")));

                    // Update sales delivery to active
                    Db.Database.ExecuteSqlRaw(
                        $"UPDATE Sales.SalesDeliveryHeader SET Mark='A' WHERE Code IN ('{doCodeJoin}')");

                    // Update sales order to partial receive or completed
                    var soMark = Db.SalesOrderDetails.Any(x => x.Code == data.SoCode && x.Qty > x.QtyDlv)
                        ? "PS"
                        : "CMP";

                    Db.Database.ExecuteSqlRaw(
                        "UPDATE Sales.SalesOrderHeader SET Mark={0} WHERE Code={1}", soMark, data.SoCode);

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    result.Message = ex.InnerException?.Message ?? ex.Message;
                    return result;
                }
            }

            result.Success = true;
            result.Message = "Data faktur penjualan berhasil ditandai sebagai void.";
            return result;
        }

        private bool IsSalesOrderInvalid(string soCode)
        {
            return Db.SalesOrderHeaders.Any(x => x.Code == soCode && !new[] { "PS", "CMP" }.Contains(x.Mark));
        }
    }
}
