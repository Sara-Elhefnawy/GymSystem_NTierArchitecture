using GymSystem.Domain.DTOs.Trainer;
using GymSystem.Domain.Services.Interfaces;
using GymSystem.Infrastructure.Entities.Enums;
using GymSystem.UI.Helpers;
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
            this.HandleErrorResult(result);
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

        // Use the error handler
        this.HandleErrorResult(result, ModelState);
        return View(model);
    }

    [HttpGet("Details/{id:int}")]
    public async Task<IActionResult> Details(int id, CancellationToken ct)
    {
        var result = await trainers.GetDetailsAsync(id, ct);

        if (result.IsFailure)
        {
            this.HandleErrorResult(result);
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
            this.HandleErrorResult(result);
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
    [ValidateAntiForgeryToken]  // Restore this
    public async Task<IActionResult> Edit([FromRoute] int id, EditTrainerViewModel model, CancellationToken ct)
    {
        if (id != model.Id)
            return NotFound();

        ModelState.Remove("Name");

        if (!ModelState.IsValid)
        {
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

        // Use the error handler
        this.HandleErrorResult(result, ModelState);

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
            this.HandleErrorResult(result);
            return RedirectToAction(nameof(Index));
        }

        var viewModel = new DeleteTrainerViewModel
        {
            Id = id,
            Name = result.Value.Name,
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
            this.HandleErrorResult(result);
            return RedirectToAction(nameof(Delete), new { id });
        }

        TempData["Success"] = "Trainer deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
}
