using System;
using System.Collections.Generic;
using System.Linq;
using ERP.Common;
using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Entity.General;
using ERP.Entity.MobileSales;
using ERP.Entity.Sales;
using ERP.Web.API.Domain.Interfaces.Mobile.VisitOrder;
using ERP.Web.API.Domain.Models.Mobile.VisitOrder;

namespace ERP.Web.API.Domain.Services.Mobile.VisitOrder
{
    public class VisitOrderService : GeneralService<MobileVisitLog>, IVisitOrderService
    {
        public VisitOrderService(TenantContext db) : base(db)
        {
        }

        public DataSourceResult GetData(int userId, string lastUpdate)
        {
            var result = new DataSourceResult();

            try
            {
                var startFrom = DateTime.Today.AddMonths(-1);
                var data = (from visits in Db.VwVisitOrders.Where(x => x.Date >= startFrom)
                            join user in Db.Users on visits.SalesmanId equals user.EmployeeId
                            where user.Id == userId
                            select new VwVisitOrder
                            {
                                Code = visits.Code,
                                Date = visits.Date,
                                SalesmanId = visits.SalesmanId,
                                VisitPlanCode = visits.VisitPlanCode,
                                Notes = visits.Notes,
                                Status = visits.Status,
                                SalesmanInitial = visits.SalesmanInitial,
                                SalesmanName = visits.SalesmanName,
                                GroupInitial = visits.GroupInitial,
                                CreatedInitial = visits.CreatedInitial,
                                UpdatedInitial = visits.UpdatedInitial,
                                ApprovedInitial = visits.ApprovedInitial,
                                Mark = visits.Mark,
                                UpdatedDate = visits.UpdatedDate
                            }).AsQueryable();

                if (lastUpdate != null)
                {
                    lastUpdate = GetLastUpdate(lastUpdate);

                    data = data.Where(x => x.UpdatedDate > DateTime.ParseExact(lastUpdate, "yyyy-MM-ddTHH:mm:ss.ffff", null));
                }

                return data.ToDataSourceResult(0, 0, null, null);
            }
            catch (Exception ex)
            {
                result.Errors = ex.InnerException?.Message ?? ex.Message;
                return result;
            }
        }

        public VwVisitOrder GetDetail(string code)
        {
            return Db.VwVisitOrders.FirstOrDefault(x => x.Code.Equals(code));
        }

        public IEnumerable<MobileVisitLog> GetLogVisit(int userId, string lastUpdate)
        {
            var startFrom = DateTime.Today.AddMonths(-1);
            var data = (from vl in Db.MobileVisitLogs
                        join visits in Db.VisitOrders on vl.VisitOrderCode equals visits.Code
                        join user in Db.Users on visits.SalesmanId equals user.EmployeeId
                        where user.Id == userId && visits.Date >= startFrom
                        select new MobileVisitLog
                        {
                            NoVisitReasonId = vl.NoVisitReasonId,
                            UnscheduledVisitReasonId = vl.UnscheduledVisitReasonId,
                            Total = vl.Total,
                            EndTime = vl.EndTime,
                            StartTime = vl.StartTime,
                            Lng = vl.Lng,
                            NoOrderReasonId = vl.NoOrderReasonId,
                            Lat = vl.Lat,
                            Scheduled = vl.Scheduled,
                            CustCode = vl.CustCode,
                            SalesmanId = vl.SalesmanId,
                            VisitOrderCode = vl.VisitOrderCode,
                            Date = vl.Date,
                            Code = vl.Code,
                            Visited = vl.Visited,
                            Image = vl.Image,
                            UpdatedDate = vl.UpdatedDate
                        });

            if (lastUpdate != null)
            {
                lastUpdate = GetLastUpdate(lastUpdate);
                data = data.Where(x => x.UpdatedDate > DateTime.ParseExact(lastUpdate, "yyyy-MM-ddTHH:mm:ss.ffff", null));
            }

            return data;
        }

        public IEnumerable<MobileOrderDetailDiscount> GetMobileOrderDetailDiscounts(int userId, string lastUpdate)
        {
            var header = Db.MobileOrderHeaders.AsQueryable();
            if (lastUpdate != null)
            {
                lastUpdate = GetLastUpdate(lastUpdate);
                header = header.Where(x => x.UpdatedDate > DateTime.ParseExact(lastUpdate, "yyyy-MM-ddTHH:mm:ss.ffff", null));
            }

            var startFrom = DateTime.Today.AddMonths(-1);
            var data = (from dd in Db.MobileOrderDetailDiscounts
                        join oh in header on dd.Code equals oh.Code
                        join user in Db.Users on oh.SalesBy equals user.EmployeeId
                        where user.Id == userId && oh.Date >= startFrom
                        select new MobileOrderDetailDiscount
                        {
                            Id = dd.Id,
                            Code = dd.Code,
                            OrderDetailId = dd.OrderDetailId,
                            LineNo = dd.LineNo,
                            PromoCode = dd.PromoCode,
                            PromoDetailId = dd.PromoDetailId,
                            Name = dd.Name,
                            IsPercentage = dd.IsPercentage,
                            Value = dd.Value,
                            Amount = dd.Amount
                        });
            return data;
        }

