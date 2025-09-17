using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CarRentalSystem.Data;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Text;

namespace CarRentalSystem.Controllers
{
    public class ReportsController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var authResult = RequireAdmin();
            if (authResult != null) return authResult;

            return View();
        }

        public async Task<IActionResult> CarInventoryReport(string format = "view")
        {
            var authResult = RequireAdmin();
            if (authResult != null) return authResult;

            var cars = await _context.Cars.ToListAsync();

            if (format == "pdf")
                return GenerateCarInventoryPDF(cars);

            return View(cars);
        }

        public async Task<IActionResult> BookingsReport(string format = "view")
        {
            var authResult = RequireAdmin();
            if (authResult != null) return authResult;

            var bookings = await _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Car)
                .OrderByDescending(b => b.BookingID)
                .ToListAsync();

            if (format == "pdf")
                return GenerateBookingsPDF(bookings);

            return View(bookings);
        }

        public async Task<IActionResult> CustomersReport(string format = "view")
        {
            var authResult = RequireAdmin();
            if (authResult != null) return authResult;

            var customers = await _context.Users
                .Where(u => u.Role == "Customer")
                .Include(u => u.Bookings)
                .ToListAsync();

            if (format == "pdf")
                return GenerateCustomersPDF(customers);

            return View(customers);
        }

        private IActionResult GenerateCarInventoryPDF(List<Models.Car> cars)
        {
            var document = new Document(PageSize.A4, 25, 25, 30, 30);
            var stream = new MemoryStream();
            var writer = PdfWriter.GetInstance(document, stream);

            document.Open();

            // Title
            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
            var title = new Paragraph("Car Inventory Report", titleFont);
            title.Alignment = Element.ALIGN_CENTER;
            document.Add(title);

            document.Add(new Paragraph($"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm}", FontFactory.GetFont(FontFactory.HELVETICA, 10)));
            document.Add(new Paragraph(" "));

            // Table
            var table = new PdfPTable(4);
            table.WidthPercentage = 100;

            // Headers
            var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
            table.AddCell(new PdfPCell(new Phrase("Car Name", headerFont)));
            table.AddCell(new PdfPCell(new Phrase("Model", headerFont)));
            table.AddCell(new PdfPCell(new Phrase("Status", headerFont)));
            table.AddCell(new PdfPCell(new Phrase("Image Source", headerFont)));

            // Data
            var cellFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
            foreach (var car in cars)
            {
                table.AddCell(new PdfPCell(new Phrase(car.CarName, cellFont)));
                table.AddCell(new PdfPCell(new Phrase(car.CarModel, cellFont)));
                table.AddCell(new PdfPCell(new Phrase(car.IsAvailable ? "Available" : "Booked", cellFont)));
                table.AddCell(new PdfPCell(new Phrase(!string.IsNullOrEmpty(car.ImageUrl) ? "URL" : !string.IsNullOrEmpty(car.ImagePath) ? "Upload" : "None", cellFont)));
            }

            document.Add(table);
            document.Close();

            return File(stream.ToArray(), "application/pdf", "CarInventoryReport.pdf");
        }



        private IActionResult GenerateBookingsPDF(List<Models.Booking> bookings)
        {
            var document = new Document(PageSize.A4.Rotate(), 25, 25, 30, 30);
            var stream = new MemoryStream();
            var writer = PdfWriter.GetInstance(document, stream);

            document.Open();

            // Title
            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
            var title = new Paragraph("Bookings Report", titleFont);
            title.Alignment = Element.ALIGN_CENTER;
            document.Add(title);

            document.Add(new Paragraph($"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm}", FontFactory.GetFont(FontFactory.HELVETICA, 10)));
            document.Add(new Paragraph(" "));

            // Table
            var table = new PdfPTable(7);
            table.WidthPercentage = 100;

            // Headers
            var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
            table.AddCell(new PdfPCell(new Phrase("Booking ID", headerFont)));
            table.AddCell(new PdfPCell(new Phrase("Customer", headerFont)));
            table.AddCell(new PdfPCell(new Phrase("Car", headerFont)));
            table.AddCell(new PdfPCell(new Phrase("Pickup Date", headerFont)));
            table.AddCell(new PdfPCell(new Phrase("Return Date", headerFont)));
            table.AddCell(new PdfPCell(new Phrase("Days", headerFont)));
            table.AddCell(new PdfPCell(new Phrase("Total Cost", headerFont)));

            // Data
            var cellFont = FontFactory.GetFont(FontFactory.HELVETICA, 9);
            foreach (var booking in bookings)
            {
                var days = (booking.ReturnDate - booking.PickupDate).Days;
                table.AddCell(new PdfPCell(new Phrase($"#{booking.BookingID}", cellFont)));
                table.AddCell(new PdfPCell(new Phrase(booking.Customer.Username, cellFont)));
                table.AddCell(new PdfPCell(new Phrase($"{booking.Car.CarName} ({booking.Car.CarModel})", cellFont)));
                table.AddCell(new PdfPCell(new Phrase(booking.PickupDate.ToString("MMM dd, yyyy"), cellFont)));
                table.AddCell(new PdfPCell(new Phrase(booking.ReturnDate.ToString("MMM dd, yyyy"), cellFont)));
                table.AddCell(new PdfPCell(new Phrase(days.ToString(), cellFont)));
                table.AddCell(new PdfPCell(new Phrase($"${booking.TotalCost:F2}", cellFont)));
            }

            document.Add(table);

            // Summary
            document.Add(new Paragraph(" "));
            document.Add(new Paragraph($"Total Bookings: {bookings.Count}", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12)));
            document.Add(new Paragraph($"Total Revenue: ${bookings.Sum(b => b.TotalCost):F2}", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12)));

            document.Close();

            return File(stream.ToArray(), "application/pdf", "BookingsReport.pdf");
        }



        private IActionResult GenerateCustomersPDF(List<Models.User> customers)
        {
            var document = new Document(PageSize.A4, 25, 25, 30, 30);
            var stream = new MemoryStream();
            var writer = PdfWriter.GetInstance(document, stream);

            document.Open();

            // Title
            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
            var title = new Paragraph("Customers Report", titleFont);
            title.Alignment = Element.ALIGN_CENTER;
            document.Add(title);

            document.Add(new Paragraph($"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm}", FontFactory.GetFont(FontFactory.HELVETICA, 10)));
            document.Add(new Paragraph(" "));

            // Table
            var table = new PdfPTable(4);
            table.WidthPercentage = 100;

            // Headers
            var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
            table.AddCell(new PdfPCell(new Phrase("Customer ID", headerFont)));
            table.AddCell(new PdfPCell(new Phrase("Username", headerFont)));
            table.AddCell(new PdfPCell(new Phrase("Total Bookings", headerFont)));
            table.AddCell(new PdfPCell(new Phrase("Total Spent", headerFont)));

            // Data
            var cellFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
            foreach (var customer in customers)
            {
                table.AddCell(new PdfPCell(new Phrase(customer.UserID.ToString(), cellFont)));
                table.AddCell(new PdfPCell(new Phrase(customer.Username, cellFont)));
                table.AddCell(new PdfPCell(new Phrase(customer.Bookings.Count.ToString(), cellFont)));
                table.AddCell(new PdfPCell(new Phrase($"${customer.Bookings.Sum(b => b.TotalCost):F2}", cellFont)));
            }

            document.Add(table);
            document.Close();

            return File(stream.ToArray(), "application/pdf", "CustomersReport.pdf");
        }


    }
}
