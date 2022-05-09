using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Entity.MobileSales;
using ERP.Web.API.Domain.Interfaces.General;
using ERP.Web.API.Domain.Interfaces.MobileSales;
using ERP.Web.API.Domain.Interfaces.Sales;
using ERP.Web.API.Model;
using Newtonsoft.Json;

namespace ERP.Web.API.Controllers.Mobile.General;

[Authorize(AppConstant.ValidateMobileTokenPolicy)]
[Route("mobile/[controller]")]
[ApiController]
public class CustomerController : ControllerBase
{
    private readonly ICustomerService _customer;
    private readonly IMobileCustomerService _mobCust;
    private readonly ICustomerTypeService _type;
    private readonly IAreaService _area;
    private readonly IClaimService _claim;

    public CustomerController(ICustomerService customer, IMobileCustomerService mobCust,
        ICustomerTypeService type, IAreaService area, IClaimService claim)
    {
        _customer = customer;
        _mobCust = mobCust;
        _type = type;
        _area = area;
        _claim = claim;
    }

    [HttpGet]
    public IActionResult GetData(string search, string filters, string sorts, int skip, int take, string lastUpdate)
    {
        var data =
            _customer.GetData(
                skip, take,
                JsonConvert.DeserializeObject<List<Filter>>(!string.IsNullOrWhiteSpace(filters) ? filters : "[]"),
                JsonConvert.DeserializeObject<List<Sort>>(!string.IsNullOrWhiteSpace(sorts) ? sorts : "[]"),
                search, lastUpdate);

        var result = data.Data.ToDynamicList().Select(x => new
        {
            x.Code,
            x.Initial,
            x.Name,
            x.TypeId,
            x.TypeName,
            x.CreditLimit,
            Used = x.CreditUsed,
            Remaining = x.CreditLimit - x.CreditUsed,
            x.AreaId1,
            x.AreaId2,
            x.AreaId3,
            x.AreaId4,
            x.AreaId5,
            x.AreaName1,
            x.AreaName2,
            x.AreaName3,
            x.AreaName4,
            x.AreaName5,
            x.Lat,
            x.Lng,
            x.InitialAddress,
            x.Address1,
            x.Address2,
            x.Phone,
            x.Fax,
            x.ContactPerson,
            x.IsActive,
            x.UpdatedDate
        }).ToList<dynamic>();

        return Ok(new MobileApiResponse
        {
            Count = data.Total,
            Data = result.ToDynamicList()
        });
    }

    [HttpGet("mobile")]
    public IActionResult GetDataMobileCustomer(string search, string filters, string sorts, int skip, int take, string lastUpdate)
    {
        var data =
            _customer.GetMobileCustomer(
                skip, take,
                JsonConvert.DeserializeObject<List<Filter>>(!string.IsNullOrWhiteSpace(filters) ? filters : "[]"),
                JsonConvert.DeserializeObject<List<Sort>>(!string.IsNullOrWhiteSpace(sorts) ? sorts : "[]"),
                search, lastUpdate);

        var result = data.Data.ToDynamicList().Select(x => new
        {
            x.Code,
            x.Initial,
            x.Name,
            x.TypeId,
            x.TypeName,
            x.CreditLimit,
            x.Used,
            Remaining = x.CreditLimit - x.Used,
            x.AreaId1,
            x.AreaId2,
            x.AreaId3,
            x.AreaId4,
            x.AreaId5,
            x.AreaName1,
            x.AreaName2,
            x.AreaName3,
            x.AreaName4,
            x.AreaName5,
            x.Lat,
            x.Lng,
            x.InitialAddress,
            x.Address1,
            x.Address2,
            x.Phone,
            x.Fax,
            x.ContactPerson,
            x.IsActive,
            x.UpdatedDate
        }).ToList<dynamic>();

        return Ok(new MobileApiResponse
        {
            Count = data.Total,
            Data = result.ToDynamicList()
        });
    }

    [HttpGet("lists")]
    public IActionResult GetList(string filters, string sorts)
    {
        var data =
            _customer.GetLists(
                    JsonConvert.DeserializeObject<List<Filter>>(!string.IsNullOrWhiteSpace(filters) ? filters : "[]"),
                    JsonConvert.DeserializeObject<List<Sort>>(!string.IsNullOrWhiteSpace(sorts) ? sorts : "[]")).Data
                .ToDynamicList()
                .Select(x => new
                {
                    x.Code,
                    x.Initial,
                    x.Name,
                    x.Address1,
                    x.Phone,
                    x.Fax,
                    x.PaymentTermId,
                    x.TypeId
                })
                .ToList<dynamic>();

        return Ok(new MobileApiResponse
        {
            Count = data.Count,
            Data = data
        });
    }

    [HttpGet("addresses")]
    public IActionResult GetAddress(string code)
    {
        var data =
            _customer.GetAddress(code)
                .Select(x => new
                {
                    x.Id,
                    x.Code,
                    x.Initial,
                    x.Address1,
                    x.Address2,
                    x.ContactPerson,
                    x.Phone,
                    x.Fax,
                    x.IsDefault
                })
                .ToList<dynamic>();

        return Ok(new MobileApiResponse
        {
            Count = data.Count,
            Data = data
        });
    }

    [HttpGet("{code}")]
    public IActionResult GetDataByCode(string code)
    {
        return Ok(_customer.FindByCode(code));
    }

    [HttpPost("add")]
    public IActionResult AddCustomer(MobileCustomer data)
    {

        data.CreatedBy = _claim.UserId;
        data.CreatedDate = DateTime.Now;
        data.UpdatedBy = data.CreatedBy;
        data.UpdatedDate = data.CreatedDate;

        var result = _mobCust.Insert(data);

        return Ok(result);
    }

    [HttpGet("area")]
    public IActionResult GetArea(string search, string filters, string sorts, int skip, int take)
    {
        var data =
            _area.GetData(
                skip, take,
                JsonConvert.DeserializeObject<List<Filter>>(!string.IsNullOrWhiteSpace(filters) ? filters : "[]"),
                JsonConvert.DeserializeObject<List<Sort>>(!string.IsNullOrWhiteSpace(sorts) ? sorts : "[]"),
                search);

        var result = data.Data.ToDynamicList().Select(x => new
        {
            x.Id,
            x.Initial,
            x.Name,
            x.ParentId,
            x.Deep,
            x.Lineage,
            x.IsActive,
            x.UpdatedDate
        }).ToList<dynamic>();

        return Ok(new MobileApiResponse
        {
            Count = data.Total,
            Data = result.ToDynamicList()
        });
    }

    [HttpGet("type")]
    public IActionResult GetCustomerType(string search, string filters, string sorts, int skip, int take)
    {
        var data =
            _type.GetData(
                skip, take,
                JsonConvert.DeserializeObject<List<Filter>>(!string.IsNullOrWhiteSpace(filters) ? filters : "[]"),
                JsonConvert.DeserializeObject<List<Sort>>(!string.IsNullOrWhiteSpace(sorts) ? sorts : "[]"),
                search);

        var result = data.Data.ToDynamicList().Select(x => new
        {
            x.Id,
            x.Initial,
            x.Name,
            x.IsActive,
            x.UpdatedDate

        }).ToList<dynamic>();

        return Ok(new MobileApiResponse
        {
            Count = data.Total,
            Data = result.ToDynamicList()
        });
    }
}