using Commentus.MVVM.Models;
using MySql.Data.MySqlClient;

namespace Commentus.MVVM.ViewModels
{
    public class UserViewModel
    {
        private readonly User _user;

        public UserViewModel(User user)
        {
            _user = user;
        }

        public bool IsLoggedIn
        {
            get { return _user.IsLoggedIn; }
            set { _user.IsLoggedIn = value; }
        }

        public int Id
        {
            get { return _user.Id; }
            set { _user.Id = value; }
        }

        public string Name
        {
            get { return _user.Name; }
            set { _user.Name = value; }
        }

        public ImageSource ProfileImage
        {
            get { return _user.ProfileImage; }
            set { _user.ProfileImage = value; }
        }

        public bool IsAdmin
        {
            get { return _user.IsAdmin; }
            set { _user.IsAdmin = value; }
        }

        public MySqlConnection DbConnection
        {
            get { return _user.DbConnection;}
            set { _user.DbConnection = value;}
        }

        public string Password
        {
            get { return _user.Password; }
            set { _user.Password = value; }
        }

        public string DbConnectionName
        {
            get { return _user.DbConnectionName; }
            set { _user.DbConnectionName = value; }
        }

        public string DbConnectionPassword
        {
            get { return _user.DbConnectionPassword; }
            set { _user.DbConnectionPassword = value; }
        }

        public string DbConnectionServerAddress
        {
            get { return _user.DbConnectionServerAddress; }
            set { _user.DbConnectionServerAddress = value; }
        }

        public string DbConnectionDatabaseName
        {
            get { return _user.DbConnectionDatabaseName; }
            set { _user.DbConnectionDatabaseName = value; }
        }
    }
}
