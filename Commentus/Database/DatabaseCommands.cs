using Commentus.MVVM.ViewModels;
using Commentus.MVVM.Views;
using MySql.Data.MySqlClient;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.RegularExpressions;
using Commentus.Cryptography;
using Commentus.MVVM.Models;

namespace Commentus.Database
{
    public static class DatabaseCommands
    {
        public static void SendMessage(MainViewModel vm)
        {
            var CommonQuery = new CommonQueries(vm).CommonQuery;
            Query.ExecNonQuery(CommonQuery["InsertMessageToDb"], MainViewModel.Instance.DbConnection);
        }

        public static void RetrieveUserData(MainViewModel vm)
        {
            vm.RoomsNames.Clear();

            var CommonQuery = new CommonQueries(MainViewModel.Instance).CommonQuery;
            string query = vm.IsAdmin ? CommonQuery["RetrieveAllRoomsNames"] : CommonQuery["RetrieveRoomsNames"];
            MySqlDataReader reader = Query.ExecReader(query, MainViewModel.Instance.DbConnection);

            while (reader.Read())
            {
                vm.RoomsNames.Add((string)reader[0]);
            }
            reader.Close();

            foreach (var name in vm.RoomsNames)
            {
                AppShell.Current.Items.Add(new RoomPage(name));
                Routing.RegisterRoute(name, typeof(RoomPage));
            }
        }

        public static void GetRoomId(MainViewModel vm)
        {
            var reader = Query.ExecReader(new CommonQueries(vm).CommonQuery["SelectIdOfSpecificRoom"],
                MainViewModel.Instance.DbConnection);

            if (reader.HasRows)
            {
                reader.Read();
                vm.RoomsId = reader.GetInt32("Id");
            }

            reader.Close();
        }

        public static void GetRoomMembers(MainViewModel vm)
        {
            vm.Members.Clear();
            var reader = Query.ExecReader(new CommonQueries(vm).CommonQuery["RetrieveRoomsMembers"],
                MainViewModel.Instance.DbConnection);

            while (reader.Read())
            {
                if (!reader.IsDBNull(0))
                {
                    long len = reader.GetBytes(0, 0, null, 0, 0);
                    byte[] binaryData = new byte[len];
                    reader.GetBytes(0, 0, binaryData, 0, (int)len);

                    SKBitmap bitmap;

                    using (var stream = new MemoryStream(binaryData))
                    {
                        bitmap = SKBitmap.Decode(stream);
                    }

                    ImageSource src = bitmap.ToImageSource();

                    vm.Members.Add(new Tuple<ImageSource, string>((ImageSource)src, (string)reader[1]));
                }
                else
                    vm.Members.Add(new Tuple<ImageSource, string>(null, (string)reader[1]));
            }

            reader.Close();
        }

        public static bool LogInUser(MainViewModel vm)
        {

            vm.IsAdmin = false;

            var CommonQuery = new CommonQueries(MainViewModel.Instance).CommonQuery;
            MySqlDataReader reader = Query.ExecReader(CommonQuery["LoginQuery"], vm.DbConnection);

            if (reader.HasRows)
            {
                reader.Read();

                byte[] salt = (byte[])reader[3];
                var hash = (byte[])reader[1];

                byte[] hashBytes = PasswordManager.ComputeHash(vm.Password,salt);  

                if (hashBytes.SequenceEqual(hash))
                {
                    if (Convert.ToBoolean(reader[2]) == true)
                        vm.IsAdmin = true;

                    vm.IsLoggedIn = true;
                    reader.Close();

                    MySqlDataReader id = Query.ExecReader(CommonQuery["GetMyId"], vm.DbConnection);
                    id.Read();
                    vm.Id = (int)id[0];
                    id.Close();


                    MySqlDataReader profilepicture = Query.ExecReader(CommonQuery["GetMyProfilePicture"], vm.DbConnection);
                    profilepicture.Read();

                    if (!profilepicture.IsDBNull(0))
                    {
                        long len = profilepicture.GetBytes(0, 0, null, 0, 0);
                        byte[] binaryData = new byte[len];
                        profilepicture.GetBytes(0, 0, binaryData, 0, (int)len);
                        SKBitmap bitmap;

                        using (var stream = new MemoryStream(binaryData))
                        {
                            bitmap = SKBitmap.Decode(stream);
                        }

                        ImageSource src = bitmap.ToImageSource();

                        vm.ProfileImage = src;
                    }
                    else
                        vm.ProfileImage = null;

                    profilepicture.Close();

                    return true;
                }
            }
            reader.Close();
            return false;
        }

