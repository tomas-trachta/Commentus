namespace Commentus.MVVM.ViewModels
{
    using Commentus.Database;
    using Commentus.MVVM.Models;
    using MySql.Data.MySqlClient;
    using System.Collections.ObjectModel;
    using Commentus.MVVM.Views;
    using CommunityToolkit.Mvvm.Input;
    using CommunityToolkit.Mvvm.ComponentModel;
    using System.Collections.Specialized;
    using System.Collections.Generic;
    using Commentus.Cryptography;

    public partial class MainViewModel : ObservableObject, INotifyCollectionChanged
    {
        #region Definitions
        private User _user;
        private Room _room;

        public string selectedUserToBeAdded;
        public string selectedUserToBeGivenPrivileges;
        public string selectedUserToBeRemoved;
        public string selectedTaskToBeDeleted;

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        private static MainViewModel _instance;
        //Singleton for user's data
        public static MainViewModel Instance => _instance ??= new MainViewModel();

        public UserViewModel userViewModel;
        public RoomViewmodel roomViewModel;
        #endregion
        public MainViewModel()
        {
            _user = new User();
            _room = new Room();

            userViewModel = new UserViewModel(_user);
            roomViewModel = new RoomViewmodel(_room);

            Messages = new ObservableCollection<Message>();
            Members = new ObservableCollection<Tuple<ImageSource, string>>();
            RoomsNames = new ObservableCollection<string>();
            Tasks = new ObservableCollection<Task>();
            Solvers = new List<object>();

            RoomsNames.CollectionChanged += OnCollectionChanged;
            Members.CollectionChanged += OnCollectionChanged;
            RoomsNames.CollectionChanged += OnCollectionChanged;
            Tasks.CollectionChanged += OnCollectionChanged;


            userViewModel.IsLoggedIn = false;
        }

        #region properties
        public bool IsLoggedIn
        {
            get { return userViewModel.IsLoggedIn; }
            set { userViewModel.IsLoggedIn = value; }
        }

        public int Id
        {
            get { return userViewModel.Id; }
            set { userViewModel.Id = value; }
        }

        public string Name
        {
            get { return userViewModel.Name; }
            set { userViewModel.Name = value; OnPropertyChanged(nameof(Name)); }
        }

        public ImageSource ProfileImage
        {
            get { return userViewModel.ProfileImage; }
            set { userViewModel.ProfileImage = value; OnPropertyChanged(nameof(ProfileImage)); }
        }

        public bool IsAdmin
        {
            get { return userViewModel.IsAdmin; }
            set { userViewModel.IsAdmin = value; }
        }

        //helping properties
        [ObservableProperty]
        public string chatEntry;

        [ObservableProperty]
        public bool isImage;

        [ObservableProperty]
        public bool isText;

        [ObservableProperty]
        public string previewImageSource;

        [ObservableProperty]
        public string taskText;

        [ObservableProperty]
        public string taskTitle;

        [ObservableProperty]
        public DateTime dueDate;

        [ObservableProperty]
        public string roomName;

        [ObservableProperty]
        public string changedName;



        public List<object> Solvers { get; set; }

        public ObservableCollection<string> RoomsNames
        {
            get; set;
        }

        public string RoomsName
        {
            get { return roomViewModel.Name; }
            set { roomViewModel.Name = value; OnPropertyChanged(nameof(RoomsName)); }
        }

        public int RoomsId 
        { 
            get { return roomViewModel.Id; } 
            set { roomViewModel.Id = value; OnPropertyChanged(nameof(Name)); }
        }

        public ObservableCollection<Message> Messages
        {
            get{ return roomViewModel.Messages; }
            set { roomViewModel.Messages = value; }
        }

        public ObservableCollection<Tuple<ImageSource, string>> Members
        {
            get { return roomViewModel.Members; }
            set { roomViewModel.Members = value; }
        }

        public ObservableCollection<Task> Tasks
        {
            get { return roomViewModel.Tasks; }
            set { roomViewModel.Tasks = value; }
        }

        public MySqlConnection DbConnection
        {
            get { return userViewModel.DbConnection; }
            set { userViewModel.DbConnection = value; }
        }

        public string Password
        {
            get { return userViewModel.Password; }
            set { userViewModel.Password = value; OnPropertyChanged(nameof(Password)); }
        }

        public string DbConnectionName
        {
            get { return userViewModel.DbConnectionName; }
            set { userViewModel.DbConnectionName = value; }
        }

        public string DbConnectionPassword
        {
            get { return userViewModel.DbConnectionPassword; }
            set { userViewModel.DbConnectionPassword = value; }
        }

        public string DbConnectionServerAddress
        {
            get { return userViewModel.DbConnectionServerAddress; }
            set { userViewModel.DbConnectionServerAddress = value; }
        }

        public string DbConnectionDatabaseName
        {
            get { return userViewModel.DbConnectionDatabaseName; }
            set { userViewModel.DbConnectionDatabaseName = value; }
        }

        #endregion

        #region RelayCommands
        [RelayCommand]
        public void LogIn()
        {
            if (Name == null || Password == null || Name == string.Empty || Password == string.Empty)
            {
                Application.Current.MainPage.DisplayAlert("Register error", "Name or password are empty!", "ok");
            }
            else if (DbConnection == null)
            {
                Application.Current.MainPage.DisplayAlert("Register error", "Set up database connection!", "ok");
            }
            else
            {
                for (int i = AppShell.Current.Items.Count - 1; i > 0; i--)
                {
                    AppShell.Current.Items.RemoveAt(i);
                }

                if (DatabaseCommands.LogInUser(this))
                {
                    AppShell.Current.Items.Add(new ProfilePage());
                    Routing.RegisterRoute(nameof(ProfilePage), typeof(ProfilePage));

                    if (IsAdmin == true)
                    {
                        AppShell.Current.Items.Add(new CreateNewRoomPage());
                        Routing.RegisterRoute(nameof(CreateNewRoomPage), typeof(CreateNewRoomPage));
                    }

                    DatabaseCommands.RetrieveUserData(this);

                    Application.Current.MainPage.DisplayAlert("Login", "You have been successfully logged in", "OK");
                    AppShell.Current.CurrentItem = AppShell.Current.Items[1];
                }
                else
                    Application.Current.MainPage.DisplayAlert("Login", "Wrong credentials", "OK");
            }
        }

        [RelayCommand]
        public void Register()
        {
            if (Name == null || Password == null || Name == string.Empty || Password == string.Empty)
            {
                Application.Current.MainPage.DisplayAlert("Register error", "Name or password are empty!", "ok");
            }
            else if (DbConnection == null)
            {
                Application.Current.MainPage.DisplayAlert("Register error", "Set up database connection!", "ok");
            }
            else
            {
                string message = DatabaseCommands.RegisterUser(this);
                Application.Current.MainPage.DisplayAlert("Login", message, "ok");

                if (message == "You have been successfully registered")
                {
                    AppShell.Current.Items.Add(new ProfilePage());
                    Routing.RegisterRoute(nameof(ProfilePage), typeof(ProfilePage));

                    if (IsAdmin == true)
                    {
                        AppShell.Current.Items.Add(new CreateNewRoomPage());
                        Routing.RegisterRoute(nameof(CreateNewRoomPage), typeof(CreateNewRoomPage));
                    }
                }
            }
        }

        [RelayCommand]
        public void OpenSetUpDbPage()
        {
            AppShell.Current.Items.Add(new SetUpDbPage());
            AppShell.Current.CurrentItem = AppShell.Current.Items[AppShell.Current.Items.Count - 1];
        }

        [RelayCommand]
        public void OpenGiveAdminPrivilegesPage()
        {
            AppShell.Current.Items.Add(new GiveAdminPrivilegesPage(this));
            AppShell.Current.CurrentItem = AppShell.Current.Items[AppShell.Current.Items.Count - 1];
        }

        [RelayCommand]
        public void ConnectDatabase()
        {
            string connstring = $"server={DbConnectionServerAddress};uid={DbConnectionName};" +
                $"pwd={DbConnectionPassword};database={DbConnectionDatabaseName}";

            var conn = new DBConnection();
            if (conn.Connect(connstring))
            {
                DbConnection = conn.MySqlConnection;

                AppShell.Current.GoToAsync("//MainPage");
                AppShell.Current.Items.RemoveAt(AppShell.Current.Items.Count - 1);
                Application.Current.MainPage.DisplayAlert("Success", "Database has been successfully set up!", "OK");

                ConfigManager.SaveConfig(connstring);
                return;
            }

            Application.Current.MainPage.DisplayAlert("error", "Connection falied!", "ok");
        }

        [RelayCommand]
        public void CreateNewRoom()
        {
            if (DatabaseCommands.InsertNewRoomToDb(this))
            {
                var page = new RoomPage(RoomName);
                AppShell.Current.Items.Add(page);
                Routing.RegisterRoute(page.Title, typeof(RoomPage));

                Application.Current.MainPage.DisplayAlert("Room creation", "Room has been created", "ok");
                return;
            }

            Application.Current.MainPage.DisplayAlert("Room creation", "Room with this name already exists in database!", "ok");
        }

        [RelayCommand]
        public void OpenAddUserPage()
        {
            AppShell.Current.Items.Add(new AddUserPage(this));
            AppShell.Current.CurrentItem = AppShell.Current.Items[AppShell.Current.Items.Count - 1];
        }

        [RelayCommand]
        public void AddUserToRoom()
        {
            if(selectedUserToBeAdded!= null && selectedUserToBeAdded != string.Empty)
                Application.Current.MainPage.DisplayAlert("Action", DatabaseCommands.AddUserToRoom(this), "ok");

            AppShell.Current.CurrentItem = AppShell.Current.Items[1];
            AppShell.Current.Items.RemoveAt(AppShell.Current.Items.Count - 1);
        }

        [RelayCommand]
        public void GiveAdminPrivileges()
        {
            if(selectedUserToBeGivenPrivileges != null && selectedUserToBeGivenPrivileges != string.Empty)
                Application.Current.MainPage.DisplayAlert("Action", DatabaseCommands.GiveAdminPrivilegesToUser(this), "ok");

            AppShell.Current.GoToAsync("//MainPage");
            AppShell.Current.Items.RemoveAt(AppShell.Current.Items.Count - 1);
        }

        [RelayCommand]
        public void AddTaskToRoom()
        {
            Application.Current.MainPage.DisplayAlert("Action", DatabaseCommands.AddTaskToDatabase(this), "ok");    
        }

        [RelayCommand]
        public void UpdateTask()
        {
            Application.Current.MainPage.DisplayAlert("Action", DatabaseCommands.UpdateTask(this), "ok");
        }

        [RelayCommand]
        public void DeleteTask()
        {
            Application.Current.MainPage.DisplayAlert("Action", DatabaseCommands.DeleteTask(this), "ok");
        }

        [RelayCommand]
        public void RenameRoom()
        {
            Application.Current.MainPage.DisplayAlert("Action", DatabaseCommands.RenameRoom(this), "ok");
        }

        [RelayCommand]
        public void DeleteRoom()
        {
            if (DatabaseCommands.DeleteRoom(this))
            {
                var item = AppShell.Current.CurrentItem;
                AppShell.Current.CurrentItem = AppShell.Current.Items[1];
                AppShell.Current.Items.Remove(item);

                Application.Current.MainPage.DisplayAlert("Action", "Room has been deleted", "ok");
            }
            else
            {
                Application.Current.MainPage.DisplayAlert("Action", "Couldn't delete room", "ok");
            }
        }

        [RelayCommand]
        public void RemoveUserFromRoom()
        {
            Application.Current.MainPage.DisplayAlert("Action", DatabaseCommands.RemoveUserFromRoom(this), "ok");
        }
        #endregion

        private void OnCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged?.Invoke(this, e);
        }
    }
}
