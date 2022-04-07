namespace ERP.Web.API.Model.Auth;

public class AuthResult
{
    public string AccessToken { get; set; }

    public long ExpToken { get; set; }

    public string UserData { get; set; }

    public bool Success { get; set; }

    public string Message { get; set; }
}

public class MobileAuthResult : MobileSimpleResponse
{
    public string AccessToken { get; set; }

    public long ExpToken { get; set; }

    public string UserData { get; set; }
}