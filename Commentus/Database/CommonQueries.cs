
using Commentus.MVVM.ViewModels;

namespace Commentus.Database
{
    public class CommonQueries
    {
        public Dictionary<string, string> CommonQuery;

        public CommonQueries(MainViewModel _vm, string lastTimestamp = null)
        {
            CommonQuery = new Dictionary<string, string>()
            {
                    {
                        "AddRoomToDatabase",
                        $"INSERT INTO rooms (Name) " +
                        $"VALUES ('{_vm.RoomName}');"
                    },
                    {
                        "CheckIfRoomExists",
                        $"SELECT Name FROM rooms " +
                        $"WHERE Name='{_vm.RoomName}';"
                    },
                    {
                        "SelectIdOfRoomByName",
                        $"SELECT Id FROM rooms " +
                        $"WHERE Name='{_vm.RoomName}';"
                    },
                    {
                        "SelectIdOfSpecificRoom",
                        $"SELECT Id FROM rooms " +
                        $"WHERE Name='{_vm.RoomsName}';"
                    },
                    {
                        "GetMyId",
                        $"SELECT Id FROM users " +
                        $"WHERE Name='{_vm.Name}';"
                    },
                    {
                        "GetMyProfilePicture",
                        $"SELECT Profilepicture FROM users " +
                        $"WHERE Name='{_vm.Name}';"
                    },
                    {
                        "LoginQuery",
                        $"SELECT Name,Password,IsAdmin,Salt FROM users " +
                        $"WHERE Name='{_vm.Name}';"
                    },
                    {
                        "CheckIfNameExists",
                        $"SELECT Name FROM users " +
                        $"WHERE Name='{_vm.Name}'"
                    },
                    {
                        "RetrieveRoomsNames",
                        $"SELECT rooms.Name " +
                        $"FROM rooms_members " +
                        $"INNER JOIN rooms ON rooms_members.Room_id=rooms.Id " +
                        $"INNER JOIN users ON rooms_members.User_id=users.Id " +
                        $"WHERE User_id={_vm.Id};"
                    },
                    {
                        "RetrieveAllRoomsNames",
                        $"SELECT Name " +
                        $"FROM rooms;"
                    },
                    {
                       "RetrieveNewMessages",
                       $"SELECT users.Name,Message,timestamp " +
                       $"FROM rooms_messages " +
                       $"INNER JOIN users ON rooms_messages.User_id=users.Id " +
                       $"WHERE Room_id={_vm.RoomsId} " +
                       $"AND timestamp > '{lastTimestamp}';"
                    },
                    {
                        "InsertMessageToDb",
                        $"INSERT INTO rooms_messages (User_id,Room_id,Message) " +
                        $"VALUES ({MainViewModel.Instance.Id},{_vm.RoomsId},'{_vm.ChatEntry}');"
                    },
                    {
                        "InsertImageToDb",
                        $"INSERT INTO rooms_messages (User_id,Room_id,Message) " +
                        $"VALUES ({MainViewModel.Instance.Id},{_vm.RoomsId},@image_data);"
                    },
                    {
                        "InsertProfilePictureToDb",
                        $"UPDATE users " +
                        $"SET Profilepicture=@image_data " +
                        $"WHERE Name='{MainViewModel.Instance.Name}';"
                    },
                    {
                        "RetrieveRoomsMembers",
                        $"SELECT users.Profilepicture, users.Name " +
                        $"FROM rooms_members " +
                        $"INNER JOIN users ON rooms_members.User_id=users.Id " +
                        $"WHERE Room_id={_vm.RoomsId};"
                    },
                    {
                        "GetAllUsersNotInRoom",
                        $"SELECT Name FROM users " +
                        $"WHERE Id!={MainViewModel.Instance.Id} AND Id NOT IN " +
                        $"(SELECT User_id FROM rooms_members WHERE Room_id={_vm.RoomsId});"
                    },
                    {
                        "GetAllNonAdminUsers",
                        $"SELECT Name " +
                        $"FROM users " +
                        $"WHERE Id!={MainViewModel.Instance.Id} AND Name NOT IN " +
                        $"(SELECT Name FROM users WHERE IsAdmin=1);"
                    },
                    {
                        "AddUserToRoom",
                        $"INSERT INTO rooms_members (User_id,Room_id) " +
                        $"VALUES ((SELECT Id FROM users WHERE Name='{_vm.selectedUserToBeAdded}'),{_vm.RoomsId});"
                    },
                    {
                        "GiveAdminPrivileges",
                        $"UPDATE users " +
                        $"SET IsAdmin=1 " +
                        $"WHERE Name='{_vm.selectedUserToBeGivenPrivileges}';"
                    },
                    {
                        "AddTaskToDb",
                        $"INSERT INTO tasks (Rooms_id,Name,Description,DueDate) " +
                        $"VALUES ({_vm.RoomsId},'{_vm.TaskTitle}','{_vm.TaskText}','{_vm.DueDate.ToString("yyyy-MM-dd")}');"
                    },
                    {
                        "DeleteTask",
                        $"DELETE FROM tasks WHERE Name='{_vm.selectedTaskToBeDeleted}' AND Rooms_id={_vm.RoomsId};"
                    },
                    {
                        "RetrieveTasksFromDb",
                        $"SELECT tasks.Id,Name,Description,DueDate,timestamp " +
                        $"FROM tasks_solvers " +
                        $"INNER JOIN tasks ON tasks_solvers.Task_id=tasks.Id " +
                        $"WHERE Rooms_id={_vm.RoomsId} AND timestamp > '{lastTimestamp}' AND User_id={MainViewModel.Instance.Id};"
                    },
                    {
                        "UpdateTask",
                        $"UPDATE tasks " +
                        $"SET Description='{_vm.TaskText}', DueDate='{_vm.DueDate.ToString("yyyy-MM-dd")}' " +
                        $"WHERE Name='{_vm.TaskTitle}';"
                    },
                    {
                        "RenameRoom",
                        $"UPDATE rooms " +
                        $"SET Name='{_vm.ChangedName}' " +
                        $"WHERE Name='{_vm.RoomsName}';"
                    },
                    {
                        "DeleteRoom",
                        $"DELETE FROM rooms " +
                        $"WHERE Name='{_vm.RoomsName}';"
                    },
                    {
                        "RemoveUserFromRoom",
                        $"DELETE FROM rooms_members " +
                        $"WHERE User_id=(SELECT Id FROM users WHERE Name='{_vm.selectedUserToBeRemoved}') AND Room_id={_vm.RoomsId};"
                    }
            };
        }
    }
}
