using Microsoft.AspNetCore.Mvc;
    using Learning_Space.DTO;
    using Learning_Space.Models;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Globalization;
using System.Collections.Generic;
using System.Linq;


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
        public async Task<IActionResult> Index(DateTime? currentDate)
        {
            if (currentDate.HasValue)
            {
                ViewData["Title"] = "Index - " + currentDate.Value.ToString("MMMM yyyy");
            }
            else
            {
                ViewData["Title"] = "Index";
                currentDate = DateTime.Now;
            }

            //        // Get today's date
            //        DateOnly currentDatee = DateOnly.FromDateTime(DateTime.Today);

            //        // Create a new list to store the convenient events

            //        List<Models.Task> filteredTasks = _context.Tasks
            //.Where(task => task.StartDate <= currentDatee && task.EndDate >= currentDatee)
            //.ToList();


            //        return View(filteredTasks);


            // Your logic to fetch or generate CalendarDTO data
            IEnumerable<CalendarDTO> calendarData = GetCalendarData(currentDate.Value.Year, currentDate.Value.Month);

            return View(calendarData);
        }


        private IEnumerable<CalendarDTO> GetCalendarData(int year, int month)
        {
          
            DateTime firstDayOfMonth = new DateTime(year, month, 1);
            int startDayOfWeek = (int)firstDayOfMonth.DayOfWeek;
            int daysInMonth = DateTime.DaysInMonth(year, month);
            int totalDays = daysInMonth + startDayOfWeek;
            int rowCount = (int)Math.Ceiling((double)totalDays / 7);

            List<CalendarDTO> calendarData = new List<CalendarDTO>();

            return calendarData;
        }

    }

}
