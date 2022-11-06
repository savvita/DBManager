using System.Data.SqlClient;

namespace DBManager.Model
{
    internal class DBConnection
    {
        private static SqlConnection? connection;
        private static readonly object _lock = new object();

        public static SqlConnection GetConnection(string connectionString)
        {
            if (connection == null)
            {
                lock(_lock)
                {
                    if (connection == null)
                    {
                        connection = new SqlConnection();
                    }
                }
            }

            if (connection.State != System.Data.ConnectionState.Open)
            {
                connection.ConnectionString = connectionString;
            }

            return connection;
        }

        private DBConnection()
        {

        }
    }
}
