using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PharmacyManager.Models;
using PharmacyManager.Models.ViewModels;
using PharmacyManager.Repository;
using PharmacyManager.Utilities;

namespace PharmacyManager.Controllers
{
    public class PrescriptionsController : BaseController
    {
        private readonly IPrescriptionRepository _repo;
        private readonly IMedicationRepository _medicationRepository;
        private readonly IMapper _mapper;
        public PrescriptionsController(IMapper mapper, IPrescriptionRepository repo, IMedicationRepository medicationRepository)
        {
            _mapper = mapper;
            _repo = repo;
            _medicationRepository = medicationRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Prescription>>> Index()
        {
            var script = await _repo.GetAllScripts();
            return View(script);
        }

        [HttpGet]
        public async Task<ActionResult<Prescription>> Details(int id)
        {
            var script = await _repo.GetScriptById(id);
            if (script == null) return NotFound();
            return View(script);
        }

        public async Task<ActionResult> Create()
        {
            await loadSelectionBoxes();
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Create(PrescriptionVM model)
        {
            var script = await _repo.AddScript(model);
            if (script == null)
            {
                return BadRequest("Could not create prescription");
            }
            return CreatedAtAction(nameof(Details), new { Id = script.Id }, script);
        }

        public IActionResult Update(int Id)
        {
            var script = _repo.GetScriptById(Id);
            return View(script);
        }

        [HttpPut]
        public async Task<ActionResult> Update(Prescription model)
        {
            bool success = await _repo.UpdateScript(model);
            if (!success)
            {
                return BadRequest("Could not update the prescription");
            }
            return RedirectToAction("Index");
        }

        [HttpDelete]
        public async Task<ActionResult> Delete(int id)
        {
            var success = await _repo.DeleteScript(id);
            if (!success) return BadRequest($"Could not delete prescription with id : {id}");
            return NoContent();
        }


        public async Task loadSelectionBoxes()
        {
            var medication = await _medicationRepository.GetAllMedications();
            ViewBag.Medication = new SelectList(medication, "MedicationId", "MedicationName");
            var doctors = await _repo.GetAllDoctors();
            ViewBag.Doctors = new SelectList(doctors
                .Select(d => new 
                {
                    d.DoctorId,
                    DoctorFullName = $"{d.DoctorName} {d.DoctorSurname} Practice Number: {d.PracticeNumber}"
                })
               .ToList(), "DoctorId", "DoctorFullName");

            var patients = await _repo.GetAllPatients();
            ViewBag.Patients = new SelectList(patients
                .Select(p => new
                {
                    p.Id,
                    PatientName = $"{p.Name} {p.Surname}"
                }).ToList(), "Id", "PatientName");
        }
    }
}
