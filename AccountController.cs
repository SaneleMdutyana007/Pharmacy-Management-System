using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PharmacyManager.Areas.Identity.Data;
using PharmacyManager.Models;
using PharmacyManager.Models.ViewModels;
using PharmacyManager.Repository;

namespace PharmacyManager.Controllers
{
    public class AccountController : BaseController
    {
        private readonly IAccountRepository _repo;
        private readonly IActiveIngredientRepository _ingredientRepository;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<UserModel> _userManager;
        private readonly SignInManager<UserModel> _signInManager;
        
        public AccountController(IAccountRepository repo, IActiveIngredientRepository ingredientRepository, RoleManager<IdentityRole> roleManager, UserManager<UserModel> userManager, SignInManager<UserModel> signInManager)
        {

            _repo = repo;
            _ingredientRepository = ingredientRepository;
            _roleManager = roleManager;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async  Task<IActionResult> Create(CreateUserVM model)
        {
            if (!ModelState.IsValid)
                return View(model);
  
            bool success = await _repo.AddUser(model);
            if(success)
            {
                return RedirectToAction("Login");
            }
            ModelState.AddModelError("", "Failed to create user. Please try again.");
            return View(model);
        }

        public IActionResult Register()
        {
            return View();
        }

        //Start
        [HttpPost]
        public IActionResult Step1(Step1VM model)
        {
            if (!ModelState.IsValid)
                return PartialView("_Step1", model);

            var customer = new CustomerVM
            {
                Name = model.Name,
                Surname = model.Surname,
                Email = model.Email,
                Password = model.Password,
                ConfirmPassword = model.ConfirmPassword
            };

            TempData["Step1"] = JsonConvert.SerializeObject(customer);
            return PartialView("_Step2",new Step2VM());
        }

        [HttpPost]
        public async Task<IActionResult> Step2(Step2VM step2Data)
        {
            if (!TempData.TryGetValue("Step1", out var json1))
                return PartialView("_Step1");

            TempData.Keep("Step1");

            var model = JsonConvert.DeserializeObject<CustomerVM>(json1.ToString());
            model.PhoneNumber = step2Data.PhoneNumber;
            model.SouthAfricanID = step2Data.SouthAfricanID;
            model.Address = step2Data.Address;

            TempData["Step2"] = JsonConvert.SerializeObject(model);

            var allergies = await _ingredientRepository.GetAllActiveIngredients();
            return PartialView("_Step3", allergies);
        }

        [HttpPost]
        public IActionResult Step3(List<int> selectedAllergyIds)
        {
            if (!TempData.TryGetValue("Step2", out var json2))
                return PartialView("_Step1");

            TempData.Keep("Step2");
            var model = JsonConvert.DeserializeObject<CustomerVM>(json2.ToString());

            model.allergyIds = selectedAllergyIds;

            TempData["Final"] = JsonConvert.SerializeObject(model);
            return PartialView("_Confirm", model);
        }

        [HttpPost]
        public async Task<IActionResult> Finish(CustomerVM model)
        {
            var result = await _repo.Register(model);

            if (!result)
            {
                ModelState.AddModelError("", "Registration failed.");
                return PartialView("_Confirm", model);
            }

            return RedirectToAction("Customer", "Dashboard");
        }//End

        //[HttpPost]
        //public async Task<IActionResult> Register(CustomerVM model)
        //{
        //    var success = await _repo.Register(model);
        //    if(success)
        //    {
        //        return RedirectToAction("Customer", "Dashboard");
        //    }
        //    return View(model);
        //}

        public IActionResult Login()
        {
            if(_signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Index", "Dashboard");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginVM model)
        {         

            var success = await _repo.Login(model);
            if (success) 
            {
                var user = await _userManager.GetUserAsync(User);
             
                HttpContext.Session.SetString("UserSession", "active");

                return RedirectToAction("Index", "Dashboard");
                
            }
            ModelState.AddModelError(string.Empty, "Incorrect Username or Password");
            return View(model);
        }

        public async Task<IActionResult> LogOut()
        {
            UserModel user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction(nameof(Login));
            await _repo.LogOut(user);
            await HttpContext.SignOutAsync();
            return RedirectToAction(nameof(Login));
        }

        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is Customer)
            {
                var customer = await _repo.GetCustomer(user.Id);
                return View(customer);
            }
            return View(user);
        }

        [HttpPost]
        public async Task<ActionResult> DeleteUser(string Id)
        {

            UserModel user = await _userManager.FindByIdAsync(Id);

            if(user == null) { return NotFound(); }
            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded) return View("LogOut");

            return View("Login");
        }

        [HttpPost]
        public async Task<IActionResult> UploadProfile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("", "No file selected.");
                return View("Profile");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            bool success = await _repo.UploadImage(file, user);

            if (!success)
            {
                ModelState.AddModelError("", "Image upload failed.");
            }

            var updatedUser = await _userManager.FindByIdAsync(user.Id);

            return View("Profile", updatedUser);
        }
    }
}
