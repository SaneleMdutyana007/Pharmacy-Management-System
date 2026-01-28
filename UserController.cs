using Microsoft.AspNetCore.Mvc;
using PharmacyManager.Areas.Identity.Data;

namespace PharmacyManager.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserRepository _userRepository;

        public UserController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userRepository.GetAllPharmacist();
            return View(users);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userRepository.GetUserById(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Index, Pharmacists");
            }
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(UserModel user)
        {
            if (ModelState.IsValid)
            {
                var result = await _userRepository.UpdateUser(user);
                if (result)
                {
                    TempData["SuccessMessage"] = "User updated successfully.";
                    return RedirectToAction("Index");
                }
                TempData["ErrorMessage"] = "Error updating user.";
            }
            // If we got this far, something failed, redisplay form
            var users = await _userRepository.GetAllUsers();
            return View("Index", users);
        }
        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userRepository.GetUserById(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Index, Pharmacists");
            }
            return View(user);
        }

        [HttpPost]
        [ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var result = await _userRepository.DeleteUser(id);
            if (result)
            {
                TempData["SuccessMessage"] = "User deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Error deleting user.";
            }
            return RedirectToAction("Index, Pharmacists");
        }

        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            var user = await _userRepository.GetUserById(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Index, Pharmacists");
            }
            return View(user);
        }
    }
}
