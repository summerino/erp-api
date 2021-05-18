using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ERP_API.Dtos;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Swift.Framework;
using Swift.Framework.Model;
using ERP_API.Domain.Extensions;
using ERP_API.Domain.Entities.General;
using System;
using System.ComponentModel.DataAnnotations;
using ERP_API.Domain.Entities;
using ERP_API.Domain.Models;
using Sort = Swift.Framework.Model.Sort;
using ERP_API.Domain.Entities.Accounting;
using ERP_API.Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace ERP_API.Controllers
{
    [Route("api/v1/master")]
    [AllowAnonymous]
    [ApiController]
    public class MasterController : ControllerBase
    {
        private readonly IClaimService _claim;
        public MasterController(IClaimService claim)
        {
            _claim = claim;
        }
        [HttpGet]
        public IActionResult GetData(string param, string fieldNames,
            int skip = 0, int take = 50,
            string filters = "", string sorts = "",
            bool isAdvancedSearch = false,
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
                    Adjective = isAdvancedSearch ? FilterAdjective.AND : FilterAdjective.OR,
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

            var toReturn = new MasterViewDto
            {
                RowCount = tableData.RowCount,
                TableData = tableData.TableData,
                PKColumnName = masterConfig.PKColumnName.ToCamelCase()
            };

            if (includeMetaData.GetValueOrDefault(true))
            {
                var metadata = dataAccess.GetGridViewSchema(masterConfig.SchemaName, masterConfig.TableName);
                var hiddenColumns = masterConfig.HiddenColumns.ToLower().Split(',').Select(p => p.Trim());
                metadata = metadata.Where((v, i) => v.Value.ToLower() != masterConfig.PKColumnName.ToLower() && !hiddenColumns.Contains(v.Value.ToLower())).ToList();
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
                var hiddenColumns = masterConfig.HiddenColumns.ToLower().Split(',').Select(p => p.Trim());
                metadata = metadata.Where((v, i) => v.Name.ToLower() != masterConfig.PKColumnName.ToLower() && !hiddenColumns.Contains(v.Name.ToLower())).ToList();
                toReturn = new MasterAddEditDto();
                toReturn.MetaData = metadata;
                return Ok(toReturn);
            }

        }

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
                var hiddenColumns = masterConfig.HiddenColumns.ToLower().Split(',').Select(p => p.Trim());
                metadata = metadata.Where((v, i) => v.Name.ToLower() != masterConfig.PKColumnName.ToLower() && !hiddenColumns.Contains(v.Name.ToLower())).ToList();
                toReturn.MetaData = metadata;
            }

            return Ok(toReturn);
        }

        [HttpPost]
        public IActionResult Post([FromBody] object jsonData, [FromQueryAttribute] string param)
        {

            var builder = new Builder();
            var result = new SaveResult(true);
            var masterConfig = builder.GetActiveConfiguration(param);
            if (masterConfig is null || masterConfig.IsActive == false)
            {
                //invalid - master belum di setup utk table ini atau inactive, return 404?
                return NotFound();
            }
            else
            {
                var dataAccess = new Swift.Framework.DataAccess(masterConfig.ConnectionString);
                var date = DateTime.Now;
                dynamic model = JsonConvert.DeserializeObject<dynamic>(jsonData.ToString());
                model.CreatedBy = _claim.UserId;
                model.CreatedDate = date;
                model.UpdatedBy = _claim.UserId;
                model.UpdatedDate = date;

                var validation = Validate(masterConfig,param, model);
                if (!validation.Success)
                {
                    return Ok(validation);
                }
               
                jsonData = JsonConvert.SerializeObject(model);
                jsonData = "{ tableData: " + jsonData + " }";
                var jToken = (JToken)JObject.Parse(string.Join(Environment.NewLine, jsonData));

                var (uniqueColumns, failedValidationMessage) = GetUniqueColumns(masterConfig);
                var saveResult = dataAccess.SaveData(masterConfig.SchemaName, masterConfig.TableName, jToken, uniqueColumns);

                if (saveResult.Result == Swift.Framework.Dtos.PageAddEdit.ResultType.Success)
                    result.Message = $"Success insert {param}.";
                else 
                {
                    result.Success = false;
                    result.Message = uniqueColumns == null ? result.Message : failedValidationMessage;
                }
                return Ok(result);
            }
        }

        [HttpPut]
        public IActionResult Put([FromQueryAttribute] string id, [FromBody] object jsonData, [FromQueryAttribute] string param)
        {

            var builder = new Builder();
            var result = new SaveResult(true);
            var masterConfig = builder.GetActiveConfiguration(param);
            if (masterConfig is null || masterConfig.IsActive == false)
            {
                //invalid - master belum di setup utk table ini atau inactive, return 404?
                return NotFound();
            }
            else
            {
                var dataAccess = new Swift.Framework.DataAccess(masterConfig.ConnectionString);
                var date = DateTime.Now;
                
                dynamic model = JsonConvert.DeserializeObject<dynamic>(jsonData.ToString());
                model.updatedBy = _claim.UserId;
                model.updatedDate = date;

                

                var validation = Validate(masterConfig, param, model);
                if (!validation.Success)
                {
                    return Ok(validation);
                }

                //var newModel = (JObject)model;
                //newModel.Remove(masterConfig.PKColumnName.ToLower());

                jsonData = JsonConvert.SerializeObject(model);
                jsonData = "{ tableData: " + jsonData + " }";
                var jToken = (JToken)JObject.Parse(string.Join(Environment.NewLine, jsonData));
                //dataAccess.UpdateData(masterConfig.SchemaName, masterConfig.TableName, jToken, new Dictionary<string, object> { { masterConfig.PKColumnName , id } });
                //result.Message = $"Success update {param}.";

                var (uniqueColumns, failedValidationMessage) = GetUniqueColumns(masterConfig);
                var primaryKey = new Dictionary<string, object>();
                primaryKey.Add(masterConfig.PKColumnName, id);
                
                var saveResult = dataAccess.UpdateData(masterConfig.SchemaName, masterConfig.TableName, jToken, primaryKey, uniqueColumns);
                
                if (saveResult.Result == Swift.Framework.Dtos.PageAddEdit.ResultType.Success)
                    result.Message = $"Success update {param}.";
                else
                {
                    result.Success = false;
                    result.Message = uniqueColumns == null ? result.Message : failedValidationMessage;
                }
                return Ok(result);
            }
        }

        [HttpDelete]
        public IActionResult Delete([FromQueryAttribute] string ids, [FromQueryAttribute] string param)
        {
            var result = new SaveResult(true);
            
            if (!string.IsNullOrWhiteSpace(ids))
            {
                var builder = new Builder();
                var masterConfig = builder.GetActiveConfiguration(param);
                var dataAccess = new Swift.Framework.DataAccess(masterConfig.ConnectionString);
                var dict = JsonConvert.DeserializeObject<Dictionary<string,object>>(ids);
                dataAccess.DeleteData(masterConfig.SchemaName, masterConfig.TableName,dict);
                result.Success = true;
                result.Message = $"Data has been deleted.";
            }
            else 
            {
                result.Success = false;
                result.Message = "Param is empty.";
            }
            return Ok(result);
        }

        #region private function

        private Dictionary<string, Type> GetTypes()
        {
            var data = new Dictionary<string, Type>();
            data.Add("customertype", typeof(CustomerType));
            data.Add("currency", typeof(Currency));
            data.Add("currencyrate", typeof(CurrencyRate));
            return data;
        }
        private SaveResult Validate(Swift.Framework.Dtos.MasterConfig.ParameterDto parameterDto, string param, object obj)
        {

            var result = new SaveResult(true);
            var type = GetTypes();

            if (!type.ContainsKey(param)) {
                result.Success = false;
                result.Message = "Model has'not been mapped yet.";
                return result;
            }

            var validationResults = new List<ValidationResult>();
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };
            var model = JsonConvert.DeserializeObject(obj.ToString(), type[param], settings);
            var context = new ValidationContext(model, null, null);
            var isValid = Validator.TryValidateObject(model, context, validationResults, true);
            if (!isValid)
            {
                result.Success = false;
                result.Message = "Please kindly check mandatory fields or fields that have an error.";
            }
            return result;
        }

        private (string[]? value, string message) GetUniqueColumns(Swift.Framework.Dtos.MasterConfig.ParameterDto parameterDto) 
        {
            if (!string.IsNullOrEmpty(parameterDto.UniqueColumnName))
            {
                var uniqueColumns = parameterDto.UniqueColumnName.Split(",");
                string failedValidationMessage = "Data " + string.Join(" and ", uniqueColumns.Select((s) => $"{s}")) + " already exist. Please change the values and try again.";
                return (uniqueColumns, failedValidationMessage);
            }
            return (null, "");
        }

        #endregion
    }
}
