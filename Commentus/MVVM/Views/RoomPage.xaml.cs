namespace Commentus.MVVM.Views;

using Commentus.Database;
using Commentus.MVVM.ViewModels;
using MySql.Data.MySqlClient;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Drawing;
using Image = Microsoft.Maui.Controls.Image;
using SkiaSharp;
using System.Text;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;

public partial class RoomPage : ContentPage
{
    #region Definitions
    private readonly MainViewModel _vm;
    private Timer messagesTimer;
    public string lastMessageTimestamp = DateTime.MinValue.ToString("yyyy-MM-dd HH:mm:ss");
    public string lastTaskTimestamp = DateTime.MinValue.ToString("yyyy-MM-dd HH:mm:ss");
    private Label selectedLabel;
    private bool disposed = false;
    private System.Timers.Timer updateTimer;
    private Timer tasksTimer;
    #endregion
    public RoomPage(string Title)
	{
        this.Title = Title;

        InitializeComponent();

        this.SizeChanged += OnSizeChanged;

        _vm = new()
        {
            RoomsName = this.Title
        };

        DatabaseCommands.GetRoomId(_vm);
        DatabaseCommands.GetRoomMembers(_vm);
        messagesTimer = DatabaseCommands.SetUpMessageTimer(_vm, this);
        tasksTimer = DatabaseCommands.SetUpTaskTimer(_vm, this);

        if (MainViewModel.Instance.IsAdmin)
        {
            var menubaritem = new MenuBarItem() { Text = "Options"};

            var flyoutRename = new MenuFlyoutItem() { Text = "Rename this room", Command = new Command(DisplayRenameEntry) };
            var flyoutDelete = new MenuFlyoutItem() { Text = "Delete this room", Command = new Command(ExecuteDeletePageCommand) };

            menubaritem.Add(flyoutRename);
            menubaritem.Add(flyoutDelete);

            this.MenuBarItems.Add(menubaritem);
        }

        this.BindingContext = _vm;
    }

    protected override void OnDisappearing()
    {
        messagesTimer?.Dispose();
        tasksTimer?.Dispose();
        disposed = true;
    }

    protected override void OnAppearing()
    {
        DatabaseCommands.GetRoomMembers(_vm);

        if (messagesTimer == null)
        {
            messagesTimer = DatabaseCommands.SetUpMessageTimer(_vm, this);
            tasksTimer = DatabaseCommands.SetUpTaskTimer(_vm, this);
        }
        else if (disposed)
        {
            messagesTimer = DatabaseCommands.SetUpMessageTimer(_vm, this);
            tasksTimer = DatabaseCommands.SetUpTaskTimer(_vm, this);
            disposed = false;
        }
    }

    private void OnSizeChanged(object sender, EventArgs e)
    {
        var width = this.Width;
        var height = this.Height;

        MainHStackLayout.WidthRequest = width;
        VStackLayout.WidthRequest = width * 0.5;
        MessagesListView.HeightRequest = height * 0.95;
        TasksCollectionView.WidthRequest = width * 0.25;
        HStackLayout.HeightRequest = height * 0.05;
        ChatEntry.WidthRequest = width * 0.5 * 0.6;
        previewImage.HeightRequest = height * 0.95;
        ChatSend.WidthRequest = width * 0.25 * 0.4;
        ScrollDown.WidthRequest = width * 0.25 * 0.4;

        if (MainViewModel.Instance.IsAdmin == true)
        {
            MembersListView.HeightRequest = height * 0.9 - LabelMembers.Height;
            TasksCollectionView.HeightRequest = height - AddTaskButton.Height - DeleteTaskButton.Height;

            AddTaskButton.HeightRequest = height * 0.05;
            AddTaskButton.WidthRequest = width * 0.25;

            DeleteTaskButton.HeightRequest = height * 0.05;
            DeleteTaskButton.WidthRequest = width * 0.25;

            AddUserButton.HeightRequest = height * 0.05;
            AddUserButton.WidthRequest = width * 0.25;

            RemoveUserButton.HeightRequest = height * 0.05;
            RemoveUserButton.WidthRequest = width * 0.25;


            taskTitleEntry.HeightRequest = height * 0.05;
            taskTitleEntry.WidthRequest = width * 0.5;
            TaskStackLayout.WidthRequest = width * 0.5;
            TaskStackLayout.HeightRequest = height;
            htmltext.WidthRequest = width * 0.5;
            htmltext.HeightRequest = height * 0.65;
            submitHtmlButton.HeightRequest = height * 0.05;
            updateHtmlButton.HeightRequest = height * 0.05;
            backButton.HeightRequest = height * 0.05;
            duedateStackLayout.HeightRequest = height * 0.1;
            htmlEditor.HeightRequest = height - height * 0.65 - (height * 0.05) * 4;
        }
        else
        {
            MembersListView.HeightRequest = height - LabelMembers.Height;
            TasksCollectionView.HeightRequest = height;


            TaskStackLayout.WidthRequest = width * 0.5;
            TaskStackLayout.HeightRequest = height;
            htmltext.WidthRequest = width * 0.5;
            submitHtmlButton.HeightRequest = 0;
            updateHtmlButton.HeightRequest = 0;
            submitHtmlButton.WidthRequest = 0;
            updateHtmlButton.WidthRequest = 0;
            backButton.HeightRequest = height * 0.05;
            backButton.WidthRequest = width * 0.5;
            duedateStackLayout.HeightRequest = 0;
            htmltext.HeightRequest = height - height * 0.05;
        }
        MembersListView.WidthRequest = width * 0.25;
    }