        public static string RegisterUser(MainViewModel vm)
        {
            var CommonQuery = new CommonQueries(MainViewModel.Instance).CommonQuery;
            try
            {
                var reader = Query.ExecReader(CommonQuery["CheckIfNameExists"], vm.DbConnection);
                if (reader.HasRows)
                {
                    reader.Close();
                    return "This name already exists in database!";
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            try
            {
                var hashedPassword = PasswordManager.HashPassword(vm.Password);
                byte[] hashBytes = hashedPassword.Item1;
                byte[] salt = hashedPassword.Item2;

                string query = $"INSERT INTO users (Name,Password,Salt) VALUES ('{vm.Name}',@hash,@saltBytes);";

                var command = new MySqlCommand(query, MainViewModel.Instance.DbConnection);
                command.Parameters.AddWithValue(@"hash", hashBytes);
                command.Parameters.AddWithValue(@"saltBytes", salt);

                command.ExecuteNonQuery();

                vm.IsLoggedIn = true;

                MySqlDataReader id = Query.ExecReader(CommonQuery["GetMyId"], vm.DbConnection);
                id.Read();
                vm.Id = (int)id[0];
                id.Close();

                return "You have been successfully registered";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public static bool InsertNewRoomToDb(MainViewModel vm)
        {
            var CommonQuery = new CommonQueries(MainViewModel.Instance).CommonQuery;

            MySqlDataReader reader = Query.ExecReader(CommonQuery["CheckIfRoomExists"], vm.DbConnection);

            if (reader.HasRows == true)
            {
                reader.Close();
                return false;
            }
            reader.Close();

            Query.ExecNonQuery(CommonQuery["AddRoomToDatabase"], vm.DbConnection);

            reader = Query.ExecReader(CommonQuery["SelectIdOfRoomByName"], vm.DbConnection);
            reader.Read();

            string query = $"INSERT INTO rooms_members (User_id, Room_id) VALUES ('{vm.Id}','{(int)reader[0]}');";

            reader.Close();

            Query.ExecNonQuery(query, vm.DbConnection);

            vm.RoomsNames.Add(vm.RoomName);

            return true;
        }

        public static Timer SetUpMessageTimer(MainViewModel vm, RoomPage page)
        {
            string connectionString = MainViewModel.Instance.DbConnection.ConnectionString;

            return new Timer(_ => {
                var CommonQuery = new CommonQueries(vm, page.lastMessageTimestamp).CommonQuery;

                using (var conn = new MySqlConnection(connectionString))
                using (MySqlCommand command = new MySqlCommand(CommonQuery["RetrieveNewMessages"], conn))
                {
                    conn.Open();
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string author = (string)reader[0];
                            long len = reader.GetBytes(1, 0, null, 0, 0);
                            byte[] binaryData = new byte[len];
                            reader.GetBytes(1, 0, binaryData, 0, (int)len);

                            //checks wheter the binary data represents jpg or png using file signature and magic number
                            if ((binaryData.Length > 3 && binaryData[0] == 0xFF && binaryData[1] == 0xD8
                                && binaryData[binaryData.Length - 2] == 0xFF && binaryData[binaryData.Length - 1] == 0xD9)
                                || (binaryData[0] == 137 && binaryData[1] == 80 && binaryData[2] == 78 && binaryData[3] == 71
                                && binaryData[4] == 13 && binaryData[5] == 10 && binaryData[6] == 26 && binaryData[7] == 10))
                            {
                                SKBitmap bitmap;

                                using (var stream = new MemoryStream(binaryData))
                                {
                                    bitmap = SKBitmap.Decode(stream);
                                }

                                ImageSource src = bitmap.ToImageSource();

                                vm.Messages.Add(new Message { Name = author, Content = src, IsText = false, IsImage = true });
                            }
                            else
                            {
                                string message = Encoding.UTF8.GetString((byte[])reader[1]);
                                string URIPattern = @"^(?:\w+:)?\/\/([^\s\.]+\.\S{2}|localhost[\:?\d]*)\S*$";

                                if (Regex.IsMatch(message, URIPattern))
                                {
                                    vm.Messages.Add(new Message { Name = author, Content = message, IsText = false, IsImage = true });
                                }
                                else
                                {
                                    vm.Messages.Add(new Message { Name = author, Content = message, IsText = true, IsImage = false });
                                }
                            }
                        }

                        if (reader.HasRows)
                        {
                            page.lastMessageTimestamp = Convert.ToDateTime(reader[2]).ToString("yyyy-MM-dd HH:mm:ss");
                        }
                    }
                }
            }, null, TimeSpan.Zero, TimeSpan.FromSeconds(1.5));
        }

        public static Timer SetUpTaskTimer(MainViewModel vm, RoomPage page)
        {
            string connectionString = MainViewModel.Instance.DbConnection.ConnectionString;

            return new Timer(_ => {
                string query = "";

                var CommonQuery = new CommonQueries(vm, page.lastTaskTimestamp).CommonQuery;

                using (var conn = new MySqlConnection(connectionString))
                using (MySqlCommand command = new MySqlCommand(CommonQuery["RetrieveTasksFromDb"], conn))
                {
                    conn.Open();
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            query = $"SELECT users.Name FROM tasks_solvers " +
                                    $"INNER JOIN users ON tasks_solvers.User_id=users.Id " +
                                    $"WHERE Task_id={reader.GetInt32("Id")};";

                            var newConnection = new MySqlConnection(connectionString);
                            newConnection.Open();
                            
                            using (var solversReader = Query.ExecReader(query, newConnection))
                            {

                                List<string> solvers = new List<string>();

                                while (solversReader.Read())
                                {
                                    solvers.Add(solversReader.GetString(0));
                                }

                                MVVM.Models.Task itemToRemove = null;

                                foreach(var task in vm.Tasks)
                                {
                                    if(task.Name == reader.GetString("Name"))
                                    {
                                        itemToRemove = task;
                                        break;
                                    }
                                }
                                
                                if(itemToRemove != null)
                                    vm.Tasks.Remove(itemToRemove);

                                vm.Tasks.Add(new MVVM.Models.Task()
                                {
                                    Name = reader.GetString("Name"),
                                    DueDatetime = reader.GetDateTime("DueDate").ToString("yyyy-MM-dd"),
                                    Solvers = solvers,
                                    Text = reader.GetString("Description")
                                });
                            }
                        }

                        if (reader.HasRows)
                        {
                            page.lastTaskTimestamp = reader.GetDateTime("timestamp").ToString("yyyy-MM-dd HH:mm:ss");
                        }
                    }
                }
            }, null, TimeSpan.Zero, TimeSpan.FromSeconds(30.0));
        }

