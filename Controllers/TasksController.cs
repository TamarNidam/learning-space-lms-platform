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
        string filePath = Path.Combine(".", "TextFiles", "Tasks", "Tasks.txt");
                
                
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
                                StartDate = u.StartDate,
                                EndDate = u.EndDate,
                                CourseId = u.CourseId,
                                CourseName = u.Course.CourseName,
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

        private string GetSubject(int taskId)
        {
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
        public async Task<IActionResult> Create([Bind("TaskId,TaskType,StartDate,EndDate,CourseId")] Task task)
        {
            if (ModelState.IsValid)
            {
                _context.Add(task);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CourseId"] = new SelectList(_context.Courses, "CourseId", "CourseName", task.CourseId);
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
