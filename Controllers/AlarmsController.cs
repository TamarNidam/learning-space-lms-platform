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
        public async Task<IActionResult> Index()
        {
            var learningSpaceContext = _context.Alarms.Include(a => a.Course);
            return View(await learningSpaceContext.ToListAsync());
        }

        // GET: Alarms/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var alarm = await _context.Alarms
                .Include(a => a.Course)
                .FirstOrDefaultAsync(m => m.AlarmId == id);
            if (alarm == null)
            {
                return NotFound();
            }

            return View(alarm);
        }

        // GET: Alarms/Create
        public IActionResult Create()
        {
            ViewData["CourseId"] = new SelectList(_context.Courses, "CourseId", "CourseName");
            return View();
        }

        // POST: Alarms/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AlarmId,CourseId,AlarmType,TypeId")] Alarm alarm)
        {
            if (ModelState.IsValid)
            {
                _context.Add(alarm);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CourseId"] = new SelectList(_context.Courses, "CourseId", "CourseName", alarm.CourseId);
            return View(alarm);
        }



        public IActionResult SendEmail()
        {
            string emailTo = "tamtam2003n@gmail.com";
            string subject = "Hello";
            string body = "This is the email body.";

            try
            {
                MailMessage mail = new MailMessage();
                SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587); // Replace with your SMTP server address and port

                mail.From = new MailAddress("tamar.nidam1@gmail.com"); // Replace with your email address
                mail.To.Add(emailTo);
                mail.Subject = subject;
                mail.Body = body;

                smtpClient.Credentials = new NetworkCredential("tamar.nidam1@gmail.com", "tamtam2003n"); // Replace with your email address and password
                smtpClient.EnableSsl = true; // Enable SSL encryption, if required by your email provider

                smtpClient.Send(mail);

                return RedirectToAction("Index", "Home"); // Redirect to a success page
            }
            catch (Exception ex)
            {
                // Handle exception or redirect to an error page
                return RedirectToAction("Error", "Home");
            }
        }
        private bool AlarmExists(int id)
        {
            return _context.Alarms.Any(e => e.AlarmId == id);
        }
    }
}
