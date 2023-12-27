using System.ComponentModel.DataAnnotations;

namespace Learning_Space.DTO
{
    public class ClassDTO
    {
        public int ClassId { get; set; }

        [RegularExpression(@"^[a-zA-Z0-9\s]+$",
            ErrorMessage = "Invalid characters in the Name field.")]
        [Required]
        public string ClassName { get; set; }

        public int Students { get; set; } = 0;
    }
}
