using System.ComponentModel.DataAnnotations;

namespace Learning_Space.DTO
{
    public class CourseDTO
    {
        public int CourseId { get; set; }
        [RegularExpression(@"^[a-zA-Z0-9]+$",
            ErrorMessage = "Invalid characters in the CourseName field.")]
        [Required]
        public string CourseName { get; set; }
        [RegularExpression(@"^[a-zA-Z0-9]+$",
            ErrorMessage = "Invalid characters in the CourseDescription field.")]
        [Required]
        public string CourseDescription { get; set; }

        public int? TeacherId { get; set; }

        public string TeacherName { get; set; } = string.Empty;

        public int? ClassId {  get; set; }

        public string ClassName { get; set; } = string.Empty;
    }
}
