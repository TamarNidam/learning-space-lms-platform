using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Learning_Space.Models;
using Learning_Space.DTO;
using System.Data;

namespace Learning_Space.Controllers
{
    public class UsersController : Controller
    {
        private readonly LearningSpaceContext _context;

        public UsersController(LearningSpaceContext context)
        {
            _context = context;
        }

        // GET: Users
        public async Task<IActionResult> Index(int user, int permission, int? courseid)
        {
            try
            {
                List<Models.User> users;

                if (!courseid.HasValue)
                {
                    users = await _context.Users.ToListAsync();
                }
                else
                {
                    var sql = $"SELECT Users.* FROM Users JOIN StudentInClass ON Users.UserId = StudentInClass.UserId JOIN CourseInClass ON StudentInClass.ClassId = CourseInClass.ClassId WHERE CourseInClass.Courseid = {courseid}";
                    users = await _context.Users.FromSqlRaw(sql).ToListAsync();
                                       }
                var userDTOs = users
                            .Select(u => new UserDTO
                            {
                                UserId = u.UserId,
                                FirstName = u.FirstName,
                                LastName = u.LastName,
                                Email = u.Email,
                                Phone = u.Phone,
                                Password = u.Password,
                                Role = u.UserId == 0 ? "Admin" :
                   _context.Teachers.Any(t => t.UserId == u.UserId) ? "Teacher" :
                   "Student"
                            }).ToList();
                return View(userDTOs);
            }
            catch (Exception ex)
            {
                return View(ex);
            }

        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(int? userid)
        {
            try
            {
                if (!userid.HasValue)
                {
                    return NotFound();
                }

                var user = await _context.Users
                    .FirstOrDefaultAsync(m => m.UserId == userid);
                if (user == null)
                {
                    return NotFound();
                }
                var userDTO = new UserDTO
                {
                    UserId = user.UserId,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Phone = user.Phone,
                    Password = user.Password,
                    Role = user.UserId == 0 ? "Admin" :
                       _context.Teachers.Any(t => t.UserId == user.UserId) ? "Teacher" :
                       "Student"
                };
                return View(userDTO);
            }
            catch (Exception ex)
            {
                return View("Error", ex);
            }
        }

        // GET: Users/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserId,FirstName,LastName,Email,Phone,Password")] User user)
        {
            try 
            { 
            if (ModelState.IsValid)
            {
                _context.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            var userDTO = new UserDTO
            {
                UserId = user.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Phone = user.Phone,
                Password = user.Password,
                Role = user.UserId == 0 ? "Admin" :
                      _context.Teachers.Any(t => t.UserId == user.UserId) ? "Teacher" :
                      "Student"
            };
            return View(userDTO);
        }
             catch (Exception ex)
            {
                return View("Error", ex);
            }
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(int userid)
        {
            try
            {
                if (!UserExists(userid))
                {
                    return NotFound();
                }

                var user = await _context.Users.FindAsync(userid);
                if (user == null)
                {
                    return NotFound();
                }
                var userDTO = new UserDTO
                {
                    UserId = user.UserId,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Phone = user.Phone,
                    Password = user.Password,
                    Role = user.UserId == 0 ? "Admin" :
                         _context.Teachers.Any(t => t.UserId == user.UserId) ? "Teacher" :
                         "Student"
                };
                return View(userDTO);
            }
            catch (Exception ex)
            {
                return View("Error", ex);
            }


        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int user, int permission, int? courseid, int userid, [Bind("UserId,FirstName,LastName,Email,Phone,Password")] UserDTO userDTO)
        {
            try
            {
                if (userid != userDTO.UserId)
                {
                    return NotFound();
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        var sql = $"UPDATE [Users] SET FirstName = '{userDTO.FirstName}',LastName = '{userDTO.LastName}',Email = '{userDTO.Email}',Phone = '{userDTO.Phone}', Password = '{userDTO.Password}' WHERE UserId = {userDTO.UserId}";
                        await _context.Database.ExecuteSqlRawAsync(sql);
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!UserExists(userDTO.UserId))
                        {
                            return NotFound();
                        }
                        else
                        {
                            throw;
                        }
                    }
                    if(courseid!=null)
                    {
                        return Redirect($"/Users/Details?user={userid}&permission={permission}&courseid={courseid}&userid={userid}");

                    }
                    else
                    {
return Redirect($"/Users/Details?user={userid}&permission={permission}&userid={userid}");
                    }
                    
                }
                return View(userDTO);
            }
            catch (Exception ex)
            {
                return View("Error", ex);
            }
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(int userid)
        {
            try
            {
                if (!UserExists(userid))
                {
                    return NotFound();
                }

                var user = await _context.Users.FindAsync(userid);
                if (user == null)
                {
                    return NotFound();
                }
                var userDTO = new UserDTO
                {
                    UserId = user.UserId,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Phone = user.Phone,
                    Password = user.Password,
                    Role = user.UserId == 0 ? "Admin" :
                         _context.Teachers.Any(t => t.UserId == user.UserId) ? "Teacher" :
                         "Student"
                };
                return View(userDTO);
            }
            catch (Exception ex)
            {
                return View("Error", ex);
            }
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int user, int permission, int? courseid, int userid)
        {
            var t = await _context.Teachers.FindAsync(userid);
            if (t != null)
            {
                _context.Teachers.Remove(t);
            }

            var u = await _context.Users.FindAsync(userid);
            if (u != null)
            {
                _context.Users.Remove(u);
            }

            await _context.SaveChangesAsync();
            if (courseid != null)
            {
                return Redirect($"/Users/Index?user=0&permission={permission}&courseid={courseid}");

            }
            else
            {
                return Redirect($"/Users/Index?user=0&permission={permission}");
            }
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.UserId == id);
        }
    }
}
