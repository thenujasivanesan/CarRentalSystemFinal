using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarRentalSystem.Models
{
    public class Car
    {
        public int CarID { get; set; }

        [Required(ErrorMessage = "Car name is required")]
        [StringLength(100, ErrorMessage = "Car name cannot exceed 100 characters")]
        public string CarName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Car brand is required")]
        [StringLength(50, ErrorMessage = "Car brand cannot exceed 50 characters")]
        public string Brand { get; set; } = string.Empty;

        [Required(ErrorMessage = "Car model is required")]
        [StringLength(50, ErrorMessage = "Car model cannot exceed 50 characters")]
        public string CarModel { get; set; } = string.Empty;

        [Required(ErrorMessage = "Number of seats is required")]
        [Range(1, 50, ErrorMessage = "Number of seats must be between 1 and 50")]
        public int Seats { get; set; } = 4;

        [Required(ErrorMessage = "Daily rate is required")]
        [Range(0.01, 9999.99, ErrorMessage = "Daily rate must be between $0.01 and $9999.99")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal DailyRate { get; set; } = 50.00m;

        [StringLength(200, ErrorMessage = "Image URL cannot exceed 200 characters")]
        public string? ImageUrl { get; set; }

        public string? ImagePath { get; set; }

        public bool IsAvailable { get; set; } = true;

        // Navigation property
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
