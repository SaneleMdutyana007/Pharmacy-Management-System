using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmacyManager.Models;
using PharmacyManager.Repositories;
using System.Security.Claims;

namespace PharmacyManager.Controllers
{
    public class RequestsController : BaseController
    {
        private readonly IRequestRepository _repo;
        public RequestsController(IRequestRepository repo)
        {
            _repo = repo;
        }
        public async Task<IActionResult> Index()
        {
            var requests = await _repo.GetAllRequests();
            return View(requests);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string filePath, string fileName)
        {
            if (string.IsNullOrEmpty(filePath) || string.IsNullOrEmpty(fileName))
            {
                ModelState.AddModelError("", "Please upload a prescription file before submitting.");
                return View();
            }

            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(customerId))
            {
                return Unauthorized();
            }

            var success = await _repo.CreateRequest(filePath, fileName, customerId);

            if (!success)
            {
                ModelState.AddModelError("", "Failed to create prescription request. Please try again.");
                return View();
            }

            return RedirectToAction("Index");
        }


        public async Task<IActionResult> Details(int id)
        {
            var request = await _repo.GetRequestById(id);

            if (request == null || request.FilePath == null) return NotFound();

            return View(request);
        }

        [HttpPost]
        public async Task<IActionResult> UploadPrescription(IFormFile pdfFile)
        {
            var result = await _repo.UploadPrescription(pdfFile); // This calls your repository/service method

            if (result.Success)
            {
                return Json(new
                {
                    success = true,
                    filePath = result.FilePath,
                    fileName = result.FileName
                });
            }

            return Json(new
            {
                success = false,
                errorMessage = result.ErrorMessage
            });
        }
        [HttpGet]
        public async Task<IActionResult> ViewPrescription(int id)
        {
            if (id <= 0)
            {
                return NotFound();
            }

            var pdf = await _repo.GetPDF(id);

            if(pdf == null)
            {
                return NotFound();
            }
            
            return File(pdf, "application/pdf");
        }

    }

}
