using GymSystem.Domain.DTOs.Memberships;
using GymSystem.Domain.Services.Interfaces;
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
            TempData["Error"] = result.Error;
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

        TempData["Error"] = result.Error;
        await PopulateDropdowns(ct);
        return View(model);
    }

    [HttpPost("Cancel")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id, CancellationToken ct)
    {
        var result = await _membershipService.CancelMembershipAsync(id, ct);

        TempData[result.IsSuccess ? "Success" : "Error"] =
            result.IsSuccess ? "Membership cancelled." : result.Error;

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
