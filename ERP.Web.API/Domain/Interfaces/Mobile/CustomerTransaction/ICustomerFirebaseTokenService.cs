using ERP.Common;
using ERP.Web.API.Domain.Models.Mobile.General;

namespace ERP.Web.API.Domain.Interfaces.Mobile.CustomerTransaction;

public interface ICustomerFirebaseTokenService
{
    SaveResult AddCustomerFirebaseToken(FirebaseTokenModel firebaseTokenId, string userCode);
}