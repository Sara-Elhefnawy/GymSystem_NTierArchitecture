using GymSystem.Domain.DTOs.Memberships;
using GymSystem.Domain.Services.Interfaces;
using GymSystem.UI.Helpers;
using GymSystem.UI.ViewModels.Memberships;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GymSystem.UI.Controllers;

[Route("Membership")]
public class MembershipController(
    IMembershipService membershipService,
    IMemberService memberService,
    IPlanService planService) : Controller
{
    private readonly IMembershipService _membershipService = membershipService;
    private readonly IMemberService _memberService = memberService;
    private readonly IPlanService _planService = planService;

    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken ct = default)
    {
        var result = await _membershipService.GetActiveMembershipsAsync(ct);

        if (result.IsFailure)
        {
            this.HandleErrorResult(result);
            return View(new List<IndexMembershipViewModel>());
        }

        var viewModel = result.Value.Select(m => new IndexMembershipViewModel
        {
            EndDate = m.EndDate,
            StartDate = m.StartDate,
            MemberId = m.MemberId,
            MemberName = m.MemberName,
            Photo = m.Photo,
            PlanName = m.PlanName
        }).ToList();

        return View(viewModel);
    }

    [HttpGet("Create")]
    public async Task<IActionResult> Create(CancellationToken ct = default)
    {
        var membersResult = await _memberService.GetAllAsync(ct);
        var plansResult = await _planService.GetAllAsync(ct);

        ViewBag.Members = membersResult.IsSuccess
            ? new SelectList(membersResult.Value, "Id", "Name")
            : new SelectList(Enumerable.Empty<object>());

        ViewBag.Plans = plansResult.IsSuccess
            ? new SelectList(plansResult.Value, "Id", "Name")
            : new SelectList(Enumerable.Empty<object>());

        return View(new CreateMembershipViewModel());
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateMembershipViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropdowns(ct);
            return View(model);
        }

        var dto = new CreateMembershipDTO
        {
            MemberId = model.MemberId,
            PlanId = model.PlanId
        };

        var result = await _membershipService.CreateMembershipAsync(dto, ct);

        if (result.IsSuccess)
        {
            TempData["Success"] = "Membership created successfully.";
            return RedirectToAction(nameof(Index));
        }

        ErrorHandler.HandleError(result, ModelState);

        if (result.ErrorKey == "ALREADY_ACTIVE")
        {
            TempData["Info"] = "You can view the active membership in the list below.";
        }

        await PopulateDropdowns(ct);
        return View(model);
    }

    [HttpPost("Cancel")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id, CancellationToken ct)
    {
        var result = await _membershipService.CancelMembershipAsync(id, ct);

        if (result.IsSuccess)
        {
            TempData["Success"] = "Membership cancelled successfully.";
        }
        else
        {
            this.HandleErrorResult(result);
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateDropdowns(CancellationToken ct)
    {
        var membersResult = await _memberService.GetAllAsync(ct);
        var plansResult = await _planService.GetAllAsync(ct);

        ViewBag.Members = membersResult.IsSuccess
            ? new SelectList(membersResult.Value, "Id", "Name")
            : new SelectList(Enumerable.Empty<object>());

        ViewBag.Plans = plansResult.IsSuccess
            ? new SelectList(plansResult.Value, "Id", "Name")
            : new SelectList(Enumerable.Empty<object>());
    }
}
