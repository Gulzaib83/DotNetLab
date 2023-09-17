using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities_ADO.Models
{
    public class ToDo
    {
        public int Id { get; set; }

        public string Title { get; set; } = null!;

        public bool? IsCompleted { get; set; }

        public string UserId { get; set; }
    }
}
