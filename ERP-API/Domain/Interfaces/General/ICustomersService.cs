using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ERP_API.Domain.Entities.General;
using ERP_API.Domain.Models;

namespace ERP_API.Domain.Interfaces.General
{
    public interface ICustomersService : IGeneralService<Customer>
    {
        Customer GetCustomers(string code);

        SaveResult Delete(string code, int userId);
    }
}
