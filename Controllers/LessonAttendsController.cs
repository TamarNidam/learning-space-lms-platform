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
    public class LessonAttendsController : Controller
    {
        private readonly LearningSpaceContext _context;

        public LessonAttendsController(LearningSpaceContext context)
        {
            _context = context;
        }

        // GET: LessonAttends
        public async Task<IActionResult> Index()
        {
            var learningSpaceContext = _context.LessonAttends.Include(l => l.Lesson).Include(l => l.User);
            return View(await learningSpaceContext.ToListAsync());
        }

        // GET: LessonAttends/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lessonAttend = await _context.LessonAttends
                .Include(l => l.Lesson)
                .Include(l => l.User)
                .FirstOrDefaultAsync(m => m.LessonAttendId == id);
            if (lessonAttend == null)
            {
                return NotFound();
            }

            return View(lessonAttend);
        }

        // GET: LessonAttends/Create
        public IActionResult Create()
        {
            ViewData["LessonId"] = new SelectList(_context.Lessons, "LessonId", "LessonType");
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "FirstName");
            return View();
        }

        // POST: LessonAttends/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("LessonAttendId,LessonId,UserId,Attend")] LessonAttend lessonAttend)
        {
            if (ModelState.IsValid)
            {
                _context.Add(lessonAttend);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["LessonId"] = new SelectList(_context.Lessons, "LessonId", "LessonType", lessonAttend.LessonId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "FirstName", lessonAttend.UserId);
            return View(lessonAttend);
        }

        // GET: LessonAttends/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lessonAttend = await _context.LessonAttends.FindAsync(id);
            if (lessonAttend == null)
            {
                return NotFound();
            }
            ViewData["LessonId"] = new SelectList(_context.Lessons, "LessonId", "LessonType", lessonAttend.LessonId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "FirstName", lessonAttend.UserId);
            return View(lessonAttend);
        }

        // POST: LessonAttends/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("LessonAttendId,LessonId,UserId,Attend")] LessonAttend lessonAttend)
        {
            if (id != lessonAttend.LessonAttendId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(lessonAttend);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LessonAttendExists(lessonAttend.LessonAttendId))
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
            ViewData["LessonId"] = new SelectList(_context.Lessons, "LessonId", "LessonType", lessonAttend.LessonId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "FirstName", lessonAttend.UserId);
            return View(lessonAttend);
        }

        // GET: LessonAttends/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lessonAttend = await _context.LessonAttends
                .Include(l => l.Lesson)
                .Include(l => l.User)
                .FirstOrDefaultAsync(m => m.LessonAttendId == id);
            if (lessonAttend == null)
            {
                return NotFound();
            }

            return View(lessonAttend);
        }

        // POST: LessonAttends/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var lessonAttend = await _context.LessonAttends.FindAsync(id);
            if (lessonAttend != null)
            {
                _context.LessonAttends.Remove(lessonAttend);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LessonAttendExists(int id)
        {
            return _context.LessonAttends.Any(e => e.LessonAttendId == id);
        }
    }
}
