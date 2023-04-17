using Commentus.MVVM.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Task = Commentus.MVVM.Models.Task;

namespace Commentus.MVVM.ViewModels
{
    public class RoomViewmodel : ObservableObject, INotifyCollectionChanged
    {
        private readonly Room _room;
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public RoomViewmodel(Room room)
        {
            _room = room;
            _room.Messages = new ObservableCollection<Message>();
            _room.Messages.CollectionChanged += OnCollectionChanged;

            _room.Members = new ObservableCollection<Tuple<ImageSource, string>>();
            _room.Members.CollectionChanged += OnCollectionChanged;

            _room.Tasks = new ObservableCollection<Task>();
            _room.Tasks.CollectionChanged += OnCollectionChanged;
        }

        private void OnCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged?.Invoke(this, e);
        }

        public int Id
        {
            get { return _room.Id; }
            set { _room.Id = value; OnPropertyChanged(nameof(Name)); }
        }

        public string Name
        {
            get { return _room.Name; }
            set { _room.Name = value; OnPropertyChanged(nameof(Name));}
        }

        public ObservableCollection<Message> Messages
        {
            get { return _room.Messages; }
            set { _room.Messages = value; }
        }

        public ObservableCollection<Tuple<ImageSource, string>> Members
        {
            get { return _room.Members; }
            set { _room.Members = value; }
        }

        public ObservableCollection<Task> Tasks
        {
            get { return _room.Tasks; }
            set { _room.Tasks = value; }
        }
    }
}
