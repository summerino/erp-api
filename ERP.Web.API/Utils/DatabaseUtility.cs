using Microsoft.Data.SqlClient;

namespace ERP.Web.API.Utils;

public static class DatabaseUtility
{
    public static string GetTenantSlug(string instanceName) => instanceName.ToLower().Replace(' ', '-');

    public static bool TryConnectToSqlDatabase(string connectionString)
    {
        try
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                conn.Close();
            }

            return true;
        } 
        catch (SqlException ex)
        {
            Console.WriteLine("{0}\n{1}\n", ex.Message, connectionString);
            return false;
        }
    }

    public static bool DatabaseExists(string connectionString)
    {
        SqlConnectionStringBuilder sqlConnectionString = new SqlConnectionStringBuilder(connectionString);
        SqlConnectionStringBuilder sqlMasterConnectionString = new SqlConnectionStringBuilder(connectionString);

        sqlMasterConnectionString.InitialCatalog = "master";

        using (SqlConnection conn = new SqlConnection(sqlMasterConnectionString.ConnectionString))
        {
            conn.Open();

            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM SYS.DATABASES WHERE NAME = @dbname";
            cmd.Parameters.AddWithValue("@dbname", sqlConnectionString.InitialCatalog);
            cmd.CommandTimeout = 60;

            int count = (int)cmd.ExecuteScalar();
            conn.Close();

            return count > 0;
        }
    }

    private static string BracketEscapeName(string sqlName)
    {
        return '[' + sqlName.Replace("]", "]]") + "]";
    }

    public static string CreateDatabase(string connectionString)
    {
        SqlConnectionStringBuilder sqlConnectionString = new SqlConnectionStringBuilder(connectionString);
        SqlConnectionStringBuilder sqlMasterConnectionString = new SqlConnectionStringBuilder(connectionString);

        sqlMasterConnectionString.InitialCatalog = "master";
        string databaseName = string.Empty;

        try
        {
            using (SqlConnection conn = new SqlConnection(sqlMasterConnectionString.ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = conn.CreateCommand();

                cmd.CommandText = "SELECT SERVERPROPERTY('EngineEdition')";
                cmd.CommandTimeout = 60;

                databaseName = BracketEscapeName(sqlConnectionString.InitialCatalog);
                cmd.CommandText = string.Format("CREATE DATABASE {0}", databaseName);

                cmd.ExecuteScalar();
                conn.Close();

                Console.WriteLine($"{databaseName} created!");
                return string.Format("CREATE DATABASE {0}", databaseName);
            }
        } 
        catch (Exception)
        {
            throw;
        }
    }
}