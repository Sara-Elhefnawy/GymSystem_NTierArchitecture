using GymSystem.Domain.Common;
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
            TempData["Error"] = "Unable to load trainers. Please try again.";
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
            Duration = dto.EndDate-dto.StartDate,
            MaxCapacity = dto.MaxCapacity,
            AvailableSlots = dto.AvailableSlots,
            Status = dto.Status
        }).ToList();

        return View(viewModels);
    }

    [HttpGet("Details/{id}")]
    public async Task<IActionResult> Details(int id, CancellationToken ct)
    {
        var result = await sessions.GetDetailsAsync(id, ct);
        if (result.IsFailure)
        {
            TempData["Error"] = "Session not found.";
            return RedirectToAction(nameof(Index));
        }

        return View(result.Value);
    }

    [HttpGet("Create")]
    public async Task<IActionResult> Create()
    {
        var viewModel = new CreateSessionViewModel
        {
            CategoryList = await GetCategorySelectList(),
            TrainerList = await GetTrainerSelectList(),
            Capacity = 25,
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddHours(1)
        };

        return View(viewModel);
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateSessionViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            // Repopulate dropdowns on validation error
            viewModel.CategoryList = await GetCategorySelectList();
            viewModel.TrainerList = await GetTrainerSelectList();
            return View(viewModel);
        }

        // Map ViewModel to DTO
        var createSessionDto = new CreateSessionDTO
        {
            CategoryId = viewModel.CategoryId,
            TrainerId = viewModel.TrainerId,
            Description = viewModel.Description,
            Capacity = viewModel.Capacity,
            StartDate = viewModel.StartDate,
            EndDate = viewModel.EndDate
        };

        var result = await sessions.CreateAsync(createSessionDto);

        if (result.IsSuccess)
        {
            TempData["Success"] = "Session created successfully!";
            return RedirectToAction(nameof(Index));
        }

        // Handle error
        ModelState.AddModelError("", result.Error);
        viewModel.CategoryList = await GetCategorySelectList();
        viewModel.TrainerList = await GetTrainerSelectList();
        return View(viewModel);
    }

    private async Task<SelectList> GetCategorySelectList()
    {
        var result = await categories.GetAllAsync();

        if (result.IsFailure)
            return new SelectList(new List<object>());

        return new SelectList(result.Value, "Id", "Name");
    }

    private async Task<SelectList> GetTrainerSelectList()
    {
        var result = await trainers.GetAllAsync();

        if (result.IsFailure)
            return new SelectList(new List<object>());

        return new SelectList(result.Value, "Id", "Name");
    }




}
