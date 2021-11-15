namespace ERP.Web.API.Model.Auth
{
    public class JwtConfig
    {
        public string Secret { get; set; }

        public string Issuer { get; set; }

        public int ExpiresInMinute { get; set; }

        public int MobileExpiresInMinute { get; set; }
    }
}
