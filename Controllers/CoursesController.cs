using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Learning_Space.Models;
using Newtonsoft.Json.Linq;
using Learning_Space.DTO;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.IO;
using System.Text;
using MyFile = System.IO.File;
using DDirectory = System.IO.Directory;
using MyTask = System.Threading.Tasks.Task;
using System.Security.AccessControl;
using NuGet.DependencyResolver;
using Microsoft.Data.SqlClient;
using Microsoft.Build.Framework;



namespace Learning_Space.Controllers
{
    public class CoursesController : Controller
    {
        private readonly LearningSpaceContext _context;
        private readonly string baseFolderPath = Path.Combine(".", "TextFiles");

        public CoursesController(LearningSpaceContext context)
        {
            _context = context;
        }

        // GET: Courses
        public async Task<IActionResult> Index(int user, int permission, int? classid)
        {
            try
            {
                List<Course> courses = new List<Course>();
                if (user == 0)
                {
                    if (classid.HasValue)
                    {
                        var sql = $"SELECT Courses.* " +
                            $"FROM Courses JOIN CourseInClass " +
                            $"ON Courses.CourseId = CourseInClass.CourseId " +
                            $"WHERE CourseInClass.ClassId = {classid}";
                        courses = await _context.Courses.FromSqlRaw(sql).ToListAsync();
                    }
                    else
                    {
                        courses = await _context.Courses.ToListAsync();
                        courses.RemoveAt(0);
                    }
                }
                else if (permission == 2)
                {
                    // Get all the class IDs for the user
                    List<int> classIds = GetClassIdsForUser(user);

                    foreach (int classId in classIds)
                    {
                        // Get the courses associated with each class
                        List<Course> classCourses = await _context.CourseInClasses
                            .Where(cic => cic.ClassId == classId)
                            .Select(cic => cic.Course)
                            .ToListAsync();

                        courses.AddRange(classCourses);
                    }
                }
                else
                {
                    List<int?> courseids = await _context.Teachers
                        .Where(t => t.UserId == user)
                        .Select(t => t.CourseId)
                        .ToListAsync();
                    foreach (int courseidl in courseids)
                    {
                        Course vourses = _context.Courses.FirstOrDefault(c => c.CourseId == courseidl);


                        courses.Add(vourses);
                    }
                    courses.RemoveAt(0);

                }

                var courseDTOs = courses
                               .Select(u => new CourseDTO
                               {
                                   CourseId = u.CourseId,
                                   CourseName = u.CourseName,
                                   CourseDescription = u.CourseDescription
                               }).ToList();
                return View(courseDTOs);
            }
            catch (Exception ex)
            {
                return View(ex);
            }
        }

        private List<int> GetClassIdsForUser(int user)
        {
            List<int> classIds = new List<int>();

            // Retrieve the class IDs associated with the user from the UserClass table
            var userClasses = _context.StudentInClasses.Where(uc => uc.UserId == user);

            foreach (var userClass in userClasses)
            {
                classIds.Add((int)(userClass.ClassId));
            }

            return classIds;
        }


        // GET: Courses/Details/5
        public async Task<IActionResult> Details(int? courseid)
        {
            try
            {
                if (!courseid.HasValue)
                {
                    return NotFound();
                }


                var course = await _context.Courses
                    .FirstOrDefaultAsync(m => m.CourseId == courseid);
                if (course == null)
                {
                    return NotFound();
                }
                var teacher = _context.Teachers.FirstOrDefault(t => t.CourseId == courseid);
                var classs = _context.CourseInClasses.FirstOrDefault(c => c.CourseId == courseid);
                if (teacher == null || classs == null)
                {
                    return NotFound();
                }
                var courseDTO = new CourseDTO
                {
                    CourseId = course.CourseId,
                    CourseName = course.CourseName,
                    CourseDescription = course.CourseDescription,
                    TeacherId = teacher?.TeacherId,
                    TeacherName = _context.Users.FirstOrDefault(u => u.UserId == teacher.UserId)?.FirstName,
                    ClassId = classs?.ClassId,
                    ClassName = _context.Classes.FirstOrDefault(c => c.ClassId == classs.ClassId)?.ClassName
                };
                return View(courseDTO);
            }
            catch (Exception ex)
            {
                return View("Error", ex);
            }
        }


