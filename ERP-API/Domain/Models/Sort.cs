namespace ERP_API.Domain.Models
{
    public class Sort
    {
        public string Field { get; set; }
        public string Direction { get; set; }

        public string ToExpression()
        {
            return Field + " " + Direction;
        }
    }
}