    private void ChatEntry_Completed(object sender, EventArgs e)
    {
        string WindowsPathPattern = @"^[a-zA-Z]:\\(((?![<>:""/\\|?*]).)+((?<![ .])\\)?)*$";
        string MacOsPattern = @"^(?:\/|(?:\/[\w.-]+)+)(?:\/?)$";

        if (_vm.ChatEntry == string.Empty)
            return;

        if (Regex.IsMatch(_vm.ChatEntry, WindowsPathPattern) || Regex.IsMatch(_vm.ChatEntry, MacOsPattern))
        {
            DatabaseCommands.SendImageToDb(_vm);
        }

        else
        {
            DatabaseCommands.SendMessage(_vm);
        }

        _vm.ChatEntry = string.Empty;
        MessagesListView.IsVisible = true;
        previewImage.IsVisible = false;
    }

    private void ChatMessage_Tapped(object sender, EventArgs e)
    {
        if (selectedLabel != null)
        {
            selectedLabel.Text = selectedLabel.Text.Remove(selectedLabel.Text.Length - 10);
            selectedLabel.BackgroundColor = Colors.Transparent;
        }

        var label = (Label)sender;

        label.BackgroundColor = Colors.LightGray;
        selectedLabel = label;
        Clipboard.SetTextAsync(label.Text.Trim());
        selectedLabel.Text += " - Coppied";
    }

    private void ChatImage_Tapped(object sender, EventArgs e)
    {
        var image = (Image)sender;
        string src = image.Source.ToString().Remove(0, 5);
        Clipboard.SetTextAsync(src);
    }

    private void ChatEntry_TextChanged(object sender, TextChangedEventArgs e)
    {
        if(_vm.ChatEntry == string.Empty)
        {
            MessagesListView.IsVisible = true;
            previewImage.IsVisible = false;
            return;
        }
        string URIPattern = @"^(?:\w+:)?\/\/([^\s\.]+\.\S{2}|localhost[\:?\d]*)\S*$";
        string WindowsPathPattern = @"^[a-zA-Z]:\\(((?![<>:""/\\|?*]).)+((?<![ .])\\)?)*$";
        string MacOsPattern = @"^(?:\/|(?:\/[\w.-]+)+)(?:\/?)$";

        if(Regex.IsMatch(_vm.ChatEntry,URIPattern) ||
           Regex.IsMatch(_vm.ChatEntry, WindowsPathPattern) ||
           Regex.IsMatch(_vm.ChatEntry, MacOsPattern))
        {
            _vm.PreviewImageSource = _vm.ChatEntry;
            MessagesListView.IsVisible = false;
            previewImage.IsVisible = true;
        }
    }

