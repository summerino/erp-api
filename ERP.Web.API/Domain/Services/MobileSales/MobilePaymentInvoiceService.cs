using ERP.Common;
using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Entity.Finance;
using ERP.Entity.MobileSales;
using ERP.Web.API.Domain.Interfaces.MobileSales;
using Microsoft.EntityFrameworkCore;

namespace ERP.Web.API.Domain.Services.MobileSales
{
    public class MobilePaymentInvoiceService : GeneralService<MobilePaymentInvoice>, IMobilePaymentInvoiceService
    {
        public MobilePaymentInvoiceService(TenantContext db)
            :base(db)
        {

        }

        public SaveResult Approve(List<MobilePaymentInvoice> data, int userId)
        {
            var result = new SaveResult(false);

            if (!data.Any())
                return new SaveResult(false, "Tidak ada data yang di proses");

            if (data.Any(x => x.Mark != "A"))
                return new SaveResult(false, "Tidak dapat menyetujui data yang sudah disetujui atau ditolak");

            using var transaction = Db.Database.BeginTransaction();
            try
            {
                foreach (var item in data)
                {
                    var vlData = Db.MobileVisitLogs.FirstOrDefault(x => x.Code == item.VisitLogCode);
                    if(vlData != null)
                    {
                        if (vlData.VisitOrderCode == null && (vlData.Scheduled || !vlData.Scheduled) && vlData.ApprovedBy == null)
                        {
                            result.Message = $"Data catatan kunjungan mobile {item.VisitLogCode} harus di approve terlebih dahulu.";
                            return result;
                        }

                        var voData = Db.VisitOrders.FirstOrDefault(x => x.Code == vlData.VisitOrderCode);
                        if (voData == null)
                        {
                            result.Message = $"Data perintah kunjungan {vlData.VisitOrderCode} tidak ada.";
                            return result;
                        }

                        if (item.SrcTrans == "INV")
                        {
                            var voiData = Db.VisitOrderInvoices.FirstOrDefault(x => x.Code == voData.Code && x.InvCode == item.TransCode);
                            if (voiData == null)
                            {
                                result.Message = $"Data perintah kunjungan {vlData.VisitOrderCode} - {item.TransCode} tidak ada.";
                                return result;
                            }

                            voiData.Collecting = (item.NotesFailCollect == null);
                            voiData.FailCollect = (item.NotesFailCollect != null);
                            voiData.NotesFailCollect = item.NotesFailCollect;
                            Db.VisitOrderInvoices.Update(voiData);
                        }
                        else if (item.SrcTrans == "ORD")
                        {
                            var voiData = Db.MobileOrderHeaders.FirstOrDefault(x => x.Code == item.TransCode);
                            if (voiData == null)
                            {
                                result.Message = $"Data pesanan mobile {item.TransCode} tidak ada.";
                                return result;
                            }

                            switch (voiData.Mark)
                            {
                                case "A":
                                    result.Message = $"Data pesanan mobile {item.TransCode} status masih aktif (belum disetujui).";
                                    return result;

                                case "REJ":
                                    result.Message = $"Data pesanan mobile {item.TransCode} status ditolak.";
                                    return result;
                            }
                        }

                        var newCode = GetNewCode("CB_NUM_FMT", DateTime.Now);

                        var headCBData = new GeneralCashBankHeader
                        {
                            Code = newCode,
                            Type = "D",
                            Date = item.Date,
                            CoaCode = item.CoaCode,
                            CurrCode = "IDR",
                            Rate = 1,
                            Amount = item.Amount,
                            Notes = $"Terbentuk dari Pembayaran Mobile {item.Code}",
                            Mark = "A",
                            CreatedBy = userId,
                            CreatedDate = DateTime.Now,
                            UpdatedBy = userId,
                            UpdatedDate = DateTime.Now,
                            ApprovedBy = userId,
                            ApprovedDate = DateTime.Now
                        };
                        Db.GeneralCashBankHeaders.Add(headCBData);

                        var cusData = Db.Customers.FirstOrDefault(x => x.Code == item.CustCode);
                        var ordData = Db.MobileOrderHeaders.FirstOrDefault(x => x.Code == item.TransCode);

                        var detailCBData = new GeneralCashBankDetail
                        {
                            Code = newCode,
                            LineNo = 1,
                            Type = "AR",
                            TransCode = item.SrcTrans == "ORD" ? ordData?.SalesOrderCode ?? "" : item.TransCode,
                            CoaCode = item.CoaCode,
                            CurrCode = "IDR",
                            Rate = 1,
                            Amount = item.Amount,
                            TypeAmount = "C",
                            TransAmount = item.Amount,
                            Notes = cusData.Name
                        };
                        Db.GeneralCashBankDetails.Add(detailCBData);

                        item.Mark = "APR";
                        item.ApprovedBy = userId;
                        item.ApprovedDate = headCBData.ApprovedDate;
                        Db.MobilePaymentInvoices.Update(item);

                        Db.SaveChanges();

                        var queries = new List<string>
                        {
                            $"update General.Customer set CreditUsed= (CreditUsed - {item.Amount}) where code = '{item.CustCode}'"
                        };

                        var header = Db.SalesInvoiceHeaders.SingleOrDefault(x => x.Code == (item.SrcTrans == "ORD" ? ordData.SalesOrderCode ?? "" : item.TransCode));
                        if (header == null) continue;
                        var mark = header.Total == (header.PaidAmount + item.Amount) ? "CMP" : "PP";

                        queries.Add(
                            $"UPDATE Sales.SalesInvoiceHeader SET PaidAmount= PaidAmount + '{item.Amount}', Mark='{mark}' WHERE Code='{item.TransCode}';");

                        var detail = Db.SalesInvoiceDetails.Where(x => x.Code == (item.SrcTrans == "ORD" ? ordData.SalesOrderCode ?? "" : item.TransCode));
                        foreach (var item2 in detail)
                        {
                            var proRateValue = (header.PaidAmount + item.Amount) * item2.Total / header.Total;
                            queries.Add(
                                $"UPDATE Sales.SalesDeliveryHeader SET PaidAmount= PaidAmount + '{proRateValue}' WHERE Code='{item2.DoCode}';");
                        }

                        var query = string.Join("", queries);
                        Db.Database.ExecuteSqlRaw(query);
                    }
                    else
                    {
                        result.Message = $"Data catatan kunjungan mobile {item.VisitLogCode} tidak ada.";
                        return result;
                    }
                }
                transaction.Commit();
            }
            catch (Exception ex)
            {
                result.Message = ex.InnerException?.Message ?? ex.Message;
                return result;
            }

            result.Success = true;
            result.Message = "Data pembayaran mobile berhasil disetujui.";
            return result;
        }

