using GymSystem.Domain.DTOs.Plan;
using GymSystem.Domain.Services.Interfaces;
using GymSystem.UI.ViewModels.Plan;
using Microsoft.AspNetCore.Mvc;

namespace GymSystem.UI.Controllers;


[Route("Plan")]
public class PlanController(IPlanService plans) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var result = await plans.GetAllAsync(ct);

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
            return View(new List<IndexPlanViewModel>());
        }

        var viewModel = result.Value.Select(p => new IndexPlanViewModel
        {
            Id = p.Id,
            Description = p.Description,
            DurationDays = p.DurationDays,
            IsActive = p.IsActive,
            Name = p.Name,
            Price = p.Price,
        }).ToList();

        return View(viewModel);
    }

    [HttpGet("Details/{id:int}")]
    public async Task<IActionResult> Details(int id, CancellationToken ct)
    {
        var result = await plans.GetDetailsAsync(id, ct);

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(Index));
        }

        var viewModel = new DetailsPlanViewModel
        {
            Id = result.Value.Id,
            Description = result.Value.Description,
            DurationDays = result.Value.DurationDays,
            IsActive = result.Value.IsActive,
            Name = result.Value.Name,
            Price = result.Value.Price,
        };

        return View(viewModel);
    }

    [HttpGet("Edit/{id:int}")]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var result = await plans.GetForEditAsync(id, ct);

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
            return NotFound();
        }

        var viewModel = new EditPlanViewModel
        {
            Id = id,
            Description = result.Value.Description,
            DurationDays = result.Value.DurationDays,
            Price = result.Value.Price
        };

        // Load Name from a separate call or modify GetForEditAsync to include them
        var planDetails = await plans.GetDetailsAsync(id, ct);
        if (planDetails.IsSuccess)
        {
            viewModel.Name = planDetails.Value.Name;
        }

        return View(viewModel);
    }

    [HttpPost("Edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EditPlanDTO model, CancellationToken ct)
    {
        if (id != model.Id)
            return NotFound();

        // Remove Name from validation since they're not editable
        ModelState.Remove("Name");

        if (!ModelState.IsValid)
        {
            // Need to reload Name and Photo for the view
            var planDetailss = await plans.GetDetailsAsync(id);
            if (planDetailss.IsSuccess)
            {
                model.Name = planDetailss.Value.Name;
            }
            return View(model);
        }

        var dto = new EditPlanDTO
        {
            Id = id,
            Description = model.Description,
            DurationDays = model.DurationDays,
            Name = model.Name,
            Price = model.Price
        };

        var result = await plans.UpdateAsync(dto, ct);

        if (result.IsSuccess)
        {
            TempData["Success"] = "Plan updated successfully";
            return RedirectToAction(nameof(Index));
        }

        // Handle different error types
        switch (result.ErrorKey)
        {
            case "PLAN_NOT_FOUND":
                ModelState.AddModelError(string.Empty, "Plan not found");
                TempData["Error"] = "Plan not found. It may have been deleted.";
                break;

            case "UPDATE_ERROR":
                ModelState.AddModelError(string.Empty, "Failed to update plan. Please try again.");
                TempData["Error"] = "Failed to update plan. Please try again.";
                break;

            case "INTERNAL_ERROR":
            case "DATABASE_ERROR":
                ModelState.AddModelError(string.Empty, "A system error occurred. Please try again later.");
                TempData["Error"] = "A system error occurred. Please try again later.";
                break;

            default:
                ModelState.AddModelError(string.Empty, result.Error);
                break;
        }

        // Reload Name for the view
        var planDetails = await plans.GetDetailsAsync(id);
        if (planDetails.IsSuccess)
        {
            model.Name = planDetails.Value.Name;
        }

        return View(model);
    }

    [HttpPost("Toggle-Activation/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActivation(int id, CancellationToken ct)
    {
        var result = await plans.ToggleActivationAsync(id, ct);

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(ToggleActivation), new {id});
        }

        TempData["Success"] = "Plan status updated successfully";
        return RedirectToAction(nameof(Index));
    }
}