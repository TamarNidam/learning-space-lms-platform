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
    public class ClassesController : Controller
    {
        private readonly LearningSpaceContext _context;

        public ClassesController(LearningSpaceContext context)
        {
            _context = context;
        }

        // GET: Classes
        public async Task<IActionResult> Index()
        {
            var classes = await _context.Classes.ToListAsync();
            var courseDTOs = classes
                           .Select(u => new ClassDTO
                           {
                               ClassId = u.ClassId,
                               ClassName = u.ClassName
                           }).ToList();
            return View(courseDTOs);
        }

        // GET: Classes/Details/5
        public async Task<IActionResult> Details(int? classid)
        {
            try
            {

                if (classid == null)
                {
                    return NotFound();
                }

                var @class = await _context.Classes
                    .FirstOrDefaultAsync(m => m.ClassId == classid);
                if (@class == null)
                {
                    return NotFound();
                }
                
                var classDTO = new ClassDTO
                {
                    ClassId = @class.ClassId,
                    ClassName = @class.ClassName,
                    Students = _context.StudentInClasses.Count(s => s.ClassId == classid)
                };
                return View(classDTO);
            }
            catch (Exception ex)
            {
                return View("Error", ex);
            }
        }

        // GET: Classes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Classes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ClassId,ClassName")] ClassDTO @classDTO)
        {
            if (ModelState.IsValid)
            {
               var u = await _context.Classes
                   .FromSqlRaw("SELECT TOP 1 * FROM Classes WHERE ClassName = {0}", @classDTO.ClassName)
                   .FirstOrDefaultAsync();
                if (u != null)
                {
                    ViewBag.ErrorMessage = "An existing name";
                    return View(@classDTO);
                }

                var maxClassId = await _context.Classes.MaxAsync(u => (int?)u.ClassId) ?? 0;
                var newClassId = maxClassId + 1;
                var sql = $"INSERT INTO [Classes] (ClassId,ClassName) VALUES ({newClassId}, '{@classDTO.ClassName}')";
                await _context.Database.ExecuteSqlRawAsync(sql);

                return Redirect($"/Classes/Index?user=0&permission=0");
            }
            return View(@classDTO);
        }

        // GET: Classes/Edit/5
        public async Task<IActionResult> Edit(int? classid)
        {
                       if (classid == null)
            {
                return NotFound();
            }

            var @class = await _context.Classes.FindAsync(classid);
            if (@class == null)
            {
                return NotFound();
            }

            var classDTO = new ClassDTO
            {
                ClassId = @class.ClassId,
                ClassName = @class.ClassName
            };

            return View(classDTO);
        }

        // POST: Classes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int user, int permission, int classid, [Bind("ClassId,ClassName")] ClassDTO @classDTO)
        {
            if (classid != @classDTO.ClassId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var sql = $"UPDATE [Classes] SET ClassName = '{@classDTO.ClassName}' WHERE ClassId = {@classDTO.ClassId}";
                    await _context.Database.ExecuteSqlRawAsync(sql);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClassExists(@classDTO.ClassId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return Redirect($"/Classes/Details?user={user}&permission={permission}&classid={classid}");
            }
            return View(@classDTO);
        }

        // GET: Classes/Delete/5
        public async Task<IActionResult> Delete(int classid)
        {

            try
            {

                if (classid == null)
                {
                    return NotFound();
                }

                var @class = await _context.Classes
                    .FirstOrDefaultAsync(m => m.ClassId == classid);
                if (@class == null)
                {
                    return NotFound();
                }

                var classDTO = new ClassDTO
                {
                    ClassId = @class.ClassId,
                    ClassName = @class.ClassName,
                    Students = _context.StudentInClasses.Count(s => s.ClassId == classid)
                };
                return View(classDTO);
            }
            catch (Exception ex)
            {
                return View("Error", ex);
            }

        }

        // POST: Classes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int user, int permission, int classid)
        {
            var @class = await _context.Classes.FindAsync(classid);

            if (@class != null)
            {
                var classes = await _context.StudentInClasses.Where(c => c.ClassId == classid).ToListAsync();
                _context.StudentInClasses.RemoveRange(classes);
                await _context.SaveChangesAsync();
                _context.Classes.Remove(@class);
            }

            await _context.SaveChangesAsync();
            return Redirect($"/Classes/Index?user=0&permission=0");
        }

        private bool ClassExists(int id)
        {
            return _context.Classes.Any(e => e.ClassId == id);
        }
    }
}
