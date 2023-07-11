using MySqlConnector;

namespace AdminToolV3.CommonClass
{
    public class DBManager : Singleton<DBManager>
    {
        public async Task<MySqlConnection> AdminConnection()
        {
            MySqlConnection connection = new MySqlConnection
            {
                ConnectionString = CommonDefine.Configuration?.GetSection("ConnectionStrings").GetValue<string>("Admin") ?? ""
            };

            await connection.OpenAsync().ConfigureAwait(false);
            return connection;
        }

        public async Task<MySqlConnection> CommonConnection()
        {
            MySqlConnection connection = new MySqlConnection
            {
                ConnectionString = CommonDefine.Configuration?.GetSection("ConnectionStrings").GetValue<string>("Common") ?? ""
            };
            await connection.OpenAsync().ConfigureAwait(false);
            return connection;
        }

        public async Task<MySqlConnection> GameConnection()
        {
            MySqlConnection connection = new MySqlConnection
            {
                ConnectionString = CommonDefine.Configuration?.GetSection("ConnectionStrings").GetValue<string>("Game") ?? ""
            };
            await connection.OpenAsync().ConfigureAwait(false);
            return connection;
        }

        public async Task<MySqlConnection> LogConnection()
        {
            MySqlConnection connection = new MySqlConnection
            {
                ConnectionString = CommonDefine.Configuration?.GetSection("ConnectionStrings").GetValue<string>("Log") ?? ""
            };
            await connection.OpenAsync().ConfigureAwait(false);
            return connection;
        }
    }
}
