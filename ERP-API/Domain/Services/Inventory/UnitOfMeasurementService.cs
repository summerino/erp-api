using System;
using System.Collections.Generic;
using System.Linq;
using ERP_API.Domain.Entities;
using ERP_API.Domain.Entities.Inventory;
using ERP_API.Domain.Extensions;
using ERP_API.Domain.Interfaces.Inventory;
using ERP_API.Domain.Models;
using ERP_API.Model.Inventory;

namespace ERP_API.Domain.Services.Inventory
{
    public class UnitOfMeasurementService : GeneralService<UoM>, IUnitOfMeasurementService
    {
        public UnitOfMeasurementService(TenantContext db)
            : base(db)
        {
        }

        public  SaveResult Insert(UnitOfMeasurementRequest data, int userId)
        {
            var result = new SaveResult(false);

            using var transaction = Db.Database.BeginTransaction();
            try
            {
                DateTime insertDate = DateTime.Now;
                // Checking initial already exists or not
                if (IsInitialExists(data.Initial, 0))
                {
                    result.Message = "Initial is already exists. Please use another initial.";
                    return result;
                }
                data.IsActive = true;
                data.CreatedBy = userId;
                data.CreatedDate = insertDate;
                data.UpdatedBy = userId;
                data.UpdatedDate = insertDate;

                // Insert data
                Db.UoMs.Add(data);
                Db.SaveChanges();

                foreach (var item in data.Details)
                {
                    Db.UoMConversions.Add(new UoMConversion
                    {
                        UomId = data.Id,
                        Conversion = item.Conversion,
                        IsBaseUnit = item.IsBaseUnit,
                        Seq = item.Seq,
                        UnitEquivalent = item.UnitEquivalent,
                        UnitToConvert = item.UnitToConvert,
                        CreatedBy = userId,
                        CreatedDate = insertDate,
                        UpdatedBy = userId,
                        UpdatedDate = insertDate
                    });
                }

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
            result.Message = "Success insert item.";
            return result;
        }

        public SaveResult Update(UnitOfMeasurementRequest data, int userId)
        {
            var result = new SaveResult(false);

            using var transaction = Db.Database.BeginTransaction();
            try
            {
                DateTime updateDate = DateTime.Now;
                data.UpdatedBy = userId;
                data.UpdatedDate = updateDate;

                // Update header data
                Db.UoMs.Update(data);
                Db.Entry(data).Property(e => e.Id).IsModified = false;
                Db.Entry(data).Property(e => e.CreatedBy).IsModified = false;
                Db.Entry(data).Property(e => e.CreatedDate).IsModified = false;

                // Get detail data that exists in order before
                var delDetails = Db.UoMConversions
                    .Where(d => d.UomId == data.Id && !data.Details.Select(x => x.Id).Contains(d.Id))
                    .ToList();

                // Delete detail data that exists in order before
                Db.UoMConversions.RemoveRange(delDetails);

                // Update detail data
                short i = 0;
                foreach (var item in data.Details)
                {
                    if (item.Id < 0)
                    {
                        Db.UoMConversions.Add(new UoMConversion
                        {
                            UomId = data.Id,
                            Conversion = item.Conversion,
                            IsBaseUnit = item.IsBaseUnit,
                            Seq = item.Seq,
                            UnitEquivalent = item.UnitEquivalent,
                            UnitToConvert = item.UnitToConvert,
                            CreatedBy = userId,
                            CreatedDate = updateDate,
                            UpdatedBy = userId,
                            UpdatedDate = updateDate
                        });
                    }
                    else
                    {
                        item.UpdatedBy = userId;
                        item.UpdatedDate = updateDate;
                        Db.UoMConversions.Update(item);
                        Db.Entry(item).Property(e => e.Id).IsModified = false;
                        Db.Entry(item).Property(e => e.UomId).IsModified = false;
                        Db.Entry(item).Property(e => e.CreatedBy).IsModified = false;
                        Db.Entry(item).Property(e => e.CreatedDate).IsModified = false;
                    }
                }

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
            result.Message = "Success update unit of measurement.";
            return result;
        }

        public SaveResult Delete(int id, int userId)
        {
            var result = new SaveResult(false);

            var data = Db.UoMs.Find(id);
            if (data != null)
            {
                // Checking active
                if (!data.IsActive)
                {
                    result.Message = "Can't inactive the item because data already inactive.";
                    return result;
                }

                // Update data
                data.IsActive = false;
                data.UpdatedBy = userId;
                data.UpdatedDate = DateTime.Now;

                Db.SaveChanges();
            }

            result.Success = true;
            result.Message = "Success inactive item.";
            return result;
        }

        public bool IsInitialExists(string initial, int id)
        {
            return Db.Items.Any(x => x.Initial == initial && x.Id != id);
        }

        public IEnumerable<UoMConversion> GetDataConversion(int? uomId)
        {
            var data = Db.UoMConversions.AsQueryable();

            if (uomId.HasValue)
                data = data.Where(x => x.UomId == uomId);

            return data.OrderBy(x => x.Seq);
        }

        public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort, string search)
        {
            var data = Db.UoMs.Where(x => x.IsActive).AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                data = data.Where(x =>
                            x.Initial.Contains(search) || x.Description.Contains(search) ||
                            x.BaseUnit.Contains(search));
            }

            return data.ToDataSourceResult(skip, take, filter, sort);
        }
    }
}
