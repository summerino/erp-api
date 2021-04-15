using ERP_API.Extensions;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using ERP_API.Domain.Entities.Core;
using ERP_API.Domain.Entities.General;

namespace ERP_API.Database
{
    public class DbInitializer
    {
        public void EnsureSeeded(ERPDbContext dbContext)
        {
            var resourceString = "ERP_API.Data.{0}.json";

            SeedEntity<Supplier>(string.Format(resourceString, "Supplier"), dbContext);
        }

        public void SeedEntity<T>(string resource, ERPDbContext dbContext, bool overrideValues = true) where T : BaseEntity
        {
            var assembly = Assembly.GetExecutingAssembly();

            using (Stream stream = assembly.GetManifestResourceStream(resource))
            using (StreamReader reader = new StreamReader(stream))
            {
                string result = reader.ReadToEnd();

                var jsonSettings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Include,
                };

                var types = JsonConvert.DeserializeObject<List<T>>(result, jsonSettings);

                dbContext.Set<T>().AddOrUpdateRange(types, dbContext.Shardingkey, overrideValues);
                dbContext.SaveChanges();
            }
        }
    }
}
