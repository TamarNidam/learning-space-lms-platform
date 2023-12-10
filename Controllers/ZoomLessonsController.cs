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
    public class ZoomLessonsController : Controller
    {
        private readonly LearningSpaceContext _context;

        public ZoomLessonsController(LearningSpaceContext context)
        {
            _context = context;
        }

        // GET: ZoomLessons
        public async Task<IActionResult> Index()
        {
            var learningSpaceContext = _context.ZoomLessons.Include(z => z.Lesson);
            return View(await learningSpaceContext.ToListAsync());
        }

        // GET: ZoomLessons/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var zoomLesson = await _context.ZoomLessons
                .Include(z => z.Lesson)
                .FirstOrDefaultAsync(m => m.ZoomLesson1 == id);
            if (zoomLesson == null)
            {
                return NotFound();
            }

            return View(zoomLesson);
        }

        // GET: ZoomLessons/Create
        public IActionResult Create()
        {
            ViewData["LessonId"] = new SelectList(_context.Lessons, "LessonId", "LessonType");
            return View();
        }

        // POST: ZoomLessons/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ZoomLesson1,LessonId,ZoomUrl")] ZoomLesson zoomLesson)
        {
            if (ModelState.IsValid)
            {
                _context.Add(zoomLesson);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["LessonId"] = new SelectList(_context.Lessons, "LessonId", "LessonType", zoomLesson.LessonId);
            return View(zoomLesson);
        }

        // GET: ZoomLessons/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var zoomLesson = await _context.ZoomLessons.FindAsync(id);
            if (zoomLesson == null)
            {
                return NotFound();
            }
            ViewData["LessonId"] = new SelectList(_context.Lessons, "LessonId", "LessonType", zoomLesson.LessonId);
            return View(zoomLesson);
        }

        // POST: ZoomLessons/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ZoomLesson1,LessonId,ZoomUrl")] ZoomLesson zoomLesson)
        {
            if (id != zoomLesson.ZoomLesson1)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(zoomLesson);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ZoomLessonExists(zoomLesson.ZoomLesson1))
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
            ViewData["LessonId"] = new SelectList(_context.Lessons, "LessonId", "LessonType", zoomLesson.LessonId);
            return View(zoomLesson);
        }

        // GET: ZoomLessons/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var zoomLesson = await _context.ZoomLessons
                .Include(z => z.Lesson)
                .FirstOrDefaultAsync(m => m.ZoomLesson1 == id);
            if (zoomLesson == null)
            {
                return NotFound();
            }

            return View(zoomLesson);
        }

        // POST: ZoomLessons/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var zoomLesson = await _context.ZoomLessons.FindAsync(id);
            if (zoomLesson != null)
            {
                _context.ZoomLessons.Remove(zoomLesson);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ZoomLessonExists(int id)
        {
            return _context.ZoomLessons.Any(e => e.ZoomLesson1 == id);
        }
    }
}
