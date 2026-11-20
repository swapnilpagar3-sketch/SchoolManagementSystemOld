using Microsoft.AspNetCore.Mvc;

namespace EntityFrameworkCore.Controllers
{
    public class BaseController : Controller
    {
        protected bool IsLoggedIn()
        {
            return !string.IsNullOrEmpty(Request.Cookies["UserId"]);
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
