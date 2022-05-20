using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.Mobile.TransactionHistory;
using ERP.Web.API.Domain.Models.Mobile.TransactionHistory;
using ERP.Web.API.Model;
using Newtonsoft.Json;

namespace ERP.Web.API.Controllers.Mobile.TransactionHistory;

[Authorize(AppConstant.ValidateMobileTokenPolicy)]
[Route("mobile/[controller]")]
[ApiController]
public class TransactionHistoryController : ControllerBase
{
    private readonly ITransactionHistoryService _transactionHistory;
    private readonly IClaimService _claim;

    public TransactionHistoryController(ITransactionHistoryService transactionHistory, IClaimService claim)
    {
        _transactionHistory = transactionHistory;
        _claim = claim;
    }

    [HttpGet("by-customer")]
    public IActionResult GetDataByCustomer(string search, string filters, string sorts, int skip, int take, DateTime? startDate, DateTime? endDate)
    {
        var data =
            _transactionHistory.GetDataByCustomer(skip, take,
                JsonConvert.DeserializeObject<List<Filter>>(!string.IsNullOrWhiteSpace(filters) ? filters : "[]"),
                JsonConvert.DeserializeObject<List<Sort>>(!string.IsNullOrWhiteSpace(sorts) ? sorts : "[]"),
                startDate, endDate, search);

        var temp = ((List<TransactionHistoryByCustomer>)data.Data);

        var result = temp.Select(x => new
        {
            x.Date,
            x.CustomerId,
            x.CustomerName,
            x.Total
        }).ToList<dynamic>();
        return Ok(new MobileApiResponse
        {
            Count = data.Total,
            Data = result
        });
    }

    [HttpGet("by-product")]
    public IActionResult GetDataByProduct(string search, string filters, string sorts, int skip, int take, DateTime? startDate, DateTime? endDate)
    {
        var data =
            _transactionHistory.GetDataByProduct(skip, take,
                JsonConvert.DeserializeObject<List<Filter>>(!string.IsNullOrWhiteSpace(filters) ? filters : "[]"),
                JsonConvert.DeserializeObject<List<Sort>>(!string.IsNullOrWhiteSpace(sorts) ? sorts : "[]"),
                startDate, endDate, search);

        var temp = ((List<TransactionHistoryByProduct>)data.Data);
        var result = temp.Select(x => new
        {
            x.Date,
            x.ItemId,
            x.ItemName,
            x.Quantity,
            x.Unit,
            x.Total
        }).ToList<dynamic>();
        return Ok(new MobileApiResponse
        {
            Count = data.Total,
            Data = result
        });
    }

    [HttpGet("by-unit-product")]
    public IActionResult GetData(int filterUnit, DateTime? startDate, DateTime? endDate)
    {
        var result = _transactionHistory.GetDataByUnitProduct(filterUnit, startDate, endDate, _claim.UserId);

        return Ok(result);
    }

    [HttpGet("by-date")]
    public IActionResult GetDataByDate(string filters, string sorts, int skip, int take, DateTime? startDate, DateTime? endDate)
    {
        var data =
            _transactionHistory.GetDataByDate(skip, take,
                JsonConvert.DeserializeObject<List<Filter>>(!string.IsNullOrWhiteSpace(filters) ? filters : "[]"),
                JsonConvert.DeserializeObject<List<Sort>>(!string.IsNullOrWhiteSpace(sorts) ? sorts : "[]"),
                startDate, endDate);

        var temp = ((List<TransactionHistoryByDate>)data.Data);
        var result = temp.Select(x => new
        {
            x.Date,
            x.Total
        }).ToList<dynamic>();
        return Ok(new MobileApiResponse
        {
            Count = data.Total,
            Data = result
        });
    }

