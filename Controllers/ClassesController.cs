using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Learning_Space.Models;
using Learning_Space.DTO;
using Learning_Space.Controllers;
using Microsoft.Build.Framework;
using MyFile = System.IO.File;
using DDirectory = System.IO.Directory;
using MyTask = System.Threading.Tasks.Task;

namespace Learning_Space.Controllers
{
    public class ClassesController : Controller
    {
        private readonly LearningSpaceContext _context;

        public ClassesController(LearningSpaceContext context)
        {
            _context = context;
        }

        // GET: Classes
        public async Task<IActionResult> Index()
        {
            var classes = await _context.Classes.ToListAsync();
            var classDTOs = classes
                           .Select(u => new ClassDTO
                           {
                               ClassId = u.ClassId,
                               ClassName = u.ClassName
                           }).ToList();
            return View(classDTOs);
        }

        // GET: Classes/Details/5
        public async Task<IActionResult> Details(int? classid)
        {
            try
            {

                if (classid == null)
                {
                    return NotFound();
                }

                var @class = await _context.Classes
                    .FirstOrDefaultAsync(m => m.ClassId == classid);
                if (@class == null)
                {
                    return NotFound();
                }

                var classDTO = new ClassDTO
                {
                    ClassId = @class.ClassId,
                    ClassName = @class.ClassName,
                    Students = _context.StudentInClasses.Count(s => s.ClassId == classid)
                };
                return View(classDTO);
            }
            catch (Exception ex)
            {
                return View("Error", ex);
            }
        }

        // GET: Classes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Classes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ClassId,ClassName")] ClassDTO @classDTO)
        {
            if (ModelState.IsValid)
            {
                var u = await _context.Classes
                    .FromSqlRaw("SELECT TOP 1 * FROM Classes WHERE ClassName = {0}", @classDTO.ClassName)
                    .FirstOrDefaultAsync();
                if (u != null)
                {
                    ViewBag.ErrorMessage = "An existing name";
                    return View(@classDTO);
                }

                var maxClassId = await _context.Classes.MaxAsync(u => (int?)u.ClassId) ?? 0;
                var newClassId = maxClassId + 1;
                var sql = $"INSERT INTO [Classes] (ClassId,ClassName) VALUES ({newClassId}, '{@classDTO.ClassName}')";
                await _context.Database.ExecuteSqlRawAsync(sql);

                return Redirect($"/Classes/Index?user=0&permission=0");
            }
            return View(@classDTO);
        }

        // GET: Classes/Edit/5
        public async Task<IActionResult> Edit(int? classid)
        {
            if (classid == null)
            {
                return NotFound();
            }

            var @class = await _context.Classes.FindAsync(classid);
            if (@class == null)
            {
                return NotFound();
            }

            var classDTO = new ClassDTO
            {
                ClassId = @class.ClassId,
                ClassName = @class.ClassName
            };

            return View(classDTO);
        }

