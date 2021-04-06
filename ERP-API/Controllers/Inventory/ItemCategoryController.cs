using System.Linq;
using Microsoft.AspNetCore.Mvc;
using ERP_API.Domain.Interfaces.Inventory;
using ERP_API.Dtos;

namespace ERP_API.Controllers.Inventory
{
    [Route("api/v1/item-category")]
    //[Authorize]
    [ApiController]
    public class ItemCategoryController : ControllerBase
    {
        private readonly IItemCategoryService _category;

        public ItemCategoryController(IItemCategoryService category)
        {
            _category = category;
        }

        [HttpGet]
        public IActionResult GetData(string search) 
        {
            var data = _category.GetData().ToList<dynamic>();

            return Ok(new MasterViewDto
            {
                RowCount = data.Count,
                TableData = data
            });
        }

        [HttpGet("hierarchy")]
        public IActionResult GetHierarchy()
        {
            return Ok(_category.GetHierarchy());
        }
    }
}
