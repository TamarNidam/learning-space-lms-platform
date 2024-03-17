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



       
        // GET: UserTasks/Edit/5
        public async Task<IActionResult> Edit(int? usertaskid)
        {
            if (usertaskid == null)
            {
                return NotFound();
            }

            var u = await _context.UserTasks.FindAsync(usertaskid);
            var u2 = await _context.Users.FindAsync(u.UserId);
            if (u == null && u2 == null)
            {
                return NotFound();
            }
            string subject = GetSubject((int)u.TaskId);
            var DTo = new UserTaskDTO
            {
                UserTaskId = u.UserTaskId,
                UserId = u.UserId,
                UserName = u2.FirstName + " " + u2.LastName,
                TaskSubject = subject,
                TaskId = u.TaskId,
                Mark = u.Mark,
                Remarks = u.Remarks,
                Done = u.Done
            };

            ViewBag.Context = GetContext((int)u.TaskId,(int)u.UserId);            
                         
            return View(DTo);
        }

        // POST: UserTasks/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int user, int permission, int courseid, int usertaskid, [Bind("UserTaskId,UserId,TaskId,Mark,Remarks,Done")] UserTaskDTO userTask)
        {
            if (usertaskid != userTask.UserTaskId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var sql = $"UPDATE [UserTask] SET Mark = '{userTask.Mark}',Remarks = '{userTask.Remarks}' WHERE UserTaskId = {usertaskid}";
                    await _context.Database.ExecuteSqlRawAsync(sql);
                    //_context.Update(userTask);
                    //await _context.SaveChangesAsync();

                    //Insert a new alarm entry into the Alarm table
                    var maxIdAlarm = await _context.Alarms.MaxAsync(u => (int?)u.AlarmId) ?? 0;
                    var newIdAlarm = maxIdAlarm + 1;
                    sql = $"INSERT INTO [Alarms] (AlarmId,CourseId,AlarmType,TypeId) VALUES ({newIdAlarm},{courseid}, 'Message', {(usertaskid * 10) + 4})";
                    await _context.Database.ExecuteSqlRawAsync(sql);

                    var usere = await _context.Users.Where(u => u.UserId == user).FirstOrDefaultAsync();
                    bool emailSent = AlarmsController.SendContactFormEmail($"{usere.FirstName}", $"{usere.Email}", 4, $"{courseid}", "");
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
                return Redirect($"/Tasks/Details?user={user}&permission={permission}&courseid={courseid}&taskid={userTask.TaskId}");
            }

            return View(userTask);
        }

        private string GetContext(int taskId, int user)
        {
            string filePath = Path.Combine(".", "TextFiles", "Tasks", "UserTasks", $"taskid_{taskId}__userid_{user}.txt");
            if (MyFile.Exists(filePath))
            {
                return MyFile.ReadAllText(filePath);
            }
            return null;
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


        private bool UserTaskExists(int id)
        {
            return _context.UserTasks.Any(e => e.UserTaskId == id);
        }
    }
}
