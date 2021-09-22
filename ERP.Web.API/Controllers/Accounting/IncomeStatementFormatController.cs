using ERP.Web.API.Domain.Interfaces.Accounting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERP.Web.API.Controllers.Accounting
{
    [Route("is-format")]
    [ApiController]
    public class IncomeStatementFormatController : ControllerBase
    {
        private readonly IIncomeStatementFormatService _isf;
        public IncomeStatementFormatController(IIncomeStatementFormatService isf)
        {
            _isf = isf;
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
