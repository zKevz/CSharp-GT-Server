using MySql.Data.MySqlClient;
using System.Threading.Tasks;

namespace GTServer.Resources
{
    public static class DbContext
    {
        public static string ConnectionString { get; set; } = "server=localhost;uid=root;password=kevin1206;database=growtopiadb";
        public static MySqlConnection Connection { get; set; } = new MySqlConnection(ConnectionString);
        public static async Task OpenConnection() => await Connection.OpenAsync();
    }
}
