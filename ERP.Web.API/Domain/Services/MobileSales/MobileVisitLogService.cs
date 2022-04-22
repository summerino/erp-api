using ERP.Common;
using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Entity.MobileSales;
using ERP.Entity.Sales;
using ERP.Web.API.Domain.Interfaces.MobileSales;
using Geolocation;

namespace ERP.Web.API.Domain.Services.MobileSales;

public class MobileVisitLogService : GeneralService<MobileVisitLog>, IMobileVisitLogService
{
    public MobileVisitLogService(TenantContext db)
        :base(db)
    {
    }

    public SaveResult Approve(List<MobileVisitLog> data, int userId)
    {
        var result = new SaveResult(false);

        if (!data.Any())
            return new SaveResult(false, "Tidak ada data yang di proses");

        if (data.Any(x => x.Mark != "A"))
            return new SaveResult(false, "Tidak dapat menyetujui data yang sudah disetujui atau ditolak");

        using var transaction = Db.Database.BeginTransaction();
        try
        {
            var isDuplicate = data.GroupBy(x => new { x.SalesmanId, x.CustCode, x.Date }).Any(x => x.Count() > 1);
            if (isDuplicate)
            {
                result.Message = "Terdapat data penjual mengunjungi pelanggan di hari yang sama lebih dari 1 kali.";
                return result;
            }

            foreach (var item in data)
            {
                var mcustData = Db.MobileCustomers.FirstOrDefault(x => x.Code == item.CustCode);

                if (mcustData != null && mcustData.Mark == "A")
                {
                    result.Message = "Terdapat data pelanggan baru yang belum disetujui.";
                    return result;
                }

                var custAddData = Db.CustomerAddress.FirstOrDefault(x => x.Code == item.CustCode && x.IsDefault);
                if (custAddData != null)
                {
                    custAddData.Lat = item.Lat;
                    custAddData.Lng = item.Lng;
                    Db.CustomerAddress.Update(custAddData);
                }

                if (item.VisitOrderCode == null && item.Scheduled)
                {
                    // Get new code
                    var newCode = GetNewCode("VST_ORD_NUM_FMT", item.Date);

                    var hData = new VisitOrder
                    {
                        Code = newCode,
                        Date = item.Date,
                        SalesmanId = item.SalesmanId,
                        Mark = "A",
                        CreatedBy = userId,
                        CreatedDate = DateTime.Now,
                        UpdatedBy = userId,
                        UpdatedDate = DateTime.Now,
                        ApprovedBy = userId,
                        ApprovedDate = DateTime.Now
                    };

                    Db.VisitOrders.Add(hData);

                    var cData = new VisitOrderCustomer
                    {
                        Code = newCode,
                        CustCode = item.CustCode,
                        Visited = (bool)item.Visited
                    };

                    Db.VisitOrderCustomers.Add(cData);

                    item.Mark = "APR";
                    item.VisitOrderCode = newCode;
                    item.ApprovedBy = userId;
                    item.ApprovedDate = hData.ApprovedDate;
                    Db.MobileVisitLogs.Update(item);
                }
                else
                {
                    var voData = Db.VisitOrders.FirstOrDefault(x => x.Code == item.VisitOrderCode);
                    if(voData != null)
                    {
                        voData.UpdatedBy = userId;
                        voData.UpdatedDate = DateTime.Now;
                        voData.ApprovedBy = userId;
                        voData.UpdatedDate = voData.UpdatedDate;

                        Db.VisitOrders.Update(voData);
                    }

                    var cData = Db.VisitOrderCustomers
                        .FirstOrDefault(x => x.Code == item.VisitOrderCode && x.CustCode == item.CustCode);
                    if(cData != null)
                    {
                        cData.Visited = (bool)item.Visited;
                        Db.VisitOrderCustomers.Update(cData);
                    }
                    else
                    {
                        var newData = new VisitOrderCustomer
                        {
                            Code = voData.Code,
                            CustCode = item.CustCode,
                            Visited = (bool)item.Visited
                        };
                        Db.VisitOrderCustomers.Add(newData);
                    }

                    item.Mark = "APR";
                    item.ApprovedBy = userId;
                    item.ApprovedDate = DateTime.Now;
                    Db.MobileVisitLogs.Update(item);
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
        result.Message = "Data log kunjungan mobile berhasil disetujui.";
        return result;
    }

    public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts, string search)
    {
        var data = Db.VwMobileVisitLogs.AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            data = DateTime.TryParse(search, out var searchDate)
                ? data.Where(x => x.Date == searchDate)
                : data.Where(x =>
                    x.Code.Contains(search) || x.SalesmanInitial.Contains(search) || x.SalesmanName.Contains(search)
                    || x.CustomerInitial.Contains(search) || x.CustomerName.Contains(search)
                    || x.Mark.Contains(search) || x.Status.Contains(search));
        }

        return data.ToDataSourceResult(skip, take, filters, sorts);
    }

    public dynamic GetRadius(string custCode, decimal? lat, decimal? lng)
    {
        var result = new
        {
            Distance = 0.00,
            Lat = 0m,
            Lng = 0m,
            Radius = 0.00,
            RadiusColor = "red"
        };

        if (lat.HasValue && lng.HasValue)
        {
            var custAddData = Db.CustomerAddress.FirstOrDefault(x => x.Code == custCode && x.IsDefault);
            if (custAddData != null)
            {
                if (!custAddData.Lat.HasValue || !custAddData.Lng.HasValue)
                    return result;

                var radius = Convert.ToDouble(Db.SystemParameters.FirstOrDefault(x => x.Code == "MOB_VST_RANGE_RADIUS").Value);
                var distance = GeoCalculator.GetDistance((double)custAddData.Lat, (double)custAddData.Lng, (double)lat, (double)lng, distanceUnit: DistanceUnit.Meters);
                result = new
                {
                    Distance = distance,
                    Lat = (decimal)custAddData.Lat,
                    Lng = (decimal)custAddData.Lng,
                    Radius = radius,
                    RadiusColor = distance <= radius ? "green" : "red"
                };
            }
        }

        return result;
    }

    public SaveResult Reject(List<MobileVisitLog> data, int userId)
    {
        var result = new SaveResult(false);

        if (!data.Any())
            return new SaveResult(false, "Tidak ada data yang di proses");

        if (data.Any(x => x.Mark != "A"))
            return new SaveResult(false, "Tidak dapat menolak data yang sudah disetujui atau ditolak");

        foreach (var item in data)
        {
            var vlData = Db.MobileVisitLogs.FirstOrDefault(x => x.Code == item.Code);
            vlData.RejectedBy = userId;
            vlData.RejectedDate = DateTime.Now;
            vlData.Mark = "REJ";
            Db.MobileVisitLogs.Update(vlData);
        }

        Db.SaveChanges();

        result.Success = true;
        result.Message = "Data log kunjungan mobile berhasil ditolak.";
        return result;
    }

    public override SaveResult Update(MobileVisitLog data)
    {
        var result = new SaveResult(false);

        // Update data
        Db.MobileVisitLogs.Update(data);
        Db.Entry(data).Property(e => e.Code).IsModified = false;
        Db.Entry(data).Property(e => e.CreatedBy).IsModified = false;
        Db.Entry(data).Property(e => e.CreatedDate).IsModified = false;

        var moData = Db.MobileOrderHeaders.FirstOrDefault(x => x.VisitLogCode == data.Code);
        moData.CustCode = data.CustCode;
        moData.UpdatedBy = data.UpdatedBy;
        moData.UpdatedDate = DateTime.Now;
        Db.MobileOrderHeaders.Update(moData);

        var mpiData = Db.MobilePaymentInvoices.FirstOrDefault(x => x.VisitLogCode == data.Code);
        mpiData.CustCode = data.CustCode;
        mpiData.UpdatedBy = data.UpdatedBy;
        mpiData.UpdatedDate = DateTime.Now;
        Db.MobilePaymentInvoices.Update(mpiData);

        Db.SaveChanges();

        result.Success = true;
        result.Data = data.Code;
        result.Message = "Data log kunjungan mobile berhasil diperbarui.";
        return result;
    }
}