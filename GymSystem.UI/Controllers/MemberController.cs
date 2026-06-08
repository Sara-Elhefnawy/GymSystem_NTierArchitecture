using GymSystem.Domain.DTOs.HealthRecord;
using GymSystem.Domain.DTOs.Member;
using GymSystem.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace GymSystem.UI.Controllers;

public class MemberController(IMemberService members) : Controller
{
    [HttpGet]
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
            return View(model);

        var success = await members.CreateAsync(model, ct);

        if (success)
        {
            TempData["Success"] = "Member created successfully!";
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError(string.Empty, "Unable to create member.");
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id, CancellationToken ct)
    {
        var dto = await members.GetDetailsAsync(id, ct);
        if (dto is null) return NotFound();
        return View(dto);
    }

    [HttpGet]
    public async Task<IActionResult> HealthRecord(int id, CancellationToken ct)
    {
        var dto = await members.GetHealthRecordAsync(id, ct);
        if (dto is null) return NotFound();
        return View(dto);
    }
    
    [HttpGet]
    public async Task<IActionResult> HealthRecordDetails(int id, CancellationToken ct)
    {
        var dto = await members.GetHealthRecordAsync(id, ct);
        
        if (dto is null)
            return NotFound();

        return View(dto);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var dto = await members.GetForEditAsync(id, ct);
        if (dto is null) return NotFound();
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditMemberDTO dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(dto);

        var success = await members.UpdateAsync(dto, ct);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, "Email or phone already in use.");
            return View(dto);
        }

        TempData["SuccessMessage"] = "Member updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var dto = await members.GetForDeleteAsync(id, ct);
        if (dto is null) return NotFound();
        return View(dto);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken ct)
    {
        var success = await members.DeleteAsync(id, ct);

        if (!success)
        {
            TempData["ErrorMessage"] = "Cannot delete member with active bookings.";
            return RedirectToAction(nameof(Delete), new { id });
        }

        TempData["SuccessMessage"] = "Member deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

}
