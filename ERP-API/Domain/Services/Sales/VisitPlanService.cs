using System;
using System.Collections.Generic;
using System.Linq;
using ERP.Entity;
using ERP.Entity.Sales;
using ERP.Web.API.Domain.Extensions;
using ERP.Web.API.Domain.Interfaces.Sales;
using ERP.Web.API.Domain.Models;
using ERP.Web.API.Model.Sales;

namespace ERP.Web.API.Domain.Services.Sales
{
    public class VisitPlanService : GeneralService<VisitPlanHeader>, IVisitPlanService
    {
        public VisitPlanService(TenantContext db)
            : base(db)
        {
        }

        public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
            List<int> category, string search)
        {
            var data = Db.VwVisitPlanHeaders.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                data = DateTime.TryParse(search, out var searchDate)
                    ? data.Where(x => x.Date == searchDate)
                    : data.Where(x =>
                        x.Code.Contains(search) || x.GroupId == Convert.ToInt32(search));
            }

            return data.ToDataSourceResult(skip, take, filter, sort);
        }

        public IEnumerable<VwVisitPlanDetail> GetDetailData(string code)
        {
            var data = Db.VwVisitPlanDetails.Where(x => x.Code == code);

            return data.OrderBy(x => x.Id);
        }

        public IEnumerable<VwVisitPlanDetailCustomer> GetCustomerDetailData(List<long> id)
        {
            var data = Db.VwVisitPlanDetailCustomers.AsQueryable();

            if (id?.Any() ?? false)
            {
                data = Db.VwVisitPlanDetailCustomers.Where(x => id.Contains(x.VisitPlanDetailId));
            }

            return data.OrderBy(x => x.VisitPlanDetailId);
        }

        public SaveResult Insert(VisitPlanRequest data)
        {
            var result = new SaveResult(false);
            var listIdDetail = new List<long>();
            var tempBeforeId = new List<long>();

            using var transaction = Db.Database.BeginTransaction();
            try
            {
                // Get new code
                var newCode = GetNewCode("VST_PLAN_NUM_FMT", data.Date);

                // Insert data
                data.Code = newCode;
                Db.VisitPlanHeaders.Add(data);
                Db.SaveChanges();

                // Insert detail data VisitPlanDetails
                foreach (var item in data.ItemDetails)
                {
                    var visitDetail = new VisitPlanDetail
                    {
                        Code = newCode,
                        SalesmanId = item.SalesmanId
                    };
                    tempBeforeId.Add(item.Id);
                    Db.VisitPlanDetails.Add(visitDetail);
                    Db.SaveChanges();

                    listIdDetail.Add(visitDetail.Id);
                }

                // Insert detail data VisitPlanDetailCustomers
                foreach (var item in data.CustomerDetails)
                {
                    for (var i = 0; i < tempBeforeId.Count; i++)
                    {
                        if (item.VisitPlanDetailId == tempBeforeId[i])
                        {
                            Db.VisitPlanDetailCustomers.Add(new VisitPlanDetailCustomer
                            {
                                VisitPlanDetailId = listIdDetail[i],
                                CustCode = item.CustCode
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
            result.Message = "Data rencana kunjungan berhasil disimpan.";
            return result;
        }

        public SaveResult Update(VisitPlanRequest data)
        {
            var result = new SaveResult(false);
            var listIdDetail = new List<long>();
            var tempBeforeId = new List<long>();

            using var transaction = Db.Database.BeginTransaction();
            try
            {
                // Checking mark header data
                if (Db.VisitPlanHeaders.Any(x => x.Code == data.Code && x.Mark == "V"))
                {
                    result.Message = "Data rencana kunjungan tidak bisa diubah karena sudah ditandai sebagai void.";
                    return result;
                }

                data.ApprovedBy = null;
                data.ApprovedDate = null;

                // Update data
                Db.VisitPlanHeaders.Update(data);
                Db.Entry(data).Property(e => e.Code).IsModified = false;
                Db.Entry(data).Property(e => e.CreatedBy).IsModified = false;
                Db.Entry(data).Property(e => e.CreatedDate).IsModified = false;

                // Delete existing item detail VisitPlanDetails
                var delVisitPlanDetails = Db.VisitPlanDetails
                        .Where(d => d.Code == data.Code).ToList();

                for (var i = 0; i < delVisitPlanDetails.Count; i++)
                {
                    // Delete existing item detail VisitPlanDetailCustomers
                    var delVisitPlanDetailCustomers = Db.VisitPlanDetailCustomers
                            .Where(d => d.VisitPlanDetailId == delVisitPlanDetails[i].Id).ToList();

                    Db.VisitPlanDetailCustomers.RemoveRange(delVisitPlanDetailCustomers);
                }                

                Db.VisitPlanDetails.RemoveRange(delVisitPlanDetails);

                // Insert detail data VisitPlanDetails
                foreach (var item in data.ItemDetails)
                {
                    var visitDetail = new VisitPlanDetail
                    {
                        Code = data.Code,
                        SalesmanId = item.SalesmanId
                    };
                    tempBeforeId.Add(item.Id);
                    Db.VisitPlanDetails.Add(visitDetail);
                    Db.SaveChanges();

                    listIdDetail.Add(visitDetail.Id);
                }

                // Insert detail data VisitPlanDetailCustomers
                foreach (var item in data.CustomerDetails)
                {
                    for (var i = 0; i < tempBeforeId.Count; i++)
                    {
                        if (item.VisitPlanDetailId == tempBeforeId[i])
                        {
                            Db.VisitPlanDetailCustomers.Add(new VisitPlanDetailCustomer
                            {
                                VisitPlanDetailId = listIdDetail[i],
                                CustCode = item.CustCode
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
            result.Message = "Data rencana kunjungan berhasil diperbarui.";
            return result;
        }

        public SaveResult Delete(string code, int userId)
        {
            var result = new SaveResult(false);

            using var transaction = Db.Database.BeginTransaction();
            try
            {
                var data = Db.VisitPlanHeaders.Find(code);
                if (data != null)
                {
                    // Checking mark header data
                    if (data.Mark == "V")
                    {
                        result.Message = "Data rencana kunjungan tidak bisa dihapus karena data sudah tidak ada.";
                        return result;
                    }

                    // Update data
                    data.Mark = "V";
                    data.UpdatedBy = userId;
                    data.UpdatedDate = DateTime.Now;

                    // Un-comment this section if you wanna hard delete
                    // ==================================================
                    //// Delete existing item detail VisitPlanDetails
                    //var delVisitPlanDetails = Db.VisitPlanDetails
                    //        .Where(d => d.Code == data.Code).ToList();

                    //for (var i = 0; i < delVisitPlanDetails.Count; i++)
                    //{
                    //    // Delete existing item detail VisitPlanDetailCustomers
                    //    var delVisitPlanDetailCustomers = Db.VisitPlanDetailCustomers
                    //            .Where(d => d.VisitPlanDetailId == delVisitPlanDetails[i].Id).ToList();

                    //    Db.VisitPlanDetailCustomers.RemoveRange(delVisitPlanDetailCustomers);
                    //}

                    //Db.VisitPlanDetails.RemoveRange(delVisitPlanDetails);
                    // ==================================================
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
            result.Message = "Data rencana kunjungan berhasil dihapus.";
            return result;
        }
    }
}
