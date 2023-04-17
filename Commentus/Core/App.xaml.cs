using Commentus.Cryptography;
using Commentus.Database;
using Commentus.MVVM.ViewModels;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Platform;
using MySql.Data.MySqlClient;

namespace Commentus;

public partial class App : Application
{
    public App()
	{
		InitializeComponent();

		MainPage = new AppShell();
    }
}
