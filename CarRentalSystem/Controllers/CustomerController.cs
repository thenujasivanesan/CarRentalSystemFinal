using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CarRentalSystem.Data;

namespace CarRentalSystem.Controllers
{
    public class CustomerController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public CustomerController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Dashboard()
        {
            var authResult = RequireCustomer();
            if (authResult != null) return authResult;

            var customerId = CurrentUserId!.Value;

            // Get customer statistics
            var totalBookings = await _context.Bookings.CountAsync(b => b.CustomerID == customerId);
            var totalSpent = await _context.Bookings
                .Where(b => b.CustomerID == customerId)
                .SumAsync(b => (decimal?)b.TotalCost) ?? 0;

            ViewBag.TotalBookings = totalBookings;
            ViewBag.TotalSpent = totalSpent;

            // Recent bookings
            var recentBookings = await _context.Bookings
                .Include(b => b.Car)
                .Where(b => b.CustomerID == customerId)
                .OrderByDescending(b => b.BookingID)
                .Take(5)
                .ToListAsync();

            return View(recentBookings);
        }

        public async Task<IActionResult> Profile()
        {
            var authResult = RequireCustomer();
            if (authResult != null) return authResult;

            var customerId = CurrentUserId!.Value;
            var user = await _context.Users.FindAsync(customerId);

            if (user == null) return NotFound();

            // Get customer statistics for the profile page
            var totalBookings = await _context.Bookings.CountAsync(b => b.CustomerID == customerId);
            var totalSpent = await _context.Bookings
                .Where(b => b.CustomerID == customerId)
                .SumAsync(b => (decimal?)b.TotalCost) ?? 0;

            ViewBag.TotalBookings = totalBookings;
            ViewBag.TotalSpent = totalSpent;

            return View(user);
        }
    }
}
