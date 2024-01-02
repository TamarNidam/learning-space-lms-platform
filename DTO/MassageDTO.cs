using Learning_Space.Models;

namespace Learning_Space.DTO
{
    public class MassageDTO
    {
        public int MessageId { get; set; }

        public int? CourseId { get; set; }

        public string MassageSubject { get; set; }

        public DateTime? MassageDateTime { get; set; }

        public string Content { get; set; }

    }
}
