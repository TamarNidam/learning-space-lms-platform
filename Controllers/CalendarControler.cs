using Microsoft.AspNetCore.Mvc;
    using Learning_Space.DTO;
    using Learning_Space.Models;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Globalization;
using System.Collections.Generic;


namespace Learning_Space.Controllers
{

    public class CalendarController : Controller
    {
        private readonly LearningSpaceContext _context;
        private List<CalendarDTO> events;

        public CalendarController(LearningSpaceContext context)
        {
            _context = context;
            //events = new List<CalendarDTO>()
            //{
            //    new CalendarDTO { Title = "Event 1", Date = new DateTime(2023, 12, 26), Description = "Description 1" },
            //    new CalendarDTO { Title = "Event 2", Date = new DateTime(2023, 12, 27), Description = "Description 2" }
            //};
        }

        // GET: Classes
        public async Task<IActionResult> Index()
        {
            
            return View(events);
        }
        //private List<DTO.EventDTO> _events;

        //public CalendarController()
        //{
        //    _events = new List<DTO.EventDTO>();
        //}

        //public void AddEvent(EventDTO eventDto)
        //{
        //    _events.Add(eventDto);
        //}

        //public CalendarDTO.MonthlyCalendarDTO GetMonthlyCalendar(int year, int month)
        //{
        //    var calendar = new CalendarDTO.MonthlyCalendarDTO
        //    {
        //        Year = year,
        //        Month = month
        //    };

        //    // סנן את האירועים של החודש המבוקש
        //    var eventsOfMonth = _events
        //        .Where(e => e.Date.Year == year && e.Date.Month == month)
        //        .OrderBy(e => e.Date)
        //        .ToList();

        //    // אורגן לפי ימים
        //    for (int day = 1; day <= DateTime.DaysInMonth(year, month); day++)
        //    {
        //        var dailyEvents = new CalendarDTO.DailyEventsDTO
        //        {
        //            Day = day,
        //            Events = eventsOfMonth
        //                .Where(e => e.Date.Day == day)
        //                .ToList()
        //        };

        //        calendar.Days.Add(dailyEvents);
        //    }

        //    return calendar;
        //}
    }

}
