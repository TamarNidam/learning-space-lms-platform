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
using System.Security.AccessControl;
using NuGet.DependencyResolver;


namespace Learning_Space.Controllers
{
    public class CoursesController : Controller
    {
        private readonly LearningSpaceContext _context;

        public CoursesController(LearningSpaceContext context)
        {
            _context = context;
        }

        // GET: Courses
        public async Task<IActionResult> Index(int user, int? classid)
        {
            try
            {
                List<Course> courses = new List<Course>();
                if (user == 0)
                {
                    if (classid.HasValue)
                    {
                        var sql = $"SELECT Courses.* FROM Courses JOIN CourseInClasses ON CourseInClasses.ClassId = {classid}";
                        courses = await _context.Courses.FromSqlRaw(sql).ToListAsync();

                    }
                    else
                    {
                        courses = await _context.Courses.ToListAsync();
                        courses.RemoveAt(0);
                    }

                }
                else
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
                classIds.Add((int)userClass.ClassId);
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
        public IActionResult Create()
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
            ViewData["ClassId"] = new SelectList(_context.Classes, "ClassId", "ClassName");
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

                    //Create folders for course
                    CreateFolder(newCourseId);

                    //// Create notebook text files for each student in the course
                    //var studentsInClass = _context.StudentInClasses
                    //    .Where(s => s.ClassId == courseDTO.ClassId)
                    //    .Select(s => s.UserId)
                    //    .ToList();
                    //foreach (int UserId in studentsInClass)
                    //{
                    //    CreateNotebookFile(newCourseId, UserId);
                    //}

                    return Redirect($"/Courses/Index?user=0&permission=0");
                }
                return View(courseDTO);
            }
            catch (Exception ex)
            {
                return View("Error", ex);
            }
        }

        private void CreateFolder(int newCourseId)
        {
            string baseFolderPath = Path.Combine(".", "TextFiles");
            string courseFolderPath = Path.Combine(baseFolderPath, "Courses", $"{newCourseId}");
            string courseChatFilePath = Path.Combine(baseFolderPath, "Chats", "Course", $"{newCourseId}"+".txt");
            Console.WriteLine(courseFolderPath);
            MyFile.Create(courseChatFilePath).Close();

            //// Create the folder if it doesn't exist
            //if (!DDirectory.Exists(courseFolderPath))
            //{
            //    DDirectory.CreateDirectory(courseChatFilePath);
           
            //    DDirectory.CreateDirectory(Path.Combine(courseFolderPath, "MoreStudy"));
            //   DDirectory.CreateDirectory(Path.Combine(courseFolderPath, "Notebooks"));
            //    DDirectory.CreateDirectory(Path.Combine(courseFolderPath, "Task"));
            //    Console.WriteLine("Course folder created successfully!");
            //}
            //else
            //{
            //    Console.WriteLine("Course folder already exists!");
            //}
        }

        private void CreateNotebookFile(int courseid, int userid)
        {

            string baseFolderPath = Path.Combine("..", "TextFiles");
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

        // POST: Courses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int courseid)
        {
            var course = await _context.Courses.FindAsync(courseid);
            if (course != null)
            {
                var documentsToDelete = await _context.Teachers.Where(t => t.CourseId == courseid).ToListAsync();
                _context.Teachers.RemoveRange(documentsToDelete);
                await _context.SaveChangesAsync();
                var documentsToDelete1 = await _context.CourseInClasses.Where(t => t.CourseId == courseid).ToListAsync();
                _context.CourseInClasses.RemoveRange(documentsToDelete1);
                await _context.SaveChangesAsync();
                //var teacher = await _context.Teachers.FindAsync(courseid);
                //if (teacher != null)


                //{ _context.Teachers.Remove(teacher); }
                
                //var classs = await _context.CourseInClasses.FindAsync(courseid);
                //if (classs != null)
                //{
                //  _context.CourseInClasses.Remove(classs);  
                //}
                //await _context.SaveChangesAsync();
                _context.Courses.Remove(course);

                string baseFolderPath = Path.Combine(".", "TextFiles");
                string courseFolderPath = Path.Combine(baseFolderPath, "Courses", $"{courseid}");
                string courseChatFilePath = Path.Combine(baseFolderPath, "Chats", "Course", $"{courseid}" + ".txt");
                Console.WriteLine(courseFolderPath);
                MyFile.Delete(courseChatFilePath);

                //// delete the folder if it  exist
                //if (DDirectory.Exists(courseFolderPath))
                //{
                //    DDirectory.DeleteDirectory(courseChatFilePath);

                
                //    Console.WriteLine("Course folder created successfully!");
                //}
                //else
                //{
                //    Console.WriteLine("Course folder already exists!");
                //}

            }

            await _context.SaveChangesAsync();
            return Redirect($"/Courses/Index?user=0&permission=0");
        }

        private bool CourseExists(int id)
        {
            return _context.Courses.Any(e => e.CourseId == id);
        }
    }
}





//using System;
//using System.Net.Http;
//using System.Threading.Tasks;
//using Newtonsoft.Json.Linq;
//string ZOOM_API_KEY="דד"

//class Program
//{
//    static async Task Main(string[] args)
//    {
//        var apiKey = ZOOM_API_KEY;
//        var apiSecret = "YOUR_ZOOM_API_SECRET";

//        // Step 1: Generate a JWT token for authentication
//        var jwtToken = ZoomTokenHelper.GenerateJWTToken(apiKey, apiSecret);

//        // Step 2: Create a new meeting
//        var httpClient = new HttpClient();
//        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {jwtToken}");

//        var requestUri = "https://api.zoom.us/v2/users/YOUR_USER_ID/meetings";
//        var requestData = new
//        {
//            topic = "New Video Meeting",
//            type = 1 // 1 for instant meetings
//        };
//        var response = await httpClient.PostAsJsonAsync(requestUri, requestData);
//        var responseBody = await response.Content.ReadAsStringAsync();
//        var meetingId = JObject.Parse(responseBody)["id"].ToString();

//        // Step 3: Generate the meeting link
//        var meetingLink = $"https://zoom.us/j/{meetingId}";

//        Console.WriteLine($"Meeting Link: {meetingLink}");
//    }
//}