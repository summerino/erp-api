using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.SystemManagement;
using ERP.Web.API.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERP.Web.API.Controllers.SystemManagement
{
    [Route("journal-state")]
    [ApiController]
    public class JournalStateController : ControllerBase
    {
        private readonly IJournalStateService _js;
        private readonly IClaimService _claim;
        public JournalStateController(IJournalStateService js, IClaimService claim)
        {
            _js = js;
            _claim = claim;
        }

        [HttpGet]
        public IActionResult GetData()
        {
            var data = _js.GetStatusPost(_claim.UserId);
            return Ok(data);
        }
    }
}
