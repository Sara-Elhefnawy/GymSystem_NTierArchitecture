namespace GymSystem.Domain.Attachments;

public class AttachmentRule
{
    public const long MaxByte = 5*1024*1024;

    public const int MinWidth = 100;
    public const int MinHeight = 100;

    public const int MaxWidth = 4000;
    public const int MaxHeight = 4000;

    public static readonly HashSet<string> AllowedExtensions = 
        new (StringComparer.OrdinalIgnoreCase) { ".jpg", ".png" };
}