        public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts, string search)
        {
            var data = Db.VwMobilePaymentInvoices.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                data = DateTime.TryParse(search, out var searchDate)
                    ? data.Where(x => x.Date == searchDate)
                    : data.Where(x =>
                        x.Code.Contains(search) || x.SalesmanInitial.Contains(search) || x.SalesmanName.Contains(search)
                        || x.CustomerInitial.Contains(search) || x.CustomerName.Contains(search) || x.CoaName.Contains(search)
                        || x.Mark.Contains(search) || x.Status.Contains(search) || x.VisitLogCode.Contains(search)
                        || x.CoaCode.Contains(search));
            }

            return data.ToDataSourceResult(skip, take, filters, sorts);
        }

        public SaveResult Reject(List<MobilePaymentInvoice> data, int userId)
        {
            var result = new SaveResult(false);

            if (!data.Any())
                return new SaveResult(false, "Tidak ada data yang di proses");

            if (data.Any(x => x.Mark != "A"))
                return new SaveResult(false, "Tidak dapat menolak data yang sudah disetujui atau ditolak");

            foreach (var item in data)
            {
                var vlData = Db.MobilePaymentInvoices.FirstOrDefault(x => x.Code == item.Code);
                vlData.RejectedBy = userId;
                vlData.RejectedDate = DateTime.Now;
                vlData.Mark = "REJ";
                Db.MobilePaymentInvoices.Update(vlData);
            }

            Db.SaveChanges();

            result.Success = true;
            result.Message = "Data pembayaran mobile berhasil ditolak.";
            return result;
        }
    }
}
