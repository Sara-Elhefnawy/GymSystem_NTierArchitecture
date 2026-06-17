using GymSystem.Domain.DTOs.Trainer;
using GymSystem.Domain.Services.Interfaces;
using GymSystem.Infrastructure.Entities.Enums;
using GymSystem.UI.ViewModels.Trainer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GymSystem.UI.Controllers;

[Route("Trainer")]
public class TrainerController(ITrainerService trainers) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken ct = default)
    {
        var result = await trainers.GetAllAsync(ct);

        if (result.IsFailure)
        {
            TempData["Error"] = "Unable to load trainers. Please try again.";
            return View(new List<IndexTrainerViewModel>());
        }

        var viewModels = result.Value.Select(m => new IndexTrainerViewModel
        {
            Id = m.Id,
            Name = m.Name,
            Email = m.Email,
            Phone = m.Phone,
            Specialties = m.Specialties
        }).ToList();

        return View(viewModels);
    }

    [HttpGet("Create")]
    public IActionResult Create()
    {
        return View(new CreateTrainerViewModel());
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateTrainerViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Please correct the validation errors.";
            return View(model);
        }

        var dto = new CreateTrainerDTO
        {
            Name = model.Name,
            Email = model.Email,
            Phone = model.Phone,
            DateOfBirth = model.DateOfBirth,
            Gender = model.Gender,
            BuildingNumber = model.BuildingNumber,
            City = model.City,
            Street = model.Street,
            Specialties = model.Specialties
        };

        var result = await trainers.CreateAsync(dto, ct);

        if (result.IsSuccess)
        {
            TempData["Success"] = "Trainer created successfully!";
            return RedirectToAction(nameof(Index));
        }

        // Handle specific error cases
        switch (result.ErrorKey)
        {
            case "EMAIL_TAKEN":
                ModelState.AddModelError("Email", "This email is already registered");
                TempData["Warning"] = "This email is already registered to another trainer. Please use a different email.";
                break;

            case "PHONE_TAKEN":
                ModelState.AddModelError("Phone", "This phone number is already registered");
                TempData["Warning"] = "This phone number is already registered to another trainer. Please use a different number.";
                break;

            case "INVALID_AGE":
                ModelState.AddModelError("DateOfBirth", "Age must be between 12 and 120 years");
                TempData["Warning"] = "Age must be between 12 and 120 years.";
                break;

            case "INVALID_NAME":
                ModelState.AddModelError("Name", "Name can only contain letters, spaces, hyphens, and apostrophes");
                TempData["Warning"] = "Name contains invalid characters. Use only letters, spaces, hyphens, and apostrophes.";
                break;

            case "INVALID_GENDER":
                ModelState.AddModelError("Gender", "Please select a valid gender");
                TempData["Warning"] = "Please select a valid gender option.";
                break;

            case "INVALID_BLOOD_TYPE":
                ModelState.AddModelError("HealthRecord.BloodType", "Please select a valid blood type");
                TempData["Warning"] = "Please select a valid blood type from the list.";
                break;

            default:
                ModelState.AddModelError(string.Empty, result.Error);
                TempData["Error"] = result.Error;
                break;
        }

        return View(model);
    }

    [HttpGet("Details/{id:int}")]
    public async Task<IActionResult> Details(int id, CancellationToken ct)
    {
        var result = await trainers.GetDetailsAsync(id, ct);

        if (result.IsFailure)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(Index));
        }

        var viewModel = new DetailsTrainerViewModel
        {
            Id = result.Value.Id,
            Name = result.Value.Name,
            Email = result.Value.Email,
            Phone = result.Value.Phone,
            Address = result.Value.Address,
            DateOfBirth = result.Value.DateOfBirth,
            Specialty = result.Value.Specialty,
        };

        return View(viewModel);
    }

    [HttpGet("Edit/{id:int}")]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var result = await trainers.GetForEditAsync(id, ct);

        if (result.IsFailure)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(Index));
        }

        var viewModel = new EditTrainerViewModel
        {
            Id = result.Value.Id,
            Email = result.Value.Email,
            Phone = result.Value.Phone,
            BuildingNumber = result.Value.BuildingNumber,
            City = result.Value.City,
            Street = result.Value.Street,
            Specialty = result.Value.Specialty,
        };

        // Load Name from a separate call or modify GetForEditAsync to include them
        var trainersDetails = await trainers.GetDetailsAsync(id, ct);
        if (trainersDetails.IsSuccess)
        {
            viewModel.Name = trainersDetails.Value.Name;
        }

        ViewBag.Specialties = Enum.GetValues(typeof(Specialty))
        .Cast<Specialty>()
        .Select(e => new SelectListItem
        {
            Value = e.ToString(),
            Text = e.ToString(),
            Selected = e.ToString() == viewModel.Specialty
        }).ToList();

        return View(viewModel);
    }

    [HttpPost("Edit/{id:int}")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Edit([FromRoute] int id, EditTrainerViewModel model, CancellationToken ct)
    {

        if (id != model.Id)
            return NotFound();

        // Remove Name from validation since they're not editable
        ModelState.Remove("Name");

        if (!ModelState.IsValid)
        {
            // Need to reload Name for the view
            var trainerDetailss = await trainers.GetDetailsAsync(id, ct);
            if (trainerDetailss.IsSuccess)
            {
                model.Name = trainerDetailss.Value.Name;
            }
            return View(model);
        }

        var dto = new EditTrainerDTO
        {
            Id = model.Id,
            Email = model.Email,
            Phone = model.Phone,
            BuildingNumber = model.BuildingNumber,
            City = model.City,
            Street = model.Street,
            Specialty = model.Specialty
        };

        var result = await trainers.UpdateAsync(dto, ct);

        if (result.IsSuccess)
        {
            TempData["Success"] = "Trainer updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        // Handle specific error cases
        switch (result.ErrorKey)
        {
            case "EMAIL_TAKEN":
                ModelState.AddModelError("Email", "This email is already registered");
                TempData["Warning"] = "This email is already registered to another trainer. Please use a different email.";
                break;

            case "PHONE_TAKEN":
                ModelState.AddModelError("Phone", "This phone number is already registered");
                TempData["Warning"] = "This phone number is already registered to another trainer. Please use a different number.";
                break;

            case "INVALID_AGE":
                ModelState.AddModelError("DateOfBirth", "Age must be between 12 and 120 years");
                TempData["Warning"] = "Age must be between 12 and 120 years.";
                break;

            case "INVALID_NAME":
                ModelState.AddModelError("Name", "Name can only contain letters, spaces, hyphens, and apostrophes");
                TempData["Warning"] = "Name contains invalid characters. Use only letters, spaces, hyphens, and apostrophes.";
                break;

            case "INVALID_GENDER":
                ModelState.AddModelError("Gender", "Please select a valid gender");
                TempData["Warning"] = "Please select a valid gender option.";
                break;

            case "INVALID_BLOOD_TYPE":
                ModelState.AddModelError("HealthRecord.BloodType", "Please select a valid blood type");
                TempData["Warning"] = "Please select a valid blood type from the list.";
                break;

            default:
                ModelState.AddModelError(string.Empty, result.Error);
                TempData["Error"] = result.Error;
                break;
        }

        // Reload Name for the view
        var trainerDetails = await trainers.GetDetailsAsync(id, ct);
        if (trainerDetails.IsSuccess)
        {
            model.Name = trainerDetails.Value.Name;
        }

        return View(model);
    }

    [HttpGet("Delete/{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await trainers.GetForDeleteAsync(id, ct);

        if (result.IsFailure)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(Index));
        }

        var viewModel = new DeleteTrainerViewModel
        {
            Id = id,
            Name = result.Value.Name,
            Photo = result.Value.Photo
        };

        return View(viewModel);
    }

    [HttpPost("Delete/{id:int}"), ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken ct)
    {
        var result = await trainers.DeleteAsync(id, ct);

        if (result.IsFailure)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(Delete), new { id });
        }

        TempData["Success"] = "Trainer deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
}
