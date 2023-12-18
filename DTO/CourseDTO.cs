namespace Learning_Space.DTO
{
    public class CourseDTO
    {
        public int CourseId { get; set; }

        public string CourseName { get; set; }

        public string CourseDescription { get; set; }

        public int TeacherId { get; set; }

        public string TeacherName { get; set; } = string.Empty;

        public int ClassId {  get; set; }

        public string ClassName { get; set; } = string.Empty;
    }
}
