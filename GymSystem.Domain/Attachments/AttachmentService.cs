using GymSystem.Infrastructure.Attachments;
using GymSystem.Shared.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SkiaSharp;

namespace GymSystem.Domain.Attachments;

public class AttachmentService : IAttachmentService
{
    private readonly IHostEnvironment _hostEnvironment;
    private readonly ILogger<AttachmentService> _logger;

    public AttachmentService(IHostEnvironment hostEnvironment, ILogger<AttachmentService> logger)
    {
        _hostEnvironment = hostEnvironment;
        _logger = logger;
    }

    public Task<Result> DeleteAsync(string fileName, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                _logger.LogWarning("Delete called with empty or null filename");
                return Task.FromResult(Result.Fail("File name cannot be empty"));
            }

            var fullPath = GetFullPath(fileName);
            _logger.LogInformation("Attempting to delete file: {FullPath}", fullPath);

            if (!File.Exists(fullPath))
            {
                _logger.LogWarning("File not found for deletion: {FileName} at path: {FullPath}", fileName, fullPath);
                return Task.FromResult(Result.Fail("File not found"));
            }

            File.Delete(fullPath);
            _logger.LogInformation("File deleted successfully: {FileName}", fileName);
            return Task.FromResult(Result.Ok());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file: {FileName}", fileName);
            return Task.FromResult(Result.Fail($"Error deleting file: {ex.Message}"));
        }
    }

    public Task<Result<Stream>> GetAsync(string fileName, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                _logger.LogWarning("Get called with empty or null filename");
                return Task.FromResult(Result.Fail<Stream>("File name cannot be empty"));
            }

            var fullPath = GetFullPath(fileName);
            _logger.LogInformation("Attempting to retrieve file: {FullPath}", fullPath);

            if (!File.Exists(fullPath))
            {
                _logger.LogWarning("File not found: {FileName} at path: {FullPath}", fileName, fullPath);
                return Task.FromResult(Result.Fail<Stream>("File not found"));
            }

            var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            _logger.LogInformation("File retrieved successfully: {FileName}", fileName);
            return Task.FromResult(Result.Ok<Stream>(stream));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving file: {FileName}", fileName);
            return Task.FromResult(Result.Fail<Stream>($"Error retrieving file: {ex.Message}"));
        }
    }

    public async Task<Result<byte[]>> GetBytesAsync(string fileName, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                _logger.LogWarning("GetBytes called with empty or null filename");
                return Result.Fail<byte[]>("File name cannot be empty");
            }

            var fullPath = GetFullPath(fileName);
            _logger.LogInformation("Attempting to read file: {FullPath}", fullPath);

            if (!File.Exists(fullPath))
            {
                _logger.LogWarning("File not found: {FileName} at path: {FullPath}", fileName, fullPath);
                return Result.Fail<byte[]>("File not found");
            }

            // Read all bytes directly - this closes the file immediately
            var bytes = await File.ReadAllBytesAsync(fullPath, ct);
            _logger.LogInformation("File read successfully: {FileName} (Size: {Size} bytes)", fileName, bytes.Length);
            return Result.Ok(bytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading file bytes: {FileName}", fileName);
            return Result.Fail<byte[]>($"Error reading file: {ex.Message}");
        }
    }

    public async Task<Result<string>> SaveAsync(IFormFile file, string category, CancellationToken ct = default)
    {
        _logger.LogInformation("Starting file save operation for category: {Category}", category);

        var validate = await ValidateImageAsync(file);

        if (validate.IsFailure)
        {
            _logger.LogWarning("Image validation failed for category: {Category}, Error: {Error}", category, validate.Error);
            return Result.Fail<string>("Image not valid");
        }

        var extension = NormalizedExtension(Path.GetExtension(file.FileName));
        var fileName = $"{Guid.NewGuid():N}{extension}";

        // Use the category parameter directly - it should already be the folder name
        var directory = Path.Combine(_hostEnvironment.ContentRootPath, category);

        try
        {
            // if directory already exists no exception happens
            Directory.CreateDirectory(directory);

            var fullPath = Path.Combine(directory, fileName);

            await using var stream = new FileStream(fullPath, FileMode.CreateNew);
            await file.CopyToAsync(stream, ct);

            var relativePath = Path.Combine(category, fileName).Replace('\\', '/');

            _logger.LogInformation("File saved successfully: {RelativePath} (Size: {FileSize} bytes)",
                relativePath, file.Length);

            // Return the path relative to the content root
            return Result.Ok(relativePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving file: {FileName} in category: {Category}", fileName, category);
            return Result.Fail<string>($"Error saving file: {ex.Message}");
        }
    }

    public string GetFullPath(string fileName)
        => Path.Combine(_hostEnvironment.ContentRootPath, fileName.Replace('/', Path.DirectorySeparatorChar));

    private string NormalizedExtension(string extension)
        => StringComparer.OrdinalIgnoreCase.Equals(extension, ".jpeg")
            ? ".jpg"
            : extension.ToLowerInvariant();

    private async Task<Result> ValidateImageAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            _logger.LogWarning("File validation failed: File is null or empty");
            return Result.Fail("Unable to upload file");
        }

        if (file.Length > AttachmentRule.MaxByte)
        {
            _logger.LogWarning("File validation failed: File size {FileSize} exceeds maximum {MaxSize}",
                file.Length, AttachmentRule.MaxByte);
            return Result.Fail("File size exceeded maximum size approved");
        }

        try
        {
            await using var stream = file.OpenReadStream();

            // 1. Wrap the .NET Stream into a Skia managed stream
            using var managedStream = new SKManagedStream(stream);

            // 2. Decode only the metadata (bounds) to save memory and verify the file is a valid image
            using var codec = SKCodec.Create(managedStream);
            if (codec == null)
            {
                _logger.LogWarning("File validation failed: Invalid image format or corrupted file");
                return Result.Fail("Invalid image format or corrupted file");
            }

            // 3. Extract dimensions from the codec info
            int width = codec.Info.Width;
            int height = codec.Info.Height;

            // 4. Validate image dimensions against your rules
            if (width < AttachmentRule.MinWidth || height < AttachmentRule.MinHeight)
            {
                _logger.LogWarning("File validation failed: Image resolution too low ({Width}x{Height})", width, height);
                return Result.Fail("Image resolution is too low");
            }

            if (width > AttachmentRule.MaxWidth || height > AttachmentRule.MaxHeight)
            {
                _logger.LogWarning("File validation failed: Image resolution exceeds maximum ({Width}x{Height})", width, height);
                return Result.Fail("Image resolution exceeds maximum allowed limits");
            }

            _logger.LogInformation("Image validation passed: {Width}x{Height}, Size: {FileSize} bytes",
                width, height, file.Length);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Image validation failed with exception");
            return Result.Fail("Invalid image");
        }
    }

    /// Saves byte content directly to the attachments folder
    /// Used for QR codes and other generated content
    public async Task<Result<string>> SaveBytesAsync(byte[] content, string fileName, string category, CancellationToken ct = default)
    {
        _logger.LogInformation("Saving byte content to category: {Category}, FileName: {FileName}", category, fileName);

        try
        {
            if (content == null || content.Length == 0)
            {
                return Result.Fail<string>("File content cannot be empty");
            }

            if (string.IsNullOrWhiteSpace(fileName))
            {
                return Result.Fail<string>("File name cannot be empty");
            }

            // Ensure the directory exists
            var directory = Path.Combine(_hostEnvironment.ContentRootPath, category);
            Directory.CreateDirectory(directory);

            // Build the full path
            var fullPath = Path.Combine(directory, fileName);

            // Write the bytes to disk
            await File.WriteAllBytesAsync(fullPath, content, ct);

            // Return the relative path
            var relativePath = Path.Combine(category, fileName).Replace('\\', '/');

            _logger.LogInformation("Byte content saved successfully: {RelativePath} (Size: {Size} bytes)",
                relativePath, content.Length);

            return Result.Ok(relativePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving byte content: {FileName} in category: {Category}", fileName, category);
            return Result.Fail<string>($"Error saving file: {ex.Message}");
        }
    }
}
