using Commentus.Cryptography;
using Commentus.Database;
using Commentus.MVVM.ViewModels;

namespace Commentus;

public partial class MainPage : ContentPage
{
    public MainPage()
	{
        InitializeComponent();

        ConfigManager.LoadConfig();
    }

    protected override void OnAppearing()
    {
        if(MainViewModel.Instance.DbConnection != null && MainViewModel.Instance.IsAdmin == true)
        {
            var flyout = new MenuFlyoutItem() { Text = "Give admin privileges", Command = new Command(ExecuteOpenGiveAdminPrivilegesPageCommand) };

            MenuBar.Add(flyout);
        }
    }
    
    protected override void OnDisappearing()
    {
        if(MenuBar.Count > 1)
            MenuBar.RemoveAt(1);
    }

    private void ExecuteOpenGiveAdminPrivilegesPageCommand()
    {
        MainViewModel.Instance.OpenGiveAdminPrivilegesPageCommand.Execute(this);
    }
    private void Register_Tapped(object sender, EventArgs e)
    {
        loginGrid.IsVisible = false;
        registerGrid.IsVisible = true;
    }

    private void LogIn_Tapped(object sender, EventArgs e)
    {
        loginGrid.IsVisible = true;
        registerGrid.IsVisible = false;
    }
}

