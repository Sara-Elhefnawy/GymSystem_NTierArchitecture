using GymSystem.Domain.Abstractions.Services;
using GymSystem.Domain.DTOs.Session;
using GymSystem.UI.Helpers;
using GymSystem.UI.ViewModels.Session;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GymSystem.UI.Controllers;

[Route("Session")]
public class SessionController(
    ISessionService sessions,
    ICategoryService categories,
    ITrainerService trainers) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var result = await sessions.GetAllAsync(ct);

        if (result.IsFailure)
        {
            this.HandleErrorResult(result);
            return View(new List<IndexSessionViewModel>());
        }

        var viewModels = result.Value.Select(dto => new IndexSessionViewModel
        {
            Id = dto.Id,
            CategoryName = dto.CategoryName,
            Description = dto.Description,
            TrainerName = dto.TrainerName,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Duration = dto.EndDate - dto.StartDate,
            MaxCapacity = dto.MaxCapacity,
            AvailableSlots = dto.AvailableSlots,
            Status = dto.Status
        }).ToList();

        return View(viewModels);
    }

    [HttpGet("ChooseCategory")]
    public async Task<IActionResult> ChooseCategory()
    {
        var categoriesList = await GetCategorySelectList();
        return View(categoriesList);
    }

    [HttpGet("Create/{categoryId:int}")]
    public async Task<IActionResult> Create(int categoryId, CancellationToken ct)
    {
        var categoryResult = await categories.GetAllAsync();
        var category = categoryResult.Value?.FirstOrDefault(c => c.Id == categoryId);

        if (category == null)
        {
            TempData["Error"] = "Category not found";
            return RedirectToAction(nameof(ChooseCategory));
        }

        var availableTrainers = await GetTrainersByCategoryNameSelectList(category.Name);

        var viewModel = new CreateSessionViewModel
        {
            CategoryId = categoryId,
            CategoryName = category.Name,
            TrainerList = availableTrainers,
            Capacity = 25,
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddHours(1)
        };

        return View(viewModel);
    }

    [HttpPost("Create/{categoryId:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int categoryId, CreateSessionViewModel model, CancellationToken ct)
    {
        if (categoryId != model.CategoryId)
        {
            TempData["Error"] = "Category mismatch";
            return RedirectToAction(nameof(ChooseCategory));
        }

        ModelState.Remove("CategoryName");

        if (!ModelState.IsValid)
        {
            var categoryResult = await categories.GetAllAsync();
            var category = categoryResult.Value?.FirstOrDefault(c => c.Id == categoryId);
            model.CategoryName = category?.Name ?? "";
            model.TrainerList = await GetTrainersByCategoryNameSelectList(category?.Name ?? "");

            TempData["Error"] = "Please correct the validation errors.";
            return View(model);
        }

        var dto = new CreateSessionDTO
        {
            CategoryId = model.CategoryId,
            TrainerId = model.TrainerId,
            Description = model.Description,
            Capacity = model.Capacity,
            StartDate = model.StartDate,
            EndDate = model.EndDate
        };

        var result = await sessions.CreateAsync(dto, ct);

        if (result.IsSuccess)
        {
            TempData["Success"] = "Session created successfully!";
            return RedirectToAction(nameof(Index));
        }

        // Use the error handler
        this.HandleErrorResult(result, ModelState);

        var categoryData = await categories.GetAllAsync(ct);
        var categoryInfo = categoryData.Value?.FirstOrDefault(c => c.Id == categoryId);
        model.CategoryName = categoryInfo?.Name ?? "";
        model.TrainerList = await GetTrainersByCategoryNameSelectList(categoryInfo?.Name ?? "");

        return View(model);
    }

    [HttpGet("Details/{id}")]
    public async Task<IActionResult> Details(int id, CancellationToken ct)
    {
        var result = await sessions.GetDetailsAsync(id, ct);

        if (result.IsFailure)
        {
            this.HandleErrorResult(result);
            return RedirectToAction(nameof(Index));
        }

        var viewModel = new DetailsSessionViewModel
        {
            Id = id,
            CategoryName = result.Value.CategoryName,
            TrainerName = result.Value.TrainerName,
            StartDate = result.Value.StartDate,
            EndDate = result.Value.EndDate,
            Capacity = result.Value.Capacity,
            AvailableSlots = result.Value.AvailableSlots,
            Description = result.Value.Description,
            Status = result.Value.Status
        };

        return View(viewModel);
    }

    [HttpGet("Edit/{id:int}")]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var detailsResult = await sessions.GetDetailsAsync(id, ct);

        if (detailsResult.IsFailure)
        {
            this.HandleErrorResult(detailsResult);
            return RedirectToAction(nameof(Index));
        }

        var editResult = await sessions.GetForEditAsync(id, ct);
        if (editResult.IsFailure)
        {
            this.HandleErrorResult(editResult);
            return RedirectToAction(nameof(Index));
        }

        var canEdit = detailsResult.Value.StartDate > DateTime.Now;
        var availableTrainers = await GetTrainersByCategoryNameSelectList(detailsResult.Value.CategoryName);

        var viewModel = new EditSessionViewModel
        {
            Id = id,
            CategoryName = detailsResult.Value.CategoryName,
            MaxCapacity = detailsResult.Value.Capacity,
            TrainerId = editResult.Value.TrainerId,
            Description = editResult.Value.Description,
            StartDate = editResult.Value.StartDate,
            EndDate = editResult.Value.EndDate,
            CanEdit = canEdit,
            Status = detailsResult.Value.Status.ToString(),
            TrainerList = availableTrainers
        };

        if (!canEdit)
        {
            TempData["Warning"] = $"This session is {detailsResult.Value.Status.ToString().ToLower()} and cannot be edited.";
        }

        return View(viewModel);
    }

    [HttpPost("Edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit([FromRoute] int id, [FromForm] EditSessionViewModel model, CancellationToken ct)
    {
        if (model.Id == 0 && id > 0)
        {
            model.Id = id;
        }

        ModelState.Remove("CategoryName");
        ModelState.Remove("MaxCapacity");
        ModelState.Remove("Status");
        ModelState.Remove("CanEdit");

        var detailsResult = await sessions.GetDetailsAsync(id, ct);

        if (detailsResult.IsFailure)
        {
            this.HandleErrorResult(detailsResult);
            return RedirectToAction(nameof(Index));
        }

        if (model == null)
        {
            model = new EditSessionViewModel();
        }

        model.CategoryName = detailsResult.Value.CategoryName;
        model.MaxCapacity = detailsResult.Value.Capacity;
        model.Status = detailsResult.Value.Status.ToString();
        model.CanEdit = detailsResult.Value.StartDate > DateTime.Now;

        if (!model.CanEdit)
        {
            ModelState.AddModelError("", "This session has already started and cannot be edited");
            TempData["Error"] = "Cannot edit sessions that have already started";

            var trainers = await GetTrainersByCategoryNameSelectList(model.CategoryName);
            model.TrainerList = trainers;
            return View(model);
        }

        if (!ModelState.IsValid)
        {
            var trainers = await GetTrainersByCategoryNameSelectList(model.CategoryName);
            model.TrainerList = trainers;
            return View(model);
        }

        var dto = new EditSessionDTO
        {
            Id = id,
            Description = model.Description,
            EndDate = model.EndDate,
            StartDate = model.StartDate,
            TrainerId = model.TrainerId
        };

        var result = await sessions.UpdateAsync(dto, ct);

        if (result.IsSuccess)
        {
            TempData["Success"] = "Session updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        // Use the error handler
        this.HandleErrorResult(result, ModelState);

        var availableTrainers = await GetTrainersByCategoryNameSelectList(model.CategoryName);
        model.TrainerList = availableTrainers;
        model.Status = detailsResult.Value.Status.ToString();
        model.CanEdit = detailsResult.Value.StartDate > DateTime.Now;

        return View(model);
    }

    [HttpGet("Delete/{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await sessions.GetForDeleteAsync(id, ct);

        if (result.IsFailure)
        {
            this.HandleErrorResult(result);
            return RedirectToAction(nameof(Index));
        }

        var viewModel = new DeleteSessionViewModel
        {
            Id = id,
            Specialty = result.Value.Specialty,
            TrainerName = result.Value.TrainerName,
            Description = result.Value.Description,
            StartDate = result.Value.StartDate,
            EndDate = result.Value.EndDate,
            BookedCount = result.Value.BookedCount,
            MaxCapacity = result.Value.MaxCapacity,
            Status = result.Value.Status.ToString(),
            CanDelete = result.Value.CanDelete
        };

        return View(viewModel);
    }

    [HttpPost("Delete/{id:int}"), ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken ct)
    {
        var result = await sessions.DeleteAsync(id, ct);

        if (result.IsFailure)
        {
            this.HandleErrorResult(result);

            var getResult = await sessions.GetForDeleteAsync(id, ct);
            if (getResult.IsSuccess)
            {
                var viewModel = new DeleteSessionViewModel
                {
                    Id = getResult.Value.Id,
                    Specialty = getResult.Value.Specialty,
                    TrainerName = getResult.Value.TrainerName,
                    Description = getResult.Value.Description,
                    StartDate = getResult.Value.StartDate,
                    EndDate = getResult.Value.EndDate,
                    BookedCount = getResult.Value.BookedCount,
                    MaxCapacity = getResult.Value.MaxCapacity,
                    Status = getResult.Value.Status.ToString(),
                    CanDelete = getResult.Value.CanDelete
                };

                return View("Delete", viewModel);
            }

            return RedirectToAction(nameof(Delete), new { id });
        }

        TempData["Success"] = "Session deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<SelectList> GetTrainersByCategoryNameSelectList(string categoryName)
    {
        try
        {
            var specialty = categoryName switch
            {
                "Yoga" => "Yoga",
                "Cardio" => "Cardio",
                "CrossFit" => "CrossFit",
                "Boxing" => "Boxing",
                "Bodybuilding" => "Bodybuilding",
                "General Fitness" => "GeneralFitness",
                "Personal Training" => "PersonalTraining",
                _ => ""
            };

            if (string.IsNullOrEmpty(specialty))
            {
                Console.WriteLine($"Warning: No specialty mapping found for category '{categoryName}'");
                return new SelectList(new List<SelectListItem>());
            }

            var trainersResult = await trainers.GetAllAsync();
            if (trainersResult.IsFailure || trainersResult.Value == null)
                return new SelectList(new List<SelectListItem>());

            var filteredTrainers = trainersResult.Value
                .Where(t => t.Specialties == specialty)
                .ToList();

            Console.WriteLine($"Found {filteredTrainers.Count} trainers for specialty {specialty}");

            return new SelectList(filteredTrainers, "Id", "Name");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting trainers: {ex.Message}");
            return new SelectList(new List<SelectListItem>());
        }
    }

    private async Task<SelectList> GetCategorySelectList()
    {
        var result = await categories.GetAllAsync();
        if (result.IsFailure || result.Value == null)
            return new SelectList(new List<SelectListItem>());

        return new SelectList(result.Value.ToList(), "Id", "Name");
    }
}
