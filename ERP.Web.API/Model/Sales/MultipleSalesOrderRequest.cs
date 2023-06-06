namespace ERP.Web.API.Model.Sales
{
    public class MultipleSalesOrderRequest
    {
        public List<string> SOCodes { get; set; }

        public DateTime DlvDate { get; set; }

        public bool IsSoDlv { get; set; }

        public DateTime InvDate { get; set; }

        public DateTime InvDueDate { get; set; }

        public bool IsSoInv { get; set; }
    }
}
