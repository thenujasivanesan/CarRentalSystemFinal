using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CarRentalSystem.Data;

namespace CarRentalSystem.Controllers
{
    public class AdminController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Dashboard()
        {
            var authResult = RequireAdmin();
            if (authResult != null) return authResult;

            var totalCars = await _context.Cars.CountAsync();
            var availableCars = await _context.Cars.CountAsync(c => c.IsAvailable);
            var totalBookings = await _context.Bookings.CountAsync();
            var totalCustomers = await _context.Users.CountAsync(u => u.Role == "Customer");

            ViewBag.TotalCars = totalCars;
            ViewBag.AvailableCars = availableCars;
            ViewBag.TotalBookings = totalBookings;
            ViewBag.TotalCustomers = totalCustomers;

            // Recent bookings
            var recentBookings = await _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Car)
                .OrderByDescending(b => b.BookingID)
                .Take(5)
                .ToListAsync();

            return View(recentBookings);
        }
    }
}
