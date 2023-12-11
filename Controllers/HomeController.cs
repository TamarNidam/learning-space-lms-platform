using Learning_Space.DTO;
using Learning_Space.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Learning_Space.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly LearningSpaceContext _context;

        public HomeController(LearningSpaceContext context, ILogger<HomeController> logger)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
        // GET: Home/SignUp

        public IActionResult SignUp()
        {
            return View();
        }

 
      // POST: Home/SignUp
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignUp([Bind("FirstName,Password")] UserSignUpDTO userDTO)
        {
            try
            {

                if (ModelState.IsValid)
                {
                    var sql = "SELECT UserId,FirstName,LastName,Email,Phone,Password FROM [Users] WHERE FirstName = {0} AND Password = {1}";
                    var user = await _context.Users.FromSqlRaw(sql, userDTO.FirstName, userDTO.Password)
                        .FirstOrDefaultAsync();

                    //var user = await _context.Users.FindAsync(id);
                    if (user == null)
                    {
                        ViewBag.ErrorMessage = "User does not exist";
                        return View(userDTO);
                    }

                    return Redirect($"/Home/Index");

                }
                return View(userDTO);
            }
            catch(Exception ex) 
            {
                return View("Error",ex);
            }
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
