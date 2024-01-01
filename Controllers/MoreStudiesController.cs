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
                return Redirect($"/MoreStudies/Index?user={user}&permission={permission}&courseid={courseid}");
            }
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
                return Redirect($"/Classes/Details?user=");
            }
            ViewData["CourseId"] = new SelectList(_context.Courses, "CourseId", "CourseName", moreStudy.CourseId);
            return View(moreStudy);
        }

       

        // POST: MoreStudies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int user, int permission, int courseid, int moreid)
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
