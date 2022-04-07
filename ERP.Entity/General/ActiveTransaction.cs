using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERP.Entity.General;

public class ActiveTransactionGetDataRequest
{
    public string Code { get; set; }

    public string SrcName { get; set; }

    public string Src { get; set; }

    public string UserName { get; set; }

    public DateTime ViewedDate { get; set; }
}