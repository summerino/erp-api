using ERP.Entity.Sales;
using ERP.Web.API.Domain.Extensions;
using ERP.Web.API.Domain.Interfaces.Sales;
using ERP.Web.API.Domain.Models;
using ERP.Web.API.Model.Sales;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ERP.Entity;

namespace ERP.Web.API.Domain.Services.Sales
{
    public class PromoService : GeneralService<PromoHeader>, IPromoService
    {
        public PromoService(TenantContext db)
            : base(db)
        {

        }

        public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort, string search)
        {
            var data = Db.VwPromoHeaders.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                data = DateTime.TryParse(search, out var searchDate)
                    ? data.Where(x => x.StartDate == searchDate || x.EndDate == searchDate)
                    : data.Where(x =>
                        x.Code.Contains(search) || x.Name.Contains(search));
            }

            return data.ToDataSourceResult(skip, take, filter, sort);
        }

        public IEnumerable<PromoDetail> GetDetailData(string code)
        {
            return Db.PromoDetails.Where(x => x.Code == code).OrderBy(x => x.LineNo);
        }

        public IEnumerable<PromoDetailTier> GetDetailTierData()
        {
            return Db.PromoDetailTiers.ToList();
        }

        public SaveResult Insert(PromoRequest data)
        {
            var result = new SaveResult(false);

            using var transaction = Db.Database.BeginTransaction();
            try
            {
                // Insert header data
                var newCode = GetNewCode("PROMO_NUM_FMT", DateTime.Now);

                data.Code = newCode;
                Db.PromoHeaders.Add(data);

                // Insert detail data
                short i = 0;
                foreach (var item in data.ItemDetails)
                {
                    var newItem = new PromoDetail
                    {
                        Code = data.Code,
                        LineNo = ++i,
                        ApplyTo = item.ApplyTo,
                        ItemId = item.ItemId,
                        PromoType = item.PromoType,
                        IsPercentage = item.IsPercentage,
                        ValuePercentage = item.ValuePercentage,
                        ValueAmount = item.ValueAmount,
                        IsPromoWithBudget = item.IsPromoWithBudget,
                        BudgetMaximumValue = item.BudgetMaximumValue,
                        OverBudgetAction = item.OverBudgetAction
                    };

                    Db.PromoDetails.Add(newItem);

                    Db.SaveChanges();

                    var idNewItem = newItem.Id;

                    if (item.PromoTierList.Any())
                    {
                        foreach (var tItem in item.PromoTierList)
                        {
                            Db.PromoDetailTiers.Add(new PromoDetailTier
                            {
                                PromoDetailId = idNewItem,
                                FromQty = tItem.FromQty,
                                ToQty = tItem.ToQty,
                                IsPercentage = item.IsPercentage,
                                Value = tItem.Value,
                                SaleUnit = item.SaleUnit,
                                ApplyToAllUnit = item.ApplyToAllUnit,
                                FreeGoodItemId = item.FreeGoodItemId,
                                UnitFreeGood = item.UnitFreeGood,
                                IsMultiple = item.IsMultiple,
                                PaymentTermId = tItem.PaymentTermId
                            });
                        }
                    }
                }

                // Save changes
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
            result.Message = "Data promo berhasil disimpan.";
            return result;
        }

        public SaveResult Update(PromoRequest data)
        {
            var result = new SaveResult(false);

            using var transaction = Db.Database.BeginTransaction();
            try
            {
                if (Db.PromoHeaders.Any(x => x.Code == data.Code && x.Mark == "V"))
                {
                    result.Message = "Data promo tidak bisa diubah karena data sudah ditandai sebagai void.";
                    return result;
                }

                data.ApprovedBy = null;
                data.ApprovedDate = null;

                Db.PromoHeaders.Update(data);
                Db.Entry(data).Property(e => e.Code).IsModified = false;
                Db.Entry(data).Property(e => e.CreatedBy).IsModified = false;
                Db.Entry(data).Property(e => e.CreatedDate).IsModified = false;

                var delDetails = Db.PromoDetails
                    .Where(d => d.Code == data.Code && !data.ItemDetails.Select(x => x.Id).Contains(d.Id))
                    .ToList();

                Db.PromoDetails.RemoveRange(delDetails);

                short i = 0;
                foreach (var item in data.ItemDetails)
                {
                    var delTierDetails = Db.PromoDetailTiers
                                            .Where(d => d.PromoDetailId == item.Id && !item.PromoTierList.Select(x => x.Id).Contains(d.Id))
                                            .ToList();

                    Db.PromoDetailTiers.RemoveRange(delTierDetails);

                    long idDetail = 0;
                    if (item.Id < 0)
                    {
                        var newItem = new PromoDetail
                        {
                            Code = data.Code,
                            LineNo = ++i,
                            ApplyTo = item.ApplyTo,
                            ItemId = item.ItemId,
                            PromoType = item.PromoType,
                            IsPercentage = item.IsPercentage,
                            ValuePercentage = item.ValuePercentage,
                            ValueAmount = item.ValueAmount,
                            IsPromoWithBudget = item.IsPromoWithBudget,
                            BudgetMaximumValue = item.BudgetMaximumValue,
                            OverBudgetAction = item.OverBudgetAction
                        };

                        Db.PromoDetails.Add(newItem);

                        Db.SaveChanges();

                        idDetail = newItem.Id;
                    }
                    else
                    {
                        item.LineNo = ++i;

                        Db.PromoDetails.Update(item);
                        Db.Entry(item).Property(e => e.Code).IsModified = false;

                        idDetail = item.Id;
                    }

                    if (item.PromoTierList.Any())
                    {
                        foreach (var tItem in item.PromoTierList)
                        {
                            if (tItem.Id < 0)
                            {
                                Db.PromoDetailTiers.Add(new PromoDetailTier
                                {
                                    PromoDetailId = idDetail,
                                    FromQty = tItem.FromQty,
                                    ToQty = tItem.ToQty,
                                    IsPercentage = item.IsPercentage,
                                    Value = tItem.Value,
                                    SaleUnit = item.SaleUnit,
                                    ApplyToAllUnit = item.ApplyToAllUnit,
                                    FreeGoodItemId = item.FreeGoodItemId,
                                    UnitFreeGood = item.UnitFreeGood,
                                    IsMultiple = item.IsMultiple,
                                    PaymentTermId = tItem.PaymentTermId
                                });
                            }
                            else
                            {
                                var trItem = Db.PromoDetailTiers.FirstOrDefault(x => x.PromoDetailId == idDetail);
                                trItem.FromQty = tItem.FromQty;
                                trItem.ToQty = tItem.ToQty;
                                trItem.IsPercentage = tItem.IsPercentage;
                                trItem.Value = tItem.Value;
                                trItem.SaleUnit = tItem.SaleUnit;
                                trItem.ApplyToAllUnit = tItem.ApplyToAllUnit;
                                trItem.FreeGoodItemId = tItem.FreeGoodItemId;
                                trItem.UnitFreeGood = tItem.UnitFreeGood;
                                trItem.IsMultiple = tItem.IsMultiple;
                                trItem.PaymentTermId = tItem.PaymentTermId;

                                Db.PromoDetailTiers.Update(trItem);
                            }
                        }
                    }
                }

                // Save changes
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
            result.Message = "Data promo berhasil diperbarui.";
            return result;
        }

        public SaveResult Delete(string code, int userId)
        {
            var result = new SaveResult(false);

            var data = Db.PromoHeaders.Find(code);
            if (data != null)
            {
                // Checking mark header data
                if (data.Mark == "V")
                {
                    result.Message = "Data promo tidak bisa ditandai sebagai void karena sudah ditandai sebagai void.";
                    return result;
                }

                // Update header data
                data.Mark = "V";
                data.UpdatedBy = userId;
                data.UpdatedDate = DateTime.Now;

                Db.SaveChanges();
            }

            result.Success = true;
            result.Message = "Data promo berhasil ditandai sebagai void.";
            return result;
        }
    }
}
