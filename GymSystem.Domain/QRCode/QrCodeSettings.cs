namespace GymSystem.Domain.QRCode;

/// Configuration settings for QR code generation
public class QrCodeSettings
{
    /// Secret key used for HMAC signature generation
    /// Must be kept secure and not exposed in code
    public string SecretKey { get; set; } = string.Empty;

    /// Folder where QR codes will be stored
    public string StorageFolder { get; set; } = "Attachments/QRCode";
}
