using GymSystem.Domain.Abstractions.Services;
using GymSystem.Domain.DTOs.Plan;
using GymSystem.UI.Helpers;
using GymSystem.UI.ViewModels.Plan;
using Mapster;
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

        var viewModel = result.Value.Adapt<IReadOnlyList<IndexPlanViewModel>>();

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

        var viewModel = result.Value.Adapt<DetailsPlanViewModel>();

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

        var viewModel = result.Value.Adapt<EditPlanViewModel>();

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

        Console.WriteLine($"Edit POST received: Id={model.Id}, IsActive={model.IsActive}, Name={model.Name}, Price={model.Price}");

        if (id != model.Id)
            return NotFound();

        ModelState.Remove("Name");

        if (!ModelState.IsValid)
        {
            var planDetailss = await plans.GetDetailsAsync(id, ct);
            if (planDetailss.IsSuccess)
            {
                model.Name = planDetailss.Value.Name;
            }
            return View(model);
        }

        var dto = model.Adapt<EditPlanDTO>();

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
