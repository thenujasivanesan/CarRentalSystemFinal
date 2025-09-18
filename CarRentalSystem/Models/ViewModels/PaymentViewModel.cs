using System.ComponentModel.DataAnnotations;

namespace CarRentalSystem.Models.ViewModels
{
    public class PaymentViewModel
    {
        public int BookingID { get; set; }
        public string PaymentMethod { get; set; } = string.Empty; // "Cash" or "Card"
        
        // Card details (only required if PaymentMethod is "Card")
        [Display(Name = "Card Number")]
        public string? CardNumber { get; set; }
        
        [Display(Name = "Cardholder Name")]
        public string? CardholderName { get; set; }
        
        [Display(Name = "Expiry Date (MM/YY)")]
        public string? ExpiryDate { get; set; }
        
        [Display(Name = "CVV")]
        public string? CVV { get; set; }
        
        // Booking details for display
        public string CarName { get; set; } = string.Empty;
        public decimal TotalCost { get; set; }
        public DateTime PickupDate { get; set; }
        public DateTime ReturnDate { get; set; }
    }
}