using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CarRentalSystem.Data;
using CarRentalSystem.Models;
using CarRentalSystem.Models.ViewModels;

namespace CarRentalSystem.Controllers
{
    public class BookingsController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public BookingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Bookings/Book/5
        public async Task<IActionResult> Book(int carId)
        {
            var authResult = RequireCustomer();
            if (authResult != null) return authResult;

            var car = await _context.Cars.FindAsync(carId);
            if (car == null || !car.IsAvailable)
            {
                TempData["Error"] = "Car is not available for booking.";
                return RedirectToAction("Index", "Home");
            }

            var model = new BookingViewModel
            {
                CarID = car.CarID,
                CarName = car.CarName,
                CarModel = car.CarModel,
                ImageUrl = car.ImageUrl,
                ImagePath = car.ImagePath,
                PickupDate = DateTime.Today,
                ReturnDate = DateTime.Today.AddDays(1),
                DailyRate = car.DailyRate
            };

            return View(model);
        }

        // POST: Bookings/Book - Modified to redirect to payment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Book(BookingViewModel model)
        {
            var authResult = RequireCustomer();
            if (authResult != null) return authResult;

            // Validate dates
            if (model.PickupDate < DateTime.Today)
            {
                ModelState.AddModelError("PickupDate", "Pickup date cannot be in the past.");
            }

            if (model.ReturnDate <= model.PickupDate)
            {
                ModelState.AddModelError("ReturnDate", "Return date must be after pickup date.");
            }

            // Check if car is still available
            var car = await _context.Cars.FindAsync(model.CarID);
            if (car == null || !car.IsAvailable)
            {
                ModelState.AddModelError("", "Car is no longer available for booking.");
            }

            if (ModelState.IsValid)
            {
                var days = (model.ReturnDate - model.PickupDate).Days;
                var totalCost = days * model.DailyRate;

                var booking = new Booking
                {
                    CustomerID = CurrentUserId!.Value,
                    CarID = model.CarID,
                    PickupDate = model.PickupDate,
                    ReturnDate = model.ReturnDate,
                    TotalCost = totalCost,
                    PaymentMethod = "Pending", // Temporary until payment is selected
                    PaymentStatus = "Pending"
                };

                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();

                // Redirect to payment page instead of MyBookings
                return RedirectToAction("Payment", new { bookingId = booking.BookingID });
            }

            // Reload car data for the view
            if (car != null)
            {
                model.CarName = car.CarName;
                model.CarModel = car.CarModel;
                model.ImageUrl = car.ImageUrl;
                model.ImagePath = car.ImagePath;
            }

            return View(model);
        }

        // GET: Bookings/MyBookings
        public async Task<IActionResult> MyBookings()
        {
            var authResult = RequireCustomer();
            if (authResult != null) return authResult;

            var bookings = await _context.Bookings
                .Include(b => b.Car)
                .Where(b => b.CustomerID == CurrentUserId!.Value)
                .OrderByDescending(b => b.BookingID)
                .ToListAsync();

            return View(bookings);
        }

        // GET: Bookings/All (Admin only)
        public async Task<IActionResult> All()
        {
            var authResult = RequireAdmin();
            if (authResult != null) return authResult;

            var bookings = await _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Car)
                .OrderByDescending(b => b.BookingID)
                .ToListAsync();

            return View(bookings);
        }

        // POST: Bookings/Cancel/5 (Customer can cancel their own bookings)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var authResult = RequireLogin();
            if (authResult != null) return authResult;

            var booking = await _context.Bookings
                .Include(b => b.Car)
                .FirstOrDefaultAsync(b => b.BookingID == id);

            if (booking == null)
            {
                return NotFound();
            }

            // Check if user can cancel this booking
            if (!IsAdmin && booking.CustomerID != CurrentUserId)
            {
                return Forbid();
            }

            // Make car available again
            booking.Car.IsAvailable = true;
            _context.Update(booking.Car);

            // Remove booking
            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Booking cancelled successfully!";

            if (IsAdmin)
                return RedirectToAction("All");
            else
                return RedirectToAction("MyBookings");
        }

        // GET: Bookings/Payment/5
        public async Task<IActionResult> Payment(int bookingId)
        {
            var authResult = RequireCustomer();
            if (authResult != null) return authResult;

            var booking = await _context.Bookings
                .Include(b => b.Car)
                .FirstOrDefaultAsync(b => b.BookingID == bookingId && b.CustomerID == CurrentUserId);

            if (booking == null)
            {
                return NotFound();
            }

            var model = new PaymentViewModel
            {
                BookingID = booking.BookingID,
                CarName = booking.Car.CarName,
                TotalCost = booking.TotalCost,
                PickupDate = booking.PickupDate,
                ReturnDate = booking.ReturnDate
            };

            return View(model);
        }

        // POST: Bookings/ProcessPayment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessPayment(PaymentViewModel model)
        {
            var authResult = RequireCustomer();
            if (authResult != null) return authResult;

            var booking = await _context.Bookings
                .Include(b => b.Car)
                .FirstOrDefaultAsync(b => b.BookingID == model.BookingID && b.CustomerID == CurrentUserId);

            if (booking == null)
            {
                return NotFound();
            }

            // Validate card details if payment method is Card
            if (model.PaymentMethod == "Card")
            {
                if (string.IsNullOrEmpty(model.CardNumber))
                    ModelState.AddModelError("CardNumber", "Card number is required");
                if (string.IsNullOrEmpty(model.CardholderName))
                    ModelState.AddModelError("CardholderName", "Cardholder name is required");
                if (string.IsNullOrEmpty(model.ExpiryDate))
                    ModelState.AddModelError("ExpiryDate", "Expiry date is required");
                if (string.IsNullOrEmpty(model.CVV))
                    ModelState.AddModelError("CVV", "CVV is required");
            }

            if (ModelState.IsValid)
            {
                // Update booking with payment information
                booking.PaymentMethod = model.PaymentMethod;
                booking.PaymentStatus = "Completed";

                // Mark car as unavailable
                booking.Car.IsAvailable = false;
                _context.Update(booking.Car);
                _context.Update(booking);

                await _context.SaveChangesAsync();

                TempData["Success"] = $"Booking confirmed successfully! Payment method: {model.PaymentMethod}";
                return RedirectToAction("MyBookings");
            }

            return View("Payment", model);
        }
    }
}