    [HttpGet("detail-item")]
    public IActionResult GetDetailItem(string search, string filters, string sorts, int skip, int take)
    {
        var data =
            _transactionHistory.GetItemDetail(skip, take,
                JsonConvert.DeserializeObject<List<Filter>>(!string.IsNullOrWhiteSpace(filters) ? filters : "[]"),
                JsonConvert.DeserializeObject<List<Sort>>(!string.IsNullOrWhiteSpace(sorts) ? sorts : "[]"),
                search);

        var temp = ((List<TransactionItemDetail>)data.Data);
        var result = temp.Select(x => new
        {
            x.CustomerId,
            x.ItemId,
            x.ItemName,
            x.Quantity,
            x.Unit,
            x.Price,
            x.Discount,
            x.Total
        }).ToList<dynamic>();
        return Ok(new MobileApiResponse
        {
            Count = data.Total,
            Data = result
        });
    }

    [HttpGet("detail-customer")]
    public IActionResult GetDetailCustomer(string search, string filters, string sorts, int skip, int take)
    {
        var data =
            _transactionHistory.GetCustomerDetail(skip, take,
                JsonConvert.DeserializeObject<List<Filter>>(!string.IsNullOrWhiteSpace(filters) ? filters : "[]"),
                JsonConvert.DeserializeObject<List<Sort>>(!string.IsNullOrWhiteSpace(sorts) ? sorts : "[]"), search);

        var temp = ((List<TransactionCustomerDetail>)data.Data);
        var result = temp.Select(x => new
        {
            x.CustomerId,
            x.CustomerName,
            x.Quantity,
            x.Unit,
            x.Total
        }).ToList<dynamic>();
        return Ok(new MobileApiResponse
        {
            Count = data.Total,
            Data = result
        });
    }

    [HttpGet("cumulative")]
    public IActionResult GetDataCumulative(int year, string custCode, string filters, string sorts, int skip, int take)
    {
        var data =
            _transactionHistory.GetDataCumulative(skip, take,
                JsonConvert.DeserializeObject<List<Filter>>(!string.IsNullOrWhiteSpace(filters) ? filters : "[]"),
                JsonConvert.DeserializeObject<List<Sort>>(!string.IsNullOrWhiteSpace(sorts) ? sorts : "[]"),
                year, custCode, _claim.UserId);

        var temp = ((List<TransactionCumulative>)data.Data);
        var result = temp.Select(x => new
        {
            x.Month,
            x.Total
        }).ToList<dynamic>();
        return Ok(new MobileApiResponse
        {
            Count = data.Total,
            Data = result
        });
    }

    [HttpGet("individual")]
    public IActionResult GetDataIndividual(string custCode, string filters, string sorts, int skip, int take)
    {
        var data =
            _transactionHistory.GetDataByLog(skip, take,
                JsonConvert.DeserializeObject<List<Filter>>(!string.IsNullOrWhiteSpace(filters) ? filters : "[]"),
                JsonConvert.DeserializeObject<List<Sort>>(!string.IsNullOrWhiteSpace(sorts) ? sorts : "[]"),
                custCode, _claim.UserId);

        var result = ((List<TransactionIndividual>)data.Data).ToList<dynamic>();

        return Ok(new MobileApiResponse
        {
            Count = data.Total,
            Data = result
        });
    }

    [HttpGet("sub-group")]
    public IActionResult GetSubGroup()
    {
        var data =
            _transactionHistory.GetSubGroup();

        return Ok(data);
    }

    [HttpGet("by-subgroup")]
    public IActionResult GetDataByGroup(string filters, string sorts, int skip, int take, DateTime? startDate, DateTime? endDate, int groupId, string subGroup)
    {
        var data =
            _transactionHistory.GetDataBySubGroup(skip, take,
                JsonConvert.DeserializeObject<List<Filter>>(!string.IsNullOrWhiteSpace(filters) ? filters : "[]"),
                JsonConvert.DeserializeObject<List<Sort>>(!string.IsNullOrWhiteSpace(sorts) ? sorts : "[]"),
                startDate, endDate, groupId, subGroup);

        var result = ((List<TransactionHistoryBySubGroup>)data.Data).ToList<dynamic>();

        return Ok(new MobileApiResponse
        {
            Count = data.Total,
            Data = result
        });
    }

