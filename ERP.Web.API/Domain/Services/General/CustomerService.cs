using Microsoft.AspNetCore.Identity;
using ERP.Common;
using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Entity.General;
using ERP.Entity.Inventory;
using ERP.Web.API.Domain.Interfaces.General;
using ERP.Web.API.Model.General;
using UserCatalog = ERP.Entity.Catalog.CustomerUser;
using ERP.Web.API.Domain.Models.Mobile.General;

namespace ERP.Web.API.Domain.Services.General;

public class CustomerService : GeneralService<Customer>, ICustomerService
{
    private readonly CatalogContext _catalogCtx;
    private readonly IClaimService _claim;

    public CustomerService(TenantContext db, CatalogContext catalogCtx, IClaimService claim)
        : base(db)
    {
        _catalogCtx = catalogCtx;
        _claim = claim;
    }

    public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
        string search, string mobileLastSync)
    {
        var data = Db.VwCustomers.AsQueryable();

        if (!string.IsNullOrEmpty(mobileLastSync))
        {
            switch (mobileLastSync.Length)
            {
                case 19:
                    mobileLastSync += ".0000";
                    break;
                case 21:
                    mobileLastSync += "000";
                    break;
                case 22:
                    mobileLastSync += "00";
                    break;
                case 23:
                    mobileLastSync += "0";
                    break;
            }
            data = data.Where(x => x.UpdatedDate > DateTime.ParseExact(mobileLastSync, "yyyy-MM-ddTHH:mm:ss.ffff", null));
        }

        if (!string.IsNullOrEmpty(search))
        {
            data = data.Where(x =>
                x.Code.Contains(search) || x.Initial.Contains(search) || x.Name.Contains(search) || x.TypeName.Contains(search) ||
                x.Address1.Contains(search) || x.Phone.Contains(search));
        }
        return data.ToDataSourceResult(skip, take, filter, sort);
    }

    public DataSourceResult GetMobileCustomer(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
        string search, string mobileLastSync)
    {
        var dataOriginal = (from cust in Db.VwCustomers
                            select new CustomerModel
                            {
                                Code = cust.Code,
                                Initial = cust.Initial,
                                Name = cust.Name,
                                TypeId = cust.TypeId,
                                TypeName = cust.TypeName,
                                CreditLimit = cust.CreditLimit,
                                Used = cust.CreditUsed,
                                Remaining = cust.CreditLimit - cust.CreditUsed,
                                AreaId1 = cust.AreaId1,
                                AreaId2 = cust.AreaId2,
                                AreaId3 = cust.AreaId3,
                                AreaId4 = cust.AreaId4,
                                AreaId5 = cust.AreaId5,
                                AreaName1 = cust.AreaName1,
                                AreaName2 = cust.AreaName2,
                                AreaName3 = cust.AreaName3,
                                AreaName4 = cust.AreaName4,
                                AreaName5 = cust.AreaName5,
                                Lat = cust.Lat,
                                Lng = cust.Lng,
                                InitialAddress = cust.InitialAddress,
                                Address1 = cust.Address1,
                                Address2 = cust.Address2,
                                Phone = cust.Phone,
                                Fax = cust.Fax,
                                ContactPerson = cust.ContactPerson,
                                IsActive = cust.IsActive,
                                UpdatedDate = cust.UpdatedDate,
                            });

        var dataMobile = (from custMobile in Db.VwMobileCustomers
                          where custMobile.CustCode == null && custMobile.Mark == "A"
                          select new CustomerModel
                          {
                              Code = custMobile.Code,
                              Initial = custMobile.Initial,
                              Name = custMobile.Name,
                              TypeId = custMobile.TypeId,
                              TypeName = custMobile.TypeName,
                              CreditLimit = (decimal)0.00,
                              Used = (decimal)0.00,
                              Remaining = (decimal)0.00,
                              AreaId1 = custMobile.AreaId1,
                              AreaId2 = custMobile.AreaId2,
                              AreaId3 = custMobile.AreaId3,
                              AreaId4 = custMobile.AreaId4,
                              AreaId5 = custMobile.AreaId5,
                              AreaName1 = custMobile.AreaName1,
                              AreaName2 = custMobile.AreaName2,
                              AreaName3 = custMobile.AreaName3,
                              AreaName4 = custMobile.AreaName4,
                              AreaName5 = custMobile.AreaName5,
                              Lat = custMobile.Lat,
                              Lng = custMobile.Lng,
                              InitialAddress = custMobile.InitialAddress,
                              Address1 = custMobile.Address1,
                              Address2 = custMobile.Address2,
                              Phone = custMobile.Phone,
                              Fax = custMobile.Fax,
                              ContactPerson = custMobile.ContactPerson,
                              IsActive = true,
                              UpdatedDate = custMobile.UpdatedDate,
                          });

        var data = dataOriginal.Union(dataMobile).OrderBy(x => x.UpdatedDate).AsQueryable();

        if (!string.IsNullOrEmpty(mobileLastSync))
        {
            switch (mobileLastSync.Length)
            {
                case 19:
                    mobileLastSync += ".0000";
                    break;
                case 21:
                    mobileLastSync += "000";
                    break;
                case 22:
                    mobileLastSync += "00";
                    break;
                case 23:
                    mobileLastSync += "0";
                    break;
            }
            data = data.Where(x => x.UpdatedDate > DateTime.ParseExact(mobileLastSync, "yyyy-MM-ddTHH:mm:ss.ffff", null));
        }

        if (!string.IsNullOrEmpty(search))
        {
            data = data.Where(x =>
                x.Code.Contains(search) || x.Initial.Contains(search) || x.Name.Contains(search) || x.TypeName.Contains(search) ||
                x.Address1.Contains(search) || x.Phone.Contains(search));
        }
        return data.ToDataSourceResult(skip, take, filter, sort);
    }

    public DataSourceResult GetLists(IEnumerable<Filter> filters, IEnumerable<Sort> sorts)
    {
        var data = Db.VwCustomers.Where(x => x.IsActive);

        return data.ToDataSourceResult(0, -1, filters, sorts);
    }

    public IEnumerable<CustomerAddress> GetAddress(string code)
    {
        var data = Db.CustomerAddress.Where(x => x.Code == code);

        return data.OrderBy(x => x.Id);
    }

    public VwCustomer FindByCode(string code)
    {
        return Db.VwCustomers.SingleOrDefault(x => x.Code.Equals(code));
    }

    public SaveResult Insert(CustomerRequest data)
    {
        var result = new SaveResult(false);

        using var transaction = Db.Database.BeginTransaction();

        try
        {
            // Checking initial already exists or not
            if (IsInitialExists(data.Initial, ""))
            {
                result.Message = "Inisial sudah terdaftar. Tolong gunakan inisial lain.";
                return result;
            }
            // handling mobile sign in
            AddOrUpdateMobileSignIn(data);

            // Get new code
            var newCode = GetNewCode("CUST_NUM_FMT", data.CreatedDate);

            // Insert data
            data.Code = newCode;
            Db.Customers.Add(data);
            Db.SaveChanges();

            // Get temporary billing & shipping
            var tempBillingId = data.BillingAddressId != null ? data.BillingAddressId : 0;
            var tempShippingId = data.ShippingAddressId != null ? data.ShippingAddressId : 0;
            var tempBillingInitial = "";
            var tempShippingInitial = "";

            if (data.ItemDetails.Any())
            {
                // Insert detail data
                foreach (var item in data.ItemDetails)
                {
                    // Checking initial already exists or not
                    if (IsInitialAddressExists(data, data.Initial))
                    {
                        result.Message = "Alamat inisial sudah terdaftar. Tolong gunakan alamat inisial lain pada daftar alamat.";
                        return result;
                    }

                    // Check user's choice
                    if (tempBillingId == item.Id)
                    {
                        tempBillingInitial = item.Initial;
                    }

                    if (tempShippingId == item.Id)
                    {
                        tempShippingInitial = item.Initial;
                    }

                    Db.CustomerAddress.Add(new CustomerAddress
                    {
                        Code = newCode,
                        Initial = item.Initial,
                        Address1 = item.Address1,
                        Address2 = item.Address2,
                        ContactPerson = item.ContactPerson,
                        Phone = item.Phone,
                        Fax = item.Fax,
                        IsDefault = item.IsDefault
                    });
                }
            }

            if (data.IsConsignee)
            {
                Db.Warehouses.Add(new Warehouse
                {
                    Code = data.Code,
                    Initial = String.IsNullOrWhiteSpace(Db.SystemParameters.FirstOrDefault(x => x.Code == "CNEE_PREFIX_WHS_INITIAL")?.Value) ? data.Initial : $"{Db.SystemParameters.FirstOrDefault(x => x.Code == "CNEE_PREFIX_WHS_INITIAL")?.Value} - {data.Initial}",
                    Name = String.IsNullOrWhiteSpace(Db.SystemParameters.FirstOrDefault(x => x.Code == "CNEE_PREFIX_WHS_NAME")?.Value) ? data.Name : $"{Db.SystemParameters.FirstOrDefault(x => x.Code == "CNEE_PREFIX_WHS_NAME")?.Value} - {data.Name}",
                    IsActive = true,
                    CreatedBy = data.CreatedBy,
                    CreatedDate = data.CreatedDate,
                    UpdatedBy = data.UpdatedBy,
                    UpdatedDate = data.UpdatedDate,
                    CustCode = data.Code
                });
            }

            Db.SaveChanges();

            if (data.ItemDetails.Any())
            {
                // Update billing & shipping address id
                data.BillingAddressId = GetIdAddress(tempBillingInitial, newCode);
                data.ShippingAddressId = GetIdAddress(tempShippingInitial, newCode);

                Db.Customers.Update(data);
                Db.Entry(data).Property(e => e.Code).IsModified = false;
                Db.Entry(data).Property(e => e.CreatedBy).IsModified = false;
                Db.Entry(data).Property(e => e.CreatedDate).IsModified = false;
                Db.SaveChanges();
            }
            transaction.Commit();
        }
        catch (Exception ex)
        {
            result.Message = ex.InnerException?.Message ?? ex.Message;
            return result;
        }

        result.Success = true;
        result.Data = data.Code;
        result.Message = "Data pelanggan berhasil disimpan.";
        return result;
    }

    public SaveResult Update(CustomerRequest data)
    {
        var result = new SaveResult(false);

        // Checking initial already exists or not
        if (IsInitialExists(data.Initial, data.Code))
        {
            result.Message = "Inisial sudah terdaftar. Tolong gunakan inisial lain.";
            return result;
        }

        //Check if any consignee warehouse has item
        if ((!data.IsActive || !data.IsConsignee) && Db.WarehouseQuantities.Any(x => x.WarehouseCode == data.Code))
        {
            result.Message = "Tidak bisa menonaktifkan data pelanggan karena terdapat barang pada gudang konsinyi.";
            return result;
        }

        // handling mobile sign in
        AddOrUpdateMobileSignIn(data);

        // Update data
        Db.Customers.Update(data);
        Db.Entry(data).Property(e => e.Code).IsModified = false;
        Db.Entry(data).Property(e => e.CreatedBy).IsModified = false;
        Db.Entry(data).Property(e => e.CreatedDate).IsModified = false;

        // Delete existing item detail
        var delDetails = Db.CustomerAddress
            .Where(d => d.Code == data.Code && !data.ItemDetails.Select(x => x.Id).Contains(d.Id))
            .ToList();

        Db.CustomerAddress.RemoveRange(delDetails);

        // Get temporary billing & shipping to prevent if user choose a new address
        var tempBillingId = data.BillingAddressId != null ? data.BillingAddressId : 0;
        var tempShippingId = data.ShippingAddressId != null ? data.ShippingAddressId : 0;
        var tempBillingInitial = "";
        var tempShippingInitial = "";

        if (data.ItemDetails.Any())
        {
            // Update detail data
            foreach (var item in data.ItemDetails)
            {
                // Checking initial already exists or not
                if (IsInitialAddressExists(data, item.Initial))
                {
                    result.Message = "Alamat inisial sudah terdaftar. Tolong gunakan alamat inisial lain pada daftar alamat.";
                    return result;
                }

                if (item.Id < 0)
                {
                    // Check user's choice
                    if (tempBillingId == item.Id)
                    {
                        tempBillingInitial = item.Initial;
                    }

                    if (tempShippingId == item.Id)
                    {
                        tempShippingInitial = item.Initial;
                    }

                    Db.CustomerAddress.Add(new CustomerAddress
                    {
                        Code = data.Code,
                        Initial = item.Initial,
                        Address1 = item.Address1,
                        Address2 = item.Address2,
                        ContactPerson = item.ContactPerson,
                        Phone = item.Phone,
                        Fax = item.Fax,
                        IsDefault = item.IsDefault
                    });
                }
                else
                {
                    Db.CustomerAddress.Update(item);
                    Db.Entry(item).Property(e => e.Id).IsModified = false;
                    Db.Entry(item).Property(e => e.Code).IsModified = false;
                }
            }
        }

        if (data.IsConsignee)
        {
            var dataWH = Db.Warehouses.FirstOrDefault(x => x.Code == data.Code);
            if (dataWH != null)
            {
                dataWH.IsActive = true;
                dataWH.Initial = String.IsNullOrWhiteSpace(Db.SystemParameters.FirstOrDefault(x => x.Code == "CNEE_PREFIX_WHS_INITIAL")?.Value) ? data.Initial : $"{Db.SystemParameters.FirstOrDefault(x => x.Code == "CNEE_PREFIX_WHS_INITIAL")?.Value} - {data.Initial}";
                dataWH.Name = String.IsNullOrWhiteSpace(Db.SystemParameters.FirstOrDefault(x => x.Code == "CNEE_PREFIX_WHS_NAME")?.Value) ? data.Name : $"{Db.SystemParameters.FirstOrDefault(x => x.Code == "CNEE_PREFIX_WHS_NAME")?.Value} - {data.Name}";
                dataWH.UpdatedBy = data.UpdatedBy;
                dataWH.UpdatedDate = data.UpdatedDate;
                Db.Warehouses.Update(dataWH);
            }
            else
            {
                Db.Warehouses.Add(new Warehouse
                {
                    Code = data.Code,
                    Initial = String.IsNullOrWhiteSpace(Db.SystemParameters.FirstOrDefault(x => x.Code == "CNEE_PREFIX_WHS_INITIAL")?.Value) ? data.Initial : $"{Db.SystemParameters.FirstOrDefault(x => x.Code == "CNEE_PREFIX_WHS_INITIAL")?.Value} - {data.Initial}",
                    Name = String.IsNullOrWhiteSpace(Db.SystemParameters.FirstOrDefault(x => x.Code == "CNEE_PREFIX_WHS_NAME")?.Value) ? data.Name : $"{Db.SystemParameters.FirstOrDefault(x => x.Code == "CNEE_PREFIX_WHS_NAME")?.Value} - {data.Name}",
                    IsActive = true,
                    CreatedBy = data.CreatedBy,
                    CreatedDate = data.CreatedDate,
                    UpdatedBy = data.UpdatedBy,
                    UpdatedDate = data.UpdatedDate,
                    CustCode = data.Code
                });
            }
        }
        else
        {
            var dataWH = Db.Warehouses.FirstOrDefault(x => x.Code == data.Code);
            if (dataWH != null)
            {
                dataWH.IsActive = false;
                Db.Warehouses.Update(dataWH);
            }
        }

        Db.SaveChanges();

        if (data.ItemDetails.Any())
        {
            // Update billing & shipping address id
            if (data.BillingAddressId < 0)
            {
                data.BillingAddressId = GetIdAddress(tempBillingInitial, data.Code);
            }

            if (data.ShippingAddressId < 0)
            {
                data.ShippingAddressId = GetIdAddress(tempShippingInitial, data.Code);
            }

            // Update data
            Db.Customers.Update(data);
            Db.Entry(data).Property(e => e.Code).IsModified = false;
            Db.Entry(data).Property(e => e.CreatedBy).IsModified = false;
            Db.Entry(data).Property(e => e.CreatedDate).IsModified = false;
            Db.SaveChanges();
        }

        result.Success = true;
        result.Data = data.Code;
        result.Message = "Data pelanggan berhasil diperbarui.";
        return result;
    }

    public SaveResult Delete(string code, int userId)
    {
        var result = new SaveResult(false);

        var data = Db.Customers.Find(code);
        if (data != null)
        {
            //Check if any sales order already using this customer
            if (Db.SalesOrderHeaders.Any(x => x.CustCode == data.Code))
            {
                result.Message = "Tidak bisa menghapus data pelanggan karena telah digunakan pada data order penjualan.";
                return result;
            }

            //Check if any sales order already using this warehouse 
            if (Db.SalesOrderHeaders.Any(x => x.WarehouseCode == data.Code))
            {
                result.Message = "Tidak bisa menghapus data gudang konsinyi karena telah digunakan pada data order penjualan.";
                return result;
            }

            //Check if any consignee warehouse has item
            if (Db.WarehouseQuantities.Any(x => x.WarehouseCode == data.Code))
            {
                result.Message = "Tidak bisa menghapus data pelanggan karena terdapat barang pada gudang konsinyi.";
                return result;
            }

            var addData = Db.CustomerAddress.Where(x => x.Code == data.Code);
            Db.CustomerAddress.RemoveRange(addData);

            var dataWH = Db.Warehouses.FirstOrDefault(x => x.Code == data.Code);
            Db.Warehouses.Remove(dataWH);

            Db.SaveChanges();

            Db.Customers.Remove(data);

            Db.SaveChanges();
        }

        result.Success = true;
        result.Message = "Data pelanggan berhasil dihapus.";
        return result;
    }

    private bool IsInitialExists(string initial, string code)
    {
        return Db.Customers.Any(x => x.Initial == initial && x.Code != code);
    }

    private bool IsInitialAddressExists(CustomerRequest data, string initial)
    {
        var result = 0;

        foreach (var row in data.ItemDetails)
        {
            if (row.Initial == initial)
            {
                if (result == 0)
                {
                    result += 1;
                }
                else
                {
                    return true;
                }
            }
        }

        return false;
    }

    private int GetIdAddress(string initial, string code)
    {
        var addrDetails = Db.CustomerAddress
            .Where(d => d.Initial == initial && d.Code == code)
            .ToList();

        if (addrDetails.Count > 0)
        {
            return (addrDetails[0].Id > 0) ? addrDetails[0].Id : 0;
        }

        return 0;
    }

    private SaveResult AddOrUpdateMobileSignIn(CustomerRequest data)
    {
        var result = new SaveResult(false);
        if (data.MobileSignIn)
        {
            result = UpdateMobileSignIn(data);
        }
        else
        {
            DeleteMobileSignIn(data);
        }
        result.Success = true;
        return result;
    }
    private SaveResult UpdateMobileSignIn(CustomerRequest data)
    {
        var result = new SaveResult(false);

        var tenant = _catalogCtx.Tenants.FirstOrDefault(x => x.Id == _claim.TenantId);
        if (tenant == null)
        {
            result.Message = "Tenant tidak terdaftar.";
            return result;
        }

        if (data.CatalogUserId == null)
        {
            if (string.IsNullOrWhiteSpace(data.MobileUsername) || string.IsNullOrWhiteSpace(data.MobilePassword))
            {
                result.Message = "Username atau password tidak boleh kosong.";
                return result;
            }
            var userCtg = _catalogCtx.CustomerUsers.FirstOrDefault(x => x.Username == data.MobileUsername);
            if (userCtg != null)
            {
                result.Message = "Username sudah terdaftar.";
                return result;
            }

            var pwh = new PasswordHasher<UserCatalog>();
            var newCatalogUser = new UserCatalog()
            {
                Id = new Guid(),
                TenantId = _claim.TenantId,
                Username = data.MobileUsername
            };
            var hashPwd = pwh.HashPassword(newCatalogUser, data.MobilePassword);
            newCatalogUser.Password = hashPwd;

            _catalogCtx.Add(newCatalogUser);
            _catalogCtx.SaveChanges();
            data.CatalogUserId = newCatalogUser.Id;
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(data.MobileUsername) && !string.IsNullOrWhiteSpace(data.MobilePassword))
            {

                var pwh = new PasswordHasher<UserCatalog>();
                var newCatalogUser = new UserCatalog()
                {
                    Id = new Guid(),
                    TenantId = _claim.TenantId,
                    Username = data.MobileUsername
                };
                var hashPwd = pwh.HashPassword(newCatalogUser, data.MobilePassword);

                var customerUser = _catalogCtx.CustomerUsers.FirstOrDefault(x => x.Id.Equals(data.CatalogUserId));
                if (customerUser != null)
                {
                    customerUser.Username = data.MobileUsername;
                    customerUser.Password = hashPwd;
                    _catalogCtx.SaveChanges();
                }
            }
        }

        result.Success = true;
        return result;
    }

    public void DeleteMobileSignIn(CustomerRequest data)
    {
        if (data.CatalogUserId != null)
        {
            var custUser = _catalogCtx.CustomerUsers.FirstOrDefault(x => x.TenantId.Equals(_claim.TenantId) && x.Id.Equals(data.CatalogUserId));
            if (custUser != null)
            {
                _catalogCtx.Remove(custUser);
                _catalogCtx.SaveChanges();
                data.CatalogUserId = null;
                data.MobileSignIn = false;
                data.MobileUsername = null;
            }
        }
    }

    public IEnumerable<CustomerAddress> GetCustomerAddress()
    {
        var data = Db.CustomerAddress;

        return data.OrderBy(x => x.Id);
    }
}