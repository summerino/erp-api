using ERP.Common;
using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Entity.General;
using ERP.Entity.MobileCustomer;
using ERP.Web.API.Domain.Interfaces.Mobile.CustomerTransaction;
using ERP.Web.API.Domain.Models.Mobile.CustomerOrder;
using ERP.Web.API.Domain.Models.Mobile.CustomerPromotion;
using ERP.Web.API.Domain.Models.Mobile.VisitOrder;

namespace ERP.Web.API.Domain.Services.Mobile.CustomerTransaction;

public class CustomerOrderService : GeneralService<MobileOrderHeader>, ICustomerOrderService
{
    private readonly TenantContext _db;
    public CustomerOrderService(TenantContext db) : base(db)
    {
        _db = db;
    }

    public IEnumerable<OrderCustomerDetailModel> GetCustomerOrderDetail(string orderId)
    {
        var detailOrder = Db.VwMobileCustomerOrderDetails.Where(x => x.Code.Equals(orderId)).ToList();
        List<OrderCustomerDetailModel> data = new();
        for (int i = 0; i < detailOrder.Count; i++)
        {
            var discounts = (from disc in Db.MobileCustomerOrderDetailDiscounts.Where(x => x.PromoDetailId.Equals(detailOrder[i].Id) && x.Code.Equals(orderId))
                select new OrderDetailDiscountRequestModel
                {
                    PromoCode = disc.PromoCode,
                    PromoDetailId = disc.PromoDetailId,
                    Name = disc.Name,
                    IsPercentage = disc.IsPercentage,
                    Value = disc.Value,
                    Amount = disc.Amount
                }).ToList();

            var freeGoods = (from fg in Db.MobileCustomerOrderDetailFreeGoods.Where(x => x.Code.Equals(orderId))
                join itm in Db.Items on fg.ItemId equals itm.Id
                select new OrderCustomerFreeGoodsModel
                {
                    PromoCode = fg.PromoCode,
                    ItemId = fg.ItemId,
                    ItemName = itm.Name,
                    UomId = fg.UomId,
                    UnitId = fg.UnitId,
                    Qty = fg.Qty,
                    UnitPrice = fg.UnitPrice
                }).ToList();

            data.Add(new OrderCustomerDetailModel
            {
                Id = detailOrder[i].Id,
                Code = detailOrder[i].Code,
                LineNo = detailOrder[i].LineNo,
                ItemId = detailOrder[i].ItemId,
                ItemName = detailOrder[i].ItemName,
                UnitName = detailOrder[i].UnitName,
                UomId = detailOrder[i].UomId,
                UnitId = detailOrder[i].UnitId,
                Qty = detailOrder[i].Qty,
                UnitPrice = detailOrder[i].UnitPrice,
                Disc = detailOrder[i].Disc,
                TaxId = detailOrder[i].TaxId,
                TaxName = "",
                TaxAmount = detailOrder[i].TaxAmount,
                NettPrice = detailOrder[i].NettPrice,
                Total = detailOrder[i].Total,
                Dpp = detailOrder[i].Dpp,
                Discounts = discounts,
                FreeGoods = freeGoods,
            });
        }
        return data;
    }

    public DataSourceResult GetCustomerOrderHeader(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort, DateTime? date, string custCode)
    {
        var data = Db.MobileCustomerOrderHeaders.Where(x => x.CustCode.Equals(custCode)).OrderByDescending(x => x.Date).AsQueryable();

        if (date.HasValue)
        {
            data = data.Where(x => x.Date.Equals(date.Value));
        }

        return data.ToDataSourceResult(skip, take, filter, sort);
    }

    public bool GetParams()
    {
        var data = Db.SystemParameters.Where(x => x.Code.Equals("DEF_SALES_TAX_INC")).Select(y => y.Value).Single();
        if (data == "1")
            return true;
        else
            return false;
    }

