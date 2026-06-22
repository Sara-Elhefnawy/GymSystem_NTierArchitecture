using GymSystem.Domain.Abstractions.Services;
using GymSystem.Domain.Common;
using GymSystem.Domain.DTOs.Session;
using GymSystem.Domain.DTOs.Session.Lookups;
using GymSystem.UI.Helpers;
using GymSystem.UI.ViewModels.Session;
using Mapster;
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

        var viewModels = result.Value.Adapt<IReadOnlyList<IndexSessionViewModel>>();

        return View(viewModels);
    }

    [HttpGet("ChooseCategory")]
    public async Task<IActionResult> ChooseCategory(CancellationToken ct = default)
    {
        var result = await categories.GetAllAsync(ct);

        if (result.IsFailure)
        {
            this.HandleErrorResult(result);
            return View(new List<SelectListItem>());
        }

        var categoriesList = result.Value
            .Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            })
            .ToList();

        return View(categoriesList);
    }

    [HttpGet("Create/{categoryId:int}")]
    public async Task<IActionResult> Create(int categoryId, CancellationToken ct)
    {
        var categoryResult = await categories.GetAllAsync(ct);
        var category = categoryResult.Value?.FirstOrDefault(c => c.Id == categoryId);

        if (category is null)
        {
            TempData["Error"] = "Category not found";
            return RedirectToAction(nameof(ChooseCategory));
        }

        var availableTrainers = await GetTrainersByCategoryNameSelectList(category.Name);

        var viewModel = (category, availableTrainers).Adapt<CreateSessionViewModel>();
        viewModel.TrainerList = availableTrainers;

        return View(viewModel);
    }

    [HttpPost("Create/{categoryId:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromRoute] int categoryId, CreateSessionViewModel model, CancellationToken ct)
    {
        if (model.CategoryId == 0 && categoryId > 0)
        {
            model.CategoryId = categoryId;
            Console.WriteLine($"Using route CategoryId: {categoryId}");
        }

        if (model.CategoryId == 0)
        {
            TempData["Error"] = "Invalid category";
            return RedirectToAction(nameof(ChooseCategory));
        }

        var categoryResult = await categories.GetAllAsync(ct);
        var category = categoryResult.Value?.FirstOrDefault(c => c.Id == model.CategoryId);

        if (category is null)
        {
            TempData["Error"] = "Category not found";
            return RedirectToAction(nameof(ChooseCategory));
        }

        model.CategoryName = category.Name;

        ModelState.Remove("CategoryName");


        if (!ModelState.IsValid)
        {
            model.TrainerList = await GetTrainersByCategoryNameSelectList(category.Name);

            Console.WriteLine($"Validation failed. Category: {category.Name}, CategoryId: {categoryId}");
            Console.WriteLine($"ModelState errors: {string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))}");

            TempData["Error"] = "Please correct the validation errors.";
            return View(model);
        }

        var dto = model.Adapt<CreateSessionDTO>();

        var result = await sessions.CreateAsync(dto, ct);

        if (result.IsSuccess)
        {
            TempData["Success"] = "Session created successfully!";
            return RedirectToAction(nameof(Index));
        }

        // Use the error handler
        this.HandleErrorResult(result, ModelState);

        model.TrainerList = await GetTrainersByCategoryNameSelectList(category.Name);
        model.CategoryName = category.Name;

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

        var viewModel = result.Value.Adapt<DetailsSessionViewModel>();

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
            CanEdit = detailsResult.Value.StartDate > DateTime.Now,
            Status = detailsResult.Value.Status.ToString(),
            TrainerList = availableTrainers
        };

        if (!viewModel.CanEdit)
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

        Console.WriteLine($"=== Edit POST ===");
        Console.WriteLine($"Route ID: {id}");
        Console.WriteLine($"Model ID: {model.Id}");
        Console.WriteLine($"Model TrainerId: {model.TrainerId}");
        Console.WriteLine($"Model Description: {model.Description}");
        Console.WriteLine($"Model StartDate: {model.StartDate}");
        Console.WriteLine($"Model EndDate: {model.EndDate}");

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

        if (model is null)
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

        var dto = model.Adapt<EditSessionDTO>();

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

        var viewModel = result.Value.Adapt<DeleteSessionViewModel>();

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
                var viewModel = getResult.Value.Adapt<DeleteSessionViewModel>();

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
            if (string.IsNullOrEmpty(categoryName))
            {
                Console.WriteLine($"Warning: Category name is null or empty");
                return new SelectList(new List<SelectListItem>());
            }

            var trainersResult = await trainers.GetAllAsync();
            if (trainersResult.IsFailure || trainersResult.Value == null)
                return new SelectList(new List<SelectListItem>());

            // Normalize the category name (remove spaces and underscores, lowercase)
            var normalizedCategory = categoryName.Replace(" ", "").Replace("_", "").ToLowerInvariant();

            var filteredTrainers = trainersResult.Value
                .Where(t =>
                {
                    var specialtyString = t.Specialty?.ToString() ?? "";
                    // Normalize the specialty (remove underscores, spaces, lowercase)
                    var normalizedSpecialty = specialtyString.Replace("_", "").Replace(" ", "").ToLowerInvariant();
                    return string.Equals(normalizedSpecialty, normalizedCategory, StringComparison.OrdinalIgnoreCase);
                })
                .ToList();

            Console.WriteLine($"Found {filteredTrainers.Count} trainers for category '{categoryName}'");

            return new SelectList(filteredTrainers, "Id", "Name");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting trainers: {ex.Message}");
            return new SelectList(new List<SelectListItem>());
        }
    }
}
