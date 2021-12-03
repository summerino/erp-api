using ERP.Common;
using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.Mobile.CustomerTransaction;
using Microsoft.EntityFrameworkCore;

namespace ERP.Web.API.Domain.Services.Mobile.CustomerTransaction
{
    public class CustomerFirebaseTokenService : ICustomerFirebaseTokenService
    {
        //private readonly CatalogContext _catalogCtx;
        //private readonly IClaimService _claim;
        protected TenantContext Db;

        public CustomerFirebaseTokenService(
            //CatalogContext catalogCtx,
            //IClaimService claim,
            TenantContext db
            )
        {
            //_catalogCtx = catalogCtx;
            //_claim = claim;
            Db = db;
        }

        public SaveResult AddCustomerFirebaseToken(string firebaseTokenId, string userCode)
        {
            //var username = Db.Customers.Where(x => x.Code.Equals(userCode)).Select(y => y.MobileUsername).SingleOrDefault()!;
            var result = new SaveResult(false);

            using var transaction = Db.Database.BeginTransaction();
            try
            {
                //var catalogUser = _catalogCtx.CustomerUsers.FirstOrDefault(x => x.Username == username);
                //var tenant = _catalogCtx.Tenants.FirstOrDefault(x => x.Id == catalogUser.TenantId);

                //// Configure tenant context db
                //var contextOptions = new DbContextOptionsBuilder<TenantContext>()
                //    .UseSqlServer($"Server={tenant.ServerName};Database={tenant.DatabaseName};User Id={tenant.ServerUserId};Password={tenant.ServerPassword}")
                //    .Options;
                //var tenantCtx = new TenantContext(contextOptions, _catalogCtx, _claim);
                //var tenantCustomer = tenantCtx.Customers.FirstOrDefault(x => x.CatalogUserId == catalogUser.Id && x.MobileSignIn && x.IsActive);

                // direct tenant
                var tenantCustomer = Db.Customers.FirstOrDefault(x => x.Code == userCode && x.MobileSignIn && x.IsActive);
                if (tenantCustomer == null)
                {
                    result.Success = false;
                }

                //post token
                tenantCustomer.FirebaseTokenId = firebaseTokenId;

                Db.SaveChanges();

                transaction.Commit();
            }
            catch (Exception ex)
            {
                result.Message = ex.InnerException?.Message ?? ex.Message;
                return result;
            }

            result.Success = true;
            result.Data = firebaseTokenId;
            result.Message = "Token berhasil disimpan.";
            return result;
        }
    }
}