        public IEnumerable<OrderDetailModel> GetMobileOrderDetails(int userId, string lastUpdate)
        {

            var header = Db.MobileOrderHeaders.AsQueryable();
            if (lastUpdate != null)
            {
                lastUpdate = GetLastUpdate(lastUpdate);
                header = header.Where(x => x.UpdatedDate > DateTime.ParseExact(lastUpdate, "yyyy-MM-ddTHH:mm:ss.ffff", null));
            }

            var startFrom = DateTime.Today.AddMonths(-1);
            var data = (from od in Db.MobileOrderDetails
                        join oh in header on od.Code equals oh.Code
                        join it in Db.Items on od.ItemId equals it.Id
                        join unit in Db.UoMConversions on od.UnitId equals unit.Id
                        join tax in Db.Taxes on od.TaxId equals tax.Id into py
                        from sub in py.DefaultIfEmpty()
                        join user in Db.Users on oh.SalesBy equals user.EmployeeId
                        where user.Id == userId && oh.Date >= startFrom
                        select new OrderDetailModel
                        {
                            Id = od.Id,
                            Code = od.Code,
                            LineNo = od.LineNo,
                            ItemId = od.ItemId,
                            ItemName = it.Name,
                            UomId = od.UomId,
                            UnitId = od.UnitId,
                            UnitName = unit.UnitEquivalent,
                            Qty = od.Qty,
                            UnitPrice = od.UnitPrice,
                            Disc = od.Disc,
                            TaxId = od.TaxId,
                            TaxName = sub.Name,
                            TaxAmount = od.TaxAmount,
                            NettPrice = od.NettPrice,
                            Total = od.Total,
                            Dpp = od.Dpp
                        });

            return data;
        }

        public IEnumerable<OrderHeaderModel> GetMobileOrderHeaders(int userId, string lastUpdate)
        {

            var startFrom = DateTime.Today.AddMonths(-1);
            var data = (from oh in Db.MobileOrderHeaders
                        join cu in Db.Customers on oh.CustCode equals cu.Code
                        join curr in Db.Currencies on oh.CurrCode equals curr.Code
                        join pt in Db.PaymentTerms on oh.PaymentTermId equals pt.Id into py
                        from sub in py.DefaultIfEmpty()
                        join user in Db.Users on oh.SalesBy equals user.EmployeeId
                        where user.Id == userId && oh.Date >= startFrom
                        select new OrderHeaderModel
                        {
                            Code = oh.Code,
                            Date = oh.Date,
                            VisitLogCode = oh.VisitLogCode,
                            SalesOrderCode = oh.SalesOrderCode,
                            Type = oh.Type,
                            CustCode = oh.CustCode,
                            CustName = cu.Name,
                            CurrCode = oh.CurrCode,
                            CurrName = curr.Name,
                            PaymentTermId = oh.PaymentTermId,
                            PaymentTermName = sub.Name,
                            PaidAmount = oh.PaidAmount,
                            TaxAmount = oh.TaxAmount,
                            Dpp = oh.Dpp,
                            Rate = oh.Rate,
                            FinalDisc = oh.FinalDisc,
                            FinalDiscPercent = oh.FinalDiscPercent,
                            IncludeTax = oh.IncludeTax,
                            SubTotal = oh.SubTotal,
                            Total = oh.Total,
                            UpdatedDate = oh.UpdatedDate
                        });

            if (lastUpdate != null)
            {
                lastUpdate = GetLastUpdate(lastUpdate);
                data = data.Where(x => x.UpdatedDate > DateTime.ParseExact(lastUpdate, "yyyy-MM-ddTHH:mm:ss.ffff", null));
            }

            return data;
        }

        public IEnumerable<PaymentInvoiceModel> GetMobilePaymentInvoice(int userId, string lastUpdate)
        {
            var startFrom = DateTime.Today.AddMonths(-1);
            var data = (from mpi in Db.MobilePaymentInvoices
                        join cu in Db.Customers on mpi.CustCode equals cu.Code
                        join coa in Db.Coas on mpi.CoaCode equals coa.Code
                        join user in Db.Users on mpi.SalesmanId equals user.EmployeeId
                        where user.Id == userId && mpi.Date >= startFrom
                        select new PaymentInvoiceModel
                        {
                            Code = mpi.Code,
                            VisitLogCode = mpi.VisitLogCode,
                            Date = mpi.Date,
                            CustCode = mpi.CustCode,
                            CustName = cu.Name,
                            CoaCode = mpi.CoaCode,
                            CoaName = coa.Name,
                            TransCode = mpi.TransCode,
                            Amount = mpi.Amount,
                            NotesFailCollect = mpi.NotesFailCollect,
                            SrcTrans = mpi.SrcTrans,
                            UpdatedDate = mpi.UpdatedDate
                        });

            if (lastUpdate != null)
            {
                lastUpdate = GetLastUpdate(lastUpdate);
                data = data.Where(x => x.UpdatedDate > DateTime.ParseExact(lastUpdate, "yyyy-MM-ddTHH:mm:ss.ffff", null));
            }

            return data;
        }

