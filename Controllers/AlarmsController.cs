using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Learning_Space.Models;

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

        // GET: Alarms/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var alarm = await _context.Alarms.FindAsync(id);
            if (alarm == null)
            {
                return NotFound();
            }
            ViewData["CourseId"] = new SelectList(_context.Courses, "CourseId", "CourseName", alarm.CourseId);
            return View(alarm);
        }

        // POST: Alarms/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AlarmId,CourseId,AlarmType,TypeId")] Alarm alarm)
        {
            if (id != alarm.AlarmId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(alarm);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AlarmExists(alarm.AlarmId))
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
            ViewData["CourseId"] = new SelectList(_context.Courses, "CourseId", "CourseName", alarm.CourseId);
            return View(alarm);
        }

        // GET: Alarms/Delete/5
        public async Task<IActionResult> Delete(int? id)
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

        // POST: Alarms/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var alarm = await _context.Alarms.FindAsync(id);
            if (alarm != null)
            {
                _context.Alarms.Remove(alarm);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AlarmExists(int id)
        {
            return _context.Alarms.Any(e => e.AlarmId == id);
        }
    }
}
