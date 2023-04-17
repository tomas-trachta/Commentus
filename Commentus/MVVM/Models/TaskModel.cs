
namespace Commentus.MVVM.Models
{
    public class Task
    {
        public string Name { get; set; }
        public string Text { get; set; }
        public List<string> Solvers { get; set; }

        public string DueDatetime { get; set; }
    }
}
