using GymSystem.Domain.Abstractions.Services;
using GymSystem.Domain.DTOs.Trainer;
using GymSystem.Domain.Entities.Enums;
using GymSystem.UI.Helpers;
using GymSystem.UI.ViewModels.Trainer;
using Mapster;
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

        var viewModels = result.Value.Adapt<IReadOnlyList<IndexTrainerViewModel>>();

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

        var dto = model.Adapt<CreateTrainerDTO>();

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

        var viewModel = result.Value.Adapt<DetailsTrainerViewModel>();

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

        var viewModel = result.Value.Adapt<EditTrainerViewModel>();

        var trainersDetails = await trainers.GetDetailsAsync(id, ct);
        if (trainersDetails.IsSuccess)
        {
            viewModel.Name = trainersDetails.Value.Name;
        }

        PopulateSpecialtiesDropdown(viewModel.Specialty);

        return View(viewModel);
    }

    [HttpPost("Edit/{id:int}")]
    [ValidateAntiForgeryToken]
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
            PopulateSpecialtiesDropdown(model.Specialty);
            return View(model);
        }

        var dto = model.Adapt<EditTrainerDTO>();

        var result = await trainers.UpdateAsync(dto, ct);

        if (result.IsSuccess)
        {
            TempData["Success"] = "Trainer updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        // Use the error handler
        this.HandleErrorResult(result, ModelState);

        PopulateSpecialtiesDropdown(model.Specialty);
        
        return View(model);
    }

    private void PopulateSpecialtiesDropdown(string selectedSpecialty = null)
    {
        ViewBag.Specialties = Enum.GetValues(typeof(Specialty))
        .Cast<Specialty>()
        .Select(e => new SelectListItem
        {
            Value = e.ToString(),
            Text = e.ToString(),
            Selected = e.ToString() == selectedSpecialty
        }).ToList();
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

        var viewModel = result.Value.Adapt<DeleteTrainerViewModel>();

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
