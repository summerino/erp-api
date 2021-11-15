namespace ERP.Web.API.Model.Auth
{
    public class JwtConfig
    {
        public string Secret { get; set; }
        public int TimeInMinute { get; set; }
        public string Issuer { get; set; }
    }
}