        // GET: Courses/Create
        public async Task<IActionResult> CreateAsync(int? classid)
        {

            var teacherUserIds = _context.Teachers.Select(t => t.UserId).Distinct().ToList();
            var users = _context.Users
                .Where(u => teacherUserIds.Contains(u.UserId))
                .Select(u => new
                {
                    TeacherId = _context.Teachers.FirstOrDefault(t => t.UserId == u.UserId).UserId,
                    FirstName = u.FirstName
                })
                .Distinct() // Optional: Remove duplicates if any
                .ToList();


            ViewData["TeacherId"] = new SelectList(users, "TeacherId", "FirstName");
            if (classid.HasValue)
            {
                var clas = await _context.Classes.Where(c => c.ClassId == classid.Value).ToListAsync();
                var selectList = new SelectList(clas, "ClassId", "ClassName");
                ViewData["ClassId"] = selectList;
            }
            else
            {

                ViewData["ClassId"] = new SelectList(_context.Classes, "ClassId", "ClassName");
            }
            return View();
        }


        // POST: Courses/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CourseId,CourseName,CourseDescription,TeacherId,ClassId")] CourseDTO courseDTO)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var name = await _context.Courses
                  .FromSqlRaw("SELECT TOP 1 * FROM Courses WHERE CourseName = {0}", courseDTO.CourseName)
                  .FirstOrDefaultAsync();
                    if (name != null)
                    {
                        ViewBag.ErrorMessage = "Course name exist";
                        return View(courseDTO);
                    }
                    Console.WriteLine($"TeacherId: {courseDTO.TeacherId}");

                    //Create course
                    var maxCourseId = await _context.Courses.MaxAsync(u => (int?)u.CourseId) ?? 0;
                    var newCourseId = maxCourseId + 1;
                    var sql = $"INSERT INTO [Courses] (CourseId,CourseName,CourseDescription) " +
                        $"VALUES ({newCourseId}, '{courseDTO.CourseName}', '{courseDTO.CourseDescription}')";
                    await _context.Database.ExecuteSqlRawAsync(sql);

                    //Create teacher for course
                    var maxTeacherId = await _context.Teachers.MaxAsync(u => (int?)u.TeacherId) ?? 0;
                    var newTeacherId = maxTeacherId + 1;
                    sql = $"INSERT INTO [Teachers] (TeacherId,UserId,CourseId) " +
                        $"VALUES ({newTeacherId},{courseDTO.TeacherId},{newCourseId})";
                    await _context.Database.ExecuteSqlRawAsync(sql);

                    //Create class in course
                    var maxCourseInClassId = await _context.CourseInClasses.MaxAsync(u => (int?)u.CourseInClass1) ?? 0;
                    var newCourseInClassId = maxCourseInClassId + 1;
                    sql = $"INSERT INTO [CourseInClass] (CourseInClass,ClassId,CourseId) " +
                        $"VALUES ({newCourseInClassId},{courseDTO.ClassId},{newCourseId})";
                    await _context.Database.ExecuteSqlRawAsync(sql);

                    //Create chat file
                    string courseChatFilePath = Path.Combine(baseFolderPath, "Chats", "Courses", $"{newCourseId}" + ".txt");
                    MyFile.Create(courseChatFilePath).Close();

                    ////Create folders for course
                    //CreateFolder(newCourseId);
                    string courseUserNotbookFilePath;
                    // Create notebook text files for each student in the course
                    var studentsInClass = _context.StudentInClasses
                        .Where(s => s.ClassId == courseDTO.ClassId)
                        .Select(s => s.UserId)
                        .ToList();
                    foreach (int UserId in studentsInClass)
                    {
                        // CreateNotebookFile(newCourseId, UserId);
                        courseUserNotbookFilePath = Path.Combine(baseFolderPath, "Notebooks", $"courseid_{newCourseId}__userid_" + UserId.ToString() + ".txt");
                        MyFile.Create(courseUserNotbookFilePath).Close();
                    }

