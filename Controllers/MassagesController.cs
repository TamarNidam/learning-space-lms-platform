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
    public class MassagesController : Controller
    {
        private readonly LearningSpaceContext _context;

        public MassagesController(LearningSpaceContext context)
        {
            _context = context;
        }

        // GET: Massages
        public async Task<IActionResult> Index()
        {
            var learningSpaceContext = _context.Massages.Include(m => m.Course);
            return View(await learningSpaceContext.ToListAsync());
        }

        // GET: Massages/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var massage = await _context.Massages
                .Include(m => m.Course)
                .FirstOrDefaultAsync(m => m.MessageId == id);
            if (massage == null)
            {
                return NotFound();
            }

            return View(massage);
        }

        // GET: Massages/Create
        public IActionResult Create()
        {
            ViewData["CourseId"] = new SelectList(_context.Courses, "CourseId", "CourseName");
            return View();
        }

        // POST: Massages/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MessageId,CourseId,MassageSubject,MassageDateTime,Content")] Massage massage)
        {
            if (ModelState.IsValid)
            {
                _context.Add(massage);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CourseId"] = new SelectList(_context.Courses, "CourseId", "CourseName", massage.CourseId);
            return View(massage);
        }

        // GET: Massages/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var massage = await _context.Massages.FindAsync(id);
            if (massage == null)
            {
                return NotFound();
            }
            ViewData["CourseId"] = new SelectList(_context.Courses, "CourseId", "CourseName", massage.CourseId);
            return View(massage);
        }

        // POST: Massages/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MessageId,CourseId,MassageSubject,MassageDateTime,Content")] Massage massage)
        {
            if (id != massage.MessageId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(massage);
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
                return RedirectToAction(nameof(Index));
            }
            ViewData["CourseId"] = new SelectList(_context.Courses, "CourseId", "CourseName", massage.CourseId);
            return View(massage);
        }

        // GET: Massages/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var massage = await _context.Massages
                .Include(m => m.Course)
                .FirstOrDefaultAsync(m => m.MessageId == id);
            if (massage == null)
            {
                return NotFound();
            }

            return View(massage);
        }

        // POST: Massages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var massage = await _context.Massages.FindAsync(id);
            if (massage != null)
            {
                _context.Massages.Remove(massage);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MassageExists(int id)
        {
            return _context.Massages.Any(e => e.MessageId == id);
        }
    }
}
