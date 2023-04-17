namespace Commentus.MVVM.Views;

using Commentus.MVVM.ViewModels;
using Commentus.Database;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

public partial class AddUserPage : ContentPage, INotifyCollectionChanged
{
    #region Definitions
    private readonly MainViewModel _vm;
    public event NotifyCollectionChangedEventHandler CollectionChanged;
    public ObservableCollection<string> Users;
    private List<string> removedUsers;
    #endregion

    public AddUserPage(MainViewModel vm)
	{
        _vm = vm;

        this.Title = "Add user to room " + _vm.RoomsId.ToString();
		InitializeComponent();

        removedUsers = new List<string>();
        Users = DatabaseCommands.GetAllUsersNotInRoom(_vm);
        Users.CollectionChanged += OnCollectionChanged;
        UsersCollectionView.ItemsSource = Users;

        BindingContext = _vm;
	}

    private void NameEntry_TextChanged(object sender, TextChangedEventArgs e)
    {
        for(int i = removedUsers.Count - 1; i >= 0; i--)
        {
            if (removedUsers[i].Contains(NameEntry.Text))
            {
                Users.Add(removedUsers[i]);
                removedUsers.RemoveAt(i);
            }
        }
        for (int i = Users.Count - 1; i >= 0; i--)
        {
            if (!Users[i].Contains(NameEntry.Text))
            {
                removedUsers.Add(Users[i]);
                Users.RemoveAt(i);
            }
        }
    }

    private void OnCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        CollectionChanged?.Invoke(this, e);
    }

    private void UsersCollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _vm.selectedUserToBeAdded = UsersCollectionView.SelectedItem.ToString();
    }
}