        public IEnumerable<MobileOrderDetailFreeGood> GetMobileOrderDetailFreeGoods(int userId, string lastUpdate)
        {
            var header = Db.MobileOrderHeaders.AsQueryable();
            if (lastUpdate != null)
            {
                lastUpdate = GetLastUpdate(lastUpdate);
                header = header.Where(x => x.UpdatedDate > DateTime.ParseExact(lastUpdate, "yyyy-MM-ddTHH:mm:ss.ffff", null));
            }

            var startFrom = DateTime.Today.AddMonths(-1);
            var data = (from dd in Db.MobileOrderDetailFreeGoods
                        join oh in header on dd.Code equals oh.Code
                        join user in Db.Users on oh.SalesBy equals user.EmployeeId
                        where user.Id == userId && oh.Date >= startFrom
                        select new MobileOrderDetailFreeGood
                        {
                            Id = dd.Id,
                            Code = dd.Code,
                            OrderDetailId = dd.OrderDetailId,
                            LineNo = dd.LineNo,
                            PromoCode = dd.PromoCode,
                            ItemId = dd.ItemId,
                            UomId = dd.UomId,
                            UnitId = dd.UnitId,
                            Qty = dd.Qty,
                            UnitPrice = dd.UnitPrice
                        });
            return data;
        }

        public IEnumerable<PromoDetail> GetPromoDetail(string lastUpdate)
        {
            var header = Db.PromoHeaders.Where(x => DateTime.Now.Date >= x.StartDate && DateTime.Now.Date <= x.EndDate);

            if (lastUpdate != null)
            {
                lastUpdate = GetLastUpdate(lastUpdate);
                header = header.Where(x => x.UpdatedDate > DateTime.ParseExact(lastUpdate, "yyyy-MM-ddTHH:mm:ss.ffff", null));
            }

            var data = (from pd in Db.PromoDetails
                        join ph in header
                        on pd.Code equals ph.Code
                        select new PromoDetail
                        {
                            Id = pd.Id,
                            Code = pd.Code,
                            LineNo = pd.LineNo,
                            ApplyTo = pd.ApplyTo,
                            ItemId = pd.ItemId,
                            PromoType = pd.PromoType,
                            IsPercentage = pd.IsPercentage,
                            ValuePercentage = pd.ValuePercentage,
                            ValueAmount = pd.ValueAmount,
                            IsPromoWithBudget = pd.IsPromoWithBudget,
                            BudgetMaximumValue = pd.BudgetMaximumValue,
                            OverBudgetAction = pd.OverBudgetAction,
                            SubGroup1 = pd.SubGroup1,
                            SubGroup2 = pd.SubGroup2,
                            SubGroup3 = pd.SubGroup3,
                            SubGroup4 = pd.SubGroup4,
                            SubGroup5 = pd.SubGroup5
                        });
            return data;
        }

        public IEnumerable<PromoDetailTier> GetPromoDetailTier(string lastUpdate)
        {
            var header = Db.PromoHeaders.Where(x => DateTime.Now.Date >= x.StartDate && DateTime.Now.Date <= x.EndDate);

            if (lastUpdate != null)
            {
                lastUpdate = GetLastUpdate(lastUpdate);
                header = header.Where(x => x.UpdatedDate > DateTime.ParseExact(lastUpdate, "yyyy-MM-ddTHH:mm:ss.ffff", null));
            }

            var data = (from pdt in Db.PromoDetailTiers
                        join pd in Db.PromoDetails on pdt.PromoDetailId equals pd.Id
                        join ph in header
                        on pd.Code equals ph.Code
                        select new PromoDetailTier
                        {
                            Id = pdt.Id,
                            PromoDetailId = pdt.PromoDetailId,
                            FromQty = pdt.FromQty,
                            ToQty = pdt.ToQty,
                            IsPercentage = pdt.IsPercentage,
                            Value = pdt.Value,
                            SaleUnit = pdt.SaleUnit,
                            ApplyToAllUnit = pdt.ApplyToAllUnit,
                            FreeGoodItemId = pdt.FreeGoodItemId,
                            UnitFreeGood = pdt.UnitFreeGood,
                            IsMultiple = pdt.IsMultiple,
                            PaymentTermId = pdt.PaymentTermId
                        });
            return data;
        }

        public IEnumerable<PromoHeader> GetPromoHeader(string lastUpdate)
        {

            var data = Db.PromoHeaders.Where(x => DateTime.Now.Date >= x.StartDate && DateTime.Now.Date <= x.EndDate);

            if (lastUpdate != null)
            {
                lastUpdate = GetLastUpdate(lastUpdate);
                data = data.Where(x => x.UpdatedDate > DateTime.ParseExact(lastUpdate, "yyyy-MM-ddTHH:mm:ss.ffff", null));
            }

            return data;
        }

        public IEnumerable<PromoSubject> GetPromoSubjects(string lastUpdate)
        {
            var header = Db.PromoHeaders.Where(x => DateTime.Now.Date >= x.StartDate && DateTime.Now.Date <= x.EndDate);

            if (lastUpdate != null)
            {
                lastUpdate = GetLastUpdate(lastUpdate);
                header = header.Where(x => x.UpdatedDate > DateTime.ParseExact(lastUpdate, "yyyy-MM-ddTHH:mm:ss.ffff", null));
            }

            var data = (from ps in Db.PromoSubjects
                        join ph in header
                          on ps.Code equals ph.Code
                        select new PromoSubject
                        {
                            Id = ps.Id,
                            Code = ps.Code,
                            CustCode = ps.CustCode,
                            CustTypeId = ps.CustTypeId
                        });
            return data;
        }

        public IEnumerable<MobileReason> GetReason(string lastUpdate)
        {
            var data = Db.MobileReasons.AsQueryable();

            if (lastUpdate != null)
            {
                lastUpdate = GetLastUpdate(lastUpdate);
                data = data.Where(x => x.UpdatedDate > DateTime.ParseExact(lastUpdate, "yyyy-MM-ddTHH:mm:ss.ffff", null));
            }

            return data;
        }

