namespace GymSystem.UI.ViewModels.Memberships;

public class IndexMembershipViewModel
{
    public int Id { get; set; }
    public int MemberId { get; set; }
    public string MemberName { get; set; } = default!;
    public string PlanName { get; set; } = default!;

    public string? Photo { get; set; }

    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }

    public string QrCodeUrl => $"/Member/QrCode/{MemberId}";

    public string DownloadFileName => $"QRCode_{MemberName}.png";

    public int DaysLeft
    {
        get
        {
            var endDateTime = EndDate.ToDateTime(TimeOnly.MinValue);
            var now = DateTime.Now;
            return (endDateTime - now).Days;
        }
    }

    public string Status
    {
        get
        {
            var days = DaysLeft;
            return days > 30 ? "Active" : days > 7 ? "Expiring Soon" : "About to Expire";
        }
    }

    public string StatusBadgeClass
    {
        get
        {
            var days = DaysLeft;
            return days > 30 ? "bg-success" : days > 7 ? "bg-warning" : "bg-danger";
        }
    }
}
