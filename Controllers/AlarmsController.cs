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
            //SendMail("tamtam2003n@gmail.com", "j", "workk");

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
               .Where(alarm => courseIds.Contains(alarm.CourseId) && alarm.TypeId % 10 != 4)
               .ToListAsync();
            }

            var userAlarm = await _context.Alarms
                .Where(alarm => alarm.TypeId % 10 == 8 && (alarm.TypeId / 10) == user).FirstOrDefaultAsync();
            if (userAlarm != null)
            {
                alarms.AddRange(new List<Alarm> { userAlarm });
            }

            var alarmDTOs = alarms
                           .Select(u => new AlarmDTO
                           {
                               AlarmId = u.AlarmId,
                               CourseId = (int)u.CourseId,
                               CorseName = u.CourseId == 0 ? string.Empty : GetCourseName(u.CourseId),
                               AlarmType = GetTypeAsync(u.TypeId % 10),
                               TypeId = (int)u.TypeId / 10,
                               TaskId = GetTaskId((GetTypeAsync(u.TypeId % 10)), (u.TypeId / 10))
                           }).OrderByDescending(a => a.AlarmId).ToList();

            return View(alarmDTOs);
        }


        private string GetTypeAsync(int? v)

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


        //[HttpPost("send")]
        //public bool sentMail(string email, string subject, string body)
        //{
        //    try
        //    {
        //        var mail= 
        //        SmtpClient smtpClient = new SmtpClient("smtp.office365.com", 587);
        //        smtpClient.EnableSsl = true;
        //        smtpClient.UseDefaultCredentials = false;
        //        smtpClient.Credentials = new System.Net.NetworkCredential("tamar.nidam1@gmail.com", "tamtam2003n");

        //        MailMessage mailMessage = new MailMessage();
        //        mailMessage.From = new MailAddress("tamar.nidam1@gmail.com");
        //        mailMessage.To.Add(email);
        //        mailMessage.Subject = subject;
        //        mailMessage.Body = body;

        //        smtpClient.Send(mailMessage);
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.Message);
        //        return false;
        //    }
        //}

        //public IActionResult SendEmail()
        //{
        //    string emailTo = "tamtam2003n@gmail.com";
        //    string subject = "Hello";
        //    string body = "This is the email body.";

        //    try
        //    {
        //        MailMessage mail = new MailMessage();
        //        SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587); // Replace with your SMTP server address and port

        //        mail.From = new MailAddress("tamar.nidam1@gmail.com"); // Replace with your email address
        //        mail.To.Add(emailTo);
        //        mail.Subject = subject;
        //        mail.Body = body;

        //        smtpClient.Credentials = new NetworkCredential("tamar.nidam1@gmail.com", "tamtam2003n"); // Replace with your email address and password
        //        smtpClient.EnableSsl = true; // Enable SSL encryption, if required by your email provider

        //        smtpClient.Send(mail);

        //        return RedirectToAction("Index", "Home"); // Redirect to a success page
        //    }
        //    catch (Exception ex)
        //    {
        //        // Handle exception or redirect to an error page
        //        return RedirectToAction("Error", "Home");
        //    }
        //}

        [HttpPost("send")]
        public async Task<bool> SendMail(string email, string subject, string body)
        {
            var mail = "tamar.nidam1@gmail.com";
            var ps = "tamtam2003n";

            using (var client = new SmtpClient("smtp-mail.outlook.com", 587))
            {
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential(mail, ps);

                var mailMessage = new MailMessage(
                    from: mail,
                    to: email,
                    subject: subject,
                    body: body
                );

                await client.SendMailAsync(mailMessage);
            }

            return true;
        }

        private bool AlarmExists(int id)
        {
            return _context.Alarms.Any(e => e.AlarmId == id);
        }
    }
}
