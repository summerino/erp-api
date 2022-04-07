using System.Linq;
using Microsoft.EntityFrameworkCore;
using ERP.Entity.Catalog;

namespace ERP.Entity;

public class CatalogContext : DbContext
{
    public CatalogContext(DbContextOptions<CatalogContext> options)
        : base(options)
    {
    }

    public DbSet<CustomerUser> CustomerUsers { get; set; }
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Set database collation
        modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");

        // Force all string-based columns to non-unicode equivalent when no column type is explicitly set
        foreach (
            var property in modelBuilder.Model
                .GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => p.ClrType == typeof(string) &&  // Entity is a string
                            p.GetColumnType() == null &&    // No column type is set
                            !p.DeclaringEntityType.GetTableName().StartsWith("AspNet")))
        {
            property.SetIsUnicode(false);
        }
    }
}