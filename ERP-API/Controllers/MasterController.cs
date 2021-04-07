using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ERP_API.Dtos;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Swift.Framework;
using Swift.Framework.Model;

namespace ERP_API.Controllers
{
    [Route("api/v1/master")]
    //[Authorize]
    [ApiController]
    public class MasterController : ControllerBase
    {
        public MasterController() { }

        [HttpGet]
        public IActionResult GetData(string param, string fieldNames,
            int skip = 0, int take = 50,
            string filters = "", string sorts = "",
            bool? includeMetaData = true) 
        {
            var builder = new Builder();
            var masterConfig = builder.GetActiveConfiguration(param);
            if (masterConfig is null || masterConfig.IsActive == false)
            {
                //invalid - master belum di setup utk table ini atau inactive, return 404?
                return NotFound();
            }

            WhereFilter whFilter = null;
            if (!string.IsNullOrWhiteSpace(filters))
            {
                whFilter = new WhereFilter
                {
                    Adjective = FilterAdjective.AND,
                    Statements = JsonConvert.DeserializeObject<List<WhereStatement>>(filters)
                };
            }

            var sortLists = !string.IsNullOrWhiteSpace(sorts)
                ? JsonConvert.DeserializeObject<List<Sort>>(sorts)
                : null;

            using var dataAccess = builder.CreateDataAccess(masterConfig.ConnectionString);

            var tableData =
                dataAccess.GetData(
                    masterConfig.SchemaName, masterConfig.TableName,
                    fieldNames?.Split(','), whFilter, sortLists, -1, -1);

            var toReturn = new MasterViewDto {
                RowCount = tableData.RowCount,
                TableData = tableData.TableData,
                PKColumnName = masterConfig.PKColumnName
            };

            if (includeMetaData.GetValueOrDefault(true))
            {
                var metadata = dataAccess.GetGridViewSchema(masterConfig.SchemaName, masterConfig.TableName);
                var hiddenColumns = masterConfig.HiddenColumns.Split(',').Select(p => p.Trim());
                metadata = metadata.Where((v, i) => v.Value != masterConfig.PKColumnName && !hiddenColumns.Contains( v.Value )).ToList();

                toReturn.MetaData = metadata;
            } 

            return Ok(toReturn);
        }

        [HttpGet("addnew")]
        public IActionResult GetAddMetadata([FromQueryAttribute] string param)
        {
            var builder = new Builder();
            var toReturn = default(MasterAddEditDto);
            var masterConfig = builder.GetActiveConfiguration(param);
            if (masterConfig is null || masterConfig.IsActive == false)
            {
                //invalid - master belum di setup utk table ini atau inactive, return 404?
                return NotFound();
            }
            else
            {
                using var dataAccess = builder.CreateDataAccess(masterConfig.ConnectionString);
                var metadata = dataAccess.GetInputFormSchema(masterConfig.SchemaName, masterConfig.TableName);
                toReturn = new MasterAddEditDto();
                toReturn.MetaData = metadata;
                return Ok(toReturn);
            }
            
        }

        // GET api/<MasterViewController>/5
        [HttpGet("{id}")]
        public IActionResult GetOneData(string id, string param, string fieldNames, bool? includeMetaData)
        {
            var builder = new Builder();
            var masterConfig = builder.GetActiveConfiguration(param);
            if (masterConfig is null || masterConfig.IsActive == false)
            {
                //invalid - master belum di setup utk table ini atau inactive, return 404?
                return NotFound();
            }

            var dataAccess = builder.CreateDataAccess(masterConfig.ConnectionString);

            var primaryKeys = new Dictionary<string, object>
            {
                [masterConfig.PKColumnName] = id
            };

            var tableData =
                dataAccess.GetOneData(
                    masterConfig.SchemaName, masterConfig.TableName, primaryKeys, fieldNames?.Split(','));

            var toReturn = new MasterAddEditDto { TableData = tableData };

            if (includeMetaData.GetValueOrDefault(true))
            {
                var metadata = dataAccess.GetInputFormSchema(masterConfig.SchemaName, masterConfig.TableName);
                toReturn.MetaData = metadata;
            }

            return Ok(toReturn);
        }

        // POST api/<MasterViewController>
        [HttpPost]
        public IActionResult Post([FromBody] JToken jsonData, [FromQueryAttribute] string param)
        {

            var builder = new Builder();
            var masterConfig = builder.GetActiveConfiguration(param);
            if (masterConfig is null || masterConfig.IsActive == false)
            {
                //invalid - master belum di setup utk table ini atau inactive, return 404?
                return NotFound();
            }
            else
            {
                var dataAccess = builder.CreateDataAccess(masterConfig.ConnectionString);
                //var result = builder.SaveOrUpdateData(masterConfig.SchemaName
                //    , masterConfig.TableName
                //    , jsonData
                //    , new KeyValuePair<string, object> (masterConfig.PKColumnName,(object)"-1"));
                return Ok();
            }
            

            //JObject inner = jsonData.Value<JObject>();
            /*JObject my_obj = JsonConvert.DeserializeObject<JObject>(your_json);
            foreach (KeyValuePair<string, JToken> sub_obj in (JObject)my_obj["ADDRESS_MAP"])
            {
                Console.WriteLine(sub_obj.Key);
            }
            
             or
            
               JObject headerData= body["headerData"].Value<JObject>();
    JObject headerData= body["rowData"].Value<JObject>();
            
             or
            
             public HttpResponseMessage Post(Body body,
    [FromUri]string id, 
    [FromUri]string culture = null,
    [FromUri]uint layout = 0
)*/
        }

        // PUT api/<MasterViewController>/5
        //[HttpPut("{id}")]
        //public void Put(Object id, [FromBody] JToken jsonData)
        //{
        //    JObject inner = jsonData.Value<JObject>();
        //}

        // DELETE api/<MasterViewController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
