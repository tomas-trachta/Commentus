using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Commentus.MVVM.Views;

using Commentus.Database;
using Commentus.MVVM.ViewModels;

public partial class GiveAdminPrivilegesPage : ContentPage
{
    private readonly MainViewModel _vm;
    public event NotifyCollectionChangedEventHandler CollectionChanged;
    public ObservableCollection<string> Users;
    private List<string> removedUsers;
    public GiveAdminPrivilegesPage(MainViewModel vm)
	{
        _vm = vm;

		InitializeComponent();

        removedUsers = new List<string>();
        Users = DatabaseCommands.GetAllNonAdminUsers(_vm);
        Users.CollectionChanged += OnCollectionChanged;
        UsersCollectionView.ItemsSource = Users;

        BindingContext = _vm;
    }

    private void NameEntry_TextChanged(object sender, TextChangedEventArgs e)
    {
        for (int i = removedUsers.Count - 1; i >= 0; i--)
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

    private void UsersCollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _vm.selectedUserToBeGivenPrivileges = UsersCollectionView.SelectedItem.ToString();
    }

    private void OnCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        CollectionChanged?.Invoke(this, e);
    }
}