using GymSystem.Domain.Abstractions.Attachments;
using GymSystem.Domain.Abstractions.QrService;
using GymSystem.Domain.Abstractions.UnitOfWorks;
using GymSystem.Domain.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QRCoder;
using System.Security.Cryptography;
using System.Text;

namespace GymSystem.Domain.QRCode;

public class QrService : IQrService
{
    private readonly string _secretKey;
    private readonly string _storageFolder;
    private readonly ILogger<QrService> _logger;
    private readonly IAttachmentService _attachmentService;
    private readonly IUnitOfWork _uow;

    public QrService(
        IOptions<QrCodeSettings> settings,
        ILogger<QrService> logger,
        IAttachmentService attachmentService,
        IUnitOfWork uow)
    {
        _secretKey = settings.Value.SecretKey ?? throw new ArgumentNullException(
            nameof(settings), "QR Secret Key is required");
        _storageFolder = settings.Value.StorageFolder ?? "Attachments/QRCode";
        _logger = logger;
        _attachmentService = attachmentService;
        _uow = uow;
    }

    public async Task<Result<byte[]>> GenerateMemberQrPngAsync(int memberId, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Generating QR code for member {MemberId}", memberId);

            // Check if QR code already exists using predictable filename
            var fileName = GetQrFileName(memberId);
            var qrPath = Path.Combine(_storageFolder, fileName).Replace('\\', '/');

            // Try to get existing QR code
            var existingResult = await _attachmentService.GetBytesAsync(qrPath, ct);
            if (existingResult.IsSuccess)
            {
                _logger.LogInformation("Returning existing QR code for member {MemberId}", memberId);
                return existingResult;
            }

            // Generate new QR code
            var payload = BuildSignedUrl(memberId);

            using var generator = new QRCodeGenerator();
            var qrData = generator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
            var pngBytes = new PngByteQRCode(qrData).GetGraphic(20);

            // Save QR code
            var saveResult = await _attachmentService.SaveBytesAsync(
                pngBytes,
                fileName,
                _storageFolder,
                ct);

            if (saveResult.IsFailure)
            {
                _logger.LogError("Failed to save QR code: {Error}", saveResult.Error);
                return Result.Fail<byte[]>($"Failed to save QR code: {saveResult.Error}");
            }

            _logger.LogInformation("QR code generated and saved successfully for member {MemberId}", memberId);

            return Result.Ok(pngBytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating QR code for member {MemberId}", memberId);
            return Result.Fail<byte[]>($"Failed to generate QR code: {ex.Message}", "QR_GENERATION_ERROR");
        }
    }

    public async Task<Result> DeleteMemberQrCodeAsync(int memberId, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Deleting QR code for member {MemberId}", memberId);

            // Check if member exists
            var member = await _uow.Members.GetByIdAsync(memberId, ct);
            if (member == null)
            {
                _logger.LogWarning("Member {MemberId} not found", memberId);
                return Result.Fail("Member not found", "MEMBER_NOT_FOUND");
            }

            // Build the QR code path using predictable naming
            var fileName = GetQrFileName(memberId);
            var qrPath = Path.Combine(_storageFolder, fileName).Replace('\\', '/');

            // Delete using attachment service
            var deleteResult = await _attachmentService.DeleteAsync(qrPath, ct);

            if (deleteResult.IsFailure)
            {
                // If file not found, consider it as success (already deleted)
                if (deleteResult.Error == "File not found")
                {
                    _logger.LogInformation("QR code file already deleted for member {MemberId}", memberId);
                    return Result.Ok();
                }

                _logger.LogWarning("Failed to delete QR code file: {Error}", deleteResult.Error);
                return Result.Fail($"Failed to delete QR code: {deleteResult.Error}", "DELETE_ERROR");
            }

            _logger.LogInformation("QR code deleted successfully for member {MemberId}", memberId);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting QR code for member {MemberId}", memberId);
            return Result.Fail($"Failed to delete QR code: {ex.Message}", "DELETE_ERROR");
        }
    }

    public async Task<Result<byte[]>> GetMemberQrCodeAsync(int memberId, CancellationToken ct = default)
    {
        try
        {
            var fileName = GetQrFileName(memberId);
            var qrPath = Path.Combine(_storageFolder, fileName).Replace('\\', '/');

            return await _attachmentService.GetBytesAsync(qrPath, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting QR code for member {MemberId}", memberId);
            return Result.Fail<byte[]>($"Failed to get QR code: {ex.Message}", "GET_ERROR");
        }
    }

    public async Task<Result> DeleteMemberQrCodeByPathAsync(string qrPath, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrEmpty(qrPath))
            {
                return Result.Ok(); // No path to delete
            }

            var deleteResult = await _attachmentService.DeleteAsync(qrPath, ct);

            if (deleteResult.IsFailure && deleteResult.Error != "File not found")
            {
                _logger.LogWarning("Failed to delete QR code by path: {Error}", deleteResult.Error);
                return Result.Fail($"Failed to delete QR code: {deleteResult.Error}");
            }

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting QR code by path: {QrPath}", qrPath);
            return Result.Fail($"Failed to delete QR code: {ex.Message}");
        }
    }

    public string BuildSignedUrl(int memberId)
    {
        var signature = GenerateHmacSignature(memberId);
        return $"/CheckIn/Confirm?memberId={memberId}&sig={signature}";
    }

    public bool ValidateSignature(int memberId, string signature)
    {
        try
        {
            var expectedSignature = GenerateHmacSignature(memberId);

            var isValid = CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(expectedSignature),
                Encoding.UTF8.GetBytes(signature)
            );

            if (!isValid)
            {
                _logger.LogWarning("Invalid QR signature for member {MemberId}", memberId);
            }

            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating QR signature for member {MemberId}", memberId);
            return false;
        }
    }

    private string GenerateHmacSignature(int memberId)
    {
        var dataBytes = Encoding.UTF8.GetBytes(memberId.ToString());
        var secretKeyBytes = Encoding.UTF8.GetBytes(_secretKey);

        using var hmac = new HMACSHA256(secretKeyBytes);
        var hashBytes = hmac.ComputeHash(dataBytes);

        return Convert.ToHexString(hashBytes);
    }

    private string GetQrFileName(int memberId)
    {
        return $"qr_code_member_{memberId}.png";
    }
}