    public IEnumerable<PromotionDiscountModel> GetPromotionDiscount(int itemId, int itemCatId, decimal qty, int unitId, string custCode)
    {
        var currentDate = DateTime.Now.Date;
        var custTypeId = Db.Customers.Where(x => x.Code.Equals(custCode)).Select(y => y.TypeId).SingleOrDefault();
        var data1 = (from H in Db.PromoHeaders
            join D in Db.PromoDetails on H.Code equals D.Code
            where H.ApplyTo == 1 && D.ApplyTo == 1 && D.PromoType == 1 && D.ItemId == itemId &&
                  currentDate >= H.StartDate && currentDate <= H.EndDate
            select new PromotionDiscountModel
            {
                PromoCode = H.Code,
                PromoDetailId = D.Id,
                Name = H.Name,
                IsPercentage = D.IsPercentage,
                Value = D.IsPercentage ? D.ValuePercentage : D.ValueAmount,
                IsPromoWithBudget = D.IsPromoWithBudget,
                BudgetMaximumValue = D.BudgetMaximumValue,
                OverBudgetAction = D.OverBudgetAction,
            }).AsQueryable();

        var data2 = (from H in Db.PromoHeaders
            join D in Db.PromoDetails on H.Code equals D.Code
            where H.ApplyTo == 1 && D.ApplyTo == 3 && D.PromoType == 1 && D.ItemId == itemCatId &&
                  currentDate >= H.StartDate && currentDate <= H.EndDate
            select new PromotionDiscountModel
            {
                PromoCode = H.Code,
                PromoDetailId = D.Id,
                Name = H.Name,
                IsPercentage = D.IsPercentage,
                Value = D.IsPercentage ? D.ValuePercentage : D.ValueAmount,
                IsPromoWithBudget = D.IsPromoWithBudget,
                BudgetMaximumValue = D.BudgetMaximumValue,
                OverBudgetAction = D.OverBudgetAction,
            }).AsQueryable();

        var data3 = (from H in Db.PromoHeaders
            join D in Db.PromoDetails on H.Code equals D.Code
            join T in Db.PromoDetailTiers on D.Id equals T.PromoDetailId
            where H.ApplyTo == 1 && D.ApplyTo == 1 && D.PromoType == 2 && D.ItemId == itemId &&
                  T.SaleUnit == (T.ApplyToAllUnit ? T.SaleUnit : unitId) &&
                  (T.FromQty <= qty && qty <= T.ToQty) &&
                  currentDate >= H.StartDate && currentDate <= H.EndDate
            select new PromotionDiscountModel
            {
                PromoCode = H.Code,
                PromoDetailId = D.Id,
                Name = H.Name,
                IsPercentage = T.IsPercentage,
                Value = T.Value,
                IsPromoWithBudget = D.IsPromoWithBudget,
                BudgetMaximumValue = D.BudgetMaximumValue,
                OverBudgetAction = D.OverBudgetAction,
            }).AsQueryable();

        var data4 = (from H in Db.PromoHeaders
            join D in Db.PromoDetails on H.Code equals D.Code
            join T in Db.PromoDetailTiers on D.Id equals T.PromoDetailId
            where H.ApplyTo == 1 && D.ApplyTo == 3 && D.PromoType == 2 && D.ItemId == itemCatId &&
                  T.SaleUnit == (T.ApplyToAllUnit ? T.SaleUnit : unitId) &&
                  (T.FromQty <= qty && qty <= T.ToQty) &&
                  currentDate >= H.StartDate && currentDate <= H.EndDate
            select new PromotionDiscountModel
            {
                PromoCode = H.Code,
                PromoDetailId = D.Id,
                Name = H.Name,
                IsPercentage = T.IsPercentage,
                Value = T.Value,
                IsPromoWithBudget = D.IsPromoWithBudget,
                BudgetMaximumValue = D.BudgetMaximumValue,
                OverBudgetAction = D.OverBudgetAction,
            }).AsQueryable();

        var data5 = (from H in Db.PromoHeaders
            join D in Db.PromoDetails on H.Code equals D.Code
            join S in Db.PromoSubjects on D.Code equals S.Code
            where H.ApplyTo == 2 && D.ApplyTo == 1 && D.PromoType == 1 && D.ItemId == itemId &&
                  S.CustCode == custCode &&
                  currentDate >= H.StartDate && currentDate <= H.EndDate
            select new PromotionDiscountModel
            {
                PromoCode = H.Code,
                PromoDetailId = D.Id,
                Name = H.Name,
                IsPercentage = D.IsPercentage,
                Value = D.IsPercentage ? D.ValuePercentage : D.ValueAmount,
                IsPromoWithBudget = D.IsPromoWithBudget,
                BudgetMaximumValue = D.BudgetMaximumValue,
                OverBudgetAction = D.OverBudgetAction,
            }).AsQueryable();

        var data6 = (from H in Db.PromoHeaders
            join D in Db.PromoDetails on H.Code equals D.Code
            join S in Db.PromoSubjects on D.Code equals S.Code
            where H.ApplyTo == 2 && D.ApplyTo == 3 && D.PromoType == 1 && D.ItemId == itemCatId &&
                  S.CustCode == custCode &&
                  currentDate >= H.StartDate && currentDate <= H.EndDate
            select new PromotionDiscountModel
            {
                PromoCode = H.Code,
                PromoDetailId = D.Id,
                Name = H.Name,
                IsPercentage = D.IsPercentage,
                Value = D.IsPercentage ? D.ValuePercentage : D.ValueAmount,
                IsPromoWithBudget = D.IsPromoWithBudget,
                BudgetMaximumValue = D.BudgetMaximumValue,
                OverBudgetAction = D.OverBudgetAction,
            }).AsQueryable();

        var data7 = (from H in Db.PromoHeaders
            join D in Db.PromoDetails on H.Code equals D.Code
            join T in Db.PromoDetailTiers on D.Id equals T.PromoDetailId
            join S in Db.PromoSubjects on D.Code equals S.Code
            where H.ApplyTo == 2 && D.ApplyTo == 1 && D.PromoType == 2 && D.ItemId == itemId &&
                  T.SaleUnit == (T.ApplyToAllUnit ? T.SaleUnit : unitId) &&
                  (T.FromQty <= qty && qty <= T.ToQty) && S.CustCode == custCode &&
                  currentDate >= H.StartDate && currentDate <= H.EndDate
            select new PromotionDiscountModel
            {
                PromoCode = H.Code,
                PromoDetailId = D.Id,
                Name = H.Name,
                IsPercentage = T.IsPercentage,
                Value = T.Value,
                IsPromoWithBudget = D.IsPromoWithBudget,
                BudgetMaximumValue = D.BudgetMaximumValue,
                OverBudgetAction = D.OverBudgetAction,
            }).AsQueryable();

        var data8 = (from H in Db.PromoHeaders
            join D in Db.PromoDetails on H.Code equals D.Code
            join T in Db.PromoDetailTiers on D.Id equals T.PromoDetailId
            join S in Db.PromoSubjects on D.Code equals S.Code
            where H.ApplyTo == 2 && D.ApplyTo == 3 && D.PromoType == 2 && D.ItemId == itemCatId &&
                  T.SaleUnit == (T.ApplyToAllUnit ? T.SaleUnit : unitId) &&
                  (T.FromQty <= qty && qty <= T.ToQty) && S.CustCode == custCode &&
                  currentDate >= H.StartDate && currentDate <= H.EndDate
            select new PromotionDiscountModel
            {
                PromoCode = H.Code,
                PromoDetailId = D.Id,
                Name = H.Name,
                IsPercentage = T.IsPercentage,
                Value = T.Value,
                IsPromoWithBudget = D.IsPromoWithBudget,
                BudgetMaximumValue = D.BudgetMaximumValue,
                OverBudgetAction = D.OverBudgetAction,
            }).AsQueryable();

        var data9 = (from H in Db.PromoHeaders
            join D in Db.PromoDetails on H.Code equals D.Code
            join S in Db.PromoSubjects on D.Code equals S.Code
            where H.ApplyTo == 3 && D.ApplyTo == 1 && D.PromoType == 1 && D.ItemId == itemId &&
                  S.CustTypeId == custTypeId &&
                  currentDate >= H.StartDate && currentDate <= H.EndDate
            select new PromotionDiscountModel
            {
                PromoCode = H.Code,
                PromoDetailId = D.Id,
                Name = H.Name,
                IsPercentage = D.IsPercentage,
                Value = D.IsPercentage ? D.ValuePercentage : D.ValueAmount,
                IsPromoWithBudget = D.IsPromoWithBudget,
                BudgetMaximumValue = D.BudgetMaximumValue,
                OverBudgetAction = D.OverBudgetAction,
            }).AsQueryable();

        var data10 = (from H in Db.PromoHeaders
            join D in Db.PromoDetails on H.Code equals D.Code
            join S in Db.PromoSubjects on D.Code equals S.Code
            where H.ApplyTo == 3 && D.ApplyTo == 3 && D.PromoType == 1 && D.ItemId == itemCatId &&
                  S.CustTypeId == custTypeId &&
                  currentDate >= H.StartDate && currentDate <= H.EndDate
            select new PromotionDiscountModel
            {
                PromoCode = H.Code,
                PromoDetailId = D.Id,
                Name = H.Name,
                IsPercentage = D.IsPercentage,
                Value = D.IsPercentage ? D.ValuePercentage : D.ValueAmount,
                IsPromoWithBudget = D.IsPromoWithBudget,
                BudgetMaximumValue = D.BudgetMaximumValue,
                OverBudgetAction = D.OverBudgetAction,
            }).AsQueryable();

        var data11 = (from H in Db.PromoHeaders
            join D in Db.PromoDetails on H.Code equals D.Code
            join T in Db.PromoDetailTiers on D.Id equals T.PromoDetailId
            join S in Db.PromoSubjects on D.Code equals S.Code
            where H.ApplyTo == 3 && D.ApplyTo == 1 && D.PromoType == 2 && D.ItemId == itemId &&
                  T.SaleUnit == (T.ApplyToAllUnit ? T.SaleUnit : unitId) &&
                  (T.FromQty <= qty && qty <= T.ToQty) && S.CustTypeId == custTypeId &&
                  currentDate >= H.StartDate && currentDate <= H.EndDate
            select new PromotionDiscountModel
            {
                PromoCode = H.Code,
                PromoDetailId = D.Id,
                Name = H.Name,
                IsPercentage = T.IsPercentage,
                Value = T.Value,
                IsPromoWithBudget = D.IsPromoWithBudget,
                BudgetMaximumValue = D.BudgetMaximumValue,
                OverBudgetAction = D.OverBudgetAction,
            }).AsQueryable();

        var data12 = (from H in Db.PromoHeaders
            join D in Db.PromoDetails on H.Code equals D.Code
            join T in Db.PromoDetailTiers on D.Id equals T.PromoDetailId
            join S in Db.PromoSubjects on D.Code equals S.Code
            where H.ApplyTo == 3 && D.ApplyTo == 3 && D.PromoType == 2 && D.ItemId == itemCatId &&
                  T.SaleUnit == (T.ApplyToAllUnit ? T.SaleUnit : unitId) &&
                  (T.FromQty <= qty && qty <= T.ToQty) && S.CustTypeId == custTypeId &&
                  currentDate >= H.StartDate && currentDate <= H.EndDate
            select new PromotionDiscountModel
            {
                PromoCode = H.Code,
                PromoDetailId = D.Id,
                Name = H.Name,
                IsPercentage = T.IsPercentage,
                Value = T.Value,
                IsPromoWithBudget = D.IsPromoWithBudget,
                BudgetMaximumValue = D.BudgetMaximumValue,
                OverBudgetAction = D.OverBudgetAction,
            }).AsQueryable();

        var union1 = data1.Union(data2).Union(data5).Union(data6).Union(data9).Union(data10).ToList();
        var union2 = data3.Union(data4).Union(data7).Union(data8).Union(data11).Union(data12).ToList();
        var data = union1.Union(union2);
        return data;
    }

