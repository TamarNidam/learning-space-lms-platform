using Learning_Space.Models;

namespace Learning_Space.DTO
{
    public class MoreStudyDTO
    {
        public int MoreId { get; set; }

        public int? CourseId { get; set; }

        public string MoreStudySubject { get; set; }

        public string Content { get; set; }

        public string MoreStudyUrl { get; set; }

       // public string CourseName { get; set; }
    }
}
