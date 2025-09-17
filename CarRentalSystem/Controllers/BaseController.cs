using Microsoft.AspNetCore.Mvc;
using CarRentalSystem.Helpers;

namespace CarRentalSystem.Controllers
{
    public class BaseController : Controller
    {
        protected bool IsLoggedIn => SessionHelper.IsLoggedIn(HttpContext.Session);
        protected bool IsAdmin => SessionHelper.IsAdmin(HttpContext.Session);
        protected bool IsCustomer => SessionHelper.IsCustomer(HttpContext.Session);
        protected int? CurrentUserId => SessionHelper.GetUserId(HttpContext.Session);
        protected string? CurrentUsername => SessionHelper.GetUsername(HttpContext.Session);
        protected string? CurrentUserRole => SessionHelper.GetUserRole(HttpContext.Session);

        protected IActionResult RedirectToLogin()
        {
            return RedirectToAction("Login", "Account");
        }

        protected IActionResult RedirectToDashboard()
        {
            if (IsAdmin)
                return RedirectToAction("Dashboard", "Admin");
            else if (IsCustomer)
                return RedirectToAction("Dashboard", "Customer");
            else
                return RedirectToAction("Index", "Home");
        }

        protected IActionResult RequireLogin()
        {
            if (!IsLoggedIn)
                return RedirectToLogin();
            return null!;
        }

        protected IActionResult RequireAdmin()
        {
            if (!IsLoggedIn)
                return RedirectToLogin();
            if (!IsAdmin)
                return RedirectToDashboard();
            return null!;
        }

        protected IActionResult RequireCustomer()
        {
            if (!IsLoggedIn)
                return RedirectToLogin();
            if (!IsCustomer)
                return RedirectToDashboard();
            return null!;
        }
    }
}
