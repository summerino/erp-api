using ERP.Entity;
using ERP.Entity.General;
using ERP.Web.API.Domain.Interfaces.Mobile.CustomerTransaction;
using ERP.Web.API.Domain.Models.Mobile.CustomerPromotion;

namespace ERP.Web.API.Domain.Services.Mobile.CustomerTransaction
{
    public class CustomerOrderService : ICustomerOrderService
    {
        protected TenantContext Db;

        public CustomerOrderService(TenantContext db)
        {
            Db = db;
        }

        public bool GetParams()
        {
            var data = Db.SystemParameters.Where(x => x.Code.Equals("DEF_SALES_TAX_INC")).Select(y=>y.Value).Single();
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
                           (T.FromQty <= qty && qty <= (T.IsMultiple?qty: T.ToQty)) &&
                           currentDate >= H.StartDate && currentDate <= H.EndDate
                         select new PromotionFreeGoodModel
                         {
                             PromoCode= H.Code,
                             ItemId= (int)T.FreeGoodItemId,
                             ItemName=I.Name,
                             UomId= (int)I.UomId,
                             UnitId=int.Parse(T.UnitFreeGood),
                             UnitName=U.UnitEquivalent,
                             Qty=T.IsMultiple?T.Value*Math.Round(10/T.FromQty,0):T.Value,
                             UnitPrice= (decimal)I.SellPrice
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

    }
}
