using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CarRentalSystem.Data;
using CarRentalSystem.Models;
using CarRentalSystem.Models.ViewModels;

namespace CarRentalSystem.Controllers
{
    public class CarsController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CarsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Cars
        public async Task<IActionResult> Index()
        {
            var authResult = RequireAdmin();
            if (authResult != null) return authResult;

            var cars = await _context.Cars.ToListAsync();
            return View(cars);
        }

        // GET: Cars/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            var authResult = RequireAdmin();
            if (authResult != null) return authResult;

            if (id == null) return NotFound();

            var car = await _context.Cars.FirstOrDefaultAsync(m => m.CarID == id);
            if (car == null) return NotFound();

            return View(car);
        }

        // GET: Cars/Create
        public IActionResult Create()
        {
            var authResult = RequireAdmin();
            if (authResult != null) return authResult;

            return View();
        }

        // POST: Cars/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CarViewModel model)
        {
            var authResult = RequireAdmin();
            if (authResult != null) return authResult;

            // Validate that either ImageUrl or ImageUpload is provided
            if (string.IsNullOrEmpty(model.ImageUrl) && model.ImageUpload == null)
            {
                ModelState.AddModelError("", "Please provide either an Image URL or upload an image file.");
            }

            if (ModelState.IsValid)
            {
                var car = new Car
                {
                    CarName = model.CarName,
                    Brand = model.Brand,
                    CarModel = model.CarModel,
                    Seats = model.Seats,
                    DailyRate = model.DailyRate,
                    ImageUrl = model.ImageUrl,
                    IsAvailable = model.IsAvailable
                };

                // Handle file upload
                if (model.ImageUpload != null)
                {
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + model.ImageUpload.FileName;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ImageUpload.CopyToAsync(fileStream);
                    }

                    car.ImagePath = uniqueFileName;
                    car.ImageUrl = null; // Clear URL if file is uploaded
                }

                _context.Add(car);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Car created successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // GET: Cars/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            var authResult = RequireAdmin();
            if (authResult != null) return authResult;

            if (id == null) return NotFound();

            var car = await _context.Cars.FindAsync(id);
            if (car == null) return NotFound();

            var model = new CarViewModel
            {
                CarID = car.CarID,
                CarName = car.CarName,
                Brand = car.Brand,
                CarModel = car.CarModel,
                Seats = car.Seats,
                DailyRate = car.DailyRate,
                ImageUrl = car.ImageUrl,
                IsAvailable = car.IsAvailable,
                ExistingImagePath = car.ImagePath
            };

            return View(model);
        }

        // POST: Cars/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CarViewModel model)
        {
            var authResult = RequireAdmin();
            if (authResult != null) return authResult;

            if (id != model.CarID) return NotFound();

            // Validate that either ImageUrl or ImageUpload is provided (or existing image exists)
            if (string.IsNullOrEmpty(model.ImageUrl) && model.ImageUpload == null && string.IsNullOrEmpty(model.ExistingImagePath))
            {
                ModelState.AddModelError("", "Please provide either an Image URL or upload an image file.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var car = await _context.Cars.FindAsync(id);
                    if (car == null) return NotFound();

                    car.CarName = model.CarName;
                    car.Brand = model.Brand;
                    car.CarModel = model.CarModel;
                    car.Seats = model.Seats;
                    car.DailyRate = model.DailyRate;
                    car.IsAvailable = model.IsAvailable;

                    // Handle image update
                    if (model.ImageUpload != null)
                    {
                        // Delete old uploaded file if exists
                        if (!string.IsNullOrEmpty(car.ImagePath))
                        {
                            var oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", car.ImagePath);
                            if (System.IO.File.Exists(oldFilePath))
                                System.IO.File.Delete(oldFilePath);
                        }

                        // Upload new file
                        var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                        if (!Directory.Exists(uploadsFolder))
                            Directory.CreateDirectory(uploadsFolder);

                        var uniqueFileName = Guid.NewGuid().ToString() + "_" + model.ImageUpload.FileName;
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await model.ImageUpload.CopyToAsync(fileStream);
                        }

                        car.ImagePath = uniqueFileName;
                        car.ImageUrl = null;
                    }
                    else if (!string.IsNullOrEmpty(model.ImageUrl))
                    {
                        // Delete old uploaded file if switching to URL
                        if (!string.IsNullOrEmpty(car.ImagePath))
                        {
                            var oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", car.ImagePath);
                            if (System.IO.File.Exists(oldFilePath))
                                System.IO.File.Delete(oldFilePath);
                        }

                        car.ImageUrl = model.ImageUrl;
                        car.ImagePath = null;
                    }

                    _context.Update(car);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Car updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CarExists(model.CarID))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // GET: Cars/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            var authResult = RequireAdmin();
            if (authResult != null) return authResult;

            if (id == null) return NotFound();

            var car = await _context.Cars
                .FirstOrDefaultAsync(m => m.CarID == id);
            if (car == null) return NotFound();

            return View(car);
        }

        // POST: Cars/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var authResult = RequireAdmin();
            if (authResult != null) return authResult;

            var car = await _context.Cars.FindAsync(id);
            if (car != null)
            {
                // Delete uploaded image file if exists
                if (!string.IsNullOrEmpty(car.ImagePath))
                {
                    var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", car.ImagePath);
                    if (System.IO.File.Exists(filePath))
                        System.IO.File.Delete(filePath);
                }

                _context.Cars.Remove(car);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Car deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool CarExists(int id)
        {
            return _context.Cars.Any(e => e.CarID == id);
        }
    }
}
