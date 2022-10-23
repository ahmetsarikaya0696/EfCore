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
