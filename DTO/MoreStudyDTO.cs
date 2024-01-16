using Learning_Space.Models;
using System.ComponentModel.DataAnnotations;

namespace Learning_Space.DTO
{
    public class MoreStudyDTO
    {
        public int MoreId { get; set; }

        public int? CourseId { get; set; }

        [Required]
        public string MoreStudySubject { get; set; }
        [Required]
        public string Content { get; set; }
        [Display(Name = "Study URL")]
        [Url(ErrorMessage = "Please enter a valid URL for the Study URL.")]
        public string? MoreStudyUrl { get; set; }

       // public string CourseName { get; set; }
    }
}
