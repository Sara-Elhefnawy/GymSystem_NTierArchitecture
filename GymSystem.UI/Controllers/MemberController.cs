using GymSystem.Domain.DTOs.HealthRecord;
using GymSystem.Domain.DTOs.Member;
using GymSystem.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace GymSystem.UI.Controllers;

public class MemberController(IMemberService members) : Controller
{
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var items = await members.GetAllAsync(ct);

        if (TempData["Success"] != null)
        {
            ViewBag.SuccessMessage = TempData["Success"].ToString();
        }
        if (TempData["Error"] != null)
        {
            ViewBag.ErrorMessage = TempData["Error"].ToString();
        }

        return View(items);
    }
    [HttpGet]
    public IActionResult Create()
    {
        return View(new CreateMemberDTO
        {
            HealthRecord = new CreateHealthRecordDTO()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateMemberDTO model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            // Log validation errors
            var errors = ModelState.Values.SelectMany(v => v.Errors);
            foreach (var error in errors)
            {
                Console.WriteLine($"Validation error: {error.ErrorMessage}");
            }
            return View(model);
        }

        try
        {
            var success = await members.CreateAsync(model, ct);

            if (success)
            {
                TempData["Success"] = "Member created successfully!";
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = "Failed to create member. Email or phone may already exist, or invalid data provided.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }
}
