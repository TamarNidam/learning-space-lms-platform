using Learning_Space.Models;
using System.ComponentModel.DataAnnotations;

namespace Learning_Space.DTO
{
    public class TaskDTO
    {
        public int TaskId { get; set; }

        public string TaskType { get; set; }
        [Required]
        public DateOnly StartDate { get; set; }
        [Required]
        public DateOnly EndDate { get; set; }

        public int? CourseId { get; set; }

        public string? CourseName { get; set; }
        [Required]
        public string Subject { get; set; }
        [Required]
        public string Context {  get; set; }

        public int Done { get; set; } =0;
        public string? PerformanceContent { get; set; }

    }
}
