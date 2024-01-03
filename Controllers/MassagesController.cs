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
    public class MassagesController : Controller
    {
        private readonly LearningSpaceContext _context;

        public MassagesController(LearningSpaceContext context)
        {
            _context = context;
        }

        // GET: Massages
        public async Task<IActionResult> Index(int? courseid)
        {
            if (!courseid.HasValue)
            {
                return NotFound();
            }
            var message = await _context.Massages
             .Where(m => m.CourseId == courseid)
             .Select(u => new MassageDTO
             {
                 MessageId = u.MessageId,
                 CourseId = u.CourseId,
                 MassageSubject = u.MassageSubject,
                 MassageDateTime = u.MassageDateTime,
                 Content = u.Content
             }).ToListAsync();
            return View(message);
        }

        // GET: Massages/Details/5
        public async Task<IActionResult> Details(int? messageid)
        {
            if (messageid == null)
            {
                return NotFound();
            }

            var massage = await _context.Massages.FirstOrDefaultAsync(m => m.MessageId == messageid);
            if (massage == null)
            {
                return NotFound();
            }
            var messageDTO = new MassageDTO
            {
                MessageId = massage.MessageId,
                CourseId = massage.CourseId,
                MassageSubject = massage.MassageSubject,
                MassageDateTime = massage.MassageDateTime,
                Content = massage.Content
            };

            return View(messageDTO);
        }

        // GET: Massages/Create
        public IActionResult Create()
        {

            return View();
        }

        // POST: Massages/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int user, int permission, int courseid, [Bind("MessageId,CourseId,MassageSubject,MassageDateTime,Content")] MassageDTO massage)
        {
            if (ModelState.IsValid)
            {
                var maxId = await _context.Massages.MaxAsync(u => (int?)u.MessageId) ?? 0;
                var newId = maxId + 1;
                var sql = $"INSERT INTO [Massages] (MessageId,CourseId,MassageSubject,MassageDateTime,Content) VALUES ({newId}, {courseid},'{massage.MassageSubject}', GETDATE(), '{massage.Content}')";
                await _context.Database.ExecuteSqlRawAsync(sql);

                var maxIdAlarm = await _context.Alarms.MaxAsync(u => (int?)u.AlarmId) ?? 0;
                var newIdAlarm = maxIdAlarm + 1;
                sql = $"INSERT INTO [Alarms] (AlarmId,CourseId,AlarmType,TypeId) VALUES ({newIdAlarm},{courseid}, 'Message', {newId})";
                await _context.Database.ExecuteSqlRawAsync(sql);

                return Redirect($"/Massages/Index?user={user}&permission={permission}&courseid={courseid}");
            }

            return View(massage);
        }

        // GET: Massages/Edit/5
        public async Task<IActionResult> Edit(int? messageid)
        {
            if (messageid == null)
            {
                return NotFound();
            }

            var massage = await _context.Massages.FindAsync(messageid);
            if (massage == null)
            {
                return NotFound();
            }
            var messageDTO = new MassageDTO
            {
                MessageId = massage.MessageId,
                CourseId = massage.CourseId,
                MassageSubject = massage.MassageSubject,
                MassageDateTime = massage.MassageDateTime,
                Content = massage.Content
            };
            
            return View(messageDTO);
        }

        // POST: Massages/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int user, int permission, int courseid, int messageid, [Bind("MessageId,CourseId,MassageSubject,MassageDateTime,Content")] MassageDTO massage)
        {
            if (messageid != massage.MessageId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var messageDTO = new Massage
                    {
                        MessageId = massage.MessageId,
                        CourseId = courseid,
                        MassageSubject = massage.MassageSubject,
                        MassageDateTime = massage.MassageDateTime,
                        Content = massage.Content
                    };
                    _context.Update(messageDTO);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MassageExists(massage.MessageId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return Redirect($"/Massages/Details?user={user}&permission={permission}&courseid={courseid}&messageid={messageid}");
            }
            return View(massage);
        }

       

        // POST: Massages/Delete/5
        [HttpPost]
       
        public async Task<IActionResult> Deletey(int user, int permission, int courseid, int messageid)
        {
            var massage = await _context.Massages.FindAsync(messageid);
            if (massage != null)
            {
                _context.Massages.Remove(massage);
            }

            await _context.SaveChangesAsync();
            return Redirect($"/Massages/Index?user={user}&permission={permission}&courseid={courseid}");

        }

        private bool MassageExists(int id)
        {
            return _context.Massages.Any(e => e.MessageId == id);
        }
    }
}
