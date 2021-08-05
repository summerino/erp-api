using System.Linq;
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
        private readonly IClaimService _claim;
        private readonly IAuthService _auth;

        private const int _menuId = (int)Menu.Posting;

        public JournalController(IJournalService journalService, IClaimService claimService, IAuthService authService)
        {
            _js = journalService;
            _claim = claimService;
            _auth = authService;
        }

        [HttpPost]
        public IActionResult OnPost(JournalRequest data)
        {
            if (!_auth.GetActions(_menuId, _claim.RoleId, new[] { Actions.Post }).Any())
            {
                return Ok(new SaveResult(false, AppConstant.UnAuthMessage));
            }

            var result = _js.PostingJournal(data);

            return Ok(result);
        }
    }
}