        public static void SendImageToDb(MainViewModel vm)
        {
            string extension = Path.GetExtension(vm.ChatEntry);
            byte[] byteArray;
            if (extension.Contains("jpg") || extension.Contains("jpeg"))
            {
                byteArray = SKBitmap.Decode(vm.ChatEntry).ToByteArray(SKEncodedImageFormat.Jpeg);
            }
            else
            {
                byteArray = SKBitmap.Decode(vm.ChatEntry).ToByteArray(SKEncodedImageFormat.Png);
            }
            using var command = new MySqlCommand(new CommonQueries(vm).CommonQuery["InsertImageToDb"],
                                                 MainViewModel.Instance.DbConnection);
            command.Parameters.AddWithValue("@image_data", byteArray);
            command.ExecuteNonQuery();
        }

        public static void SendUsersProfileImageToDb(byte[] byteArray)
        {
            using var command = new MySqlCommand(new CommonQueries(MainViewModel.Instance).CommonQuery["InsertProfilePictureToDb"],
                                                 MainViewModel.Instance.DbConnection);
            command.Parameters.AddWithValue("@image_data", byteArray);
            command.ExecuteNonQuery();
        }

        public static ObservableCollection<string> GetAllUsersNotInRoom(MainViewModel vm)
        {
            var reader = Query.ExecReader(new CommonQueries(vm).CommonQuery["GetAllUsersNotInRoom"],
                                      MainViewModel.Instance.DbConnection);
            ObservableCollection<string> users = new ObservableCollection<string>();
            while (reader.Read())
            {
                users.Add(reader.GetString("Name"));
            }
            reader.Close();
            return users;
        }