                    return Redirect($"/Courses/Index?user=0&permission=0");
                }
                return View(courseDTO);
            }
            catch (SqlException ex)
            {
                ErrorViewModel errorModel = new ErrorViewModel
                {
                    ErrorMessage = ex.Message,
                    // Populate any other necessary properties of ErrorViewModel
                };

                return View("Error", errorModel);
            }
        }

        private async MyTask CreateFolder(int newCourseId)
        {

            string courseFolderPath = Path.Combine(baseFolderPath, "Courses", $"{newCourseId}");


            // Create the folder if it doesn't exist
            if (!DDirectory.Exists(courseFolderPath))
            {

                DDirectory.CreateDirectory(Path.Combine(courseFolderPath, "MoreStudy"));
                DDirectory.CreateDirectory(Path.Combine(courseFolderPath, "Notebooks"));
                DDirectory.CreateDirectory(Path.Combine(courseFolderPath, "Task"));
                Console.WriteLine("Course folder created successfully!");
            }
            else
            {
                Console.WriteLine("Course folder already exists!");
            }
        }

        private void CreateNotebookFile(int courseid, int userid)
        {
            string courseFolderPath = Path.Combine(baseFolderPath, "Courses", courseid.ToString());
            string noteFolderPath = Path.Combine(courseFolderPath, "Notebooks");
            string notebookFilePath = Path.Combine(noteFolderPath, userid.ToString() + ".txt");

            // Create the file with the received UserId
            MyFile.Create(notebookFilePath);
            Console.WriteLine("Notebook file created successfully.");
        }


        // GET: Courses/Edit/5
        public async Task<IActionResult> Edit(int courseid)
        {
            try
            {
                if (courseid == null || !CourseExists(courseid))
                {
                    return NotFound();
                }

                var course = await _context.Courses.FindAsync(courseid);

                if (course == null)
                {
                    return NotFound();
                }
                var teacher = _context.Teachers.FirstOrDefault(t => t.CourseId == courseid);
                var classs = _context.CourseInClasses.FirstOrDefault(c => c.CourseId == courseid);
                var courseDTO = new CourseDTO
                {
                    CourseId = course.CourseId,
                    CourseName = course.CourseName,
                    CourseDescription = course.CourseDescription,
                    TeacherId = teacher?.TeacherId,
                    TeacherName = _context.Users.FirstOrDefault(u => u.UserId == teacher.UserId)?.FirstName,
                    ClassId = classs?.ClassId,
                    ClassName = _context.Classes.FirstOrDefault(c => c.ClassId == classs.ClassId)?.ClassName
                };
                var teacherUserIds = _context.Teachers.Select(t => t.UserId).Distinct().ToList();
                var users = _context.Users
                    .Where(u => teacherUserIds.Contains(u.UserId))
                    .Select(u => new
                    {
                        TeacherId = _context.Teachers.FirstOrDefault(t => t.UserId == u.UserId).UserId,
                        FirstName = u.FirstName
                    })
                    .Distinct() // Optional: Remove duplicates if any
                    .ToList();


                ViewData["TeacherId"] = new SelectList(users, "TeacherId", "FirstName");
                ViewData["ClassId"] = new SelectList(_context.Classes, "ClassId", "ClassName");

                return View(courseDTO);
            }
            catch (Exception ex)
            {
                return View("Error", ex);
            }

        }

        // POST: Courses/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int user, int permission, int? courseid, [Bind("CourseId,CourseName,CourseDescription,TeacherId,ClassId")] CourseDTO courseDTO)
        {
            try
            {
                if (courseid != courseDTO.CourseId)
                {
                    return NotFound();
                }

                if (ModelState.IsValid)
                {
                    //Update course
                    var sql = $"UPDATE [Courses] SET " +
                        $"CourseName = '{courseDTO.CourseName}',CourseDescription = '{courseDTO.CourseDescription}' " +
                        $"WHERE CourseId = {courseDTO.CourseId}";
                    await _context.Database.ExecuteSqlRawAsync(sql);

                    //Update teacher for course
                    sql = $"UPDATE [Teachers] SET " +
                        $"UserId = '{courseDTO.TeacherId}' " +
                        $"WHERE CourseId = {courseDTO.CourseId}";
                    await _context.Database.ExecuteSqlRawAsync(sql);

                    //Update class in course
                    sql = $"UPDATE [CourseInClass] SET " +
                        $"ClassId = '{courseDTO.ClassId}' " +
                        $"WHERE CourseId = {courseDTO.CourseId}";
                    await _context.Database.ExecuteSqlRawAsync(sql);

                    return Redirect($"/Courses/Details?user={user}&permission={permission}&courseid={courseid}");
                }
                return View(courseDTO);
            }
            catch (Exception ex)
            {
                return View("Error", ex);
            }
        }


        // GET: Courses/Delete/5
        public async Task<IActionResult> Delete(int? courseid)
        {
            try
            {
                if (!courseid.HasValue || courseid == null)
                {
                    return NotFound();
                }

                var course = await _context.Courses
                    .FirstOrDefaultAsync(m => m.CourseId == courseid);
                if (course == null)
                {
                    return NotFound();
                }
                var teacher = _context.Teachers.FirstOrDefault(t => t.CourseId == courseid);
                var classs = _context.CourseInClasses.FirstOrDefault(c => c.CourseId == courseid);
                if (teacher == null || classs == null)
                {
                    return NotFound();
                }
                var courseDTO = new CourseDTO
                {
                    CourseId = course.CourseId,
                    CourseName = course.CourseName,
                    CourseDescription = course.CourseDescription,
                    TeacherId = teacher?.TeacherId,
                    TeacherName = _context.Users.FirstOrDefault(u => u.UserId == teacher.UserId)?.FirstName,
                    ClassId = classs?.ClassId,
                    ClassName = _context.Classes.FirstOrDefault(c => c.ClassId == classs.ClassId)?.ClassName
                };
                return View(courseDTO);
            }
            catch (Exception ex)
            {
                return View("Error", ex);
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int courseid)
        {
            var course = await _context.Courses.FindAsync(courseid);
            if (course != null)
            {
                //remove course from the textfiles:
                await RemoveFiles(courseid);

                //remove course from the tables:
                await RemoveFromTables(courseid);


            }
            return Redirect($"/Courses/Index?user=0&permission=0");
        }

        private async MyTask RemoveFiles(int courseid)
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

        private async MyTask RemoveFromTables(int courseid)
        {
            //remove course from the teacher
            var documentsToDelete = await _context.Teachers.Where(t => t.CourseId == courseid).ToListAsync();
            _context.Teachers.RemoveRange(documentsToDelete);
            await _context.SaveChangesAsync();
            //remove course from the class
            var documentsToDelete1 = await _context.CourseInClasses.Where(t => t.CourseId == courseid).ToListAsync();
            _context.CourseInClasses.RemoveRange(documentsToDelete1);
            await _context.SaveChangesAsync();
            //remove course from the courses
            var course = await _context.Courses.FindAsync(courseid);
            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();
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

        public IActionResult Notebook(int user, int courseid)
        {
            string notebookContent = string.Empty;

            string courseUserNotebookFilePath = Path.Combine(baseFolderPath, "Notebooks", $"courseid_{courseid}__userid_{user}.txt");

            // Check if the notebook file exists
            if (MyFile.Exists(courseUserNotebookFilePath))
            {
                // Read the content from the notebook file
                notebookContent = MyFile.ReadAllText(courseUserNotebookFilePath);

            }

            // Pass the notebook content to the view
            return View("Notebook", notebookContent);
        }

        [HttpPost]
        public IActionResult SaveNotebook(int user, int permission, int courseid, string notebookContent)
        {

            // Create the file path based on the provided user, permission, and courseId
            string courseUserNotebookFilePath = Path.Combine(baseFolderPath, "Notebooks", $"courseid_{courseid}__userid_{user}.txt");

            // Save the updated notebook content to the file
            MyFile.WriteAllText(courseUserNotebookFilePath, notebookContent);
            // Redirect back to the Notebook action to display the updated content
            return Redirect($"/Courses/Details?user={user}&permission={permission}&courseid={courseid}");
        }



        private bool CourseExists(int id)
        {
            return _context.Courses.Any(e => e.CourseId == id);
        }
    }
}