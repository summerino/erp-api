using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ERP_API.Domain.Entities;
using ERP_API.Domain.Entities.Sales;
using ERP_API.Domain.Interfaces.Sales;
using ERP_API.Domain.Models;
using ERP_API.Domain.Models.Sales;
using ERP_API.Model.Sales;

namespace ERP_API.Domain.Services.Sales
{
    public class DirectInvoiceService : GeneralService<SalesInvoiceHeader>, IDirectInvoiceService
    {
        public DirectInvoiceService(TenantContext db)
            : base(db)
        {
        }

        public DirectInvoiceHeader FindByCode(string code)
        {
            var invData = Db.VwSalesInvoiceHeaders.FirstOrDefault(x => x.Code == code && x.FromDirectInvoice);

            if (invData == null)
                return null;

            var ordData = Db.SalesOrderHeaders.FirstOrDefault(x => x.Code == invData.SoCode);

            return new DirectInvoiceHeader
            {
                Code = invData.Code,
                Date = invData.Date,
                DueDate = invData.DueDate,
                SoCode = invData.SoCode,
                CurrCode = invData.CurrCode,
                CustCode = invData.CustCode,
                IssuedBy = invData.IssuedBy,
                PaidAmount = invData.PaidAmount,
                Total = invData.Total,
                Notes = invData.Notes,
                FromDirectInvoice = invData.FromDirectInvoice,
                Mark = invData.Mark,
                CreatedBy = invData.CreatedBy,
                CreatedDate = invData.CreatedDate,
                UpdatedBy = invData.UpdatedBy,
                UpdatedDate = invData.UpdatedDate,
                ApprovedBy = invData.ApprovedBy,
                ApprovedDate = invData.ApprovedDate,
                CustName = invData.CustName,
                IssuedInitial = invData.IssuedInitial,
                CreatedInitial = invData.CreatedInitial,
                UpdatedInitial = invData.UpdatedInitial,
                ApprovedInitial = invData.ApprovedInitial,
                Status = invData.Status,
                SalesBy = ordData?.SalesBy ?? 0,
                WarehouseCode = ordData?.WarehouseCode,
                ShipmentFee = ordData?.ShipmentFee ?? 0m,
                HandlingFee = ordData?.HandlingFee ?? 0m,
                SubTotal = ordData?.SubTotal ?? 0m,
                FinalDiscPercent = ordData?.FinalDiscPercent ?? 0m,
                FinalDisc = ordData?.FinalDisc ?? 0m,
                IncludeTax = ordData?.IncludeTax ?? false,
                TaxAmount = ordData?.TaxAmount ?? 0m,
                Dpp = ordData?.Dpp ?? 0m
            };
        }
        
        public SaveResult Insert(SalesInvoiceRequest data)
        {
            throw new NotImplementedException();
        }

        public SaveResult Update(SalesInvoiceRequest data)
        {
            throw new NotImplementedException();
        }

        public SaveResult Delete(string code, int userId)
        {
            throw new NotImplementedException();
        }
    }
}
