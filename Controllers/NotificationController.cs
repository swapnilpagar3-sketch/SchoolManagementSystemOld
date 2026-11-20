//using EntityFrameworkCore.Models;
//using EntityFrameworkCore.ViewModels;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;

//namespace EntityFrameworkCore.Controllers
//{
//    public class NotificationController : Controller
//    {
//        private readonly ApplicationDbContext _context;

//        public NotificationController(ApplicationDbContext context)
//        {
//            _context = context;
//        }

//        public IActionResult Index()
//        {
//            return View();
//        }

//        public async Task<IActionResult> GetNotifications()
//        {
//            var notifications = await _context.Notifications
//                .OrderByDescending(x => x.CreatedDate)
//                .Take(10)
//                .ToListAsync();

//            var vm = new NotificationVM
//            {
//                Count = notifications.Count(x => !x.IsRead),
//                Notifications = notifications
//            };

//            return PartialView("_NotificationPartial", vm);
//        }

//        //public async Task<IActionResult> MarkAsRead(int id)
//        //{
//        //    var notification =
//        //        await _context.Notifications.FindAsync(id);

//        //    if (notification != null)
//        //    {
//        //        notification.IsRead = true;

//        //        await _context.SaveChangesAsync();
//        //    }

//        //    return RedirectToAction("Index", "Home");
//        //}

//        public async Task<IActionResult> MarkAsRead(int id)
//        {
//            var notification =
//                await _context.Notifications.FindAsync(id);

//            if (notification != null)
//            {
//                notification.IsRead = true;
//                notification.ReadDate = DateTime.Now;

//                await _context.SaveChangesAsync();
//            }

//            return RedirectToAction(nameof(Index));
//        }

//        public async Task<IActionResult> MarkAsUnread(int id)
//        {
//            var notification = await _context.Notifications.FindAsync(id);

//            if (notification != null)
//            {
//                notification.IsRead = false;
//                await _context.SaveChangesAsync();
//            }

//            return Redirect(Request.Headers["Referer"].ToString());
//        }

//        public async Task<IActionResult> DeleteNotification(int id)
//        {
//            var notification =
//                await _context.Notifications.FindAsync(id);

//            if (notification != null)
//            {
//                notification.IsDeleted = true;

//                await _context.SaveChangesAsync();
//            }

//            return RedirectToAction(nameof(Index));
//        }
//    }
//}
