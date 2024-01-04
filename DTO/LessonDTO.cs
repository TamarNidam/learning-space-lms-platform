using System.Security.Policy;

namespace Learning_Space.DTO
{
    public class LessonDTO
    {
        public int LessonId { get; set; }

        public int? CourseId { get; set; }

        public string? CourseName { get; set; }

        public string LessonSubject { get; set; }

        public DateOnly LessonDate { get; set; }

        public TimeOnly? StartTime { get; set; }

        public TimeOnly? EndTime { get; set; }

        public string LessonType { get; set; }

        public Url? ZoomUrl { get; set; }
    }
}
