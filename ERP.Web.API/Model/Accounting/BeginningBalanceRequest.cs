using ERP.Entity.Accounting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERP.Web.API.Model.Accounting
{
    public class BeginningBalanceAPRequest : BeginningBalanceAP
    {
        public DateTime? OriginalDate { get; set; }
    }

    public class BeginningBalanceARRequest : BeginningBalanceAR
    {
        public DateTime? OriginalDate { get; set; }
    }

    public class BeginningBalanceCreditMemoRequest : BeginningBalanceCreditMemo
    {
        public DateTime? OriginalDate { get; set; }
    }

    public class BeginningBalanceDebitMemoRequest : BeginningBalanceDebitMemo
    {
        public DateTime? OriginalDate { get; set; }
    }

    public class UploadBBAPRequest
    {
        public string Catatan { get; set; }

        public string Kode { get; set; }

        public string Kodepemasok { get; set; }

        public decimal Nilai { get; set; }

        public int No { get; set; }

        public DateTime? Tanggal { get; set; }

        public DateTime? Tgljatuhtempo { get; set; }

        public bool Mark { get; set; }
    }

    public class UploadBBARRequest
    {
        public string Catatan { get; set; }

        public string Kode { get; set; }

        public string Kodepelanggan { get; set; }

        public decimal Nilai { get; set; }

        public int No { get; set; }

        public DateTime? Tanggal { get; set; }

        public DateTime? Tgljatuhtempo { get; set; }

        public bool Mark { get; set; }
    }

    public class UploadBBCMRequest
    {
        public string Catatan { get; set; }

        public string Kode { get; set; }

        public string Kodepelanggan { get; set; }

        public decimal Nilai { get; set; }

        public int No { get; set; }

        public DateTime? Tanggal { get; set; }

        public string Tipe { get; set; }

        public bool Mark { get; set; }
    }

    public class UploadBBDMRequest
    {
        public string Catatan { get; set; }

        public string Kode { get; set; }

        public string Kodepemasok { get; set; }

        public decimal Nilai { get; set; }

        public int No { get; set; }

        public DateTime? Tanggal { get; set; }

        public string Tipe { get; set; }

        public bool Mark { get; set; }
    }
}
