using GymSystem.Domain.DTOs.Session;
using GymSystem.Domain.Services;
using GymSystem.UI.ViewModels.Session;
using Microsoft.AspNetCore.Mvc;

namespace GymSystem.UI.Controllers;

[Route("Session")]
public class SessionController(ISessionService sessions) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        IReadOnlyList<IndexSessionDTO> dtos = await sessions.GetAllAsync(ct);

        IReadOnlyList<IndexSessionViewModel> viewModels = dtos.Select(dto => new IndexSessionViewModel
        {
            Id = dto.Id,
            Specialty = dto.Specialty,
            Description = dto.Description,
            TrainerName = dto.TrainerName,
            StartDate = dto.StartDate,
            TimeRange = dto.TimeRange,
            Duration = dto.Duration,
            Capacity = dto.Capacity,
            AvailableSlots = dto.AvailableSlots,
            Status = dto.Status
        }).ToList();

        return View(viewModels);
    }
}
