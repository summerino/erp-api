namespace ERP.Entity.Accounting;

public class BalanceSheetResult
{
    public string Code { get; set; }

    public string Name { get; set; }

    public decimal Amount { get; set; }

    public int? Deep { get; set; }

    public bool IsBold { get; set; }
}