        // POST: Classes/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int user, int permission, int classid, [Bind("ClassId,ClassName")] ClassDTO @classDTO)
        {
            if (classid != @classDTO.ClassId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var sql = $"UPDATE [Classes] SET ClassName = '{@classDTO.ClassName}' WHERE ClassId = {@classDTO.ClassId}";
                    await _context.Database.ExecuteSqlRawAsync(sql);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClassExists(@classDTO.ClassId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return Redirect($"/Classes/Details?user={user}&permission={permission}&classid={classid}");
            }
            return View(@classDTO);
        }

        // GET: Classes/Delete/5
        public async Task<IActionResult> Delete(int classid)
        {

            try
            {

                if (classid == null)
                {
                    return NotFound();
                }

                var @class = await _context.Classes
                    .FirstOrDefaultAsync(m => m.ClassId == classid);
                if (@class == null)
                {
                    return NotFound();
                }

                var classDTO = new ClassDTO
                {
                    ClassId = @class.ClassId,
                    ClassName = @class.ClassName,
                    Students = _context.StudentInClasses.Count(s => s.ClassId == classid)
                };
                return View(classDTO);
            }
            catch (Exception ex)
            {
                return View("Error", ex);
            }

        }


        // POST: Classes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int user, int permission, int classid)
        {
            var @class = await _context.Classes.FindAsync(classid);

            if (@class != null)
            {
                var sql = $"SELECT Courses.* " +
                           $"FROM Courses JOIN CourseInClass " +
                           $"ON Courses.CourseId = CourseInClass.CourseId " +
                           $"WHERE CourseInClass.ClassId = {classid}";
                var courses = await _context.Courses.FromSqlRaw(sql).ToListAsync();


                foreach (var course in courses)
                {
                    await RemoveFiles(course.CourseId);
                    await RemoveFromTables(course.CourseId);
                }
                var classes = await _context.StudentInClasses.Where(c => c.ClassId == classid).ToListAsync();
                _context.StudentInClasses.RemoveRange(classes);
                await _context.SaveChangesAsync();

                _context.Classes.Remove(@class);
                await _context.SaveChangesAsync();
            }

            return Redirect($"/Classes/Index?user=0&permission=0");
        }
        public async MyTask RemoveFiles(int courseid)
        {

            //remove course file from the chats
            string baseFolderPath = Path.Combine(".", "TextFiles");
            string courseChatFilePath = Path.Combine(baseFolderPath, "Chats", "Courses", $"{courseid}" + ".txt");
            MyFile.Delete(courseChatFilePath);

            //remove tasks course file from the tasks
            courseChatFilePath = Path.Combine(baseFolderPath, "Tasks", "Courses", "Tasks" + ".txt");
            string[] lines = MyFile.ReadAllLines(courseChatFilePath);
            string[] filteredLines = FilterLines(lines, $"{courseid},");
            MyFile.WriteAllLines(courseChatFilePath, filteredLines);

            //remove users tasks course file from the userstasks
            courseChatFilePath = Path.Combine(baseFolderPath, "Tasks", "UserTasks");
            var sql = $"SELECT t.*" +
                    $"FROM [Tasks] t " +
                    $"WHERE t.CourseId = {courseid}" +
                    $"AND t.TaskType = 'Task'";
            var tasks = await _context.Tasks.FromSqlRaw(sql).ToListAsync();
            foreach (var task in tasks)
            {
                DeleteFilesStartingWith(courseChatFilePath, $"taskid_{task.TaskId}_");
            }

            //remove users notebook course file from the notebooks
            courseChatFilePath = Path.Combine(baseFolderPath, "Notebooks");
            DeleteFilesStartingWith(courseChatFilePath, $"courseid_{courseid}_");
        }


        static void DeleteFilesStartingWith(string directoryPath, string searchPattern)
        {
            try
            {
                string[] files = DDirectory.GetFiles(directoryPath, searchPattern);

                foreach (string file in files)
                {
                    MyFile.Delete(file);
                    Console.WriteLine($"Deleted file: {file}");
                }

                Console.WriteLine("Deletion complete.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred: {ex.Message}");
            }
        }

        static string[] FilterLines(string[] lines, string keyword)
        {
            return Array.FindAll(lines, line => line.StartsWith(keyword));
        }


        public async MyTask RemoveFromTables(int courseid)
        {
            //remove teacher's course from the teacher
            var documentsToDelete = await _context.Teachers.Where(t => t.CourseId == courseid).ToListAsync();
            _context.Teachers.RemoveRange(documentsToDelete);
            await _context.SaveChangesAsync();

            //remove class's course from the course in class
            var documentsToDelete1 = await _context.CourseInClasses.Where(t => t.CourseId == courseid).ToListAsync();
            _context.CourseInClasses.RemoveRange(documentsToDelete1);
            await _context.SaveChangesAsync();

            //remove task's course from tasks
            var courseTasks = await _context.Tasks.Where(t => t.CourseId == courseid).ToListAsync();
            _context.Tasks.RemoveRange(courseTasks);
            await _context.SaveChangesAsync();

            //remove lessons's course from lessons
            var lessons = await _context.Lessons.Where(t => t.CourseId == courseid).ToListAsync();
            if (lessons != null)
            {
                foreach (var les in lessons)
                {
                    //remove lesson ettend
                    var lessonsAtend = await _context.LessonAttends.Where(l => l.LessonId == les.LessonId).ToListAsync();
                    if (lessonsAtend != null)
                    {
                        _context.LessonAttends.RemoveRange(lessonsAtend);
                    }
                    await _context.SaveChangesAsync();
                }
                _context.Lessons.RemoveRange(lessons);
            }
            await _context.SaveChangesAsync();

            //remove message's course from messages
            var mes = await _context.Massages.Where(t => t.CourseId == courseid).ToListAsync();
            _context.Massages.RemoveRange(mes);
            await _context.SaveChangesAsync();

            //remove alarms's course from alarms
            var alarms = await _context.Alarms.Where(t => t.CourseId == courseid).ToListAsync();
            _context.Alarms.RemoveRange(alarms);
            await _context.SaveChangesAsync();

            //remove study's course from more studies
            var s = await _context.MoreStudies.Where(t => t.CourseId == courseid).ToListAsync();
            _context.MoreStudies.RemoveRange(s);
            await _context.SaveChangesAsync();

            //remove course from the courses
            var course = await _context.Courses.FindAsync(courseid);
            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();
        }

        private bool ClassExists(int id)
        {
            return _context.Classes.Any(e => e.ClassId == id);
        }
    }
}
