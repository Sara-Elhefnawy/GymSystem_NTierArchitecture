using GymSystem.Domain.DTOs.Session;
using GymSystem.Domain.Services;
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
            TempData["Error"] = "Unable to load sessions. Please try again.";
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

    // Step 1: Choose category first
    [HttpGet("ChooseCategory")]
    public async Task<IActionResult> ChooseCategory()
    {
        var categoriesList = await GetCategorySelectList();
        return View(categoriesList);
    }

    // Step 2: Create session for specific category
    [HttpGet("Create/{categoryId:int}")]
    public async Task<IActionResult> Create(int categoryId, CancellationToken ct)
    {
        // Verify category exists
        var categoryResult = await categories.GetAllAsync();
        var category = categoryResult.Value?.FirstOrDefault(c => c.Id == categoryId);

        if (category == null)
        {
            TempData["Error"] = "Category not found";
            return RedirectToAction(nameof(ChooseCategory));
        }

        // Get trainers that match this category's specialty
        var availableTrainers = await GetTrainersByCategoryNameSelectList(category.Name);

        var viewModel = new CreateSessionViewModel
        {
            CategoryId = categoryId,
            CategoryName = category.Name,  // Display only
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

        switch (result.ErrorKey)
        {
            case "INVALID_DATE_RANGE":
                ModelState.AddModelError("", "End date must be after start date");
                TempData["Warning"] = "End date must be after start date";
                break;
            case "PAST_START_DATE":
                ModelState.AddModelError("StartDate", "Start date must be in the future");
                TempData["Warning"] = "Start date must be in the future";
                break;
            case "INVALID_CAPACITY":
                ModelState.AddModelError("Capacity", "Capacity must be between 1 and 25");
                TempData["Warning"] = "Capacity must be between 1 and 25";
                break;
            case "SPECIALTY_MISMATCH":
                ModelState.AddModelError("TrainerId", result.Error);
                TempData["Warning"] = "Session's trainer has specialty mismatch";
                break;
            case "TRAINER_CONFLICT":
                ModelState.AddModelError("TrainerId", result.Error);
                TempData["Warning"] = "Session's trainer has conflict";
                break;
            case "CATEGORY_NOT_FOUND":
                ModelState.AddModelError("CategoryId", "Selected category does not exist");
                TempData["Warning"] = "Selected category does not exist";
                break;
            case "TRAINER_NOT_FOUND":
                ModelState.AddModelError("TrainerId", "Selected trainer does not exist");
                TempData["Warning"] = "Selected trainer does not exist";
                break;
            default:
                ModelState.AddModelError("", result.Error ?? "Failed to create session");
                TempData["Warning"] = "Failed to create session";
                break;
        }

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
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(Index));
        }

        var viewModel = new DetailsSessionViewModel
        {
            Id = id,
            CategoryName = result.Value.CategoryName,
            TrainerName = result.Value.TrainerName,
            StartDate = result.Value.StartDate,
            EndDate = result.Value.EndDate,
            MaxCapacity = result.Value.MaxCapacity,
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
            TempData["Error"] = detailsResult.Error;
            return RedirectToAction(nameof(Index));
        }

        var editResult = await sessions.GetForEditAsync(id, ct);
        if (editResult.IsFailure)
        {
            TempData["Error"] = editResult.Error;
            return RedirectToAction(nameof(Index));
        }

        var canEdit = detailsResult.Value.StartDate > DateTime.Now;
        var availableTrainers = await GetTrainersByCategoryNameSelectList(detailsResult.Value.CategoryName);

        var viewModel = new EditSessionViewModel
        {
            Id = id,
            CategoryName = detailsResult.Value.CategoryName,
            MaxCapacity = detailsResult.Value.MaxCapacity,
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

        // Remove non-editable fields from validation
        ModelState.Remove("CategoryName");
        ModelState.Remove("MaxCapacity");
        ModelState.Remove("Status");
        ModelState.Remove("CanEdit");
        //ModelState.Remove("Id");

        // Get the session details for error handling and model population
        var detailsResult = await sessions.GetDetailsAsync(id, ct);

        if (detailsResult.IsFailure)
        {
            TempData["Error"] = detailsResult.Error;
            return RedirectToAction(nameof(Index));
        }

        // Ensure model has the required data
        if (model == null)
        {
            model = new EditSessionViewModel();
        }

        // Populate non-editable fields from the session details
        model.CategoryName = detailsResult.Value.CategoryName;
        model.MaxCapacity = detailsResult.Value.MaxCapacity;
        model.Status = detailsResult.Value.Status.ToString();
        model.CanEdit = detailsResult.Value.StartDate > DateTime.Now;

        // Check if session is editable
        if (!model.CanEdit)
        {
            ModelState.AddModelError("", "This session has already started and cannot be edited");
            TempData["Error"] = "Cannot edit sessions that have already started";

            var trainers = await GetTrainersByCategoryNameSelectList(model.CategoryName);
            model.TrainerList = trainers; // Ensure this is set
            return View(model);
        }

        if (!ModelState.IsValid)
        {
            var trainers = await GetTrainersByCategoryNameSelectList(model.CategoryName);
            model.TrainerList = trainers; // Ensure this is set
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

        // Handle specific errors
        switch (result.ErrorKey)
        {
            case "SESSION_NOT_FOUND":
                TempData["Error"] = "Session not found";
                return RedirectToAction(nameof(Index));
            case "SESSION_NOT_EDITABLE":
                ModelState.AddModelError("", "This session has already started and cannot be edited");
                TempData["Warning"] = "This session has already started and cannot be edited";
                model.CanEdit = false;
                break;
            case "INVALID_DATE_RANGE":
                ModelState.AddModelError("", "End date must be after start date");
                TempData["Warning"] = "End date must be after start date";
                break;
            case "PAST_START_DATE":
                ModelState.AddModelError("StartDate", "Start date must be in the future");
                TempData["Warning"] = "Start date must be in the future";
                break;
            case "TRAINER_NOT_FOUND":
                ModelState.AddModelError("TrainerId", "Selected trainer does not exist");
                TempData["Warning"] = "Selected trainer does not exist";
                break;
            case "SPECIALTY_MISMATCH":
                ModelState.AddModelError("TrainerId", result.Error);
                TempData["Warning"] = "This session has specialty mismatch issue";
                break;
            case "TRAINER_CONFLICT":
                ModelState.AddModelError("TrainerId", result.Error);
                TempData["Warning"] = "This session has trainer conflict issue";
                break;
            default:
                ModelState.AddModelError("", result.Error ?? "Failed to update session");
                TempData["Warning"] = "Failed to update session";
                break;
        }

        // Ensure the trainer list is always set before returning the view
        var availableTrainers = await GetTrainersByCategoryNameSelectList(model.CategoryName);
        model.TrainerList = availableTrainers;

        // Ensure Status and CanEdit are set
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
            TempData["Error"] = result.Error;
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

                TempData["Error"] = result.Error;
                return View("Delete", viewModel);
            }

            TempData["Error"] = result.Error;
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

    // Helper Methods
    private async Task<SelectList> GetCategorySelectList()
    {
        var result = await categories.GetAllAsync();
        if (result.IsFailure || result.Value == null)
            return new SelectList(new List<SelectListItem>());

        return new SelectList(result.Value.ToList(), "Id", "Name");
    }
}