    public IEnumerable<PromotionFreeGoodModel> GetPromotionFreeGood(int itemId, int itemCatId, decimal qty, int unitId, string custCode)
    {
        var currentDate = DateTime.Now.Date;
        var custTypeId = Db.Customers.Where(x => x.Code.Equals(custCode)).Select(y => y.TypeId).SingleOrDefault();
        var data1 = (from H in Db.PromoHeaders
            join D in Db.PromoDetails on H.Code equals D.Code
            join T in Db.PromoDetailTiers on D.Id equals T.PromoDetailId
            join I in Db.Items on T.FreeGoodItemId equals I.Id
            join U in Db.UoMConversions on Convert.ToInt32(T.UnitFreeGood) equals U.Id
            where H.ApplyTo == 1 && D.ApplyTo == 1 && D.PromoType == 3 && D.ItemId == itemId &&
                  T.SaleUnit == (T.ApplyToAllUnit ? T.SaleUnit : unitId) &&
                  (T.FromQty <= qty && qty <= (T.IsMultiple ? qty : T.ToQty)) &&
                  currentDate >= H.StartDate && currentDate <= H.EndDate
            select new PromotionFreeGoodModel
            {
                PromoCode = H.Code,
                ItemId = (int)T.FreeGoodItemId,
                ItemName = I.Name,
                UomId = (int)I.UomId,
                UnitId = int.Parse(T.UnitFreeGood),
                UnitName = U.UnitEquivalent,
                Qty = T.IsMultiple ? T.Value * Math.Round(10 / T.FromQty, 0) : T.Value,
                UnitPrice = (decimal)I.SellPrice
            }).AsEnumerable();

        var data2 = (from H in Db.PromoHeaders
            join D in Db.PromoDetails on H.Code equals D.Code
            join T in Db.PromoDetailTiers on D.Id equals T.PromoDetailId
            join I in Db.Items on T.FreeGoodItemId equals I.Id
            join U in Db.UoMConversions on Convert.ToInt32(T.UnitFreeGood) equals U.Id
            where H.ApplyTo == 1 && D.ApplyTo == 3 && D.PromoType == 3 && D.ItemId == itemCatId &&
                  T.SaleUnit == (T.ApplyToAllUnit ? T.SaleUnit : unitId) &&
                  (T.FromQty <= qty && qty <= (T.IsMultiple ? qty : T.ToQty)) &&
                  currentDate >= H.StartDate && currentDate <= H.EndDate
            select new PromotionFreeGoodModel
            {
                PromoCode = H.Code,
                ItemId = (int)T.FreeGoodItemId,
                ItemName = I.Name,
                UomId = (int)I.UomId,
                UnitId = int.Parse(T.UnitFreeGood),
                UnitName = U.UnitEquivalent,
                Qty = T.IsMultiple ? T.Value * Math.Round(10 / T.FromQty, 0) : T.Value,
                UnitPrice = (decimal)I.SellPrice
            }).AsEnumerable();

        var data3 = (from H in Db.PromoHeaders
            join D in Db.PromoDetails on H.Code equals D.Code
            join T in Db.PromoDetailTiers on D.Id equals T.PromoDetailId
            join S in Db.PromoSubjects on D.Code equals S.Code
            join I in Db.Items on T.FreeGoodItemId equals I.Id
            join U in Db.UoMConversions on Convert.ToInt32(T.UnitFreeGood) equals U.Id
            where H.ApplyTo == 2 && D.ApplyTo == 1 && D.PromoType == 3 && D.ItemId == itemId &&
                  T.SaleUnit == (T.ApplyToAllUnit ? T.SaleUnit : unitId) &&
                  (T.FromQty <= qty && qty <= (T.IsMultiple ? qty : T.ToQty)) &&
                  S.CustCode == custCode &&
                  currentDate >= H.StartDate && currentDate <= H.EndDate
            select new PromotionFreeGoodModel
            {
                PromoCode = H.Code,
                ItemId = (int)T.FreeGoodItemId,
                ItemName = I.Name,
                UomId = (int)I.UomId,
                UnitId = int.Parse(T.UnitFreeGood),
                UnitName = U.UnitEquivalent,
                Qty = T.IsMultiple ? T.Value * Math.Round(10 / T.FromQty, 0) : T.Value,
                UnitPrice = (decimal)I.SellPrice
            }).AsEnumerable();

        var data4 = (from H in Db.PromoHeaders
            join D in Db.PromoDetails on H.Code equals D.Code
            join T in Db.PromoDetailTiers on D.Id equals T.PromoDetailId
            join S in Db.PromoSubjects on D.Code equals S.Code
            join I in Db.Items on T.FreeGoodItemId equals I.Id
            join U in Db.UoMConversions on Convert.ToInt32(T.UnitFreeGood) equals U.Id
            where H.ApplyTo == 2 && D.ApplyTo == 3 && D.PromoType == 3 && D.ItemId == itemCatId &&
                  T.SaleUnit == (T.ApplyToAllUnit ? T.SaleUnit : unitId) &&
                  (T.FromQty <= qty && qty <= (T.IsMultiple ? qty : T.ToQty)) &&
                  S.CustCode == custCode &&
                  currentDate >= H.StartDate && currentDate <= H.EndDate
            select new PromotionFreeGoodModel
            {
                PromoCode = H.Code,
                ItemId = (int)T.FreeGoodItemId,
                ItemName = I.Name,
                UomId = (int)I.UomId,
                UnitId = int.Parse(T.UnitFreeGood),
                UnitName = U.UnitEquivalent,
                Qty = T.IsMultiple ? T.Value * Math.Round(10 / T.FromQty, 0) : T.Value,
                UnitPrice = (decimal)I.SellPrice
            }).AsEnumerable();

        var data5 = (from H in Db.PromoHeaders
            join D in Db.PromoDetails on H.Code equals D.Code
            join T in Db.PromoDetailTiers on D.Id equals T.PromoDetailId
            join S in Db.PromoSubjects on D.Code equals S.Code
            join I in Db.Items on T.FreeGoodItemId equals I.Id
            join U in Db.UoMConversions on Convert.ToInt32(T.UnitFreeGood) equals U.Id
            where H.ApplyTo == 3 && D.ApplyTo == 1 && D.PromoType == 3 && D.ItemId == itemId &&
                  T.SaleUnit == (T.ApplyToAllUnit ? T.SaleUnit : unitId) &&
                  (T.FromQty <= qty && qty <= (T.IsMultiple ? qty : T.ToQty)) &&
                  S.CustTypeId == custTypeId &&
                  currentDate >= H.StartDate && currentDate <= H.EndDate
            select new PromotionFreeGoodModel
            {
                PromoCode = H.Code,
                ItemId = (int)T.FreeGoodItemId,
                ItemName = I.Name,
                UomId = (int)I.UomId,
                UnitId = int.Parse(T.UnitFreeGood),
                UnitName = U.UnitEquivalent,
                Qty = T.IsMultiple ? T.Value * Math.Round(10 / T.FromQty, 0) : T.Value,
                UnitPrice = (decimal)I.SellPrice
            }).AsEnumerable();

        var data6 = (from H in Db.PromoHeaders
            join D in Db.PromoDetails on H.Code equals D.Code
            join T in Db.PromoDetailTiers on D.Id equals T.PromoDetailId
            join S in Db.PromoSubjects on D.Code equals S.Code
            join I in Db.Items on T.FreeGoodItemId equals I.Id
            join U in Db.UoMConversions on Convert.ToInt32(T.UnitFreeGood) equals U.Id
            where H.ApplyTo == 3 && D.ApplyTo == 3 && D.PromoType == 3 && D.ItemId == itemCatId &&
                  T.SaleUnit == (T.ApplyToAllUnit ? T.SaleUnit : unitId) &&
                  (T.FromQty <= qty && qty <= (T.IsMultiple ? qty : T.ToQty)) &&
                  S.CustTypeId == custTypeId &&
                  currentDate >= H.StartDate && currentDate <= H.EndDate
            select new PromotionFreeGoodModel
            {
                PromoCode = H.Code,
                ItemId = (int)T.FreeGoodItemId,
                ItemName = I.Name,
                UomId = (int)I.UomId,
                UnitId = int.Parse(T.UnitFreeGood),
                UnitName = U.UnitEquivalent,
                Qty = T.IsMultiple ? T.Value * Math.Round(10 / T.FromQty, 0) : T.Value,
                UnitPrice = (decimal)I.SellPrice
            }).AsEnumerable();

        var data = data1.Union(data2).Union(data3).Union(data4).Union(data5).Union(data6).ToList();

        return data;
    }

