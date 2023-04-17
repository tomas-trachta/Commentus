using System.Collections.ObjectModel;

namespace Commentus.MVVM.Models
{
    public class Room
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ObservableCollection<Tuple<ImageSource,string>> Members { get; set; }
        public ObservableCollection<Message> Messages { get; set; }
        public ObservableCollection <Task> Tasks { get; set; }
    }
}
