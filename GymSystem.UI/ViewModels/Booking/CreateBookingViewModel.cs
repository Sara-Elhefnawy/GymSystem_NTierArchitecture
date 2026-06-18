using System.ComponentModel.DataAnnotations;

namespace GymSystem.UI.ViewModels.Booking;

public class CreateBookingViewModel
{
    public int SessionId { get; set; }

    [Required(ErrorMessage = "Please select a member")]
    [Display(Name = "Member")]
    public int MemberId { get; set; }
}
