//using ERP_API.Domain.Entities;
//using ERP_API.Domain.Interfaces.Master;
//using ERP_API.Domain.Models;
//using Microsoft.EntityFrameworkCore;

//namespace ERP_API.Domain.Services.Master
//{
//    public class MasterService : IMasterService
//    {
//        protected TenantContext Db;

//        protected MasterService(TenantContext db)
//        {
//            Db = db;
//        }
//        public SaveResult ValidateDuplicateData(string query)
//        {
//            var result = new SaveResult(true);
//            using (var command = Db.Database.GetDbConnection().CreateCommand())
//            {
//                command.CommandText = query;
//                Db.Database.OpenConnection();
//                using (var reader = command.ExecuteReader())
//                {
//                    if (reader.HasRows)
//                    {
//                        result.Success = false;
//                        result.Message = "Data already exist.";
//                    }
//                }
//            }
//            return result;
//        }
//    }
//}
