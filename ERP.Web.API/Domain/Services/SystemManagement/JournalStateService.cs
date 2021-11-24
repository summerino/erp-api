using ERP.Common;
using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.SystemManagement;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ERP.Common.Models;
using ERP.Entity.SystemManagement;

namespace ERP.Web.API.Domain.Services.SystemManagement
{
    public class JournalStateService : IJournalStateService
    {
        private readonly TenantContext _db;
        public JournalStateService(TenantContext db)
        {
            _db = db;
        }

        public JournalState GetStatusPost(int userId)
        {
            var data = _db.JournalStates.OrderByDescending(x => x.Id).FirstOrDefault(x => x.UserId == userId);
            return data;
        }
    }
}
