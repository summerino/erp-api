using ERP_API.Entities.Control;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERP_API
{
    public class ERPControlDbContext : DbContext
    {
        public ERPControlDbContext(DbContextOptions options) : base(options) { }
        public DbSet<ShardTable> ShardTable { get; set; }
        public DbSet<TenantDetails> TenantDetails { get; set; }
        public DbSet<UserTable> UserTable { get; set; }

    }
}
