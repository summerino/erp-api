using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using ERP.Common;
using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.Accounting;
using ERP.Web.API.Domain.Interfaces.Auth;
using ERP.Web.API.Model;
using ERP.Web.API.Model.Accounting;

namespace ERP.Web.API.Controllers.Accounting
{
    [Route("[controller]")]
    [ApiController]
    public class JournalController : ControllerBase
    {
        private readonly IJournalService _js;
        private readonly IClosingMonthService _cm;
        private readonly IClaimService _claim;
        private readonly IAuthService _auth;
        private readonly IServiceScopeFactory _scopeFactory;

        private const int MenuId = (int)Menu.Posting;

        public JournalController(IJournalService journal, IClosingMonthService cm,
            IClaimService claim, IAuthService auth, IServiceScopeFactory scopeFactory)
        {
            _js = journal;
            _cm = cm;
            _claim = claim;
            _auth = auth;
            _scopeFactory = scopeFactory;
        }

        [HttpPost]
        public IActionResult OnPost(JournalRequest data)
        {
            if (!_auth.GetActions(MenuId, _claim.RoleId, new[] { Actions.Post }).Any())
                return Ok(new SaveResult(false, AppConstant.UnAuthMessage));

            if (_cm.IsMonthClosed(new List<string> { data.Date.ToString("yyyyMM") }))
                return Ok(new SaveResult(false, "Periode sudah ditutup. Silakan hubungi departemen akuntansi."));

            if (!_js.CheckPrevPeriod(data.Date))
                return Ok(new SaveResult(false, "Tidak dapat melakukan posting karena terdapat periode sebelumnya yang belum dipost."));

            var userId = _claim.UserId;
            var tenantId = _claim.TenantId;

            // Fire and forget
            Task.Run(() =>
            {
                using var scope = _scopeFactory.CreateScope();
                var repo = scope.ServiceProvider.GetRequiredService<IJournalService>();
                repo.PostingJournal(data, userId, tenantId);
            });

            return Ok();
        }

        [HttpPost("lists")]
        public IActionResult OnGet(JournalRequest data)
        {
            var result = _js.GetPostingHistory(data)
                .Select(x => new
                {
                    Period = new DateTime(Convert.ToInt32(x.Period[..4]), Convert.ToInt32(x.Period[4..]) > 9 ? Convert.ToInt32(x.Period[4..]) : Convert.ToInt32(x.Period[5..]), 1).ToString("MMM", CultureInfo.CreateSpecificCulture("id-ID")) 
                    + $"- {x.Period[..4]}",
                    x.PostedDate,
                    x.IsPosted
                }).ToList<dynamic>();

            return Ok(new ApiResponse
            {
                RowCount = result.Count,
                TableData = result
            });
        }
    }
}
