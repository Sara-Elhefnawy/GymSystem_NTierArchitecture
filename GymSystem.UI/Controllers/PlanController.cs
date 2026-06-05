using GymSystem.Infrastructure.Entities;
using GymSystem.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace GymSystem.UI.Controllers;

public class PlanController(IRepository<Plan> plans) : Controller
{
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var items = await plans.GetAllAsync(ct);

        return View(items);
    }

    public async Task<IActionResult> Details(int id, CancellationToken ct)
    {
        var item = await plans.GetByIdAsync(id, ct);

        if (item is null) 
            return NotFound();

        return View(item);
    }
}
