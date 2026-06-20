using GymSystem.Domain.Abstractions.Services;
using GymSystem.Domain.DTOs.Plan;
using GymSystem.UI.Helpers;
using GymSystem.UI.ViewModels.Plan;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymSystem.UI.Controllers;

[Route("Plan")]
public class PlanController(IPlanService plans) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var result = await plans.GetActivePlansAsync(ct);

        if (!result.IsSuccess)
        {
            this.HandleErrorResult(result);
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
            this.HandleErrorResult(result);
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
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Edit([FromRoute]int id, CancellationToken ct)
    {
        var result = await plans.GetForEditAsync(id, ct);

        if (!result.IsSuccess)
        {
            this.HandleErrorResult(result);
            return NotFound();
        }

        var viewModel = new EditPlanViewModel
        {
            Id = id,
            Description = result.Value.Description,
            DurationDays = result.Value.DurationDays,
            Price = result.Value.Price
        };

        var planDetails = await plans.GetDetailsAsync(id, ct);
        if (planDetails.IsSuccess)
        {
            viewModel.Name = planDetails.Value.Name;
        }

        return View(viewModel);
    }

    [HttpPost("Edit/{id:int}")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Edit([FromRoute] int id, EditPlanDTO model, CancellationToken ct)
    {
        if (id != model.Id)
            return NotFound();

        ModelState.Remove("Name");

        if (!ModelState.IsValid)
        {
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

        // Use the error handler
        this.HandleErrorResult(result, ModelState);

        var planDetails = await plans.GetDetailsAsync(id);
        if (planDetails.IsSuccess)
        {
            model.Name = planDetails.Value.Name;
        }

        return View(model);
    }

    [HttpPost("Toggle-Activation/{id:int}")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> ToggleActivation(int id, CancellationToken ct)
    {
        var result = await plans.ToggleActivationAsync(id, ct);

        if (!result.IsSuccess)
        {
            this.HandleErrorResult(result);
            return RedirectToAction(nameof(Index));
        }

        TempData["Success"] = "Plan status updated successfully";
        return RedirectToAction(nameof(Index));
    }
}
