using MySql.Data.MySqlClient;

namespace Commentus.Database
{
    public static class Query
    {
        private static string _query;

        public static void ExecNonQuery(string query, MySqlConnection connection)
        {
            _query = query;

            var cmd = new MySqlCommand(_query, connection);
            cmd.ExecuteNonQuery();
        }

        public static MySqlDataReader ExecReader(string query, MySqlConnection connection)
        {
            _query = query;

            var cmd = new MySqlCommand(query, connection);
            return cmd.ExecuteReader();
        }

        public static object ExecScalar(string query, MySqlConnection connection)
        {
            _query = query;

            var cmd = new MySqlCommand(query, connection);
            return cmd.ExecuteScalar();
        }
    }
}
