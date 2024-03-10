using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Learning_Space.Models;
using Learning_Space.DTO;
using MyFile = System.IO.File;

namespace Learning_Space.Controllers
{
    public class UserTasksController : Controller
    {
        private readonly LearningSpaceContext _context;

        public UserTasksController(LearningSpaceContext context)
        {
            _context = context;
        }

        // GET: UserTasks
        public async Task<IActionResult> Index(int taskid)
        {
            try
            {
                string subject = GetSubject(taskid);
                var userTasks = await _context.UserTasks
            .Where(t => t.TaskId == taskid)
            .Include(t => t.User)
            .Select(u => new UserTaskDTO
            {
                UserTaskId = u.UserTaskId,
                UserId = u.UserId,
                UserName = u.User.FirstName +" "+ u.User.LastName,
                TaskId = u.TaskId,
                TaskSubject = subject,
                Mark = u.Mark,
                Remarks = u.Remarks,
                Done = u.Done
            })
            .ToListAsync();
                return View(userTasks);
            }
            catch (Exception ex)
            {
                return View(ex);
            }
        }

        private string GetSubject(int taskId)
        {
            string filePath = Path.Combine(".", "TextFiles", "Tasks", "Tasks.txt");
            string[] lines = MyFile.ReadAllLines(filePath);

            foreach (string line in lines)
            {
                string[] parts = line.Split(',');

                if (parts.Length >= 3 && int.Parse(parts[0]) == taskId)
                {
                    return parts[1];

                }
            }
            return null;
        }


       
        // GET: UserTasks/Edit/5
        public async Task<IActionResult> Edit(int? usertaskid)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userTask = await _context.UserTasks.FindAsync(id);
            if (userTask == null)
            {
                return NotFound();
            }
            ViewData["TaskId"] = new SelectList(_context.Tasks, "TaskId", "TaskId", userTask.TaskId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "FirstName", userTask.UserId);
            return View(userTask);
        }

        // POST: UserTasks/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UserTaskId,UserId,TaskId,Mark,Remarks,Done")] UserTask userTask)
        {
            if (id != userTask.UserTaskId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(userTask);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserTaskExists(userTask.UserTaskId))
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
            ViewData["TaskId"] = new SelectList(_context.Tasks, "TaskId", "TaskId", userTask.TaskId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "FirstName", userTask.UserId);
            return View(userTask);
        }

      
        private bool UserTaskExists(int id)
        {
            return _context.UserTasks.Any(e => e.UserTaskId == id);
        }
    }
}