        public IEnumerable<SalesInvoiceModel> GetSalesInvoceHeader(int userId, string lastUpdate)
        {
            var visitOrders = Db.VisitOrders.AsQueryable();
            if (lastUpdate != null)
            {
                lastUpdate = GetLastUpdate(lastUpdate);
                visitOrders = visitOrders.Where(x => x.UpdatedDate > DateTime.ParseExact(lastUpdate, "yyyy-MM-ddTHH:mm:ss.ffff", null));
            }

            var startFrom = DateTime.Today.AddMonths(-1);
            var data = (from vi in Db.VisitOrderInvoices
                        join inv in Db.SalesInvoiceHeaders on vi.InvCode equals inv.Code
                        join visits in visitOrders on vi.Code equals visits.Code
                        join curr in Db.Currencies on inv.CurrCode equals curr.Code
                        join so in Db.SalesOrderHeaders on inv.SoCode equals so.Code
                        join pay in Db.PaymentTerms on so.PaymentTermId equals pay.Id into py
                        from sub in py.DefaultIfEmpty()
                        join user in Db.Users on visits.SalesmanId equals user.EmployeeId
                        where user.Id == userId && visits.Date >= startFrom
                        select new SalesInvoiceModel
                        {
                            Code = inv.Code,
                            Date = inv.Date,
                            DueDate = inv.DueDate,
                            SoCode = inv.SoCode,
                            CustCode = inv.CustCode,
                            PaymentTermId = so.PaymentTermId,
                            PaymentTermName = sub.Name,
                            CurrCode = inv.CurrCode,
                            CurrName = curr.Name,
                            PaidAmount = inv.PaidAmount,
                            Total = inv.Total,
                            Notes = inv.Notes,
                            UpdatedDate = inv.UpdatedDate
                        });

            return data;
        }

        public IEnumerable<VwVisitOrderCustomer> GetVisitOrderCustomer(int userId, string lastUpdate)
        {
            var visitOrders = Db.VisitOrders.AsQueryable();
            if (lastUpdate != null)
            {
                lastUpdate = GetLastUpdate(lastUpdate);
                visitOrders = visitOrders.Where(x => x.UpdatedDate > DateTime.ParseExact(lastUpdate, "yyyy-MM-ddTHH:mm:ss.ffff", null));
            }

            var startFrom = DateTime.Today.AddMonths(-1);
            var data = (from vc in Db.VwVisitOrderCustomers
                        join visits in visitOrders on vc.Code equals visits.Code
                        join user in Db.Users on visits.SalesmanId equals user.EmployeeId
                        where user.Id == userId && visits.Date >= startFrom
                        select new VwVisitOrderCustomer
                        {
                            Id = vc.Id,
                            Code = vc.Code,
                            CustCode = vc.CustCode,
                            ReplacingForSalesmanId = vc.ReplacingForSalesmanId,
                            Visited = vc.Visited,
                            CustomerInitial = vc.CustomerInitial,
                            CustomerName = vc.CustomerName,
                            Address = vc.Address,
                            AreaName1 = vc.AreaName1,
                            AreaName2 = vc.AreaName2,
                            ReplacemanInitial = vc.ReplacemanInitial,
                            ReplacemanName = vc.ReplacemanName
                        });

            return data;
        }

        public IEnumerable<VisitOrderInvoice> GetVisitOrderInvoice(int userId, string lastUpdate)
        {
            var visitOrders = Db.VisitOrders.AsQueryable();
            if (lastUpdate != null)
            {
                lastUpdate = GetLastUpdate(lastUpdate);
                visitOrders = visitOrders.Where(x => x.UpdatedDate > DateTime.ParseExact(lastUpdate, "yyyy-MM-ddTHH:mm:ss.ffff", null));
            }

            var startFrom = DateTime.Today.AddMonths(-1);
            var data = (from vi in Db.VisitOrderInvoices
                        join visits in visitOrders on vi.Code equals visits.Code
                        join user in Db.Users on visits.SalesmanId equals user.EmployeeId
                        where user.Id == userId && visits.Date >= startFrom
                        select new VisitOrderInvoice
                        {
                            Id = vi.Id,
                            Code = vi.Code,
                            InvCode = vi.InvCode,
                            Collecting = vi.Collecting,
                            FailCollect = vi.FailCollect,
                            NotesFailCollect = vi.NotesFailCollect

                        });
            return data;
        }

        public IEnumerable<VisitReasonModel> GetVisitReason(int userId, string lastUpdate)
        {
            var visitOrders = Db.VisitOrders.AsQueryable();
            if (lastUpdate != null)
            {
                lastUpdate = GetLastUpdate(lastUpdate);
                visitOrders = visitOrders.Where(x => x.UpdatedDate > DateTime.ParseExact(lastUpdate, "yyyy-MM-ddTHH:mm:ss.ffff", null));
            }

            var startFrom = DateTime.Today.AddMonths(-1);
            var data = (from vr in Db.MobileVisitReasons
                        join vl in Db.MobileVisitLogs on vr.VisitLogCode equals vl.Code
                        join visits in visitOrders on vl.VisitOrderCode equals visits.Code
                        join user in Db.Users on visits.SalesmanId equals user.EmployeeId
                        join rs in Db.MobileReasons on vr.VisitReasonId equals rs.Id
                        where user.Id == userId && visits.Date >= startFrom
                        select new VisitReasonModel
                        {
                            Id = vr.Id,
                            VisitLogCode = vr.VisitLogCode,
                            VisitReasonId = vr.VisitReasonId,
                            VisitReasonName = rs.Name
                        });
            return data;
        }

