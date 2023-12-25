namespace Learning_Space.DTO
{
    public class EventDTO
    {
        public int EventId { get; set; }
        public string Title { get; set; }
        public DateTime Date { get; set; }
        public string Type { get; set; }
        public string Description { get; set; } = string.Empty;
    }

}
