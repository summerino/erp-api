using Microsoft.AspNetCore.Mvc;
using ERP.Entity;
using ERP.Entity.Accounting;
using ERP.Web.API.Domain.Interfaces.Accounting;
using ERP.Web.API.Domain.Interfaces.SystemManagement;
using ERP.Web.API.Model;
using Newtonsoft.Json;
using System.Threading;
using ERP.Common.Models;

namespace ERP.Web.API.Controllers.Accounting;

[Route("gl-report")]
[ApiController]
public class GeneralLedgerReportController : ControllerBase
{
    private readonly IGeneralLedgerReportService _gl;
    private readonly IRoleService _role;
    private readonly IClaimService _claim;

    private const int MenuId = (int)Menu.GeneralLedgerReport;

    public GeneralLedgerReportController(IGeneralLedgerReportService gl, IRoleService role, IClaimService claim)
    {
        _gl = gl;
        _role = role;
        _claim = claim;
    }

    [HttpPost("lists")]
    public List<JournalReportWrapper> Listing(JournalReportModel data, int? caller)
    {
        var result = new List<JournalReportWrapper>();

        if (!((IEnumerable<int>)_role.GetRoleMenu(_claim.RoleId, caller ?? MenuId)).Any())
            return result;

        var details = _gl.GetGeneralLedgerLists(data.DateFrom, data.DateTo, data.Acc, data.Acc2,
            data.Curr, data.Sort, caller);

        if (!details.Any())
            return result;

        string code = "", lastCode = "";
        string newCode = "", newNotes = "";
        decimal? totalMutOc = 0;
        decimal? totalDebetOc = 0, totalCreditOc = 0, totalEndBalOc = 0;

        foreach (var item in details)
        {
            code = item.CoaCode;

            if (item.Sort == "0")
            {
                if (lastCode != "")
                    result.Add(new JournalReportWrapper());

                result.Add(new JournalReportWrapper
                {
                    AccName = item.CoaCode,
                    Notes = item.CoaName,
                    IsBold = 1
                });

                lastCode = code;
                totalMutOc = 0;
            }

            switch (item.Sort)
            {
                case "0":
                case "1":
                case "2":
                    newCode = item.Sort == "0" ? item.CoaCode : item.Code;
                    newNotes = item.Sort == "0" ? item.CoaName : item.Notes;
                    totalMutOc = totalMutOc + (item.EndBalOc ?? 0) + (item.DebetOc ?? 0) - (item.CreditOc ?? 0);
                    break;

                case "3":
                    newCode = item.Code;
                    newNotes = item.Notes;
                    totalMutOc = (item.EndBalOc ?? 0);

                    totalDebetOc = totalDebetOc + item.DebetOc;
                    totalCreditOc = totalCreditOc + item.CreditOc;
                    totalEndBalOc = totalEndBalOc + item.EndBalOc;

                    break;
            }

            result.Add(new JournalReportWrapper
            {
                AccCode = item.Date?.ToString("dd-MMM-yyyy"),
                AccName = item.Code,
                Notes = item.Notes,
                RefCode1 = item.RefCode1,
                RefCode2 = item.RefCode2,
                RefCode3 = item.RefCode3,
                RefCode4 = item.RefCode4,
                CurrCode = item.CurrCode,
                Rate = item.Rate?.ToString(),
                DebetOc = item.DebetOc?.ToString(),
                CreditOc = item.CreditOc?.ToString(),
                EndBalOc = totalMutOc?.ToString(),
                IsBold = (byte)(item.IsBold == "1" ? 1 : 0),
                Date = item.Date
            });
        }

        result.Add(new JournalReportWrapper());

        result.Add(new JournalReportWrapper
        {
            Notes = "Total",
            DebetOc = totalDebetOc?.ToString(),
            CreditOc = totalCreditOc?.ToString(),
            EndBalOc = totalEndBalOc?.ToString(),
            IsBold = 1
        });

        return result;
    }

    [HttpGet("excel")]
    public async Task<IActionResult> GetDataExcel(string data, int? caller,string search, string filters, string sorts, bool isMain, string title)
    {
        var serializeData = JsonConvert.DeserializeObject<JournalReportModel>(!string.IsNullOrWhiteSpace(data) ? data : "[]");
        var result = new List<JournalReportWrapper>();

        //if (!((IEnumerable<int>)_role.GetRoleMenu(_claim.RoleId, caller ?? MenuId)).Any())
        //    return result;

        var details = _gl.GetGeneralLedgerLists(serializeData.DateFrom, serializeData.DateTo, serializeData.Acc, serializeData.Acc2,
            serializeData.Curr, serializeData.Sort, caller);

        //if (!details.Any())
        //    return result;

        string code = "", lastCode = "";
        string newCode = "", newNotes = "";
        decimal? totalMutOc = 0;
        decimal? totalDebetOc = 0, totalCreditOc = 0, totalEndBalOc = 0;

        foreach (var item in details)
        {
            code = item.CoaCode;

            if (item.Sort == "0")
            {
                if (lastCode != "")
                    result.Add(new JournalReportWrapper());

                result.Add(new JournalReportWrapper
                {
                    AccName = item.CoaCode,
                    Notes = item.CoaName,
                    IsBold = 1
                });

                lastCode = code;
                totalMutOc = 0;
            }

            switch (item.Sort)
            {
                case "0":
                case "1":
                case "2":
                    newCode = item.Sort == "0" ? item.CoaCode : item.Code;
                    newNotes = item.Sort == "0" ? item.CoaName : item.Notes;
                    totalMutOc = totalMutOc + (item.EndBalOc ?? 0) + (item.DebetOc ?? 0) - (item.CreditOc ?? 0);
                    break;

                case "3":
                    newCode = item.Code;
                    newNotes = item.Notes;
                    totalMutOc = (item.EndBalOc ?? 0);

                    totalDebetOc = totalDebetOc + item.DebetOc;
                    totalCreditOc = totalCreditOc + item.CreditOc;
                    totalEndBalOc = totalEndBalOc + item.EndBalOc;

                    break;
            }

            result.Add(new JournalReportWrapper
            {
                AccCode = item.Date?.ToString("dd-MMM-yyyy"),
                AccName = item.Code,
                Notes = item.Notes,
                RefCode1 = item.RefCode1,
                RefCode2 = item.RefCode2,
                RefCode3 = item.RefCode3,
                RefCode4 = item.RefCode4,
                CurrCode = item.CurrCode,
                Rate = item.Rate?.ToString(),
                DebetOc = item.DebetOc?.ToString(),
                CreditOc = item.CreditOc?.ToString(),
                EndBalOc = totalMutOc?.ToString(),
                IsBold = (byte)(item.IsBold == "1" ? 1 : 0),
                Date = item.Date
            });
        }

        result.Add(new JournalReportWrapper());

        result.Add(new JournalReportWrapper
        {
            Notes = "Total",
            DebetOc = totalDebetOc?.ToString(),
            CreditOc = totalCreditOc?.ToString(),
            EndBalOc = totalEndBalOc?.ToString(),
            IsBold = 1
        });

        var stream = await _gl.GetDataExcel(
            JsonConvert.DeserializeObject<List<Filter>>(!string.IsNullOrWhiteSpace(filters) ? filters : "[]"),
            JsonConvert.DeserializeObject<List<Sort>>(!string.IsNullOrWhiteSpace(sorts) ? sorts : "[]"),
            search,
            result,
            isMain,
            title
        );

        return new FileStreamResult(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
    }
}