        public IEnumerable<Tax> GetTax(string lastUpdate)
        {
            var data = Db.Taxes.AsQueryable();

            if (lastUpdate != null)
            {
                lastUpdate = GetLastUpdate(lastUpdate);
                data = data.Where(x => x.UpdatedDate > DateTime.ParseExact(lastUpdate, "yyyy-MM-ddTHH:mm:ss.ffff", null));
            }

            return data;
        }

        public IEnumerable<PaymentTerm> GetPaymentTerms(string lastUpdate)
        {
            var data = Db.PaymentTerms.AsQueryable();

            if (lastUpdate != null)
            {
                lastUpdate = GetLastUpdate(lastUpdate);
                data = data.Where(x => x.UpdatedDate > DateTime.ParseExact(lastUpdate, "yyyy-MM-ddTHH:mm:ss.ffff", null));
            }

            return data;
        }

        public SaveResult Insert(VisitRequestModel data)
        {
            var result = new SaveResult(false);

            using var transaction = Db.Database.BeginTransaction();
            try
            {

                Db.MobileVisitLogs.Add(data);

                if (data.Visited == true)
                {
                    var vo = Db.VisitOrders.First(x => x.Code == data.VisitOrderCode);
                    vo.UpdatedBy = data.UpdatedBy;
                    vo.UpdatedDate = data.UpdatedDate;

                    if (data.Scheduled)
                    {
                        var order = Db.VisitOrderCustomers.First(x => x.Code == data.VisitOrderCode && x.CustCode == data.CustCode);
                        order.Visited = true;
                    }
                }

                foreach (var reason in data.VisitReasons)
                {
                    Db.MobileVisitReasons.Add(new MobileVisitReason
                    {
                        VisitLogCode = data.Code,
                        VisitReasonId = reason
                    });
                }


                foreach (var invoice in data.Invoices)
                {
                    var transCode = invoice.TransCode;
                    if (transCode == null)
                    {
                        transCode = data.OrderHeader.Code;
                    }
                    Db.MobilePaymentInvoices.Add(new MobilePaymentInvoice
                    {
                        Code = invoice.Code,
                        VisitLogCode = data.Code,
                        Date = data.Date,
                        SalesmanId = data.SalesmanId,
                        CustCode = data.CustCode,
                        CoaCode = invoice.CoaCode,
                        TransCode = transCode,
                        Amount = invoice.Amount,
                        NotesFailCollect = invoice.NotesFailCollect,
                        SrcTrans = invoice.SrcTrans,
                        CreatedBy = data.CreatedBy,
                        CreatedDate = data.CreatedDate,
                        UpdatedBy = data.CreatedBy,
                        UpdatedDate = data.CreatedDate,
                        Mark = "A"
                    });

                    var inv = Db.VisitOrderInvoices.FirstOrDefault(x => x.Code == data.VisitOrderCode && x.InvCode == invoice.TransCode);
                    if (inv != null)
                    {
                        inv.Collecting = true;
                    }
                }

                if (data.OrderHeader != null)
                {

                    Db.MobileOrderHeaders.Add(new MobileOrderHeader
                    {
                        Code = data.OrderHeader.Code,
                        Date = data.Date,
                        VisitLogCode = data.Code,
                        Type = data.OrderHeader.Type,
                        CustCode = data.CustCode,
                        SalesBy = data.SalesmanId,
                        PaymentTermId = data.OrderHeader.PaymentTermId,
                        CurrCode = data.OrderHeader.CurrCode,
                        Rate = data.OrderHeader.Rate,
                        SubTotal = data.OrderHeader.SubTotal,
                        FinalDiscPercent = data.OrderHeader.FinalDiscPercent,
                        FinalDisc = data.OrderHeader.FinalDisc,
                        IncludeTax = data.OrderHeader.IncludeTax,
                        TaxAmount = data.OrderHeader.TaxAmount,
                        Total = data.OrderHeader.Total,
                        Dpp = data.OrderHeader.Dpp,
                        PaidAmount = data.OrderHeader.PaidAmount,
                        CreatedBy = data.CreatedBy,
                        CreatedDate = data.CreatedDate,
                        UpdatedBy = data.CreatedBy,
                        UpdatedDate = data.CreatedDate,
                        Mark = "A"
                    });


                    short i = 0;
                    foreach (var detail in data.OrderDetail)
                    {

                        MobileOrderDetail newDetail = new()
                        {
                            Code = data.OrderHeader.Code,
                            LineNo = i++,
                            ItemId = detail.ItemId,
                            UomId = detail.UomId,
                            UnitId = detail.UnitId,
                            Qty = detail.Qty,
                            UnitPrice = detail.UnitPrice,
                            Disc = detail.Disc,
                            TaxId = detail.TaxId,
                            TaxAmount = detail.TaxAmount,
                            NettPrice = detail.NettPrice,
                            Total = detail.Total,
                            Dpp = detail.Dpp
                        };

                        Db.MobileOrderDetails.Add(newDetail);

                        Db.SaveChanges();

                        short j = 0;
                        foreach (var discount in detail.Discounts)
                        {

                            Db.MobileOrderDetailDiscounts.Add(new MobileOrderDetailDiscount
                            {
                                Code = data.OrderHeader.Code,
                                OrderDetailId = newDetail.Id,
                                LineNo = j++,
                                PromoCode = discount.PromoCode,
                                PromoDetailId = discount.PromoDetailId,
                                Name = discount.Name,
                                IsPercentage = discount.IsPercentage,
                                Value = discount.Value,
                                Amount = discount.Amount
                            });
                        }

                        short k = 0;
                        foreach (var item in detail.FreeGoods)
                        {

                            Db.MobileOrderDetailFreeGoods.Add(new MobileOrderDetailFreeGood
                            {
                                Code = data.OrderHeader.Code,
                                OrderDetailId = newDetail.Id,
                                LineNo = k++,
                                PromoCode = item.PromoCode,
                                ItemId = item.ItemId,
                                UomId = item.UomId,
                                UnitId = item.UnitId,
                                Qty = item.Qty,
                                UnitPrice = item.UnitPrice
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
            result.Message = "Kunjungan berhasil disimpan.";
            return result;
        }

        public string GetLastUpdate(string lastUpdate)
        {
            if (lastUpdate.Length == 19)
                return lastUpdate + ".0000";
            else if (lastUpdate.Length == 21)
                return lastUpdate + "000";
            else if (lastUpdate.Length == 22)
                return lastUpdate + "00";
            else if (lastUpdate.Length == 23)
                return lastUpdate + "0";
            else if (lastUpdate.Length == 25)
                return lastUpdate.Substring(0, lastUpdate.Length - 1);
            else if (lastUpdate.Length == 26)
                return lastUpdate.Substring(0, lastUpdate.Length - 2);
            else return lastUpdate;
        }

        public SaveResult InsertWithNewCode(VisitRequestModel data)
        {
            var result = new SaveResult(false);

            using var transaction = Db.Database.BeginTransaction();
            try
            {

                var newLogCode = GetNewCode("MOB_VST_LOG_NUM_FMT", data.Date);

                data.Code = newLogCode.ToString();
                Db.MobileVisitLogs.Add(data);

                if (data.Visited == true)
                {
                    var vo = Db.VisitOrders.First(x => x.Code == data.VisitOrderCode);
                    vo.UpdatedBy = data.UpdatedBy;
                    vo.UpdatedDate = data.UpdatedDate;

                    if (data.Scheduled)
                    {
                        var order = Db.VisitOrderCustomers.First(x => x.Code == data.VisitOrderCode && x.CustCode == data.CustCode);
                        order.Visited = true;
                    }
                }

                foreach (var reason in data.VisitReasons)
                {
                    Db.MobileVisitReasons.Add(new MobileVisitReason
                    {
                        VisitLogCode = newLogCode,
                        VisitReasonId = reason
                    });
                }

                var newPaymentCode = GetNewCode("MOB_PAY_NUM_FMT", data.Date);
                foreach (var invoice in data.Invoices)
                {
                    Db.MobilePaymentInvoices.Add(new MobilePaymentInvoice
                    {
                        Code = newPaymentCode,
                        VisitLogCode = newLogCode,
                        Date = data.Date,
                        SalesmanId = data.SalesmanId,
                        CustCode = data.CustCode,
                        CoaCode = invoice.CoaCode,
                        TransCode = invoice.TransCode,
                        Amount = invoice.Amount,
                        NotesFailCollect = invoice.NotesFailCollect,
                        SrcTrans = invoice.SrcTrans,
                        CreatedBy = data.CreatedBy,
                        CreatedDate = data.CreatedDate,
                        UpdatedBy = data.CreatedBy,
                        UpdatedDate = data.CreatedDate,
                        Mark = "A"
                    });

                    var inv = Db.VisitOrderInvoices.First(x => x.Code == data.VisitOrderCode && x.InvCode == invoice.TransCode);
                    if (inv != null)
                    {
                        inv.Collecting = true;
                    }
                }

                if (data.OrderHeader != null)
                {
                    var newOrderCode = GetNewCode("MOB_SO_NUM_FMT", data.Date);

                    Db.MobileOrderHeaders.Add(new MobileOrderHeader
                    {
                        Code = newOrderCode,
                        Date = data.Date,
                        VisitLogCode = newLogCode,
                        Type = data.OrderHeader.Type,
                        CustCode = data.CustCode,
                        SalesBy = data.SalesmanId,
                        PaymentTermId = data.OrderHeader.PaymentTermId,
                        CurrCode = data.OrderHeader.CurrCode,
                        Rate = data.OrderHeader.Rate,
                        SubTotal = data.OrderHeader.SubTotal,
                        FinalDiscPercent = data.OrderHeader.FinalDiscPercent,
                        FinalDisc = data.OrderHeader.FinalDisc,
                        IncludeTax = data.OrderHeader.IncludeTax,
                        TaxAmount = data.OrderHeader.TaxAmount,
                        Total = data.OrderHeader.Total,
                        Dpp = data.OrderHeader.Dpp,
                        PaidAmount = data.OrderHeader.PaidAmount,
                        CreatedBy = data.CreatedBy,
                        CreatedDate = data.CreatedDate,
                        UpdatedBy = data.CreatedBy,
                        UpdatedDate = data.CreatedDate,
                        Mark = "A"
                    });


                    short i = 0;
                    foreach (var detail in data.OrderDetail)
                    {

                        MobileOrderDetail newDetail = new()
                        {
                            Code = newOrderCode,
                            LineNo = i++,
                            ItemId = detail.ItemId,
                            UomId = detail.UomId,
                            UnitId = detail.UnitId,
                            Qty = detail.Qty,
                            UnitPrice = detail.UnitPrice,
                            Disc = detail.Disc,
                            TaxId = detail.TaxId,
                            TaxAmount = detail.TaxAmount,
                            NettPrice = detail.NettPrice,
                            Total = detail.Total,
                            Dpp = detail.Dpp
                        };

                        Db.MobileOrderDetails.Add(newDetail);

                        Db.SaveChanges();

                        short j = 0;
                        foreach (var discount in detail.Discounts)
                        {

                            Db.MobileOrderDetailDiscounts.Add(new MobileOrderDetailDiscount
                            {
                                Code = newOrderCode,
                                OrderDetailId = newDetail.Id,
                                LineNo = j++,
                                PromoCode = discount.PromoCode,
                                PromoDetailId = discount.PromoDetailId,
                                Name = discount.Name,
                                IsPercentage = discount.IsPercentage,
                                Value = discount.Value,
                                Amount = discount.Amount
                            });
                        }

                        short k = 0;
                        foreach (var item in detail.FreeGoods)
                        {

                            Db.MobileOrderDetailFreeGoods.Add(new MobileOrderDetailFreeGood
                            {
                                Code = newOrderCode,
                                OrderDetailId = newDetail.Id,
                                LineNo = k++,
                                PromoCode = item.PromoCode,
                                ItemId = item.ItemId,
                                UomId = item.UomId,
                                UnitId = item.UnitId,
                                Qty = item.Qty,
                                UnitPrice = item.UnitPrice
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
            result.Message = "Kunjungan berhasil disimpan.";
            return result;
        }

        public IEnumerable<DateTime> GetListDate(int userId, DateTime date)
        {
            var salesId = Db.Users.Where(x => x.Id.Equals(userId)).Select(y => y.EmployeeId).First();
            var data = Db.MobileVisitLogs.Where(x => x.Date.Month.Equals(date.Month) && x.Date.Year.Equals(date.Year) && x.SalesmanId.Equals(salesId)).GroupBy(y => y.Date).Select(z => z.Key); ;


            return data;
        }

        public IEnumerable<VisitLogByDateModel> GetVisitLogByDate(int userId, DateTime date)
        {
            var salesId = Db.Users.Where(x => x.Id.Equals(userId)).Select(y => y.EmployeeId).First();
            var dataVisit = Db.MobileVisitLogs.Where(x => x.Date.Equals(date) && x.SalesmanId.Equals(salesId));

            var data = (from dv in dataVisit
                        join cu in Db.VwCustomers on dv.CustCode equals cu.Code
                        select new VisitLogByDateModel
                        {
                            Code=dv.Code,
                            Date=dv.Date,
                            CustCode=dv.CustCode,
                            CustInitial=cu.Initial,
                            CustName=cu.Name,
                            StartTime=dv.StartTime,
                            EndTime=dv.EndTime,
                            Visited=dv.Visited,
                            Scheduled=dv.Scheduled,
                            UnscheduledVisitReasonId=dv.UnscheduledVisitReasonId,
                            AreaId1=cu.AreaId1,
                            AreaId2 = cu.AreaId2,
                            AreaId3 = cu.AreaId3,
                            AreaId4 = cu.AreaId4,
                            AreaId5 = cu.AreaId5,
                            AreaName1=cu.AreaName1,
                            AreaName2 = cu.AreaName2,
                            AreaName3 = cu.AreaName3,
                            AreaName4 = cu.AreaName4,
                            AreaName5 = cu.AreaName5,
                        });

            return data;
        }

        public IEnumerable<MobilePaymentMethod> GetMobilePaymentMethod(string lastUpdate)
        {
            var data = Db.MobilePaymentMethods.AsQueryable();

            if (lastUpdate != null)
            {
                lastUpdate = GetLastUpdate(lastUpdate);
                data = data.Where(x => x.UpdatedDate > DateTime.ParseExact(lastUpdate, "yyyy-MM-ddTHH:mm:ss.ffff", null));
            }

            return data;
        }

        public VisitLogDetailModel GetDetailVisitLog(string code)
        {
            var data = (from vl in Db.VwMobileVisitLogs
                        join r1 in Db.MobileReasons on vl.UnscheduledVisitReasonId equals r1.Id into p1
                        from sub in p1.DefaultIfEmpty()
                        join r2 in Db.MobileReasons on vl.NoOrderReasonId equals r2.Id into p2
                        from sub2 in p2.DefaultIfEmpty()
                        join r3 in Db.MobileReasons on vl.NoVisitReasonId equals r3.Id into p3
                        from sub3 in p3.DefaultIfEmpty()
                        where vl.Code == code
                        select new VisitLogDetailModel
                        {
                            VisitOrderCode=vl.VisitOrderCode,
                            Date=vl.Date,
                            Code=vl.Code,
                            CustCode=vl.CustCode,
                            CustInitial=vl.CustomerInitial,
                            CustName=vl.CustomerName,
                            Scheduled=vl.Scheduled,
                            Visited=vl.Visited,
                            Lat=vl.Lat,
                            Lng=vl.Lng,
                            StartTime=vl.StartTime,
                            EndTime=vl.EndTime,
                            Total=vl.Total,
                            Image=vl.Image,
                            UnscheduledVisitReasonId=vl.UnscheduledVisitReasonId,
                            UnscheduledVisitReasonName=sub.Name,
                            NoOrderReasonId=vl.NoOrderReasonId,
                            NoOrderReasonName=sub2.Name,
                            NoVisitReasonId=vl.NoVisitReasonId,
                            NoVisitReasonName=sub3.Name,
                            OnGoing=false,
                            IsDraft=false
                        }).Single();

            return data;
        }

        public IEnumerable<OrderDetailRequestModel> GetOrderDetailRequest(string orderCode)
        {
            List<OrderDetailRequestModel> data=new();

            var details = (from od in Db.MobileOrderDetails
                           join it in Db.Items on od.ItemId equals it.Id
                           join unit in Db.UoMConversions on od.UnitId equals unit.Id
                           join tax in Db.Taxes on od.TaxId equals tax.Id into py
                           from sub in py.DefaultIfEmpty()
                           where od.Code == orderCode
                           select new OrderDetailModel
                           {
                               Id = od.Id,
                               Code = od.Code,
                               LineNo = od.LineNo,
                               ItemId = od.ItemId,
                               ItemName = it.Name,
                               UomId = od.UomId,
                               UnitId = od.UnitId,
                               UnitName = unit.UnitEquivalent,
                               Qty = od.Qty,
                               UnitPrice = od.UnitPrice,
                               Disc = od.Disc,
                               TaxId = od.TaxId,
                               TaxName = sub.Name,
                               TaxAmount = od.TaxAmount,
                               NettPrice = od.NettPrice,
                               Total = od.Total,
                               Dpp = od.Dpp
                           }).AsEnumerable();

            foreach (var detail in details)
            {
                
                var discounts = (from od in Db.MobileOrderDetailDiscounts
                                 where od.OrderDetailId == detail.Id
                                 select new PromoDiscountModel
                                 {
                                     PromoCode=od.PromoCode,
                                     PromoDetailId=od.PromoDetailId,
                                     Name=od.Name,
                                     IsPercentage=od.IsPercentage,
                                     Value=od.Value,
                                     Amount=od.Amount,
                                 });
                var freeGoods = (from fg in Db.MobileOrderDetailFreeGoods
                                 join it in Db.Items on fg.ItemId equals it.Id
                                 join un in Db.UoMConversions on fg.UnitId equals un.Id
                                 where fg.OrderDetailId == detail.Id
                                 select new PromoFreeGoodsModel
                                 {
                                     PromoCode=fg.PromoCode,
                                     ItemId=fg.ItemId,
                                     ItemName=it.Name,
                                     UomId=fg.UomId,
                                     UnitId=fg.UnitId,
                                     UnitName=un.UnitEquivalent,
                                     Qty=fg.Qty,
                                     UnitPrice=fg.UnitPrice
                                 });

                data.Add(new OrderDetailRequestModel
                {
                    Id = detail.Id,
                    Code = detail.Code,
                    LineNo = detail.LineNo,
                    ItemId = detail.ItemId,
                    ItemName = detail.ItemName,
                    UomId = detail.UomId,
                    UnitId = detail.UnitId,
                    UnitName = detail.UnitName,
                    Qty = detail.Qty,
                    UnitPrice = detail.UnitPrice,
                    Disc = detail.Disc,
                    TaxId = detail.TaxId,
                    TaxName = detail.TaxName,
                    TaxAmount = detail.TaxAmount,
                    NettPrice = detail.NettPrice,
                    Total = detail.Total,
                    Dpp = detail.Dpp,
                    Discounts=discounts,
                    FreeGoods=freeGoods
                });
            }
            return data;
        }

        public OrderHeaderModel GetOrderHeader(string visitLogCode)
        {
            var data = (from oh in Db.MobileOrderHeaders
                        join cu in Db.Customers on oh.CustCode equals cu.Code
                        join curr in Db.Currencies on oh.CurrCode equals curr.Code
                        join pt in Db.PaymentTerms on oh.PaymentTermId equals pt.Id into py
                        from sub in py.DefaultIfEmpty()
                        where oh.VisitLogCode==visitLogCode
                        select new OrderHeaderModel
                        {
                            Code = oh.Code,
                            Date = oh.Date,
                            VisitLogCode = oh.VisitLogCode,
                            SalesOrderCode = oh.SalesOrderCode,
                            Type = oh.Type,
                            CustCode = oh.CustCode,
                            CustName = cu.Name,
                            CurrCode = oh.CurrCode,
                            CurrName = curr.Name,
                            PaymentTermId = oh.PaymentTermId,
                            PaymentTermName = sub.Name,
                            PaidAmount = oh.PaidAmount,
                            TaxAmount = oh.TaxAmount,
                            Dpp = oh.Dpp,
                            Rate = oh.Rate,
                            FinalDisc = oh.FinalDisc,
                            FinalDiscPercent = oh.FinalDiscPercent,
                            IncludeTax = oh.IncludeTax,
                            SubTotal = oh.SubTotal,
                            Total = oh.Total,
                            UpdatedDate = oh.UpdatedDate
                        }).SingleOrDefault();

            return data;
        }
    }
}
