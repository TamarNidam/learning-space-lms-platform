using Learning_Space.Models;

namespace Learning_Space.DTO
{
    public class TaskDTO
    {
        public int TaskId { get; set; }

        public string TaskType { get; set; }

        public DateOnly? StartDate { get; set; }

        public DateOnly EndDate { get; set; }

        public int? CourseId { get; set; }

        public string CourseName { get; set; }
    }
}
