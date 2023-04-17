using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commentus.MVVM.Models
{
    public class Message
    {
        public string Name { get; set; }
        public object Content { get; set; }
        public bool IsText { get; set; }
        public bool IsImage { get; set; }
    }
}
