using GymSystem.Domain.DTOs.Trainer;
using GymSystem.Domain.Services;
using GymSystem.UI.ViewModels.Trainer;
using Microsoft.AspNetCore.Mvc;

namespace GymSystem.UI.Controllers;

public class TrainerController(ITrainerService trainers) : Controller
{
    public async Task<IActionResult> Index(CancellationToken ct = default)
    {
        var dtos = await trainers.GetAllAsync(ct);

        var viewModels = dtos.Select(m => new IndexTrainerViewModel
        {
            Id = m.Id,
            Name = m.Name,
            Email = m.Email,
            Phone = m.Phone,
            Specialties = m.Specialties
        }).ToList();

        return View(viewModels);
    }


    [HttpGet]
    public IActionResult Create()
    {
        return View(new CreateTrainerViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateTrainerViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return View(model);

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

        var success = await trainers.CreateAsync(dto, ct);

        if (success)
        {
            TempData["Success"] = "Trainer created successfully!";
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError(string.Empty, "Unable to create member.");
        return View(model);
    }
}