    [HttpGet("item-subgroup")]
    public IActionResult GetItemByGroup(string filters, string sorts, int skip, int take, DateTime date, int groupId, string subGroup)
    {
        var data =
            _transactionHistory.GetItemBySubGroup(skip, take,
                JsonConvert.DeserializeObject<List<Filter>>(!string.IsNullOrWhiteSpace(filters) ? filters : "[]"),
                JsonConvert.DeserializeObject<List<Sort>>(!string.IsNullOrWhiteSpace(sorts) ? sorts : "[]"),
                date, groupId, subGroup);

        var result = ((List<TransactionHistoryItemBySubGroup>)data.Data).ToList<dynamic>();

        return Ok(new MobileApiResponse
        {
            Count = data.Total,
            Data = result
        });
    }

    [HttpGet("by-subgroup-summary")]
    public IActionResult GetDataByGroupSummary(string filters, string sorts, int skip, int take, DateTime date, int? groupId, int? subGroupId)
    {
        var data =
            _transactionHistory.GetDataBySubGroupSummary(skip, take,
                JsonConvert.DeserializeObject<List<Filter>>(!string.IsNullOrWhiteSpace(filters) ? filters : "[]"),
                JsonConvert.DeserializeObject<List<Sort>>(!string.IsNullOrWhiteSpace(sorts) ? sorts : "[]"),
                 date, groupId, subGroupId);

        var result = ((List<TransactionHistoryBySubGroupSummary>)data.Data).ToList<dynamic>();

        return Ok(new MobileApiResponse
        {
            Count = data.Total,
            Data = result
        });
    }

    [HttpGet("detail-subgroup-summary")]
    public IActionResult GetDataDetailByGroupSummary(string filters, string sorts, int skip, int take, DateTime date, int groupId, int subGroupId)
    {
        var data =
            _transactionHistory.GetDataDetailBySubGroupSummary(skip, take,
                JsonConvert.DeserializeObject<List<Filter>>(!string.IsNullOrWhiteSpace(filters) ? filters : "[]"),
                JsonConvert.DeserializeObject<List<Sort>>(!string.IsNullOrWhiteSpace(sorts) ? sorts : "[]"), date,
                groupId, subGroupId);

        var result = ((List<TransactionHistoryDetailBySubGroupSummary>)data.Data).ToList<dynamic>();

        return Ok(new MobileApiResponse
        {
            Count = data.Total,
            Data = result
        });
    }

    [HttpGet("item-subgroup-summary")]
    public IActionResult GetDataItemByGroupSummary(string filters, string sorts, int skip, int take, DateTime date, string detailSubGroup)
    {
        var data =
            _transactionHistory.GetDataItemBySubGroupSummary(skip, take,
                JsonConvert.DeserializeObject<List<Filter>>(!string.IsNullOrWhiteSpace(filters) ? filters : "[]"),
                JsonConvert.DeserializeObject<List<Sort>>(!string.IsNullOrWhiteSpace(sorts) ? sorts : "[]"), date,
                detailSubGroup);

        var result = ((List<TransactionHistoryItemBySubGroupSummary>)data.Data).ToList<dynamic>();

        return Ok(new MobileApiResponse
        {
            Count = data.Total,
            Data = result
        });
    }

    [HttpGet("master-item-group")]
    public IActionResult GetDataItemGroup()
    {
        var temp =
            _transactionHistory.GetItemGroup();
        var result = temp.Select(x => new
        {
            x.Id,
            x.Initial,
            x.Name,
        }).ToList<dynamic>();
        return Ok(result);
    }

    [HttpGet("master-item-sub-group")]
    public IActionResult GetDataItemSubGroup(int groupId)
    {
        var temp =
            _transactionHistory.GetItemSubGroup(groupId);

        var result = temp.Select(x => new
        {
            x.Id,
            x.Name,
        }).ToList<dynamic>();
        return Ok(result);
    }
}