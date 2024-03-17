using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Learning_Space.Models;
using Learning_Space.DTO;

namespace Learning_Space.Controllers
{
    public class MoreStudiesController : Controller
    {
        private readonly LearningSpaceContext _context;

        public MoreStudiesController(LearningSpaceContext context)
        {
            _context = context;
        }

        // GET: MoreStudies
        public async Task<IActionResult> Index(int? courseid)
        {
            if (!courseid.HasValue)
            {
                return NotFound();
            }
               var moreStudies = await _context.MoreStudies
                .Where(m => m.CourseId == courseid)
                .Select(u => new MoreStudyDTO
                           {
                              MoreId = u.MoreId,
                               CourseId = u.CourseId,
                               MoreStudySubject= u.MoreStudySubject,
                               Content = u.Content,
                               MoreStudyUrl = u.MoreStudyUrl
                           }).ToListAsync();
            return View(moreStudies);
        }

        // GET: MoreStudies/Details/5
        public async Task<IActionResult> Details(int? moreid)
        {
            if (moreid == null)
            {
                return NotFound();
            }

            var moreStudy = await _context.MoreStudies.FirstOrDefaultAsync(m => m.MoreId == moreid);
            if (moreStudy == null)
            {
                return NotFound();
            }
            var moreStudyDTO = new MoreStudyDTO
            {
                MoreId = moreStudy.MoreId,
                CourseId = moreStudy.CourseId,
                MoreStudySubject = moreStudy.MoreStudySubject,
                Content = moreStudy.Content,
                MoreStudyUrl = moreStudy.MoreStudyUrl
            };

            return View(moreStudyDTO);
        }

        // GET: MoreStudies/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: MoreStudies/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int user, int permission, int courseid, [Bind("MoreId,CourseId,MoreStudySubject,Content,MoreStudyUrl")] MoreStudyDTO moreStudy)
        {
            if (ModelState.IsValid)
            {
                var maxId = await _context.MoreStudies.MaxAsync(u => (int?)u.MoreId) ?? 0;
                var newId = maxId + 1;
                var sql = $"INSERT INTO [MoreStudies] (MoreId,CourseId,MoreStudySubject,Content,MoreStudyUrl) VALUES ({newId}, {courseid},'{moreStudy.MoreStudySubject}', '{moreStudy.Content}', '{moreStudy.MoreStudyUrl}')";
                await _context.Database.ExecuteSqlRawAsync(sql);

                //Insert a new alarm entry into the Alarm table
                var maxIdAlarm = await _context.Alarms.MaxAsync(u => (int?)u.AlarmId) ?? 0;
                var newIdAlarm = maxIdAlarm + 1;
                sql = $"INSERT INTO [Alarms] (AlarmId,CourseId,AlarmType,TypeId) VALUES ({newIdAlarm},{courseid}, 'Message', {(newId*10)+5})";
                await _context.Database.ExecuteSqlRawAsync(sql);

                var classw = await _context.CourseInClasses.Where(c => c.CourseId == courseid).Select(c => c.ClassId).FirstOrDefaultAsync();
                sql = $"SELECT Users.* " +
     $"FROM Users JOIN StudentInClass " +
     $" ON Users.UserId = StudentInClass.UserId " +
     $"WHERE StudentInClass.ClassId = {classw}";
                var users = await _context.Users.FromSqlRaw(sql).ToListAsync();
                foreach (var usere in users)
                {
                    bool emailSent = AlarmsController.SendContactFormEmail($"{usere.FirstName}", $"{usere.Email}", 5, $"{courseid}", "");
                }

                return Redirect($"/MoreStudies/Index?user={user}&permission={permission}&courseid={courseid}");
            }
            return View(moreStudy);
        }

        // GET: MoreStudies/Edit/5
        public async Task<IActionResult> Edit(int? moreid)
        {
            if (moreid == null)
            {
                return NotFound();
            }

            var moreStudy = await _context.MoreStudies.FindAsync(moreid);
            if (moreStudy == null)
            {
                return NotFound();
            }
            var moreStudyDTO = new MoreStudyDTO
            {
                MoreId = moreStudy.MoreId,
                CourseId = moreStudy.CourseId,
                MoreStudySubject = moreStudy.MoreStudySubject,
                Content = moreStudy.Content,
                MoreStudyUrl = moreStudy.MoreStudyUrl
            };
            return View(moreStudyDTO);
        }

        // POST: MoreStudies/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int user, int permission, int courseid, int moreid, [Bind("MoreId,CourseId,MoreStudySubject,Content,MoreStudyUrl")] MoreStudyDTO moreStudy)
        {
            if (moreid != moreStudy.MoreId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var moreStudyDTO = new MoreStudy
                    {
                        MoreId = moreStudy.MoreId,
                        CourseId = courseid,
                        MoreStudySubject = moreStudy.MoreStudySubject,
                        Content = moreStudy.Content,
                        MoreStudyUrl = moreStudy.MoreStudyUrl
                    };
                    _context.Update(moreStudyDTO);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MoreStudyExists(moreStudy.MoreId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return Redirect($"/MoreStudies/Details?user={user}&permission={permission}&courseid={courseid}&moreid={moreid}");
            }
           
            return View(moreStudy);
        }

       

        // POST: MoreStudies/Delete/5
        [HttpPost]
   
        public async Task<IActionResult> Deletey(int user, int permission, int courseid, int moreid)
        {
            var moreStudy = await _context.MoreStudies.FindAsync(moreid);
            if (moreStudy != null)
            {
                _context.MoreStudies.Remove(moreStudy);
            }

            await _context.SaveChangesAsync();
            return Redirect($"/MoreStudies/Index?user={user}&permission={permission}&courseid={courseid}");
        }

        private bool MoreStudyExists(int id)
        {
            return _context.MoreStudies.Any(e => e.MoreId == id);
        }
    }
}
