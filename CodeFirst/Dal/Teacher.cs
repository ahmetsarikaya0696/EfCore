using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeFirst.Dal
{
    public class Teacher
    {
        public int Id { get; set; }
        public AdditionalProperties Person { get; set; }

        public List<Student> Students { get; set; } = new();
    }
}
