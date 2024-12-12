using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace prototype.Model
{
    public class PageModel
    {
        public int TotalStudent { get; set; }

        public int Department { get; set; }

        public int ID { get; set; }

        public TimeOnly Time_In { get; set; }

        public string Name { get; set; } = string.Empty;
    }
}
