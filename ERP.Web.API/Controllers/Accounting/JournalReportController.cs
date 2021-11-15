using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using ERP.Entity;
using ERP.Entity.Accounting;
using ERP.Web.API.Domain.Interfaces.Accounting;
using ERP.Web.API.Domain.Interfaces.SystemManagement;
using ERP.Web.API.Model;

namespace ERP.Web.API.Controllers.Accounting
{
    [Route("journal-report")]
    [ApiController]
    public class JournalReportController : ControllerBase
    {
        private readonly IJournalReportService _jr;
        private readonly IRoleService _role;
        private readonly IClaimService _claim;

        private const int MenuId = (int)Menu.JournalReport;

        public JournalReportController(IJournalReportService jr, IRoleService role, IClaimService claim)
        {
            _jr = jr;
            _role = role;
            _claim = claim;
        }

        [HttpPost("lists")]
        public List<JournalReportWrapper> Listing(JournalReportModel data, int? caller)
        {
            var result = new List<JournalReportWrapper>();

            // Checking role authorization
            if (!((IEnumerable<int>)_role.GetRoleMenu(_claim.RoleId, caller ?? MenuId)).Any())
                return result;

            // Get lists
            var records =
                _jr.GetLists(data.RptBy, data.DateFrom, data.DateTo,
                    data.VouFrom, data.RptDet, data.Src, data.CoaCode, data.Sort).ToList();

            if (!records.Any())
                return result;

            string lastCode = null;
            decimal subTotDebet = 0, subTotCredit = 0, totDebet = 0, totCredit = 0;

            foreach (var item in records)
            {
                if (item.Code != lastCode)
                {
                    if (result.Any())
                    {
                        result.Add(new JournalReportWrapper
                        {
                            AccName = "Sub Total",
                            DebetOc = subTotDebet.ToString(CultureInfo.InvariantCulture),
                            CreditOc = subTotCredit.ToString(CultureInfo.InvariantCulture),
                            IsBold = 1
                        });

                        result.Add(new JournalReportWrapper());
                    }

                    result.Add(new JournalReportWrapper
                    {
                        AccCode = item.Date.ToString("dd-MMM-yyyy"),
                        AccName = item.Code,
                        IsBold = 1
                    });

                    subTotDebet = 0;
                    subTotCredit = 0;
                    lastCode = item.Code;
                }

                result.Add(new JournalReportWrapper
                {
                    AccCode = item.CoaCode,
                    AccName = item.CoaName,
                    Notes = item.Notes,
                    RefCode1 = item.RefCode1,
                    RefCode2 = item.RefCode2,
                    RefCode3 = item.RefCode3,
                    RefCode4 = item.RefCode4,
                    CurrCode = item.CurrCode,
                    Rate = item.Rate.ToString(CultureInfo.InvariantCulture),
                    DebetOc = item.DebetOc.ToString(CultureInfo.InvariantCulture),
                    CreditOc = item.CreditOc.ToString(CultureInfo.InvariantCulture)
                });

                subTotDebet += item.DebetOc;
                subTotCredit += item.CreditOc;
                totDebet += item.DebetOc;
                totCredit += item.CreditOc;
            }

            result.Add(new JournalReportWrapper
            {
                AccName = "Sub Total",
                DebetOc = subTotDebet.ToString(CultureInfo.InvariantCulture),
                CreditOc = subTotCredit.ToString(CultureInfo.InvariantCulture),
                IsBold = 1
            });

            result.Add(new JournalReportWrapper());

            result.Add(new JournalReportWrapper
            {
                AccName = "Total",
                DebetOc = totDebet.ToString(CultureInfo.InvariantCulture),
                CreditOc = totCredit.ToString(CultureInfo.InvariantCulture),
                IsBold = 1
            });

            return result;
        }
    }
}
