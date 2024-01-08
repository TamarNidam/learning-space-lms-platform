using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Learning_Space.Models;
using Learning_Space.DTO;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace Learning_Space.Controllers
{
    public class LessonsController : Controller
    {
        private readonly LearningSpaceContext _context;

        public LessonsController(LearningSpaceContext context)
        {
            _context = context;
        }

        // GET: Lessons
        public async Task<IActionResult> Index(int? courseid)
        {
            if (!courseid.HasValue)
            {
                return NotFound();
            }
            var currentDate = DateOnly.FromDateTime(DateTime.Today);

            var lessons = await _context.Lessons
    .Where(m => m.CourseId == courseid && m.LessonDate >= currentDate)
    .OrderBy(m => m.LessonDate)
    .Join(
        _context.Courses,
        l => l.CourseId,
        c => c.CourseId,
        (l, c) => new { Lesson = l, CourseName = c.CourseName }
    )
    .GroupJoin(
        _context.ZoomLessons,
        l => l.Lesson.LessonId,
        z => z.LessonId,
        (l, z) => new LessonDTO
        {
            LessonId = l.Lesson.LessonId,
            CourseId = l.Lesson.CourseId,
            CourseName = l.CourseName,
            LessonSubject = l.Lesson.LessonSubject,
            LessonDate = l.Lesson.LessonDate,
            StartTime = (TimeOnly)l.Lesson.StartTime,
            EndTime = (TimeOnly)l.Lesson.EndTime,
            LessonType = l.Lesson.LessonType,
            ZoomUrl = l.Lesson.LessonType == "Zoom" ? z.FirstOrDefault().ZoomUrl : null
        }
    )
    .ToListAsync();

            return View(lessons);
        }

        // GET: Lessons/Details/5
        public async Task<IActionResult> Details(int? lessonid)
        {
            if (lessonid == null)
            {
                return NotFound();
            }

            var l = await _context.Lessons.FirstOrDefaultAsync(m => m.LessonId == lessonid);
            if (l == null)
            {
                return NotFound();
            }
            var courseName = _context.Courses.FirstOrDefault(c => c.CourseId == l.CourseId).CourseName;
            var lessonDTO = new LessonDTO
            {
                LessonId = l.LessonId,
                CourseId = l.CourseId,
                CourseName = courseName,
                LessonSubject = l.LessonSubject,
                LessonDate = l.LessonDate,
                StartTime = (TimeOnly)l.StartTime,
                EndTime = (TimeOnly)l.EndTime,
                LessonType = l.LessonType,
                ZoomUrl = l.LessonType == "Zoom" ? _context.ZoomLessons.FirstOrDefault().ZoomUrl : null
            };
            return View(lessonDTO);
        }

        // GET: Lessons/Create
        public IActionResult Create()
        {
           
            return View();
        }

        // POST: Lessons/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int user, int permission, int courseid, [Bind("LessonId,CourseId,LessonSubject,LessonDate,StartTime,EndTime,LessonType,ZoomUrl")] LessonDTO lesson)
        {
            if (ModelState.IsValid)
            {
                DateOnly today = DateOnly.FromDateTime(DateTime.Today);
                

                if (lesson.LessonDate > today && lesson.StartTime > TimeOnly.FromDateTime(DateTime.Now))
                {
                    ViewBag.ErrorMessage = "A class can only be scheduled for a date that has not yet passed.";
                    return View(lesson);
                }
                var clas = _context.CourseInClasses.FirstOrDefault(c => c.CourseId == courseid).ClassId;
                // Check if there are any lessons scheduled for the course within the specified time range
                bool isCourseAvailable = await CheckCourseAvailability(clas, lesson.StartTime, lesson.EndTime);

                // Check if the teacher has any lessons scheduled within the specified time range for the course
                bool isTeacherAvailable = await CheckTeacherAvailability(courseid, lesson.StartTime, lesson.EndTime);

                if (!isCourseAvailable)
                {
                    ViewBag.ErrorMessage =  "There is already a lesson scheduled for the course within the specified time range.";
                    return View(lesson);
                }

                if (!isTeacherAvailable)
                {
                    ViewBag.ErrorMessage = "The teacher already has lessons scheduled within the specified time range for the course.";
                    return View(lesson);
                }

                var maxId = await _context.Lessons.MaxAsync(u => (int?)u.LessonId) ?? 0;
                var newId = maxId + 1;
              
                var sql = $"INSERT INTO [Lessons] (LessonId,CourseId,LessonSubject,LessonDate,StartTime,EndTime,LessonType) VALUES ({newId}, {courseid},'{lesson.LessonSubject}', '{lesson.LessonDate}', '{lesson.StartTime}', '{lesson.EndTime}', '{lesson.LessonType}')";
                await _context.Database.ExecuteSqlRawAsync(sql);
               
                if(lesson.LessonType == "Zoom")
                {
                    var maxIdzoom = await _context.ZoomLessons.MaxAsync(u => (int?)u.ZoomLesson1) ?? 0;
                    var newIdzoom = maxIdzoom + 1;
                     sql = $"INSERT INTO [ZoomLessons] (ZoomLesson,LessonId,ZoomUrl) VALUES ({newIdzoom}, {newId}, '{lesson.ZoomUrl}')";
                    await _context.Database.ExecuteSqlRawAsync(sql);
                }

                return Redirect($"/Lessons/Index?user={user}&permission={permission}&courseid={courseid}");
            }
            return View(lesson);
        }

        private async Task<bool> CheckCourseAvailability(int? classId, TimeOnly startTime, TimeOnly endTime)
        {
            // Get the list of CourseIds for the given ClassId
            var courseIds = await _context.CourseInClasses.Where(cic => cic.ClassId == classId).Select(cic => cic.CourseId).ToListAsync();
            // Check if there are any lessons scheduled for the course within the specified time range
            bool isAvailable = await _context.Lessons.AnyAsync(l => courseIds.Contains(l.CourseId) && l.StartTime <= endTime && l.EndTime >= startTime);
            return !isAvailable;
        }

        private async Task<bool> CheckTeacherAvailability(int courseId, TimeOnly startTime, TimeOnly endTime)
        {
            //Get the teacher
            var teacher =await  _context.Teachers.FirstOrDefaultAsync(m => m.CourseId == courseId);

            // Get the list of CourseIds for the given ClassId
            var courseIds = await _context.Teachers.Where(t => t.TeacherId == teacher.TeacherId).Select(t => t.CourseId).ToListAsync();

            // Check if the teacher has any lessons scheduled within the specified time range for the course
            bool isAvailable = await _context.Lessons.AnyAsync(l => courseIds.Contains(l.CourseId) && l.StartTime <= endTime && l.EndTime >= startTime);
            return !isAvailable;
        }

        // GET: Lessons/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lesson = await _context.Lessons.FindAsync(id);
            if (lesson == null)
            {
                return NotFound();
            }
            ViewData["CourseId"] = new SelectList(_context.Courses, "CourseId", "CourseName", lesson.CourseId);
            return View(lesson);
        }

        // POST: Lessons/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("LessonId,CourseId,LessonSubject,LessonDate,StartTime,EndTime,LessonType")] Lesson lesson)
        {
            if (id != lesson.LessonId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(lesson);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LessonExists(lesson.LessonId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CourseId"] = new SelectList(_context.Courses, "CourseId", "CourseName", lesson.CourseId);
            return View(lesson);
        }

        // GET: Lessons/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lesson = await _context.Lessons
                .Include(l => l.Course)
                .FirstOrDefaultAsync(m => m.LessonId == id);
            if (lesson == null)
            {
                return NotFound();
            }

            return View(lesson);
        }

        // POST: Lessons/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var lesson = await _context.Lessons.FindAsync(id);
            if (lesson != null)
            {
                _context.Lessons.Remove(lesson);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LessonExists(int id)
        {
            return _context.Lessons.Any(e => e.LessonId == id);
        }
    }
}