    private void ScrollDown_Clicked(object sender, EventArgs e)
    {
        this.Dispatcher.Dispatch(
         () => {
             MessagesListView.ScrollTo(_vm.Messages.Count - 1, animate: false);
         });
    }

    private void AddTaskButton_Clicked(object sender, EventArgs e)
    {
        _vm.TaskText = "";
        _vm.TaskTitle = "";

        dueDatePicker.MinimumDate = DateTime.Today;

        solversCollectionView.SelectedItems.Clear();
        _vm.Solvers.Clear();

        MessagesListView.IsVisible = false;
        HStackLayout.IsVisible = false;

        submitHtmlButton.IsVisible = true;
        updateHtmlButton.IsVisible = false;
        TaskStackLayout.IsVisible = true;
    }

    private void htmlEditor_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (updateTimer == null)
        {
            updateTimer = new System.Timers.Timer(500);
            updateTimer.Elapsed += OnUpdateTimerElapsed;
        }
        else
        {
            updateTimer.Stop();
        }
        if(updateTimer!= null)
            updateTimer.Start();
    }

    private void OnUpdateTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
        htmltext.Dispatcher.Dispatch(() => htmltext.Source = new HtmlWebViewSource { Html = _vm.TaskText});

        updateTimer.Stop();
    }

    private void BackButton_Clicked(object sender, EventArgs e)
    {
        MessagesListView.IsVisible = true;
        HStackLayout.IsVisible = true;
        TaskStackLayout.IsVisible = false;

        TasksCollectionView.SelectedItem = null;

        updateTimer?.Stop();
    }

    private void CollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _vm.Solvers.Clear();
        foreach(var item in solversCollectionView.SelectedItems)
        {
            _vm.Solvers.Add(((Tuple<ImageSource, string>)item).Item2);
        }
    }

    private void TasksCollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if(TasksCollectionView.SelectedItem != null)
        {
            var item = (Commentus.MVVM.Models.Task)TasksCollectionView.SelectedItem;

            _vm.TaskTitle = item.Name;
            _vm.DueDate = Convert.ToDateTime(item.DueDatetime);
            _vm.TaskText = item.Text;
            _vm.Solvers.Clear();

            foreach (var solver in item.Solvers)
            {
                _vm.Solvers.Add(solver);
            }

            dueDatePicker.MinimumDate = DateTime.Today;

            solversCollectionView.SelectedItems.Clear();
            _vm.Solvers.Clear();

            MessagesListView.IsVisible = false;
            HStackLayout.IsVisible = false;

            submitHtmlButton.IsVisible = false;
            updateHtmlButton.IsVisible = true;
            TaskStackLayout.IsVisible = true;

            if (!MainViewModel.Instance.IsAdmin)
            {
                taskTitleEntry.IsVisible = false;
                htmlEditor.IsVisible = false;
            }
        }
    }

    private void DisplayRenameEntry()
    {
        RenameEntry.IsVisible = true;
    }

    private void ExecuteRenamePageCommand(object sender, EventArgs e)
    {
        _vm.RenameRoomCommand.Execute(this);
        _vm.RoomsName = _vm.ChangedName;
        this.Title = _vm.RoomsName;
        RenameEntry.IsVisible = false;
    }

    private void ExecuteDeletePageCommand()
    {
        _vm.DeleteRoomCommand.Execute(this);
    }

    private void RemoveUserButton_Clicked(object sender, EventArgs e)
    {
        Tuple<ImageSource,string> item = (Tuple<ImageSource,string>)MembersListView.SelectedItem;
        _vm.selectedUserToBeRemoved = item.Item2;

        _vm.RemoveUserFromRoomCommand.Execute(this);
        DatabaseCommands.GetRoomMembers(_vm);
    }

    private void DeleteTaskButton_Clicked(object sender, EventArgs e)
    {
        var item = (Commentus.MVVM.Models.Task)TasksCollectionView.SelectedItem;
        _vm.selectedTaskToBeDeleted = item.Name;

        _vm.DeleteTaskCommand.Execute(this);
        _vm.Tasks.Clear();
        lastTaskTimestamp = DateTime.MinValue.ToString("yyyy-MM-dd HH:mm:ss");
        tasksTimer = DatabaseCommands.SetUpTaskTimer(_vm, this);
    }
}