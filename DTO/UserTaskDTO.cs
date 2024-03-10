using Learning_Space.Models;

namespace Learning_Space.DTO
{
    public class UserTaskDTO
    {
        public int UserTaskId { get; set; }

        public int? UserId { get; set; }

        public string UserName { get; set; }    = string.Empty;

        public int? TaskId { get; set; }
        public string TaskSubject { get; set; }

        public decimal? Mark { get; set; }

        public string Remarks { get; set; }

        public bool Done { get; set; }

    }
}
