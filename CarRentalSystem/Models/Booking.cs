using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarRentalSystem.Models
{
    public class Booking
    {
        public int BookingID { get; set; }

        [Required(ErrorMessage = "Customer is required")]
        public int CustomerID { get; set; }

        [Required(ErrorMessage = "Car is required")]
        public int CarID { get; set; }

        [Required(ErrorMessage = "Pickup date is required")]
        [DataType(DataType.Date)]
        public DateTime PickupDate { get; set; }

        [Required(ErrorMessage = "Return date is required")]
        [DataType(DataType.Date)]
        public DateTime ReturnDate { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalCost { get; set; }

        // Navigation properties
        public virtual User Customer { get; set; } = null!;
        public virtual Car Car { get; set; } = null!;
    }
}
