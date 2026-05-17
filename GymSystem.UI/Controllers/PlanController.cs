using GymSystem.Infrastructure.Data;
using GymSystem.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace GymSystem.UI.Controllers;

public class PlanController : Controller
{
    private readonly GymAppDbContext _context;
    public IPlanRepository Repo { get; }

    public PlanController(GymAppDbContext context)
    {
        _context = context;
        Repo = new PlanRepository(_context);
    }

    public async Task<IActionResult> Index()
    {
        var plans = await Repo.GetAllAsync();

        return View(plans);
    }

    public async Task<IActionResult> Details(int id)
    {
        var plan = await Repo.GetByIdAsync(id);

        return View(plan);
    }
}
