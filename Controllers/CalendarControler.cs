using Microsoft.AspNetCore.Mvc;

namespace Learning_Space.Controllers
{
    using Learning_Space.DTO;
    using System;
    using System.Globalization;

    public class CalendarController
    {
        private List<DTO.EventDTO> _events;

        public CalendarController()
        {
            _events = new List<DTO.EventDTO>();
        }

        public void AddEvent(EventDTO eventDto)
        {
            _events.Add(eventDto);
        }

        public CalendarDTO.MonthlyCalendarDTO GetMonthlyCalendar(int year, int month)
        {
            var calendar = new CalendarDTO.MonthlyCalendarDTO
            {
                Year = year,
                Month = month
            };

            // סנן את האירועים של החודש המבוקש
            var eventsOfMonth = _events
                .Where(e => e.Date.Year == year && e.Date.Month == month)
                .OrderBy(e => e.Date)
                .ToList();

            // אורגן לפי ימים
            for (int day = 1; day <= DateTime.DaysInMonth(year, month); day++)
            {
                var dailyEvents = new CalendarDTO.DailyEventsDTO
                {
                    Day = day,
                    Events = eventsOfMonth
                        .Where(e => e.Date.Day == day)
                        .ToList()
                };

                calendar.Days.Add(dailyEvents);
            }
                
            return calendar;
        }
    }

}
