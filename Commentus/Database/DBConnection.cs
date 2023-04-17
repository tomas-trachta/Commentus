using MySql.Data.MySqlClient;

namespace Commentus.Database
{
    public class DBConnection
    {
        private MySqlConnection _mySqlConnection;

        public bool Connect(string connectionString)
        {
            try
            {
                _mySqlConnection = new MySqlConnection();
                _mySqlConnection.ConnectionString = connectionString;
                _mySqlConnection.Open();
            }
            catch (MySqlException)
            {
                return false;
            }

            SetUpTables();

            return true;
        }

        public MySqlConnection MySqlConnection
        {
            get { return _mySqlConnection; }
            set { _mySqlConnection = value; }
        }

        private void SetUpTables()
        {
            string query = "CREATE TABLE IF NOT EXISTS users(" +
                "Id INT NOT NULL AUTO_INCREMENT," +
                "Name varchar(255) NOT NULL," +
                "Salt BLOB NOT NULL," +
                "Password BLOB NOT NULL," +
                "IsAdmin tinyint(1) NOT NULL DEFAULT 0," +
                "ProfilePicture LONGBLOB," +
                "PRIMARY KEY(Id));" +
                "\n";

            query += "CREATE TABLE IF NOT EXISTS rooms(" +
                "Id INT NOT NULL AUTO_INCREMENT," +
                "Name varchar(255) NOT NULL," +
                "PRIMARY KEY(Id));" +
                "\n";

            query += "CREATE TABLE IF NOT EXISTS rooms_members(" +
                "User_id INT NOT NULL AUTO_INCREMENT," +
                "Room_id INT NOT NULL," +
                "FOREIGN KEY (User_id) REFERENCES users(Id) ON UPDATE CASCADE ON DELETE CASCADE," +
                "FOREIGN KEY (Room_id) REFERENCES rooms(Id) ON UPDATE CASCADE ON DELETE CASCADE);" +
                "\n";

            query += "CREATE TABLE IF NOT EXISTS rooms_messages(" +
                "Id INT NOT NULL AUTO_INCREMENT," +
                "User_id INT NOT NULL," +
                "Room_id INT NOT NULL," +
                "Message LONGBLOB NOT NULL," +
                "timestamp TIMESTAMP DEFAULT CURRENT_TIMESTAMP," +
                "PRIMARY KEY (Id)," +
                "FOREIGN KEY (User_id) REFERENCES users(Id) ON UPDATE CASCADE ON DELETE CASCADE," +
                "FOREIGN KEY (Room_id) REFERENCES rooms(Id) ON UPDATE CASCADE ON DELETE CASCADE);" +
                "\n";

            query += "CREATE TABLE IF NOT EXISTS tasks(" +
                "Id INT NOT NULL AUTO_INCREMENT," +
                "Rooms_id INT NOT NULL," +
                "Name varchar(255) NOT NULL," +
                "Description TEXT NOT NULL," +
                "DueDate DATE NOT NULL," +
                "timestamp TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP," +
                "PRIMARY KEY (Id)," +
                "FOREIGN KEY (Rooms_id) REFERENCES rooms(Id) ON UPDATE CASCADE ON DELETE CASCADE);" +
                "\n";

            query += "CREATE TABLE IF NOT EXISTS tasks_solvers(" +
                "Id INT NOT NULL PRIMARY KEY AUTO_INCREMENT,"+
                "Task_id INT NOT NULL," +
                "User_id INT NOT NULL," +
                "FOREIGN KEY (Task_id) REFERENCES tasks(Id) ON UPDATE CASCADE ON DELETE CASCADE," +
                "FOREIGN KEY (User_id) REFERENCES users(Id) ON UPDATE CASCADE ON DELETE CASCADE);" +
                "\n";

            query += "CREATE INDEX IF NOT EXISTS idx_userid " +
                "ON rooms_members (User_id);" +
                "\n";

            query += "CREATE INDEX IF NOT EXISTS idx_roomid " +
                "ON rooms_messages (Room_id);";

            Query.ExecNonQuery(query, this.MySqlConnection);
        }
    }
}
