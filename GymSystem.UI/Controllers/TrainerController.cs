using GymSystem.Domain.DTOs.Member;
using GymSystem.Domain.DTOs.Trainer;
using GymSystem.Domain.Services;
using GymSystem.Infrastructure.Entities;
using GymSystem.UI.ViewModels.Member;
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

    [HttpGet]
    public async Task<IActionResult> Details(int id, CancellationToken ct)
    {
        var dto = await trainers.GetDetailsAsync(id, ct);

        if (dto is null)
            return View();

        var viewModel = new DetailsTrainerViewModel
        {
            Id = dto.Id,
            Name = dto.Name,
            Email = dto.Email,
            Phone = dto.Phone,
            Address = dto.Address,
            DateOfBirth = dto.DateOfBirth,
            Specialty = dto.Specialty,
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var dto = await trainers.GetForEditAsync(id, ct);

        if (dto is null)
            return View();

        var viewModel = new EditTrainerViewModel
        {
            Id = dto.Id,
            Name = dto.Name,
            Email = dto.Email,
            Phone = dto.Phone,
            BuildingNumber = dto.BuildingNumber,
            City = dto.City,
            Street = dto.Street,
            Specialty = dto.Specialty,
        };

        return View(viewModel);
    }

    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Edit(EditTrainerViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return View(model);

        var dto = new EditTrainerDTO
        {
            Id = model.Id,
            Name = model.Name,
            Email = model.Email,
            Phone = model.Phone,
            BuildingNumber = model.BuildingNumber,
            City = model.City,
            Street = model.Street,
            Specialty = model.Specialty
        };

        var success = await trainers.UpdateAsync(dto, ct);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, "Unable to update member.");
            return View(model);
        }

        TempData["Success"] = "Trainer updated successfully!";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var dto = await trainers.GetForDeleteAsync(id, ct);

        if (dto is null) return NotFound();

        var viewModel = new DeleteTrainerViewModel
        {
            Id = id,
            Name = dto.Name
        };

        return View(viewModel);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken ct)
    {
        var model = await trainers.GetForDeleteAsync(id, ct);

        if (!ModelState.IsValid) return View(model);

        var dto = new EditMemberDTO
        {
            Id = id,
            Name = model.Name
        };

        var success = await trainers.DeleteAsync(id, ct);

        if (!success)
        {
            TempData["ErrorMessage"] = "Cannot delete member with active bookings.";
            return RedirectToAction(nameof(Delete), new { id });
        }

        TempData["SuccessMessage"] = "Member deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
}
