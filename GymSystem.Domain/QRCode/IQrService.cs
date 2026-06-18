using GymSystem.Shared.Common;

namespace GymSystem.Domain.QRCode;

public interface IQrService
{
    Task<Result<byte[]>> GenerateMemberQrPngAsync(int memberId, CancellationToken ct = default);
    Task<Result<byte[]>> GetMemberQrCodeAsync(int memberId, CancellationToken ct = default);
    Task<Result> DeleteMemberQrCodeAsync(int memberId, CancellationToken ct = default);
    Task<Result> DeleteMemberQrCodeByPathAsync(string qrPath, CancellationToken ct = default);
    bool ValidateSignature(int memberId, string signature);
    string BuildSignedUrl(int memberId);
}