    public IEnumerable<PromotionDiscountModel> GetPromotionInvoice(decimal amount, string custCode)
    {
        var currentDate = DateTime.Now.Date;
        var custTypeId = Db.Customers.Where(x => x.Code.Equals(custCode)).Select(y => y.TypeId).SingleOrDefault();
        var data1 = (from H in Db.PromoHeaders
            join D in Db.PromoDetails on H.Code equals D.Code
            join T in Db.PromoDetailTiers on D.Id equals T.PromoDetailId
            where H.ApplyTo == 1 && D.ApplyTo == 2 && D.PromoType == 5 &&
                  (T.FromQty <= amount && amount <= T.ToQty) &&
                  currentDate >= H.StartDate && currentDate <= H.EndDate
            select new PromotionDiscountModel
            {
                PromoCode = H.Code,
                PromoDetailId = D.Id,
                Name = H.Name,
                IsPercentage = T.IsPercentage,
                Value = T.Value,
                IsPromoWithBudget = D.IsPromoWithBudget,
                BudgetMaximumValue = D.BudgetMaximumValue,
                OverBudgetAction = D.OverBudgetAction,
            }).AsEnumerable();

        var data2 = (from H in Db.PromoHeaders
            join D in Db.PromoDetails on H.Code equals D.Code
            join T in Db.PromoDetailTiers on D.Id equals T.PromoDetailId
            join S in Db.PromoSubjects on D.Code equals S.Code
            where H.ApplyTo == 2 && D.ApplyTo == 2 && D.PromoType == 5 &&
                  (T.FromQty <= amount && amount <= T.ToQty) && S.CustCode == custCode &&
                  currentDate >= H.StartDate && currentDate <= H.EndDate
            select new PromotionDiscountModel
            {
                PromoCode = H.Code,
                PromoDetailId = D.Id,
                Name = H.Name,
                IsPercentage = T.IsPercentage,
                Value = T.Value,
                IsPromoWithBudget = D.IsPromoWithBudget,
                BudgetMaximumValue = D.BudgetMaximumValue,
                OverBudgetAction = D.OverBudgetAction,
            }).AsEnumerable();

        var data3 = (from H in Db.PromoHeaders
            join D in Db.PromoDetails on H.Code equals D.Code
            join T in Db.PromoDetailTiers on D.Id equals T.PromoDetailId
            join S in Db.PromoSubjects on D.Code equals S.Code
            where H.ApplyTo == 3 && D.ApplyTo == 2 && D.PromoType == 5 &&
                  (T.FromQty <= amount && amount <= T.ToQty) && S.CustTypeId == custTypeId &&
                  currentDate >= H.StartDate && currentDate <= H.EndDate
            select new PromotionDiscountModel
            {
                PromoCode = H.Code,
                PromoDetailId = D.Id,
                Name = H.Name,
                IsPercentage = T.IsPercentage,
                Value = T.Value,
                IsPromoWithBudget = D.IsPromoWithBudget,
                BudgetMaximumValue = D.BudgetMaximumValue,
                OverBudgetAction = D.OverBudgetAction,
            }).AsEnumerable();

        var data = data1.Union(data2).Union(data3).ToList();

        return data;
    }

