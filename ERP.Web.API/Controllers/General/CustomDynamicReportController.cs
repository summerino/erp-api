using ERP.Web.API.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.API.Controllers
{
	[Route("custom-dynamic-report")]
	[ApiController]
	public class CustomDynamicReportController : ControllerBase
	{
		private readonly ICustomDynamicReportService _service;
		public CustomDynamicReportController(ICustomDynamicReportService service)
		{
			_service = service;
		}

		[HttpGet]
		public IActionResult GetData(int? id, string param1, string param2,
			string param3, string param4, string param5)
		{
			var result = _service.GetDataReport(id, param1,
				param2, param3, param4, param5);

			return Ok(result);
		}

		[HttpGet("source")]
		public IActionResult GetDataSource(string source)
		{
			var result = _service.GetDataSource(source);

			return Ok(result);
		}
	}
}
