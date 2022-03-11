using ERP.Common;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text.RegularExpressions;

namespace ERP.Web.API.Domain.Services
{
    public class GeneralQuery
    {
        public virtual SaveResult DynamicQuery(string connectionString, string query = null, string param1 = null,
            string param2 = null, string param3 = null, string param4 = null, string param5 = null, string source = null)
        {
            var result = new SaveResult(false);

            try
            {
                using var cn = new SqlConnection(connectionString);
                cn.Open();

                var finalQuery = "";
                     
                if (string.IsNullOrWhiteSpace(source))
                {
                    var trimmedQuery = Regex.Replace(query, @"\t|\n|\r", " ");
                    finalQuery = string.Format(trimmedQuery, param1 ?? "null", param2 ?? "null",
                        param3 ?? "null", param4 ?? "null", param5 ?? "null");
                }
                else
                {
                    finalQuery = $"SELECT * FROM {source}";
                }

                using var da = new SqlDataAdapter(finalQuery, cn);
                var ds = new DataSet();
                da.Fill(ds);

                if (ds.Tables[0].Rows.Count > 0)
                {
                    List<dynamic> columns = new();
                    for (int i = 0; i < ds.Tables[0].Columns.Count; i++)
                    {
                        columns.Add(new
                        {
                            ds.Tables[0].Columns[i].ColumnName,
                            ColumnType = ds.Tables[0].Columns[i].DataType.Name
                        });
                    }

                    result.Count = ds.Tables[0].Rows.Count;
                    result.Data = new
                    {
                        result = ds.Tables[0].Rows[0].Table,
                        columns
                    };
                }

                result.Success = true;
                result.Message = "Query Succeed.";
            }
            catch (Exception ex)
            {
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }

            return result;
        }
    }
}