    public Tax GetTransactionTax(int taxId)
    {
        var data = Db.Taxes.Where(x => x.Id.Equals(taxId)).Single();

        return data;
    }

    public SaveResult InsertOrderCustomer(MobileOrderHeader header, IEnumerable<VisitOrderDetailRequest> details)
    {
        var result = new SaveResult(false);

        using var transaction = Db.Database.BeginTransaction();
        try
        {
            var newCode = GetNewCode("MOB_SO_NUM_FMT", header.Date);
            header.Code = newCode;
            Db.MobileCustomerOrderHeaders.Add(header);

            short i = 0;
            foreach (var detail in details)
            {

                MobileOrderDetail newDetail = new()
                {
                    Code = newCode,
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

                Db.MobileCustomerOrderDetails.Add(newDetail);

                Db.SaveChanges();

                short j = 0;
                foreach (var discount in detail.Discounts)
                {

                    Db.MobileCustomerOrderDetailDiscounts.Add(new MobileOrderDetailDiscount
                    {
                        Code = newCode,
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

                    Db.MobileCustomerOrderDetailFreeGoods.Add(new MobileOrderDetailFreeGood
                    {
                        Code = newCode,
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

            Db.SaveChanges();

            transaction.Commit();
        }
        catch (Exception ex)
        {
            result.Message = ex.InnerException?.Message ?? ex.Message;
            return result;
        }

        result.Success = true;
        result.Data = header.Code;
        result.Message = "Order berhasil disimpan.";
        return result;
    }
}