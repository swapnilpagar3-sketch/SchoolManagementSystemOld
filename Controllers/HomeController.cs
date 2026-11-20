using System.Diagnostics;
using EntityFrameworkCore.Models;
using Microsoft.AspNetCore.Mvc;

namespace EntityFrameworkCore.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            ViewBag.TotalStudents =
                _context.Students.Count();

            ViewBag.TotalTeachers =
                _context.Teachers.Count();

            ViewBag.TotalClasses =
                _context.Classes.Count();

            ViewBag.TotalSubjects =
                _context.Subjects.Count();

            ViewBag.TotalAttendance =
                _context.Attendance.Count();

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Dashboard()
        {
            return View();
        }
    }
}
