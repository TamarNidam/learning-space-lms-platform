using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Learning_Space.Models;
using System.Net.Mail;
using System.Net;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Cors;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using Learning_Space.DTO;
using Microsoft.Build.Framework;
using NuGet.Packaging;
using System.Security.Claims;
using SystemTask = System.Threading.Tasks.Task;
using System.Text;
using NuGet.Protocol.Plugins;
using System.Data.SqlTypes;




namespace Learning_Space.Controllers
{
    public class AlarmsController : Controller
    {
        private readonly LearningSpaceContext _context;

        public AlarmsController(LearningSpaceContext context)
        {
            _context = context;
        }

        // GET: Alarms
        public async Task<IActionResult> Index(int user, int permission)
        {

            List<int?> courseIds = null;
            List<Alarm> alarms = null;

            if (permission == 2)
            {
                var classIds = await _context.StudentInClasses
                                .Where(uc => uc.UserId == user)
                                .Select(uc => uc.ClassId)
                                .ToListAsync();

                courseIds = await _context.CourseInClasses
                   .Where(cic => classIds.Contains(cic.ClassId))
                   .Select(cic => cic.CourseId)
                   .ToListAsync();

                var userTasks = await _context.UserTasks.Where(t => t.UserId == user).Select(t => t.UserTaskId).ToListAsync();

                alarms = await _context.Alarms
                                .Where(alarm => courseIds.Contains(alarm.CourseId) && alarm.TypeId % 10 != 3 && alarm.TypeId % 10 != 4)
                                .ToListAsync();

                alarms.AddRange(_context.Alarms.Where(a => a.TypeId % 10 == 4 && userTasks.Contains((int)(a.TypeId / 10))));


            }
            else if (permission == 1)
            {
                courseIds = await _context.Teachers
                        .Where(t => t.UserId == user)
                        .Select(t => t.CourseId)
                        .ToListAsync();

                alarms = await _context.Alarms
               .Where(alarm => courseIds.Contains(alarm.CourseId) && alarm.TypeId % 10 != 4 && alarm.TypeId % 10 != 8)
               .ToListAsync();
            }

            var userAlarm = await _context.Alarms
                .Where(alarm =>  (alarm.TypeId / 10) == user && (alarm.TypeId % 10) == 8).FirstOrDefaultAsync();
            if (userAlarm != null)
            {
                alarms.AddRange(new List<Alarm> { userAlarm });
            }

            DateOnly today = DateOnly.FromDateTime(DateTime.Today);
            var tasks = await _context.UserTasks.Where(t => t.UserId == user && !t.Done && t.Task.EndDate < today).CountAsync();
            var taskDTO = new AlarmDTO
            {
                AlarmId = (await _context.Alarms.MaxAsync(u => (int?)u.AlarmId) ?? 0)+1,
                CourseId = 0,
                CorseName = "",
                AlarmType = "9",
                TypeId = tasks,
                TaskId = 0
            };
            var alarmDTOs = alarms
                           .Select(u => new AlarmDTO
                           {
                               AlarmId = u.AlarmId,
                               CourseId = (int)u.CourseId,
                               CorseName = u.CourseId == 0 ? string.Empty : GetCourseName(u.CourseId),
                               AlarmType = GetTypeAsync((int)u.TypeId % 10),
                               TypeId = (int)u.TypeId / 10,
                               TaskId = GetTaskId((GetTypeAsync((int)u.TypeId % 10)), (u.TypeId / 10))
                           }).OrderByDescending(a => a.AlarmId).ToList();

            alarmDTOs.Insert(0, taskDTO);


            return View(alarmDTOs);
        }


        private string GetTypeAsync(int v)

        {
            string result = "";

            if (v == 1) { result = "1"; }
            else if (v == 2) { result = "2"; }
            else if (v == 3) { result = "3"; }
            else if (v == 4) { result = "4"; }
            else if (v == 5) { result = "5"; }
            else if (v == 6) { result = "6"; }
            else if (v == 7) { result = "7"; }
            else if (v == 8) { result = "8"; }

            return result;
        }


        private int GetTaskId(string alarmType, int? typeId)
        {
            if (alarmType == "4" || alarmType == "3")
            {
                var task = _context.UserTasks.Where(t => t.UserTaskId == typeId).Select(n => n.TaskId).FirstOrDefault();
                return (int)task;
            }

            return 0;
        }

        private string GetCourseName(int? courseId)
        {
            string name = _context.Courses.Where(c => c.CourseId == courseId).Select(cn => cn.CourseName).FirstOrDefault();
            return name ?? "";
        }

            public static bool SendContactFormEmail(string name, string email, int type, string? courseName, string? studentName)
            {
                using (var smtpClient = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtpClient.UseDefaultCredentials = false;
                    smtpClient.Credentials = new NetworkCredential("learning.space80@gmail.com", "bxnd akzo bzra qfaa");
                    smtpClient.EnableSsl = true;

                    var mailMessage = new MailMessage();
                    mailMessage.From = new MailAddress("learning.space80@gmail.com");
                    mailMessage.To.Add(email);
                    mailMessage.Subject = "New Alarm";
                string allString =$"Hi {name},\n";
                if(type == 1)
                {
                    allString = allString+$"You have a new lesson in {courseName}.";
                }
                else if(type == 2) { allString = allString + $"You have a new message in {courseName}."; }
                else if (type == 3) { allString = allString + $"Your student: {studentName}."; }
                else if (type== 4) { allString += $"You have a new mark in the {courseName} course."; }
                else if (type == 5) { allString += $"You have new study metirial in the {courseName} course."; }
                else if (type == 6) { allString += $"You have new course: {courseName} in your schduel."; }
                else if (type == 7) { allString += $"You have new task in the {courseName} course."; }
                else if (type == 8) { allString += $"Wellcome to Lerning Space😊"; }
                if (type >= 1 && type <=8)
                { 
                mailMessage.Body = allString;
                    smtpClient.Send(mailMessage);
                    return true;
                }
                else
                {
                    return false;
                }
            }

            }

            
    }
}