        public static ObservableCollection<string> GetAllNonAdminUsers(MainViewModel vm)
        {
            var reader = Query.ExecReader(new CommonQueries(vm).CommonQuery["GetAllNonAdminUsers"],
                                      MainViewModel.Instance.DbConnection);
            ObservableCollection<string> users = new ObservableCollection<string>();
            while (reader.Read())
            {
                users.Add(reader.GetString("Name"));
            }
            reader.Close();
            return users;
        }

        public static string AddUserToRoom(MainViewModel vm)
        {
            try
            {
                Query.ExecNonQuery(new CommonQueries(vm).CommonQuery["AddUserToRoom"],MainViewModel.Instance.DbConnection);
            }
            catch(Exception ex)
            {
                return ex.Message;
            }
            return "User has been added";
        }

        public static string GiveAdminPrivilegesToUser(MainViewModel vm)
        {
            try
            {
                Query.ExecNonQuery(new CommonQueries(vm).CommonQuery["GiveAdminPrivileges"], 
                    MainViewModel.Instance.DbConnection);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return "Privileges given successfully";
        }

        public static string AddTaskToDatabase(MainViewModel vm)
        {
            string query = $"SELECT Id FROM tasks WHERE Name='{vm.TaskTitle}' AND Rooms_id={vm.RoomsId};";
            try
            {
                var reader = Query.ExecReader(query, MainViewModel.Instance.DbConnection);

                if(reader.HasRows)
                {
                    reader.Close();
                    return "Task with this title already exists!";
                }
                reader.Close();
            }
            catch(Exception ex)
            {
                return ex.Message;
            }

            var CommonQuery = new CommonQueries(vm);
            try
            {
                Query.ExecNonQuery(CommonQuery.CommonQuery["AddTaskToDb"],MainViewModel.Instance.DbConnection);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            try
            {
                query = "INSERT INTO tasks_solvers (Task_id,User_id) VALUES ";
                foreach(var solver in vm.Solvers)
                {
                    query += $"((SELECT Id FROM tasks WHERE Name='{vm.TaskTitle}')," +
                            $"(SELECT Id FROM users WHERE Name='{solver}')),";
                }
                query = query.Remove(query.Length-1,1) + ";";
                Query.ExecNonQuery(query, MainViewModel.Instance.DbConnection);
            }
            catch(Exception ex)
            {
                return ex.Message;
            }

            return "Task has been added";
        }

        public static string UpdateTask(MainViewModel vm)
        {
            try
            {
                Query.ExecNonQuery(new CommonQueries(vm).CommonQuery["UpdateTask"], MainViewModel.Instance.DbConnection);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            if(vm.Solvers.Count > 0) 
            {
                try
                {
                    string query = "INSERT INTO tasks_solvers (Task_id,User_id) VALUES ";
                    foreach (var solver in vm.Solvers)
                    {
                        query += $"((SELECT Id FROM tasks WHERE Name='{vm.TaskTitle}')," +
                                $"(SELECT Id FROM users WHERE Name='{solver}')),";
                    }
                    query = query.Remove(query.Length - 1, 1) + ";";
                    Query.ExecNonQuery(query, MainViewModel.Instance.DbConnection);
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            }
            return "Task has been updated";
        }

        public static string RenameRoom(MainViewModel vm)
        {
            try
            {
                Query.ExecNonQuery(new CommonQueries(vm).CommonQuery["RenameRoom"],
                    MainViewModel.Instance.DbConnection);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return "Room has been renamed";
        }

        public static bool DeleteRoom(MainViewModel vm)
        {
            try
            {
                Query.ExecNonQuery(new CommonQueries(vm).CommonQuery["DeleteRoom"],
                    MainViewModel.Instance.DbConnection);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public static string RemoveUserFromRoom(MainViewModel vm)
        {
            try
            {
                Query.ExecNonQuery(new CommonQueries(vm).CommonQuery["RemoveUserFromRoom"],
                    MainViewModel.Instance.DbConnection);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return "User has been removed";
        }

        public static string DeleteTask(MainViewModel vm)
        {
            try
            {
                Query.ExecNonQuery(new CommonQueries(vm).CommonQuery["DeleteTask"],
                    MainViewModel.Instance.DbConnection);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return "Task has been deleted";
        }
    }
}
