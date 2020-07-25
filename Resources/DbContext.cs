using MySql.Data.MySqlClient;
using System.Threading.Tasks;

namespace GTServer.Resources
{
    public static class DbContext
    {
        public static string ConnectionString { get; set; } = "server=localhost;uid=your-uid;password=your-password;database=your-db";
        public static MySqlConnection Connection { get; set; } = new MySqlConnection(ConnectionString);
        public static async Task OpenConnection() => await Connection.OpenAsync();
    }
}
