using ERP.Common;
using ERP.Entity;
using ERP.Entity.Accounting;
using ERP.Web.API.Domain.Interfaces.Accounting;
using ERP.Web.API.Domain.Interfaces.Auth;
using ERP.Web.API.Model;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.API.Controllers.Accounting
{
    [Route("is-format")]
    [ApiController]
    public class IncomeStatementFormatController : ControllerBase
    {
        private readonly IIncomeStatementFormatService _isf;
        private readonly IClaimService _claim;
        private readonly IAuthService _auth;
 

        private const int MenuId = (int)Menu.IncomeStatementFormat;

        public IncomeStatementFormatController(IIncomeStatementFormatService isf, IClaimService claim, IAuthService auth)
        {
            _isf = isf;
            _claim = claim;
            _auth = auth;
        }

        [HttpPost]
        public IActionResult OnPost(IncomeStatementFormat data)
        {
            if (!_auth.GetActions(MenuId, _claim.RoleId, new[] { Actions.Insert }).Any())
            {
                return Ok(new SaveResult(false, AppConstant.UnAuthMessage));
            }

            data.IsActive = true;
            data.CreatedBy = _claim.UserId;
            data.CreatedDate = DateTime.Now;
            data.UpdatedBy = data.CreatedBy;
            data.UpdatedDate = data.CreatedDate;

            var result = _isf.Insert(data);

            return Ok(result);
        }

        [HttpPut("{code}")]
        public IActionResult OnPut(string code, IncomeStatementFormat data)
        {
            if (!_auth.GetActions(MenuId, _claim.RoleId, new[] { Actions.Update }).Any())
            {
                return Ok(new SaveResult(false, AppConstant.UnAuthMessage));
            }

            data.UpdatedBy = _claim.UserId;
            data.UpdatedDate = DateTime.Now;

            var result = _isf.Update(data);

            return Ok(result);
        }

        [HttpPut("move")]
        public IActionResult OnMove(string type, IncomeStatementFormat data)
        {
            if (!_auth.GetActions(MenuId, _claim.RoleId, new[] { Actions.Update }).Any())
            {
                return Ok(new SaveResult(false, AppConstant.UnAuthMessage));
            }

            data.UpdatedBy = _claim.UserId;
            data.UpdatedDate = DateTime.Now;

            var result = _isf.Move(data, type);

            return Ok(result);
        }

        [HttpDelete("{code}")]
        public IActionResult OnDelete(string code)
        {
            if (!_auth.GetActions(MenuId, _claim.RoleId, new[] { Actions.Delete }).Any())
            {
                return Ok(new SaveResult(false, AppConstant.UnAuthMessage));
            }

            var result = _isf.Delete(code, _claim.UserId);

            return Ok(result);
        }

        [HttpGet("format-sub")]
        public IActionResult GetSubFormat(string code)
        {
            return Ok(_isf.GetSubFormat(code));
        }

        [HttpGet("format-unsub")]
        public IActionResult GetUnSubFormat(string code, string category)
        {
            return Ok(_isf.GetUnSubFormat(code, category));
        }

        [HttpPut("insert-sub")]
        public IActionResult InsertSub(string subCode, string code)
        {
            if (!_auth.GetActions(MenuId, _claim.RoleId, new[] { Actions.Update }).Any())
            {
                return Ok(new SaveResult(false, AppConstant.UnAuthMessage));
            }

            var result = _isf.InsertSub(subCode, code, _claim.UserId);

            return Ok(result);
        }

        [HttpPut("remove-sub")]
        public IActionResult RemoveSub(string subCode, string code)
        {
            if (!_auth.GetActions(MenuId, _claim.RoleId, new[] { Actions.Update }).Any())
            {
                return Ok(new SaveResult(false, AppConstant.UnAuthMessage));
            }

            var result = _isf.RemoveSub(subCode, code);

            return Ok(result);
        }

        [HttpGet("format-hierarchy")]
        public IActionResult GetFormatHierarchy(string category)
        {
            return Ok(_isf.GetFormatHierarchy(category));
        }

        [HttpGet("format-lists")]
        public IActionResult GetFormatLists(string category)
        {
            return Ok(_isf.GetFormatLists(category));
        }
    }
}
