using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERP.Entity.Accounting
{
    public class IncomeStatementResult
    {
        public string IsCode { get; set; }

        public string IsName { get; set; }

        public decimal? IsNowAmountIdr { get; set; }

        public decimal? IsPrevAmountIdr { get; set; }

        public decimal? IsYtdAmountIdr { get; set; }

        public int? IsBold { get; set; }

        public int? IsDeep { get; set; }

        public int? IsHasChild { get; set; }

        public int? IsPm { get; set; }

        public int? IsPercent { get; set; }


        public decimal? IsMonth1AmountIdr { get; set; }

        public decimal? IsMonth2AmountIdr { get; set; }

        public decimal? IsMonth3AmountIdr { get; set; }

        public decimal? IsMonth4AmountIdr { get; set; }

        public decimal? IsMonth5AmountIdr { get; set; }

        public decimal? IsMonth6AmountIdr { get; set; }

        public decimal? IsMonth7AmountIdr { get; set; }

        public decimal? IsMonth8AmountIdr { get; set; }

        public decimal? IsMonth9AmountIdr { get; set; }

        public decimal? IsMonth10AmountIdr { get; set; }

        public decimal? IsMonth11AmountIdr { get; set; }

        public decimal? IsMonth12AmountIdr { get; set; }


        public decimal? IsDiffAmountIdr { get; set; }

        public decimal? IsDiffPercent { get; set; }

        public decimal? IsNowYearAmountIdr { get; set; }

        public decimal? IsPrevYearAmountIdr { get; set; }

        public decimal? IsDiffYearAmountIdr { get; set; }

        public decimal? IsDiffYearPercent { get; set; }
    }

    public class BsIsDetailResult
    {
        public string CoaCode { get; set; }

        public string CoaName { get; set; }

        public decimal? AmountOc { get; set; }

        public decimal? AmountIdr { get; set; }
    }
}
