using System.ComponentModel.DataAnnotations;

namespace CarRentalSystem.Models.ViewModels
{
    public class BookingViewModel
    {
        public int CarID { get; set; }
        public string CarName { get; set; } = string.Empty;
        public string CarModel { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string? ImagePath { get; set; }

        [Required(ErrorMessage = "Pickup date is required")]
        [DataType(DataType.Date)]
        public DateTime PickupDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Return date is required")]
        [DataType(DataType.Date)]
        public DateTime ReturnDate { get; set; } = DateTime.Today.AddDays(1);

        public decimal TotalCost { get; set; }
        public int Days { get; set; }
        public decimal DailyRate { get; set; } = 50.00m;
    }
}
