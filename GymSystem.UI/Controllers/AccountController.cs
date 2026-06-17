using GymSystem.Infrastructure.Identities;
using GymSystem.UI.ViewModels.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GymSystem.UI.Controllers;

[Route("Account")]
public class AccountController(SignInManager<ApplicationUser> signInManager) : Controller
{
    [HttpGet("")]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost("")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var result = await signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);

        if (result.IsLockedOut)
        {
            TempData["Error"] = "Account locked for 10 minutes. Please try again later.";
            return View(model);
        } 

        if (!result.Succeeded)
        {
            TempData["Error"] = "Invalid email or password";
            return View(model);
        }

        Console.WriteLine("Login successful! Redirecting to Home/Index");
        TempData["Success"] = "Login successful!";
        return RedirectToAction(nameof(HomeController.Index), "Home");
    }
}
