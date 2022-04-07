using System;

namespace ERP.Entity.General;

public class VwApproval
{
    public string Code { get; set; }

    public DateTime Date { get; set; }

    public string Descr { get; set; }

    public string SourceTrans { get; set; }

    public DateTime UpdatedDate { get; set; }

    public int Seq { get; set; }

    public int ActionId { get; set; }
}