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
using System.Linq;
using NuGet.Packaging;


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
    .ThenBy(m => m.StartTime)
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

        public async Task<IActionResult> Schedule(int user, int? courseid, int weekOffset)
        {
            DateOnly startDate = DateOnly.FromDateTime(DateTime.Today.AddDays(weekOffset * 7));
            DateOnly endDate = startDate.AddDays(6);

            List<Lesson> lessons = new List<Lesson>();
            if (courseid.HasValue)
            {
                lessons =await _context.Lessons
                    .Where(x => x.CourseId == courseid && x.LessonDate >= startDate && x.LessonDate <= endDate)
                    .ToListAsync();
            }
            else
            {
                if (user == 0)
                {
                    lessons = await _context.Lessons
                        .Where(x => x.LessonDate >= startDate && x.LessonDate <= endDate)
                        .ToListAsync();
                }
                else
                {                  
                    List<int> classIds = GetClassIdsForUser(user);
                    List<int> courseids = new List<int>();
                    foreach (int classId in classIds)
                    {
                        var sql = $"SELECT Courses.CourseId " +
                            $"FROM Courses JOIN CourseInClass " +
                            $"ON Courses.CourseId = CourseInClass.CourseId " +
                            $"WHERE CourseInClass.ClassId = {classId}";
                        var courses = await _context.Courses.FromSqlRaw(sql).ToListAsync();
                        courseids.AddRange((IEnumerable<int>)courses);
                    }
                   
                    foreach (int courseId in courseids)
                    {
                        // Get the courses associated with each class
                        List<Lesson> lesso = await _context.Lessons
                            .Where(x => x.CourseId == courseId && x.LessonDate >= startDate && x.LessonDate <= endDate)
                            .ToListAsync();


                        lessons.AddRange(lesso);
                    }
                }
            }

            List<LessonDTO> lessonDTOs = new List<LessonDTO>();

            foreach (var l in lessons)
            {
                var course = await _context.Courses.FindAsync(l.CourseId);
                string courseName = course != null ? course.CourseName : "Unknown";

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

                lessonDTOs.Add(lessonDTO);
            }

            return View(lessonDTOs);
        }

        private List<int> GetClassIdsForUser(int user)
        {
            List<int> classIds = new List<int>();

            
            var userClasses = _context.StudentInClasses.Where(uc => uc.UserId == user);

            foreach (var userClass in userClasses)
            {
                classIds.Add((int)userClass.ClassId);
            }

            return classIds;
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

                // Check if the lesson's date and start time have already passed
                if (lesson.LessonDate < today || lesson.LessonDate == today && lesson.StartTime < TimeOnly.FromDateTime(DateTime.Now))
                {
                    ViewBag.ErrorMessage = "A class can only be scheduled for a date that has not yet passed.";
                    return View(lesson);
                }

                var clas = _context.CourseInClasses.FirstOrDefault(c => c.CourseId == courseid).ClassId;

                // Check if there are any lessons scheduled for the course within the specified time range
                bool isCourseAvailable = await CheckCourseAvailability(clas, lesson.LessonDate, lesson.StartTime, lesson.EndTime);

                // Check if the teacher has any lessons scheduled within the specified time range for the course
                bool isTeacherAvailable = await CheckTeacherAvailability(courseid, lesson.LessonDate, lesson.StartTime, lesson.EndTime);

                // If there is already a lesson scheduled for the course within the specified time range
                if (!isCourseAvailable)
                {
                    ViewBag.ErrorMessage = "There is already a lesson scheduled for the course within the specified time range.";
                    return View(lesson);
                }

                // If the teacher already has lessons scheduled within the specified time range for the course
                if (!isTeacherAvailable)
                {
                    ViewBag.ErrorMessage = "The teacher already has lessons scheduled within the specified time range for the course.";
                    return View(lesson);
                }

                var maxId = await _context.Lessons.MaxAsync(u => (int?)u.LessonId) ?? 0;
                var newId = maxId + 1;

                // Insert the new lesson into the Lessons table
                var sql = $"INSERT INTO [Lessons] (LessonId,CourseId,LessonSubject,LessonDate,StartTime,EndTime,LessonType) " +
                    $"VALUES ({newId}, {courseid},'{lesson.LessonSubject}', '{lesson.LessonDate.ToString("yyyy-MM-dd")}', '{lesson.StartTime}', '{lesson.EndTime}', '{lesson.LessonType}')";
                await _context.Database.ExecuteSqlRawAsync(sql);

                // If the lesson type is Zoom, insert a new entry into the ZoomLessons table
                if (lesson.LessonType == "Zoom")
                {
                    var maxIdzoom = await _context.ZoomLessons.MaxAsync(u => (int?)u.ZoomLesson1) ?? 0;
                    var newIdzoom = maxIdzoom + 1;
                    sql = $"INSERT INTO [ZoomLessons] (ZoomLesson,LessonId,ZoomUrl) VALUES ({newIdzoom}, {newId}, '{lesson.ZoomUrl}')";
                    await _context.Database.ExecuteSqlRawAsync(sql);
                }

                var maxId3 = await _context.LessonAttends.MaxAsync(u => (int?)u.LessonAttendId) ?? 0;
                var studentsInClass = _context.StudentInClasses
       .Where(s => s.ClassId == clas)
       .Select(s => s.UserId)
       .ToList();

                // Insert attendance records for each student in the class for the new lesson
                foreach (int UserId in studentsInClass)
                {
                    maxId3 = maxId3 + 1;
                    sql = $"INSERT INTO [LessonAttend] (LessonAttendId,LessonId,UserId,Attend) VALUES ({maxId3}, {newId},{UserId},{0})";
                    await _context.Database.ExecuteSqlRawAsync(sql);

                }

                return Redirect($"/Lessons/Index?user={user}&permission={permission}&courseid={courseid}");
            }
            return View(lesson);
        }

        private async Task<bool> CheckCourseAvailability(int? classId, DateOnly date, TimeOnly startTime, TimeOnly endTime)
        {
            // Get the list of CourseIds for the given ClassId
            var courseIds = await _context.CourseInClasses
                .Where(cic => cic.ClassId == classId)
                .Select(cic => cic.CourseId)
                .ToListAsync();

            // Check if there are any lessons scheduled for the course within the specified time range
            bool isAvailable = await _context.Lessons.AnyAsync(l =>
            courseIds.Contains(l.CourseId) &&
            l.LessonDate == date &&
            l.StartTime <= endTime &&
            l.EndTime >= startTime);

            return !isAvailable;
        }

        private async Task<bool> CheckTeacherAvailability(int courseId, DateOnly date, TimeOnly startTime, TimeOnly endTime)
        {
            //Get the teacher
            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(m => m.CourseId == courseId);

            // Get the list of CourseIds for the given ClassId
            var courseIds = await _context.Teachers
                .Where(t => t.TeacherId == teacher.TeacherId)
                .Select(t => t.CourseId)
                .ToListAsync();

            // Check if the teacher has any lessons scheduled within the specified time range for the course
            bool isAvailable = await _context.Lessons.AnyAsync(l =>
            courseIds.Contains(l.CourseId) &&
             l.LessonDate == date &&
            l.StartTime <= endTime &&
            l.EndTime >= startTime);

            return !isAvailable;
        }

        // GET: Lessons/Edit/5
        public async Task<IActionResult> Edit(int? lessonid)
        {
            if (lessonid == null)
            {
                return NotFound();
            }

            var l = await _context.Lessons.FindAsync(lessonid);
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

        // POST: Lessons/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int user, int permission, int courseid, int lessonid, [Bind("LessonId,CourseId,LessonSubject,LessonDate,StartTime,EndTime,LessonType,ZoomUrl")] LessonDTO lesson)
        {
            if (lessonid != lesson.LessonId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                DateOnly today = DateOnly.FromDateTime(DateTime.Today);


                if (lesson.LessonDate < today || lesson.LessonDate == today && lesson.StartTime < TimeOnly.FromDateTime(DateTime.Now))
                {
                    ViewBag.ErrorMessage = "A class can only be scheduled for a date that has not yet passed.";
                    return View(lesson);
                }
                var clas = _context.CourseInClasses.FirstOrDefault(c => c.CourseId == courseid).ClassId;
                // Check if there are any lessons scheduled for the course within the specified time range
                bool isCourseAvailable = await CheckCourseAvailability(clas, lesson.LessonDate, lesson.StartTime, lesson.EndTime);

                // Check if the teacher has any lessons scheduled within the specified time range for the course
                bool isTeacherAvailable = await CheckTeacherAvailability(courseid, lesson.LessonDate, lesson.StartTime, lesson.EndTime);

                if (!isCourseAvailable)
                {
                    ViewBag.ErrorMessage = "There is already a lesson scheduled for the course within the specified time range.";
                    return View(lesson);
                }

                if (!isTeacherAvailable)
                {
                    ViewBag.ErrorMessage = "The teacher already has lessons scheduled within the specified time range for the course.";
                }
                    try
                    {
                    var lessonDTO = new Lesson
                    {
                        LessonId = lesson.LessonId,
                        CourseId = lesson.CourseId,
                        LessonSubject = lesson.LessonSubject,
                        LessonDate = lesson.LessonDate,
                        StartTime = (TimeOnly)lesson.StartTime,
                        EndTime = (TimeOnly)lesson.EndTime,
                        LessonType = lesson.LessonType
                    };
                    _context.Update(lessonDTO);
                    await _context.SaveChangesAsync();

                    if (lesson.LessonType == "Zoom")
                    {
                        var sql = $"UPDATE [ZoomLessons] SET ZoomUrl = '{lesson.ZoomUrl}' WHERE LessonId = {lessonid}";
                        await _context.Database.ExecuteSqlRawAsync(sql);
                    }
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
                return Redirect($"/Lessons/Index?user={user}&permission={permission}&courseid={courseid}");
            }
            return View(lesson);
        }


        // POST: Lessons/Delete/5
        [HttpPost]

        public async Task<IActionResult> Deletey(int user, int permission, int courseid, int lessonid)
        {
            var lesson = await _context.Lessons.FindAsync(lessonid);
            if (lesson != null)
            {
                var lessonsAtend = await _context.LessonAttends.Where(l => l.LessonId == lessonid).ToListAsync();
                if (lessonsAtend != null)
                {
                    _context.LessonAttends.RemoveRange(lessonsAtend);
                }

                await _context.SaveChangesAsync();
                _context.Lessons.Remove(lesson);
            }

            await _context.SaveChangesAsync();
            return Redirect($"/Lessons/Index?user={user}&permission={permission}&courseid={courseid}");
        }

        private bool LessonExists(int id)
        {
            return _context.Lessons.Any(e => e.LessonId == id);
        }
    }
}
