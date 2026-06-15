using GymSystem.Domain.DTOs.HealthRecord;
using GymSystem.Domain.DTOs.Member;
using GymSystem.Domain.Services;
using GymSystem.UI.ViewModels.Member;
using Microsoft.AspNetCore.Mvc;

namespace GymSystem.UI.Controllers;

[Route("Member")]
public class MemberController(IMemberService members) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken ct = default)
    {
        var result = await members.GetAllAsync(ct);

        if (result.IsFailure)
        {
            TempData["Error"] = "Unable to load members. Please try again.";
            return View(new List<IndexMemberViewModel>());
        }

        var viewModels = result.Value.Select(m => new IndexMemberViewModel
        {
            Id = m.Id,
            Name = m.Name,
            Email = m.Email,
            Phone = m.Phone,
            Photo = m.Photo,
            Gender = m.Gender
        }).ToList();

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

        var dto = new CreateMemberDTO
        {
            Name = model.Name,
            Email = model.Email,
            Phone = model.Phone,
            DateOfBirth = model.DateOfBirth,
            Gender = model.Gender,
            BuildingNumber = model.BuildingNumber,
            City = model.City,
            Street = model.Street,
            HealthRecord = new CreateHealthRecordDTO
            {
                BloodType = model.HealthRecord.BloodType,
                Height = model.HealthRecord.Height,
                Weight = model.HealthRecord.Weight,
                Note = model.HealthRecord.Note
            }
        };

        var result = await members.CreateAsync(dto, ct);

        if (result.IsSuccess)
        {
            TempData["Success"] = "Member created successfully!";
            return RedirectToAction(nameof(Index));
        }

        // Handle specific error cases
        switch (result.ErrorKey)
        {
            case "EMAIL_TAKEN":
                ModelState.AddModelError("Email", "This email is already registered");
                TempData["Warning"] = "This email is already registered to another member. Please use a different email.";
                break;

            case "PHONE_TAKEN":
                ModelState.AddModelError("Phone", "This phone number is already registered");
                TempData["Warning"] = "This phone number is already registered to another member. Please use a different number.";
                break;

            case "INVALID_AGE":
                ModelState.AddModelError("DateOfBirth", "Age must be between 12 and 120 years");
                TempData["Warning"] = "Age must be between 12 and 120 years.";
                break;

            case "INVALID_NAME":
                ModelState.AddModelError("Name", "Name can only contain letters, spaces, hyphens, and apostrophes");
                TempData["Warning"] = "Name contains invalid characters. Use only letters, spaces, hyphens, and apostrophes.";
                break;

            case "INVALID_GENDER":
                ModelState.AddModelError("Gender", "Please select a valid gender");
                TempData["Warning"] = "Please select a valid gender option.";
                break;

            case "INVALID_BLOOD_TYPE":
                ModelState.AddModelError("HealthRecord.BloodType", "Please select a valid blood type");
                TempData["Warning"] = "Please select a valid blood type from the list.";
                break;

            default:
                ModelState.AddModelError(string.Empty, result.Error);
                TempData["Error"] = result.Error;
                break;
        }

        return View(model);
    }

    [HttpGet("Details/{id:int}")]
    public async Task<IActionResult> Details(int id, CancellationToken ct)
    {
        var result = await members.GetDetailsAsync(id, ct);

        if (result.IsFailure)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(Index));
        }

        var viewModel = new DetailsMemberViewModel
        {
            Id = result.Value.Id,
            Name = result.Value.Name,
            Email = result.Value.Email,
            Phone = result.Value.Phone,
            Photo = result.Value.Photo,
            Gender = result.Value.Gender,
            DateOfBirth = result.Value.DateOfBirth,
            MembershipEndDate = result.Value.MembershipEndDate,
            MembershipStartDate = result.Value.MembershipStartDate,
            PlanName = result.Value.PlanName,
            Address = result.Value.Address
        };

        var healthRecord = await members.GetHealthRecordAsync(id, ct);
        if (healthRecord.IsSuccess)
        {
            viewModel.HealthRecord = new DetailsHealthRecordViewModel
            {
                BloodType = healthRecord.Value.BloodType,
                Height = healthRecord.Value.Height,
                Weight = healthRecord.Value.Weight,
                Notes = healthRecord.Value.Notes
            };
        }

        return View(viewModel);
    }

    [HttpGet("Edit/{id:int}")]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var result = await members.GetForEditAsync(id, ct);

        if (result.IsFailure)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(Index));
        }

        var viewModel = new EditMemberViewModel
        {
            Id = id,
            Email = result.Value.Email,
            Phone = result.Value.Phone,
            BuildingNumber = result.Value.BuildingNumber,
            City = result.Value.City,
            Street = result.Value.Street
        };

        // Load Name and Photo from a separate call or modify GetForEditAsync to include them
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

        // Remove Name and Photo from validation since they're not editable
        ModelState.Remove("Name");
        ModelState.Remove("Photo");

        if (!ModelState.IsValid)
        {
            // Need to reload Name and Photo for the view
            var memberDetailss = await members.GetDetailsAsync(id);
            if (memberDetailss.IsSuccess)
            {
                model.Name = memberDetailss.Value.Name;
                model.Photo = memberDetailss.Value.Photo;
            }
            return View(model);
        }

        var dto = new EditMemberDTO
        {
            Id = id,
            Email = model.Email,
            Phone = model.Phone,
            BuildingNumber = model.BuildingNumber,
            City = model.City,
            Street = model.Street
        };

        var result = await members.UpdateAsync(dto, ct);

        if (result.IsSuccess)
        {
            TempData["Success"] = "Member updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        // Handle different error types
        switch (result.ErrorKey)
        {
            case "EMAIL_TAKEN":
                ModelState.AddModelError("Email", "This email is already registered");
                TempData["Warning"] = "The email address is already registered to another member. Please use a different email.";
                break;

            case "PHONE_TAKEN":
                ModelState.AddModelError("Phone", "This phone number is already registered");
                TempData["Warning"] = "The phone number you entered is already registered to another member. Please use a different number.";
                break;

            case "MEMBER_NOT_FOUND":
                ModelState.AddModelError(string.Empty, "Member not found");
                TempData["Error"] = "Member not found. It may have been deleted.";
                break;

            case "UPDATE_ERROR":
                ModelState.AddModelError(string.Empty, "Failed to update member. Please try again.");
                TempData["Error"] = "Failed to update member. Please try again.";
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

        // Reload Name and Photo for the view
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
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(Index));
        }

        var viewModel = new DeleteMemberViewModel
        {
            Id = id,
            Name = result.Value.Name,
            Photo = result.Value.Photo
        };

        return View(viewModel);
    }

    [HttpPost("Delete/{id:int}"), ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken ct)
    {
        var result = await members.DeleteAsync(id, ct);

        if (result.IsFailure)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(Delete), new { id });
        }

        TempData["Success"] = "Member deleted successfully!";
        return RedirectToAction(nameof(Index));
    }
}