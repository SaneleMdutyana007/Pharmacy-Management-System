using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PharmacyManager.Areas.Identity.Data;
using PharmacyManager.Models;
using PharmacyManager.Repository;
using PharmacyManager.Services;

namespace PharmacyManager.Controllers
{
    public class DashboardController : BaseController
    {
        private readonly SignInManager<UserModel> signInManager;
        private readonly UserManager<UserModel> userManager;
        private readonly IPharmacyRepository _pharmacyRepository;
        private readonly INotificationService _notificationService;

        public DashboardController(SignInManager<UserModel> _signInManager, UserManager<UserModel> _userManager, IPharmacyRepository pharmacyRepository, INotificationService notificationService) { signInManager = _signInManager; userManager = _userManager; _pharmacyRepository = pharmacyRepository; _notificationService = notificationService; }

       public async Task<IActionResult> Index()
       {
            var user = await userManager.GetUserAsync(User);
            if(user is Customer)
            {

            }
            return View(user);
       }
        public async Task<IActionResult> Manager()
        {
            try
            {
                // Get pharmacy details from repository
                var pharmacyDetails = await _pharmacyRepository.GetPharmacyInfo();

                // Get notifications for initial page load
                var notifications = await _notificationService.GetRecentNotificationsAsync(5);
                var notificationCount = await _notificationService.GetUnreadCountAsync();

                // Pass data to view
                ViewBag.Pharmacy = pharmacyDetails;
                ViewBag.PharmacyExists = pharmacyDetails != null && pharmacyDetails.Id > 0;
                ViewBag.Notifications = notifications;
                ViewBag.NotificationCount = notificationCount;

                // Check for messages
                if (TempData["SuccessMessage"] != null)
                {
                    ViewBag.SuccessMessage = TempData["SuccessMessage"];
                }
                if (TempData["ErrorMessage"] != null)
                {
                    ViewBag.ErrorMessage = TempData["ErrorMessage"];
                }
                if (TempData["InfoMessage"] != null)
                {
                    ViewBag.InfoMessage = TempData["InfoMessage"];
                }

                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading dashboard: {ex.Message}");
                ViewBag.PharmacyExists = false;
                ViewBag.ErrorMessage = "Error loading dashboard. Please try again.";
                return View();
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetRecentNotifications()
        {
            try
            {
                var notifications = await _notificationService.GetRecentNotificationsAsync(5);
                return Json(notifications);
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"Error getting notifications: {ex.Message}");
                return Json(new List<object>());
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetNotificationCount()
        {
            try
            {
                var count = await _notificationService.GetUnreadCountAsync();
                return Json(new { count = count });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting notification count: {ex.Message}");
                return Json(new { count = 0 });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> MarkNotificationAsRead([FromBody] MarkAsReadRequest request)
        {
            try
            {
                await _notificationService.MarkAsReadAsync(request.Id);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error marking notification as read: {ex.Message}");
                return Json(new { success = false, error = "Error marking notification as read" });
            }
        }

        public class MarkAsReadRequest
        {
            public int Id { get; set; }
        }
    }
}
    

