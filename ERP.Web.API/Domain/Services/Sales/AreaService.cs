using System;
using System.Collections.Generic;
using System.Linq;
using ERP.Common;
using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Entity.Sales;
using ERP.Web.API.Domain.Interfaces.Sales;

namespace ERP.Web.API.Domain.Services.Sales
{
    public class AreaService : GeneralService<Area>, IAreaService
    {
        public AreaService(TenantContext db)
            : base(db)
        {
        }

        public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
            string search)
        {
            var data = Db.VwAreas.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                data =
                    data.Where(x =>
                        x.Initial.Contains(search) || x.Name.Contains(search));
            }

            return data.ToDataSourceResult(skip, take, filter, sort);
        }

        public IEnumerable<Area> GetLists()
        {
            var data = Db.Areas.Where(x => x.IsActive);

            return data.OrderBy(x => x.Name);
        }

        public object GetHierarchy()
        {
            return new
            {
                Id = 0,
                Initial = "",
                Name = "Wilayah",
                ParentId = 0,
                Deep = 0,
                Lineage = "",
                IsParent = true,
                Children = DefineChildNodes(Db.VwAreas.Where(x => x.IsActive).ToList())
            };
        }

        private static object DefineChildNodes(List<VwArea> data, int? parentId = null)
        {
            var nodes = data
                .Where(x => x.ParentId == parentId)
                .OrderBy(x => x.Id)
                .Select(x => new
                {
                    x.Id,
                    x.Initial,
                    x.Name,
                    x.ParentId,
                    x.Deep,
                    x.Lineage,
                    x.CreatedInitial,
                    x.CreatedDate,
                    x.UpdatedInitial,
                    x.UpdatedDate,
                    IsParent = IsParent(data, x.Id),
                    Children = DefineChildNodes(data, x.Id)
                });

            return nodes;
        }

        public override SaveResult Insert(Area data)
        {
            var result = new SaveResult(false);

            using var transaction = Db.Database.BeginTransaction();
            try
            {
                // Checking initial already exists or not
                if (IsInitialExists(data.Initial, 0))
                {
                    result.Message = "Inisial sudah terdaftar. Tolong gunakan inisial lain.";
                    return result;
                }

                Db.Areas.Add(data);
                Db.SaveChanges();

                transaction.Commit();
            }
            catch (Exception ex)
            {
                result.Message = ex.InnerException?.Message ?? ex.Message;
                return result;
            }

            result.Success = true;
            result.Data = data.Initial;
            result.Message = "Data wilayah berhasil disimpan.";
            return result;
        }

        public override SaveResult Update(Area data)
        {
            var result = new SaveResult(false);

            using var transaction = Db.Database.BeginTransaction();
            try
            {
                // Checking initial already exists or not
                if (IsInitialExists(data.Initial, data.Id))
                {
                    result.Message = "Inisial sudah terdaftar. Tolong gunakan inisial lain.";
                    return result;
                }

                // Update data
                Db.Areas.Update(data);
                Db.Entry(data).Property(e => e.Id).IsModified = false;
                Db.Entry(data).Property(e => e.IsActive).IsModified = false;
                Db.Entry(data).Property(e => e.CreatedBy).IsModified = false;
                Db.Entry(data).Property(e => e.CreatedDate).IsModified = false;

                Db.SaveChanges();

                transaction.Commit();
            }
            catch (Exception ex)
            {
                result.Message = ex.InnerException?.Message ?? ex.Message;
                return result;
            }

            result.Success = true;
            result.Data = data.Initial;
            result.Message = "Data wilayah berhasil diperbarui.";
            return result;
        }

        public SaveResult Delete(int id, int userId)
        {
            var result = new SaveResult(false);

            var data = Db.Areas.Find(id);
            if (data != null)
            {
                // Checking active
                if (!data.IsActive)
                {
                    result.Message = "Tidak bisa menonaktifkan data wilayah karena data sudah nonaktif.";
                    return result;
                }

                // Update data
                data.IsActive = false;
                data.UpdatedBy = userId;
                data.UpdatedDate = DateTime.Now;

                Db.SaveChanges();
            }

            result.Success = true;
            result.Message = "Data wilayah berhasil dinonaktifkan.";
            return result;
        }

        private bool IsInitialExists(string initial, int id)
        {
            return Db.Areas.Any(x => x.Initial == initial && x.Id != id);
        }

        private static bool IsParent(List<VwArea> data, int? id = null)
        {
            return data.Any(x => x.ParentId == id);
        }
    }
}
