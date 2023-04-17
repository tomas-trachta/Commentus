using CommunityToolkit.Mvvm.ComponentModel;
using MySql.Data.MySqlClient;

namespace Commentus.MVVM.Models
{
    public class User : ObservableObject
    {
        public bool IsLoggedIn { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public ImageSource ProfileImage { get; set; }
        public bool IsAdmin { get; set; }
        public MySqlConnection DbConnection { get; set; }
        public string DbConnectionName { get; set; }
        public string DbConnectionPassword { get; set; }
        public string DbConnectionServerAddress { get; set; }
        public string DbConnectionDatabaseName { get; set; }
    }
}
