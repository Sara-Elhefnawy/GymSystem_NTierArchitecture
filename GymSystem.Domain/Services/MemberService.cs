using GymSystem.Domain.Abstractions.Attachments;
using GymSystem.Domain.Abstractions.QrService;
using GymSystem.Domain.Abstractions.Services;
using GymSystem.Domain.Abstractions.UnitOfWorks;
using GymSystem.Domain.Attachments;
using GymSystem.Domain.DTOs.HealthRecord;
using GymSystem.Domain.DTOs.Member;
using GymSystem.Domain.Entities;
using GymSystem.Domain.Entities.Enums;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using GymSystem.Domain.Common;
using Mapster;

namespace GymSystem.Domain.Services;

public class MemberService(
    IUnitOfWork uow,
    ILogger<MemberService> logger,
    IAttachmentService attachmentService,
    IQrService qrService) : IMemberService
{
    public async Task<Result<IReadOnlyList<IndexMemberDTO>>> GetAllAsync(CancellationToken ct = default)
    {
        try
        {
            var members = await uow.Members.GetAllAsync(ct);
            var dtos = members.Adapt<IReadOnlyList<IndexMemberDTO>>();

            return Result.Ok(dtos);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting all members");
            return Result.Fail<IReadOnlyList<IndexMemberDTO>>("Failed to retrieve members", "DATABASE_ERROR");
        }
    }

    public async Task<Result<IReadOnlyList<IndexMemberDTO>>> GetMembersWithActiveMembershipAsync(CancellationToken ct = default)
    {
        try
        {
            logger.LogInformation("Getting members with active memberships");

            var members = await uow.Members.GetMembersWithActiveMembershipAsync(ct);

            var dtos = members.Adapt<IReadOnlyList<IndexMemberDTO>>();

            logger.LogInformation("Retrieved {Count} members with active memberships", dtos.Count());

            return Result.Ok<IReadOnlyList<IndexMemberDTO>>(dtos);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting members with active memberships");
            return Result.Fail<IReadOnlyList<IndexMemberDTO>>("Failed to retrieve members", "DATABASE_ERROR");
        }
    }

    public async Task<Result> CreateAsync(CreateMemberDTO model, CancellationToken ct = default)
    {
        string? uploadedPhotoPath = null;

        try
        {
            var email = model.Email.Trim().ToLowerInvariant();
            var phone = model.Phone.Trim();
            var name = model.Name.Trim();

            var age = CalculateAge(model.DateOfBirth);
            if (age < 12 || age > 120)
                return Result.Fail("Age must be between 12 and 120", "INVALID_AGE");

            if (!Regex.IsMatch(model.Name, @"^[a-zA-Z\s\-']+$"))
                return Result.Fail("Name contains invalid characters", "INVALID_NAME");

            var emailCheck = await IsEmailTakenAsync(model.Email, ct);
            if (emailCheck.IsSuccess && emailCheck.Value)
                return Result.Fail("Email is already taken", "EMAIL_TAKEN");

            var phoneCheck = await IsPhoneTakenAsync(model.Phone, ct);
            if (phoneCheck.IsSuccess && phoneCheck.Value)
                return Result.Fail("Phone number is already taken", "PHONE_TAKEN");

            if (!Enum.TryParse<Gender>(model.Gender, true, out var gender))
                return Result.Fail("Invalid gender value", "INVALID_GENDER");

            if (!Enum.TryParse<BloodType>(model.HealthRecord.BloodType, true, out var bloodType))
                return Result.Fail("Invalid blood type value", "INVALID_BLOOD_TYPE");

            var member = model.Adapt<Member>();
            member.Address = new Address
            {
                BuildingNumber = model.BuildingNumber,
                Street = model.Street,
                City = model.City
            };

            if (model.Photo is { Length: > 0 })
            {
                logger.LogInformation("Processing photo upload for member. File: {FileName}, Size: {FileSize} bytes",
                    model.Photo.FileName, model.Photo.Length);

                try
                {
                    var saveResult = await attachmentService.SaveAsync(model.Photo, AttachmentsCategories.Members, ct);

                    if (saveResult.IsFailure)
                    {
                        logger.LogWarning("Failed to save photo: {Error}", saveResult.Error);
                        return Result.Fail("Error saving image");
                    }

                    uploadedPhotoPath = saveResult.Value;
                    member.Photo = saveResult.Value;
                    logger.LogInformation("Photo saved successfully: {PhotoPath}", member.Photo);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Exception during photo upload");
                    return Result.Fail("Error saving image");
                }
            }
            else
            {
                logger.LogInformation("No photo provided for member");
            }

            await uow.Members.AddAsync(member, ct);
            await uow.Members.SaveChangesAsync(ct);

            logger.LogInformation("Member created successfully with ID: {Id}", member.Id);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating member");

            // Clean up uploaded photo if member creation failed
            if (!string.IsNullOrEmpty(uploadedPhotoPath))
            {
                logger.LogWarning("Member creation failed. Cleaning up uploaded photo: {PhotoPath}", uploadedPhotoPath);
                await attachmentService.DeleteAsync(uploadedPhotoPath, ct);
            }

            return Result.Fail("An unexpected error occurred", "INTERNAL_ERROR");
        }
    }

    public async Task<Result<bool>> IsEmailTakenAsync(string email, CancellationToken ct = default)
    {
        try
        {
            var normalizedEmail = email.Trim().ToLowerInvariant();
            var isTaken = await uow.Members.IsEmailTakenAsync(normalizedEmail, null, ct);
            return Result.Ok(isTaken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking email: {Email}", email);
            return Result.Fail<bool>("Failed to check email availability", "DATABASE_ERROR");
        }
    }

    public async Task<Result<bool>> IsPhoneTakenAsync(string phone, CancellationToken ct = default)
    {
        try
        {
            var normalizedPhone = phone.Trim();
            var isTaken = await uow.Members.IsPhoneTakenAsync(normalizedPhone, null, ct);
            return Result.Ok(isTaken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking phone: {Phone}", phone);
            return Result.Fail<bool>("Failed to check phone availability", "DATABASE_ERROR");
        }
    }

    public async Task<Result<DetailsMemberDTO>> GetDetailsAsync(int id, CancellationToken ct = default)
    {
        try
        {
            var member = await uow.Members.GetWithMembershipDetailsAsync(id, ct);

            if (member is null)
            {
                logger.LogWarning("Member not found with ID: {Id}", id);
                return Result.Fail<DetailsMemberDTO>("Member not found", "MEMBER_NOT_FOUND");
            }

            var dto = member.Adapt<DetailsMemberDTO>();
            dto.Address = $"{member.Address.BuildingNumber} - {member.Address.Street} - {member.Address.City}";
            var latestMembership = member.Memberships?
                .OrderByDescending(m => m.StartDate)
                .FirstOrDefault();
            if (latestMembership != null)
            {
                dto.PlanName = latestMembership.Plan?.Name ?? "No Plan";
                dto.MembershipStartDate = latestMembership.StartDate;
                dto.MembershipEndDate = latestMembership.EndDate;
            }
            else
            {
                dto.PlanName = "No Plan";
                dto.MembershipStartDate = null;
                dto.MembershipEndDate = null;
            }

            return Result.Ok(dto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting member details for ID: {Id}", id);
            return Result.Fail<DetailsMemberDTO>("Failed to retrieve member details", "DATABASE_ERROR");
        }
    }

    public async Task<Result<DetailsHealthRecordDTO>> GetHealthRecordAsync(int id, CancellationToken ct = default)
    {
        try
        {
            var member = await uow.Members.GetWithHealthRecordAsync(id, trackChanges: false, ct: ct);

            if (member is null)
            {
                logger.LogWarning("Member not found with ID: {Id}", id);
                return Result.Fail<DetailsHealthRecordDTO>("Member not found", "MEMBER_NOT_FOUND");
            }

            if (member.HealthRecord is null)
            {
                logger.LogWarning("Health record not found for member ID: {Id}", id);
                return Result.Fail<DetailsHealthRecordDTO>("Health record not found", "HEALTH_RECORD_NOT_FOUND");
            }

            var dto = member.HealthRecord.Adapt<DetailsHealthRecordDTO>();

            logger.LogInformation("Health record retrieved for member {Id}: BloodType={BloodType}, Weight={Weight}, Height={Height}",
                id, dto.BloodType, dto.Weight, dto.Height);

            return Result.Ok(dto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting health record for member ID: {Id}", id);
            return Result.Fail<DetailsHealthRecordDTO>("Failed to retrieve health record", "DATABASE_ERROR");
        }
    }

    public async Task<Result<EditMemberDTO>> GetForEditAsync(int id, CancellationToken ct = default)
    {
        try
        {
            var member = await uow.Members.GetByIdAsync(id, ct);
            if (member is null)
            {
                logger.LogWarning("Member not found with ID: {Id}", id);
                return Result.Fail<EditMemberDTO>("Member not found", "MEMBER_NOT_FOUND");
            }

            var dto = member.Adapt<EditMemberDTO>();
            dto.BuildingNumber = member.Address.BuildingNumber;
            dto.Street = member.Address.Street;
            dto.City = member.Address.City;

            return Result.Ok(dto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting member for edit, ID: {Id}", id);
            return Result.Fail<EditMemberDTO>("Failed to retrieve member data", "DATABASE_ERROR");
        }
    }

    public async Task<Result> UpdateAsync(EditMemberDTO dto, CancellationToken ct = default)
    {
        try
        {
            var member = await uow.Members.GetByIdAsync(dto.Id, ct);
            if (member is null)
            {
                logger.LogWarning("Member not found with ID: {Id}", dto.Id);
                return Result.Fail("Member not found", "MEMBER_NOT_FOUND");
            }

            // Only check email if it has changed
            if (member.Email != dto.Email.Trim().ToLowerInvariant())
            {
                var emailCheck = await IsEmailTakenAsync(dto.Email, ct);
                if (emailCheck.IsSuccess && emailCheck.Value)
                    return Result.Fail("Email is already taken", "EMAIL_TAKEN");
            }

            // Only check phone if it has changed
            if (member.Phone != dto.Phone.Trim())
            {
                var phoneCheck = await IsPhoneTakenAsync(dto.Phone, ct);
                if (phoneCheck.IsSuccess && phoneCheck.Value)
                    return Result.Fail("Phone number is already taken", "PHONE_TAKEN");
            }

            TypeAdapter.Adapt(dto, member);
            member.Address.BuildingNumber = dto.BuildingNumber;
            member.Address.Street = dto.Street;
            member.Address.City = dto.City;

            uow.Members.Update(member, ct);
            await uow.Members.SaveChangesAsync(ct);

            logger.LogInformation("Member updated successfully");
            return Result.Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating member {Id}", dto.Id);
            return Result.Fail("Failed to update member", "UPDATE_ERROR");
        }
    }

    public async Task<Result<DeleteMemberDTO>> GetForDeleteAsync(int id, CancellationToken ct = default)
    {
        try
        {
            var member = await uow.Members.GetByIdAsync(id, ct);

            if (member is null)
            {
                logger.LogWarning("Member not found with ID: {Id}", id);
                return Result.Fail<DeleteMemberDTO>("Member not found", "MEMBER_NOT_FOUND");
            }

            var dto = member.Adapt<DeleteMemberDTO>();

            return Result.Ok(dto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting member for delete, ID: {Id}", id);
            return Result.Fail<DeleteMemberDTO>("Failed to retrieve member data", "DATABASE_ERROR");
        }
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken ct = default)
    {
        try
        {
            var member = await uow.Members.GetByIdAsync(id, ct);

            if (member is null)
            {
                logger.LogWarning("Member not found with ID: {Id}", id);
                return Result.Fail<DeleteMemberDTO>("Member not found", "MEMBER_NOT_FOUND");
            }

            // Business rule: cannot delete if member has active bookings
            var hasActiveBookings = await uow.Bookings.GetWithMemberDetailsAsync(id, DateTime.Now, ct);

            if (hasActiveBookings)
                return Result.Fail("Cannot delete member with active bookings", "ACTIVE_BOOKINGS_EXIST");

            // Delete the photo if it exists
            if (!string.IsNullOrEmpty(member.Photo))
            {
                logger.LogInformation("Deleting photo for member {Id}: {Photo}", id, member.Photo);
                var deleteResult = await attachmentService.DeleteAsync(member.Photo, ct);
                if (deleteResult.IsFailure)
                {
                    logger.LogWarning("Failed to delete photo for member {Id}: {Error}", id, deleteResult.Error);
                }
            }

            // Delete the QR code using the naming convention
            try
            {
                var qrDeleteResult = await qrService.DeleteMemberQrCodeAsync(id, ct);
                if (qrDeleteResult.IsFailure && qrDeleteResult.Error != "QR code not found")
                {
                    logger.LogWarning("Failed to delete QR code for member {Id}: {Error}", id, qrDeleteResult.Error);
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Error deleting QR code for member {Id}", id);
            }

            await uow.Members.SoftDeleteAsync(member, ct);

            if (member.HealthRecord is not null)
                await uow.HealthRecords.SoftDeleteAsync(member.HealthRecord, ct);

            await uow.Members.SaveChangesAsync(ct);

            logger.LogInformation("Member {Id} deleted successfully", id);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting member {Id}", id);
            return Result.Fail("Failed to delete member", "DELETE_ERROR");
        }
    }

    public async Task<Result<byte[]>> GetMemberPhotoAsync(int id, CancellationToken ct = default)
    {
        try
        {
            var member = await uow.Members.GetByIdAsync(id, ct);
            if (member is null)
            {
                logger.LogWarning("Member not found with ID: {Id}", id);
                return Result.Fail<byte[]>("Member not found", "MEMBER_NOT_FOUND");
            }

            if (string.IsNullOrEmpty(member.Photo))
            {
                logger.LogInformation("Member {Id} has no photo", id);
                return Result.Fail<byte[]>("No photo available", "PHOTO_NOT_FOUND");
            }

            var fullPath = attachmentService.GetFullPath(member.Photo);
            if (!File.Exists(fullPath))
            {
                logger.LogWarning("Photo file not found: {FullPath}", fullPath);
                return Result.Fail<byte[]>("Photo file not found", "PHOTO_NOT_FOUND");
            }

            // Read bytes directly - file is closed immediately after reading
            var bytes = await File.ReadAllBytesAsync(fullPath, ct);
            return Result.Ok(bytes);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting photo for member {Id}", id);
            return Result.Fail<byte[]>("Failed to retrieve photo", "DATABASE_ERROR");
        }
    }

    private int CalculateAge(DateOnly dateOfBirth)
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        var age = today.Year - dateOfBirth.Year;
        if (dateOfBirth > today.AddYears(-age)) age--;
        return age;
    }
}
