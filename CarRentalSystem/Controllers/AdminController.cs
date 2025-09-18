using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CarRentalSystem.Data;
using CarRentalSystem.Models;

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

        // GET: Admin/Customers
        public async Task<IActionResult> Customers()
        {
            var authResult = RequireAdmin();
            if (authResult != null) return authResult;

            var customers = await _context.Users
                .Where(u => u.Role == "Customer")
                .Include(u => u.Bookings)
                .OrderBy(c => c.FullName)
                .ToListAsync();

            return View(customers);
        }

        // GET: Admin/CustomerDetails/5
        public async Task<IActionResult> CustomerDetails(int id)
        {
            var authResult = RequireAdmin();
            if (authResult != null) return authResult;

            var customer = await _context.Users
                .Include(u => u.Bookings)
                    .ThenInclude(b => b.Car)
                .FirstOrDefaultAsync(u => u.UserID == id && u.Role == "Customer");

            if (customer == null)
            {
                return NotFound();
            }

            // Calculate customer statistics
            var totalBookings = customer.Bookings.Count;
            var totalSpent = customer.Bookings.Sum(b => b.TotalCost);
            var avgBookingValue = totalBookings > 0 ? totalSpent / totalBookings : 0;

            ViewBag.TotalBookings = totalBookings;
            ViewBag.TotalSpent = totalSpent;
            ViewBag.AvgBookingValue = avgBookingValue;

            return View(customer);
        }
    }
}
