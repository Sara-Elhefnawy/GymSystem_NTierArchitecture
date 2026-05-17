using Microsoft.AspNetCore.Mvc;

namespace GymSystem.UI.Controllers;

public class TestController : Controller
{
    public IActionResult TriggerError()
    {
        // This should trigger your custom error page
        throw new InvalidOperationException("Test exception from TestController!");
    }

    public IActionResult DbError()
    {
        throw new Exception("Simulated database connection error");
    }

    public IActionResult NotFoundTest(int id)
    {
        if (id != 1)
        {
            throw new KeyNotFoundException($"Item with ID {id} was not found");
        }
        return Content("Found!");
    }
}