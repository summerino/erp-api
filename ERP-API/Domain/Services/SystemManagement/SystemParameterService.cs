using System.Collections.Generic;
using System.Linq;
using ERP.Entity;
using ERP.Entity.SystemManagement;
using ERP.Web.API.Domain.Extensions;
using ERP.Web.API.Domain.Interfaces.SystemManagement;
using ERP.Web.API.Domain.Models;
using ERP.Web.API.Model.SystemManagement;
using Microsoft.EntityFrameworkCore;

namespace ERP.Web.API.Domain.Services.SystemManagement
{
    public class SystemParameterService : GeneralService<SystemParameter>, ISystemParameterService
    {
        public SystemParameterService(TenantContext db)
            : base(db)
        {
        }

        public List<SystemParameterRequest> GetHierarchy()
        {
            var result = new List<SystemParameterRequest>();

            var paramModules = (from spm in Db.SystemParameterModules
                                where spm.IsActive
                                orderby spm.Seq
                               select new SystemParameterRequest
                               {
                                   Id = spm.Id,
                                   ParentId = spm.ParentId,
                                   Depth = spm.Deep,
                                   Name = spm.Name,
                                   Seq = spm.Seq,
                                   ModuleId = spm.Id
                               }).AsNoTracking().ToList();
            result.AddRange(paramModules.Where(x=>x.Depth == 0));
            foreach (var item in result)
            {
                item.Children.AddRange(paramModules.Where(x => x.ParentId == item.Id).OrderBy(x=>x.Seq));
            }
            var data = (from sp in Db.SystemParameters
                        join spm in Db.SystemParameterModules on sp.ModuleId equals spm.Id
                        where spm.IsActive && sp.IsActive
                        select new SystemParameterRequest
                        {
                            Id = sp.Id,
                            ParentId = spm.ParentId,
                            Depth = 2,
                            Name = sp.Description,
                            DataType = sp.DataType,
                            Description = sp.Description,
                            Code= sp.Code,
                            Seq = sp.Seq,
                            Value = sp.DataType == "bool" ? sp.Value == "0" ? false : true : sp.Value,
                            ModuleId = sp.ModuleId
                        }).OrderBy(x=>x.Seq).AsNoTracking().ToList();
            foreach (var item in result)
            {
                foreach (var item2 in item.Children)
                {
                    var temp = data.Where(x => x.ModuleId == item2.Id);
                    item2.ListParameters.AddRange(temp);
                }
            }
            return result;
        }

        public DataSourceResult GetLists(IEnumerable<Filter> filters, IEnumerable<Sort> sorts, IEnumerable<string> codes)
        {
            var data = Db.SystemParameters.AsQueryable();
            if (codes?.Any() ?? false)
            {
                data = data.Where(x => codes.Contains(x.Code));
            }
            return data.ToDataSourceResult(0, -1, filters, sorts);
        }

        public SaveResult Save(List<SystemParameterRequest> data)
        {
            var result = new SaveResult(false);

            var newData = new List<SystemParameterRequest>();
            foreach (var item in data)
            {
                foreach (var item2 in item.Children)
                {
                    newData.AddRange(item2.ListParameters);
                }
            }

            var db = Db.SystemParameters.ToList();
            foreach (var item in db)
            {
                var temp = newData.SingleOrDefault(x => x.Id == item.Id);
                if (temp != null) {
                    item.Code = temp.Code;
                    if (item.DataType == "string")
                    {
                        item.Value = (string)temp.Value;
                    }
                    else if (item.DataType == "bool")
                    {
                        if ((bool)temp.Value)
                        {
                            item.Value = "1";
                        }
                        else 
                        {
                            item.Value = "0";
                        }
                    }
                    else 
                    { 
                        item.Value = (string)temp.Value;
                    }
                }
            }
            SaveChanges();
            result.Success = true;
            result.Message = "Data sistem parameter berhasil disimpan.";
            return result;
        }
    }
}
