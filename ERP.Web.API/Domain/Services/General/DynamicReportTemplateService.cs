using ERP.Common;
using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Entity.General;
using ERP.Web.API.Domain.Interfaces.General;

namespace ERP.Web.API.Domain.Services.General
{
    public class DynamicReportTemplateService : GeneralService<DynamicReportTemplate>, IDynamicReportTemplateService
    {
        public DynamicReportTemplateService(TenantContext db)
            :base(db)
        {

        }
        public SaveResult Delete(int id, int userId)
        {
            var result = new SaveResult(false);

            var data = Db.DynamicReportTemplates.Find(id);
            if (data != null)
            {

                Db.DynamicReportTemplates.Remove(data);

                Db.SaveChanges();
            }

            result.Success = true;
            result.Message = "Data template laporan dinamis berhasil dihapus.";
            return result;
        }

        public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts, string search)
        {
            var data = (from t in Db.DynamicReportTemplates
                        join u in Db.Users on t.UpdatedBy equals u.Id
                        select new
                        {
                            t.Id,
                            t.Name,
                            t.DataTypeParameter1,
                            t.SourceParameter1,
                            t.DropdownValueParameter1,
                            t.DropdownTextParameter1,
                            t.DataTypeParameter2,
                            t.SourceParameter2,
                            t.DropdownValueParameter2,
                            t.DropdownTextParameter2,
                            t.DataTypeParameter3,
                            t.SourceParameter3,
                            t.DropdownValueParameter3,
                            t.DropdownTextParameter3,
                            t.DataTypeParameter4,
                            t.SourceParameter4,
                            t.DropdownValueParameter4,
                            t.DropdownTextParameter4,
                            t.DataTypeParameter5,
                            t.SourceParameter5,
                            t.DropdownValueParameter5,
                            t.DropdownTextParameter5,
                            t.Query,
                            t.IsActive,
                            t.CreatedBy,
                            t.CreatedDate,
                            t.UpdatedBy,
                            t.UpdatedDate,
                            UpdatedInitial = u.Initial
                        }).AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                data = data.Where(x =>
                        x.Name.Contains(search));
            }

            return data.ToDataSourceResult(skip, take, filters, sorts);
        }

        public DataSourceResult GetLists(IEnumerable<Filter> filters, IEnumerable<Sort> sorts)
        {
            var data = Db.DynamicReportTemplates.Where(x => x.IsActive);

            return data.ToDataSourceResult(0, -1, filters, sorts);
        }

        public override SaveResult Insert(DynamicReportTemplate data)
        {
            var result = new SaveResult(false);

            using var transaction = Db.Database.BeginTransaction();
            try
            {
                // Insert data
                Db.Add(data);

                Db.SaveChanges();
                transaction.Commit();
            }
            catch (Exception ex)
            {
                result.Message = ex.InnerException?.Message ?? ex.Message;
                return result;
            }

            result.Success = true;
            result.Data = data.Id;
            result.Message = "Data template laporan dinamis berhasil disimpan.";
            return result;
        }

        public override SaveResult Update(DynamicReportTemplate data)
        {
            var result = new SaveResult(false);

            // Update data
            Db.DynamicReportTemplates.Update(data);
            Db.Entry(data).Property(e => e.Id).IsModified = false;
            Db.Entry(data).Property(e => e.CreatedBy).IsModified = false;
            Db.Entry(data).Property(e => e.CreatedDate).IsModified = false;

            Db.SaveChanges();

            result.Success = true;
            result.Data = data.Id;
            result.Message = "Data template laporan dinamis berhasil diperbarui.";
            return result;
        }
    }
}
