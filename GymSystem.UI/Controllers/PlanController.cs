using GymSystem.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymSystem.UI.Controllers;

public class PlanController : Controller
{
    private readonly GymAppDbContext _context;
    public PlanController(GymAppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var plans = await _context.Plans.ToListAsync();

        return View(plans);
    }

    public async Task<IActionResult> Details(int id)
    {
        var plan = await _context.Plans.FindAsync(id);

        return View(plan);
    }
}
