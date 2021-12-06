using ERP.Common;

namespace ERP.Web.API.Domain.Interfaces.Mobile.CustomerTransaction
{
    public interface ICustomerFirebaseTokenService
    {
        SaveResult AddCustomerFirebaseToken(string firebaseTokenId, string userCode);
    }
}
