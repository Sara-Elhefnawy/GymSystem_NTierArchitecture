using GymSystem.Domain.Abstractions.QrService;
using GymSystem.Domain.Abstractions.Services;
using GymSystem.Domain.DTOs.Member;
using GymSystem.UI.Helpers;
using GymSystem.UI.ViewModels.Member;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymSystem.UI.Controllers;

[Route("Member")]
[Authorize(Roles = "SuperAdmin")]
public class MemberController(IMemberService members, IQrService qrService) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken ct = default)
    {
        var result = await members.GetAllAsync(ct);

        if (result.IsFailure)
        {
            this.HandleErrorResult(result);
            return View(new List<IndexMemberViewModel>());
        }

        var viewModels = result.Value.Adapt<IReadOnlyList<IndexMemberViewModel>>();

        return View(viewModels);
    }

    [HttpGet("Create")]
    public IActionResult Create()
    {
        return View(new CreateMemberViewModel
        {
            HealthRecord = new CreateHealthRecordViewModel()
        });
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateMemberViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Please correct the validation errors.";
            return View(model);
        }

        var dto = model.Adapt<CreateMemberDTO>();
        //dto.HealthRecord.Notes = model.HealthRecord.Note ?? string.Empty;

        var result = await members.CreateAsync(dto, ct);

        if (result.IsSuccess)
        {
            TempData["Success"] = "Member created successfully!";
            return RedirectToAction(nameof(Index));
        }

        // Use the error handler for both ModelState and TempData
        this.HandleErrorResult(result, ModelState);

        // Clear the photo from the model so it doesn't try to re-upload
        model.Photo = null;

        return View(model);
    }

    [HttpGet("Photo/{id:int}")]
    public async Task<IActionResult> GetPhoto(int id, CancellationToken ct = default)
    {
        try
        {
            var memberResult = await members.GetDetailsAsync(id, ct);
            if (memberResult.IsFailure || string.IsNullOrEmpty(memberResult.Value.Photo))
            {
                return File("~/images/User.png", "image/png");
            }

            var photoResult = await members.GetMemberPhotoAsync(id, ct);
            if (photoResult.IsFailure)
            {
                return File("~/images/User.png", "image/png");
            }

            var contentTypes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { ".jpg", "image/jpeg" },
                { ".jpeg", "image/jpeg" },
                { ".png", "image/png" }
            };

            var extension = Path.GetExtension(memberResult.Value.Photo).ToLowerInvariant();
            var contentType = contentTypes.TryGetValue(extension, out var value) ? value : "image/jpeg";

            return File(photoResult.Value, contentType);
        }
        catch (Exception)
        {
            return File("~/images/User.png", "image/png");
        }
    }

    [HttpGet("Details/{id:int}")]
    public async Task<IActionResult> Details(int id, CancellationToken ct)
    {
        var result = await members.GetDetailsAsync(id, ct);

        if (result.IsFailure)
        {
            this.HandleErrorResult(result);
            return RedirectToAction(nameof(Index));
        }

        var viewModel = result.Value.Adapt<DetailsMemberViewModel>();

        var healthRecord = await members.GetHealthRecordAsync(id, ct);
        if (healthRecord.IsSuccess)
        {
            viewModel.HealthRecord = healthRecord.Value.Adapt<DetailsHealthRecordViewModel>();
        }

        return View(viewModel);
    }

    [HttpGet("Edit/{id:int}")]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var result = await members.GetForEditAsync(id, ct);

        if (result.IsFailure)
        {
            this.HandleErrorResult(result);
            return RedirectToAction(nameof(Index));
        }

        var viewModel = result.Value.Adapt<EditMemberViewModel>();

        var memberDetails = await members.GetDetailsAsync(id, ct);
        if (memberDetails.IsSuccess)
        {
            viewModel.Name = memberDetails.Value.Name;
            viewModel.Photo = memberDetails.Value.Photo;
        }

        return View(viewModel);
    }

    [HttpPost("Edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit([FromRoute] int id, EditMemberViewModel model, CancellationToken ct)
    {
        if (id != model.Id)
            return NotFound();

        ModelState.Remove("Name");
        ModelState.Remove("Photo");

        if (!ModelState.IsValid)
        {
            var memberDetailss = await members.GetDetailsAsync(id);
            if (memberDetailss.IsSuccess)
            {
                model.Name = memberDetailss.Value.Name;
                model.Photo = memberDetailss.Value.Photo;
            }
            return View(model);
        }

        var dto = model.Adapt<EditMemberDTO>();

        var result = await members.UpdateAsync(dto, ct);

        if (result.IsSuccess)
        {
            TempData["Success"] = "Member updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        // Use the error handler for both ModelState and TempData
        this.HandleErrorResult(result, ModelState);

        var memberDetails = await members.GetDetailsAsync(id);
        if (memberDetails.IsSuccess)
        {
            model.Name = memberDetails.Value.Name;
            model.Photo = memberDetails.Value.Photo;
        }

        return View(model);
    }

    [HttpGet("Delete/{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await members.GetForDeleteAsync(id, ct);

        if (result.IsFailure)
        {
            this.HandleErrorResult(result);
            return RedirectToAction(nameof(Index));
        }

        var viewModel = result.Value.Adapt<DeleteMemberViewModel>();

        return View(viewModel);
    }

    [HttpPost("Delete/{id:int}"), ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken ct)
    {
        var result = await members.DeleteAsync(id, ct);

        if (result.IsFailure)
        {
            this.HandleErrorResult(result);
            return RedirectToAction(nameof(Delete), new { id });
        }

        TempData["Success"] = "Member deleted successfully!";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("QrCode/{id:int}")]
    [ResponseCache(Duration = 3600)]
    public async Task<IActionResult> QrCode(int id, CancellationToken ct = default)
    {
        try
        {
            var memberResult = await members.GetDetailsAsync(id, ct);
            if (memberResult.IsFailure)
                return NotFound($"Member with ID {id} not found");

            var qrResult = await qrService.GenerateMemberQrPngAsync(id, ct);
            if (qrResult.IsFailure)
                return BadRequest("Failed to generate QR code");

            return File(qrResult.Value, "image/png");
        }
        catch (Exception)
        {
            return StatusCode(500, "An error occurred while generating the QR code");
        }
    }
}
