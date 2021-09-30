using System.Linq;
using Microsoft.AspNetCore.Mvc;
using ERP.Common;
using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.Accounting;
using ERP.Web.API.Domain.Interfaces.Auth;
using ERP.Web.API.Model;
using ERP.Web.API.Model.Accounting;
using System.Collections.Generic;

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

        private const int MenuId = (int)Menu.Posting;

        public JournalController(IJournalService journalService, IClosingMonthService cm,
            IClaimService claimService, IAuthService authService)
        {
            _js = journalService;
            _cm = cm;
            _claim = claimService;
            _auth = authService;
        }

        [HttpPost]
        public IActionResult OnPost(JournalRequest data)
        {
            if (!_auth.GetActions(MenuId, _claim.RoleId, new[] { Actions.Post }).Any())
            {
                return Ok(new SaveResult(false, AppConstant.UnAuthMessage));
            }

            if (_cm.IsMonthClosed(new List<string> { data.Date.ToString("yyyyMM") }))
                return Ok(new SaveResult(false, "Periode sudah ditutup. Silakan hubungi departemen akuntansi."));

            var result = _js.PostingJournal(data, _claim.UserId);

            return Ok(result);
        }
    }
}
