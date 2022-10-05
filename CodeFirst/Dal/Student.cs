using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeFirst.Dal
{
    public class Student
    {
        public int Id { get; set; }
        public AdditionalProperties Person { get; set; }
        public int Grade { get; set; }


        public List<Teacher> Teachers { get; set; } = new();
    }
}
