using Microsoft.AspNetCore.Mvc;
using PharmacyManager.Repositories;
using PharmacyManager.Repository;
using System.Threading.Tasks;

namespace PharmacyManager.Controllers
{
    public class PharmacistsController : BaseController
    {
        private readonly IUserRepository _users;
        public PharmacistsController(IUserRepository users) { _users = users; }

        public async Task<IActionResult> Index()
        {

            return View(await _users.GetAllPharmacist());
        }

        public async Task<IActionResult> Details(string Id)
        {
            return View(await _users.GetPharmacistById(Id));
        }
       

    }
}
