namespace CodeFirst.Dal
{
    public class Teacher
    {
        public int Id { get; set; }
        public AdditionalProperties Person { get; set; }

        public List<Student> Students { get; set; } = new();
    }
}
