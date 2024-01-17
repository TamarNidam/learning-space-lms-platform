using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Learning_Space.Models;
using Task = Learning_Space.Models.Task;
using Learning_Space.DTO;
using MyFile = System.IO.File;

namespace Learning_Space.Controllers
{
    public class TasksController : Controller
    {
      
                
                
        private readonly LearningSpaceContext _context;

        public TasksController(LearningSpaceContext context)
        {
            _context = context;
        }

        // GET: Tasks
        public async Task<IActionResult> Index(int user, int permission, int? courseid)
        {
            try
            {
                List<Task> tasks;

                if (!courseid.HasValue)
                {
                    var sql = $"SELECT t.*" +
    $"FROM [Tasks] t " +
    $"JOIN CourseInClass cc ON cc.CourseId = t.CourseId " +
    $"JOIN StudentInClass sc ON sc.ClassId = cc.ClassId " +
    $"WHERE sc.UserId = '{user}'" +
    $"AND t.TaskType = 'Task'";
                    tasks = await _context.Tasks.FromSqlRaw(sql).ToListAsync();
                }
                else
                {
                    var sql = $"SELECT t.*" +
                        $"FROM [Tasks] t " +
                        $"WHERE t.CourseId = {courseid}" +
                        $"AND t.TaskType = 'Task'";
                    tasks = await _context.Tasks.FromSqlRaw(sql).ToListAsync();
                }

                var taskDTOs = tasks
                            .Select(u => new TaskDTO
                            {
                                TaskId = u.TaskId,
                                TaskType = u.TaskType,
                                StartDate = (DateOnly)u.StartDate,
                                EndDate = u.EndDate,
                                CourseId = u.CourseId,
                                CourseName = GetName(u.CourseId),
                                Subject = GetSubject(u.TaskId),
                                Context = "",
                                Done = permission == 2 && _context.UserTasks.FirstOrDefault(t => t.UserId == user && t.TaskId == u.TaskId)?.Done == true ? 1 : 0
                            }).OrderByDescending(u => u.EndDate).ToList();
                return View(taskDTOs);
            }
            catch (Exception ex)
            {
                return View(ex);
            }

        }

        private string GetName(int? courseId)
        {
            return _context.Courses.FirstOrDefault(c => c.CourseId == courseId).CourseName;
        }

        private string GetSubject(int taskId)
        {
            string filePath = Path.Combine(".", "TextFiles", "Tasks", "Tasks.txt");
            string[] lines = MyFile.ReadAllLines(filePath);
            
            foreach (string line in lines)
            {
                string[] parts = line.Split(',');

                if (parts.Length >= 3 && int.Parse(parts[0])  == taskId)
                {
                    return parts[1];
                    
                }
            }
            return null;
        }

        // GET: Tasks/Details/5
        public async Task<IActionResult> Details(int? taskid)
        {
            if (taskid == null)
            {
                return NotFound();
            }

            var task = await _context.Tasks
                .Include(t => t.Course)
                .FirstOrDefaultAsync(m => m.TaskId == taskid);
            if (task == null)
            {
                return NotFound();
            }

            return View(task);
        }

        // GET: Tasks/Create
        public IActionResult Create()
        {
           return View();
        }

        // POST: Tasks/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int user, int permission, int courseid, [Bind("TaskId,TaskType,StartDate,EndDate,CourseId,CourseName,Subject,Context,Done,PerformanceContent")] TaskDTO task)
        {
            if (ModelState.IsValid)
            {
                string filePath = Path.Combine(".", "TextFiles", "Tasks", "Tasks.txt");
                string taskString = $"{task.TaskId},{task.Subject},{task.Context}";

                MyFile.AppendAllText(filePath, taskString + Environment.NewLine);

                var maxId = await _context.Tasks.MaxAsync(u => (int?)u.TaskId) ?? 1;
                var newId = maxId + 1;
                var sql = $"INSERT INTO [Tasks] (TaskId,TaskType,StartDate,EndDate,CourseId) VALUES ({newId}, 'Task','{task.StartDate.ToString("yyyy-MM-dd")}', '{task.EndDate.ToString("yyyy-MM-dd")}', {courseid})";
                await _context.Database.ExecuteSqlRawAsync(sql);

                var maxIdUserTask = await _context.UserTasks.MaxAsync(u => (int?)u.UserTaskId) ?? 0;
                var courseInClass =  _context.CourseInClasses.FirstOrDefault(c => c.CourseId == courseid);
                var clas = courseInClass != null ? courseInClass.ClassId : null;
                //var clas = await _context.CourseInClasses.FirstOrDefault(c => c.CourseId == courseid).ClassId;
                var studentsInClass = _context.StudentInClasses
      .Where(s => s.ClassId == clas)
      .Select(s => s.UserId)
      .ToList();
                foreach (int UserId in studentsInClass)
                {
                    maxIdUserTask = maxIdUserTask + 1;
                    sql = $"INSERT INTO [UserTask] (UserTaskId,UserId,TaskId,Mark,Remarks,Done) VALUES ({maxIdUserTask},{UserId},{newId},0,'',0)";
                    await _context.Database.ExecuteSqlRawAsync(sql);

                }

                return Redirect($"/Tasks/Index?user={user}&permission={permission}&courseid={courseid}");

            }
            return View(task);
        }

        // GET: Tasks/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
            {
                return NotFound();
            }
            ViewData["CourseId"] = new SelectList(_context.Courses, "CourseId", "CourseName", task.CourseId);
            return View(task);
        }

        // POST: Tasks/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TaskId,TaskType,StartDate,EndDate,CourseId")] Task task)
        {
            if (id != task.TaskId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(task);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TaskExists(task.TaskId))
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
            ViewData["CourseId"] = new SelectList(_context.Courses, "CourseId", "CourseName", task.CourseId);
            return View(task);
        }

        // GET: Tasks/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var task = await _context.Tasks
                .Include(t => t.Course)
                .FirstOrDefaultAsync(m => m.TaskId == id);
            if (task == null)
            {
                return NotFound();
            }

            return View(task);
        }

        // POST: Tasks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task != null)
            {
                _context.Tasks.Remove(task);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TaskExists(int id)
        {
            return _context.Tasks.Any(e => e.TaskId == id);
        }
    }
}
