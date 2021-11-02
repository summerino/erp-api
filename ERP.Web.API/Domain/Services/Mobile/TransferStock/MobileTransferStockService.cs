using ERP.Common;
using ERP.Entity;
using ERP.Entity.MobileWarehouse;
using ERP.Web.API.Domain.Interfaces.Mobile.TransferStock;
using ERP.Web.API.Domain.Models.Mobile.TransferStock;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERP.Web.API.Domain.Services.Mobile.TransferStock
{
    public class MobileTransferStockService : GeneralService<MobileTransferStockHeader>, IMobileTransferStockService
    {
        public MobileTransferStockService(TenantContext db) : base(db)
        {
        }

        public IEnumerable<MobileTransferStockHeaderModel> getMobileTranferStockHeader(DateTime? date, string search)
        {
            var data = from th in Db.VwTransferStockHeaders
                       where th.Type == "IN" || th.Type == "OUT"
                       select new MobileTransferStockHeaderModel
                       {
                           Code = th.Code,
                           Date = th.Date,
                           Type = th.Type,
                           WarehouseCodeFrom = th.WarehouseCodeFrom,
                           WarehouseCodeTo = th.WarehouseCodeTo,
                           WarehouseInitialFrom = th.WarehouseInitialFrom,
                           WarehouseInitialTo = th.WarehouseInitialTo,
                           TypeInitial = th.TypeInitial

                       };

            if (date.HasValue)
            {
                data = data.Where(x => x.Date.Equals(date));
            }

            if (search != "")
            {
                data = data.Where(x => x.WarehouseInitialFrom.Contains(search) || x.WarehouseInitialTo.Contains(search) || x.Code.Contains(search));
            }

            return data;
        }

        public IEnumerable<MobileTransferStockDetailModel> getMobileTransferStockDetail(string code)
        {
            var data = from td in Db.VwTransferStockDetails
                       where td.Code == code
                       select new MobileTransferStockDetailModel
                       {
                           ItemId = td.ItemId,
                           UomId = td.UomId,
                           UnitId = td.UnitId,
                           ItemName = td.ItemName,
                           UnitName = td.UnitName,
                           OriginalQty = td.Qty,
                           RealizeQty = 0
                       };
            return data;
        }

        public IEnumerable<MobileTransferStockHeaderModel> getTranferStockHeader(DateTime? date, string search)
        {
            var data = from thm in Db.MobileTransferStockHeaders
                       join th in Db.VwTransferStockHeaders on thm.TransferCode equals th.Code
                       select new MobileTransferStockHeaderModel
                       {
                           Code = thm.Code,
                           Date = th.Date,
                           Type = th.Type,
                           WarehouseCodeFrom = th.WarehouseCodeFrom,
                           WarehouseCodeTo = th.WarehouseCodeTo,
                           WarehouseInitialFrom = th.WarehouseInitialFrom,
                           WarehouseInitialTo = th.WarehouseInitialTo,
                           TypeInitial = th.TypeInitial

                       };

            if (date.HasValue)
            {
                data = data.Where(x => x.Date.Equals(date));
            }

            if (search != "")
            {
                data = data.Where(x => x.WarehouseInitialFrom.Contains(search) || x.WarehouseInitialTo.Contains(search) || x.Code.Contains(search));
            }

            return data;
        }

        public IEnumerable<MobileTransferStockDetailModel> getTransferStockDetail(string code)
        {
            var data = from td in Db.VwMobileTransferStockDetails
                       where td.Code == code
                       select new MobileTransferStockDetailModel
                       {
                           ItemId = td.ItemId,
                           UomId = td.UomId,
                           UnitId = td.UnitId,
                           ItemName = td.ItemName,
                           UnitName = td.UnitName,
                           OriginalQty = td.OriginalQty,
                           RealizeQty = td.RealizeQty
                       };
            return data;
        }

        public SaveResult Insert(TransferStockRequestModel data, int userId)
        {
            var result = new SaveResult(false);

            using var transaction = Db.Database.BeginTransaction();
            try
            {
                // Get new code
                var newCode = GetNewCode("MOB_TS_NUM_FMT", data.Date);

                // Insert header data
                var date = DateTime.Now;
                Db.MobileTransferStockHeaders.Add(new MobileTransferStockHeader
                {
                    Code = newCode,
                    TransferCode = data.Code,
                    Mark = "A",
                    CreatedBy = userId,
                    CreatedDate = date,
                    UpdatedBy = userId,
                    UpdatedDate = date,

                });

                // Insert detail data
                short i = 0;
                foreach (var item in data.Details)
                {
                    Db.MobileTransferStockDetails.Add(new MobileTransferStockDetail
                    {
                        Code = newCode,
                        LineNo = ++i,
                        ItemId = item.ItemId,
                        UnitId = item.UnitId,
                        UomId = item.UomId,
                        OriginalQty = item.OriginalQty,
                        RealizeQty = item.RealizeQty
                    });
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
            result.Message = "Transfer Stok berhasil disimpan.";
            return result;
        }
    }
}
