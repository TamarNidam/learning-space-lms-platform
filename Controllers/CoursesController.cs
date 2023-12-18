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
        public async Task<IActionResult> Index()
        {
            var courses = await _context.Courses.ToListAsync();
            var courseDTOs = courses
                           .Select(u => new CourseDTO
                           {
                               CourseId = u.CourseId,
                               CourseName = u.CourseName,
                               CourseDescription = u.CourseDescription
                           }).ToList();
            return View(courseDTOs);
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

                var courseDTO = new CourseDTO
                {
                    CourseId = course.CourseId,
                    CourseName = course.CourseName,
                    CourseDescription = course.CourseDescription,
                    TeacherId = _context.Teachers.Any(t => t.CourseId == course.CourseId) ? t.TeacherId,
                    TeacherName = _context.Users.Any(t => t.CourseId == course.CourseId) ? t.TeacherId
                    ClassId = _context.CourseInClasses.FirstOrDefault(),
                    ClassName = 
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

                    //Create folder for course
                    CreateFolder("course", newCourseId);
                    CreateFolder(newCourseId, "Tasks");
                    CreateFolder(newCourseId, "MoreStudies");
                    CreateFolder(newCourseId, "Notebooks");

                    // Create notebook text files for each student in the course
                    var studentsInClass = _context.StudentInClasses
                        .Where(s => s.ClassId == courseDTO.ClassId)
                        .Select(s => s.UserId)
                        .ToList();
                    foreach (var UserId in studentsInClass)
                    {
                        CreateTextFile("Notebooks", UserId, newCourseId);
                    }

                    return Redirect($"/Courses/Index?user=0&permission=0");
                }
                return View(courseDTO);
            }
            catch (Exception ex)
            {
                return View("Error", ex);
            }
        }

        private void CreateFolder(string v, int newCourseId)
        {
            throw new NotImplementedException();
        }

        private void CreateFolder( int newCourseId, string? v1)
        {
            throw new NotImplementedException();
        }

        private void CreateTextFile(string v, int? userId, int newCourseId)
        {
            throw new NotImplementedException();
        }

        // GET: Courses/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses.FindAsync(id);
            if (course == null)
            {
                return NotFound();
            }
            return View(course);

        }

        // POST: Courses/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CourseId,CourseName,CourseDescription")] Course course)
        {
            if (id != course.CourseId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(course);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CourseExists(course.CourseId))
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
            return View(course);
        }

        // GET: Courses/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses
                .FirstOrDefaultAsync(m => m.CourseId == id);
            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        // POST: Courses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course != null)
            {
                _context.Courses.Remove(course);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
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