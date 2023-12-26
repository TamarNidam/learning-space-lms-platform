namespace Learning_Space.DTO
{
    public class CalendarDTO
    {
        public string Title { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }
        //public class MonthlyCalendarDTO
        //{
        //    public int Year { get; set; }
        //    public int Month { get; set; }
        //    public List<DailyEventsDTO> Days { get; set; } = new List<DailyEventsDTO>();
        //}

        //public class DailyEventsDTO
        //{
        //    public int Day { get; set; }
        //    public List<EventDTO> Events { get; set; } = new List<EventDTO>();
        //}
    }
}
