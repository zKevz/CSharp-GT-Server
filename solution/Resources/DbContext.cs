using MySql.Data.MySqlClient;
using System.Threading.Tasks;

namespace GTServer.Resources
{
    public static class DbContext
    {
        public static string ConnectionString { get; set; } = "your-mysql-config";
        public static MySqlConnection Connection { get; set; } = new MySqlConnection(ConnectionString);
        public static async Task OpenConnection() => await Connection.OpenAsync();
    }
}
