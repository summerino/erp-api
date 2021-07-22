using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using ERP.Common;
using ERP.Common.Models;
using Microsoft.AspNetCore.Mvc;
using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.Auth;
using ERP.Web.API.Domain.Interfaces.General;
using ERP.Web.API.Domain.Interfaces.Sales;
using ERP.Web.API.Model;
using ERP.Web.API.Model.General;
using Newtonsoft.Json;

namespace ERP.Web.API.Controllers.General
{
    [Route("[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeService _employee;
        private readonly ISalesmanService _salesman;
        private readonly IClaimService _claim;
        private readonly IAuthService _auth;

        private const int _menuId = (int)Menu.Employee;

        public EmployeeController(IEmployeeService employee, ISalesmanService salesman,
            IClaimService claim, IAuthService auth)
        {
            _employee = employee;
            _salesman = salesman;
            _claim = claim;
            _auth = auth;
        }

        [HttpGet]
        public IActionResult GetData(string search, string filters, string sorts, int skip, int take)
        {
            var data =
                _employee.GetData(
                    skip, take,
                    JsonConvert.DeserializeObject<List<Filter>>(!string.IsNullOrWhiteSpace(filters) ? filters : "[]"),
                    JsonConvert.DeserializeObject<List<Sort>>(!string.IsNullOrWhiteSpace(sorts) ? sorts : "[]"),
                    search);

            return Ok(new ApiResponse
            {
                RowCount = data.Total,
                TableData = data.Data.ToDynamicList()
            });
        }

        [HttpGet("lists")]
        public IActionResult GetList(string filters, string sorts) 
        {
            var data =
                _employee.GetLists(
                    JsonConvert.DeserializeObject<List<Filter>>(!string.IsNullOrWhiteSpace(filters) ? filters : "[]"),
                    JsonConvert.DeserializeObject<List<Sort>>(!string.IsNullOrWhiteSpace(sorts) ? sorts : "[]")).Data
                    .ToDynamicList()
                    .Select(x => new
                    {
                        x.Id, x.Initial, x.FirstName, x.WarehouseCode
                    })
                    .ToList<dynamic>();

            return Ok(new ApiResponse
            {
                RowCount = data.Count,
                TableData = data
            });
        }

        [HttpGet("un-lists")]
        public IActionResult GetUnList(long? id)
        {
            var data = _employee.GetUnUsedList(id)
                .Select(x => new
                {
                    x.Id,
                    x.Initial,
                    x.FirstName
                })
                .ToList<dynamic>();

            return Ok(new ApiResponse
            {
                RowCount = data.Count,
                TableData = data
            });
        }

        [HttpGet("salesman-schedule")]
        public IActionResult GetEmployeeSchedule(string groupId, string startDate, string recurrence, string visitDay)
        {
            var data =
                _salesman.GetSalesmanSchedule(groupId, startDate, recurrence, visitDay).ToList<dynamic>();

            return Ok(new ApiResponse
            {
                RowCount = data.Count,
                TableData = data
            });
        }

        [HttpGet("salesman-schedule-by-id")]
        public IActionResult GetEmployeeSchedule(long id)
        {
            var data =
                _salesman.GetSalesmanSchedule(id).ToList<dynamic>();

            return Ok(new ApiResponse
            {
                RowCount = data.Count,
                TableData = data
            });
        }

        [HttpGet("salesman-schedule-customer")]
        public IActionResult GetEmployeeScheduleDetailData(string ids)
        {
            var data = _salesman.GetSalesmanScheduleDetailData(JsonConvert.DeserializeObject<List<long>>(!string.IsNullOrWhiteSpace(ids) ? ids : "[]")).ToList<dynamic>();

            return Ok(new ApiResponse
            {
                RowCount = data.Count,
                TableData = data
            });
        }

        [HttpPost]
        public IActionResult OnPost(EmployeeRequest data)
        {
            // Checking role authorization
            if (!_auth.GetActions(_menuId, _claim.RoleId, new Actions[] { Actions.Insert }).Any())
                return Ok(new SaveResult(false, AppConstant.UnAuthMessage));

            data.IsActive = true;
            data.CreatedBy = _claim.UserId;
            data.CreatedDate = DateTime.Now;
            data.UpdatedBy = data.CreatedBy;
            data.UpdatedDate = data.CreatedDate;

            var result = _employee.Insert(data);

            return Ok(result);
        }

        [HttpPut("{id}")]
        public IActionResult OnPut(string id, EmployeeRequest data)
        {
            // Checking role authorization
            if (!_auth.GetActions(_menuId, _claim.RoleId, new Actions[] { Actions.Update }).Any())
                return Ok(new SaveResult(false, AppConstant.UnAuthMessage));

            data.UpdatedBy = _claim.UserId;
            data.UpdatedDate = DateTime.Now;

            var result = _employee.Update(data);

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public IActionResult OnDelete(long id)
        {
            // Checking role authorization
            if (!_auth.GetActions(_menuId, _claim.RoleId, new Actions[] { Actions.Delete }).Any())
                return Ok(new SaveResult(false, AppConstant.UnAuthMessage));

            var result = _employee.Delete(id, _claim.UserId);

            return Ok(result);
        }
    }
}
