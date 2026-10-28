using JobAppHR.Models;
using Microsoft.Data.SqlClient;

namespace JobAppHR.Repository
{
    public class DBConnection
    {
        public SqlConnection GetDbConnection()
        {
            //var builder = new ConfigurationBuilder().AddJsonFile("appSettings.json");
            //configuration = builder.Build();
            //string connectionString = configuration["ConnectionStrings:DefaultConnection"];

            string connectionString = StaticData.DefaultConnection;

            return new SqlConnection(connectionString);
        }
    }
}
