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
    public class MoreStudiesController : Controller
    {
        private readonly LearningSpaceContext _context;

        public MoreStudiesController(LearningSpaceContext context)
        {
            _context = context;
        }

        // GET: MoreStudies
        public async Task<IActionResult> Index()
        {
            var learningSpaceContext = _context.MoreStudies.Include(m => m.Course);
            return View(await learningSpaceContext.ToListAsync());
        }

        // GET: MoreStudies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var moreStudy = await _context.MoreStudies
                .Include(m => m.Course)
                .FirstOrDefaultAsync(m => m.MoreId == id);
            if (moreStudy == null)
            {
                return NotFound();
            }

            return View(moreStudy);
        }

        // GET: MoreStudies/Create
        public IActionResult Create()
        {
            ViewData["CourseId"] = new SelectList(_context.Courses, "CourseId", "CourseName");
            return View();
        }

        // POST: MoreStudies/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MoreId,CourseId,MoreStudySubject,Content,MoreStudyUrl")] MoreStudy moreStudy)
        {
            if (ModelState.IsValid)
            {
                _context.Add(moreStudy);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CourseId"] = new SelectList(_context.Courses, "CourseId", "CourseName", moreStudy.CourseId);
            return View(moreStudy);
        }

        // GET: MoreStudies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var moreStudy = await _context.MoreStudies.FindAsync(id);
            if (moreStudy == null)
            {
                return NotFound();
            }
            ViewData["CourseId"] = new SelectList(_context.Courses, "CourseId", "CourseName", moreStudy.CourseId);
            return View(moreStudy);
        }

        // POST: MoreStudies/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MoreId,CourseId,MoreStudySubject,Content,MoreStudyUrl")] MoreStudy moreStudy)
        {
            if (id != moreStudy.MoreId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(moreStudy);
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
                return RedirectToAction(nameof(Index));
            }
            ViewData["CourseId"] = new SelectList(_context.Courses, "CourseId", "CourseName", moreStudy.CourseId);
            return View(moreStudy);
        }

        // GET: MoreStudies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var moreStudy = await _context.MoreStudies
                .Include(m => m.Course)
                .FirstOrDefaultAsync(m => m.MoreId == id);
            if (moreStudy == null)
            {
                return NotFound();
            }

            return View(moreStudy);
        }

        // POST: MoreStudies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var moreStudy = await _context.MoreStudies.FindAsync(id);
            if (moreStudy != null)
            {
                _context.MoreStudies.Remove(moreStudy);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MoreStudyExists(int id)
        {
            return _context.MoreStudies.Any(e => e.MoreId == id);
        }
    }
}
