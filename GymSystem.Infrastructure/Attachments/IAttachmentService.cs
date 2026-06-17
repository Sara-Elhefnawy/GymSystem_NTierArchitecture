namespace GymSystem.Infrastructure.Attachments;

using GymSystem.Shared.Common;
using Microsoft.AspNetCore.Http;

public interface IAttachmentService
{
    Task<Result<string>> SaveAsync(IFormFile file, string category, CancellationToken ct = default);

    Task<Result<Stream>> GetAsync(string fileName, CancellationToken ct = default);

    Task<Result<byte[]>> GetBytesAsync(string fileName, CancellationToken ct = default);

    Task<Result> DeleteAsync(string fileName, CancellationToken ct = default);

    string GetFullPath(string fileName);
}
