namespace Learning_Space.DTO
{
    public class AlarmDTO
    {
        public int AlarmId { get; set; }

        public int CourseId { get; set; }

        public string CorseName { get; set; }

        public string AlarmType { get; set; }

        public int TypeId { get; set; }

        public int TaskId { get; set; } =0;

    }
}
