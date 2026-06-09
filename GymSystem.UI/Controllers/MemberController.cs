using GymSystem.Domain.DTOs.HealthRecord;
using GymSystem.Domain.DTOs.Member;
using GymSystem.Domain.Services;
using GymSystem.UI.ViewModels.Member;
using Microsoft.AspNetCore.Mvc;

namespace GymSystem.UI.Controllers;

public class MemberController(IMemberService members) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var dtos = await members.GetAllAsync(ct);

        var viewModels = dtos.Select(m => new IndexMemberViewModel
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

    [HttpGet]
    public IActionResult Create()
    {
        return View(new CreateMemberViewModel
        {
            HealthRecord = new CreateHealthRecordViewModel()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateMemberViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return View(model);

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

        var success = await members.CreateAsync(dto, ct);

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
        if (dto is null)
        {
            TempData["Error"] = "Member not found.";
            return RedirectToAction(nameof(Index));
        }

        var viewModel = new DetailsMemberViewModel
        {
            Id = dto.Id,
            Name = dto.Name,
            Email = dto.Email,
            Phone = dto.Phone,
            Photo = dto.Photo,
            Gender = dto.Gender,
            DateOfBirth = dto.DateOfBirth,
            MembershipEndDate = dto.MembershipEndDate,
            MembershipStartDate = dto.MembershipStartDate,
            PlanName = dto.PlanName,
            Address = dto.Address
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> HealthRecord(int id, CancellationToken ct)
    {
        var dto = await members.GetHealthRecordAsync(id, ct);
        if (dto is null) return NotFound();

        var viewModel = new DetailsHealthRecordViewModel
        {
            BloodType = dto.BloodType,
            Height = dto.Height,
            Weight = dto.Weight,
            Notes = dto.Notes
        };

        return View(viewModel);
    }
    
    [HttpGet]
    public async Task<IActionResult> HealthRecordDetails(int id, CancellationToken ct)
    {
        var dto = await members.GetHealthRecordAsync(id, ct);
        if (dto is null) return NotFound();

        var viewModel = new DetailsHealthRecordViewModel
        {
            BloodType = dto.BloodType,
            Height = dto.Height,
            Weight = dto.Weight,
            Notes = dto.Notes
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var dto = await members.GetForEditAsync(id, ct);
        if (dto is null) return NotFound();

        var viewModel = new EditMemberViewModel
        {
            Id = id,
            Name = dto.Name,
            Email = dto.Email,
            Phone = dto.Phone,
            BuildingNumber = dto.BuildingNumber,
            City = dto.City,
            Street = dto.Street
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit([FromRoute]int id,EditMemberViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(model);

        var dto = new EditMemberDTO
        {
            Id = id,
            Name = model.Name,
            Email = model.Email,
            Phone = model.Phone,
            BuildingNumber = model.BuildingNumber,
            City = model.City,
            Street = model.Street
        };

        var success = await members.UpdateAsync(dto, ct);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, "Email or phone already in use.");
            return View(model);
        }

        TempData["SuccessMessage"] = "Member updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var dto = await members.GetForDeleteAsync(id, ct);

        if (dto is null) return NotFound();

        var viewModel = new DeleteMemberViewModel
        {
            Id = id,
            Name = dto.Name,
            Photo = dto.Photo
        };

        return View(viewModel);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken ct)
    {
        var model = await members.GetForDeleteAsync(id, ct);

        if (!ModelState.IsValid) return View(model);

        var dto = new EditMemberDTO
        {
            Id = id,
            Name = model.Name,
            Photo = model.Photo
        };

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
