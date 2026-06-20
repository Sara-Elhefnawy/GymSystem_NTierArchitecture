using AutoMapper;
using GymSystem.Domain.Abstractions.Services;
using GymSystem.Domain.Abstractions.UnitOfWorks;
using GymSystem.Domain.DTOs.Trainer;
using GymSystem.Domain.Entities;
using GymSystem.Domain.Entities.Enums;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using GymSystem.Domain.Common;

namespace GymSystem.Domain.Services;

public class TrainerService(
    IUnitOfWork uow,
    ILogger<TrainerService> logger,
    IMapper mapper) : ITrainerService
{
    public async Task<Result<IReadOnlyList<IndexTrainerDTO>>> GetAllAsync(CancellationToken ct = default)
    {
        try
        {
            var trainers = await uow.Trainers.GetAllAsync(ct);

            var dtoList = mapper.Map<IReadOnlyList<IndexTrainerDTO>>(trainers);

            return Result.Ok(dtoList);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting all trainers");
            return Result.Fail<IReadOnlyList<IndexTrainerDTO>>("Failed to retrieve trainers", "DATABASE_ERROR");
        }
    }

    public async Task<Result> CreateAsync(CreateTrainerDTO model, CancellationToken ct = default)
    {
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

            if (!Enum.TryParse<Specialty>(model.Specialties, true, out var speciality))
                return Result.Fail("Invalid specialty value", "INVALID_SPECIALTY");

            var trainer = mapper.Map<Trainer>(model);

            await uow.Trainers.AddAsync(trainer, ct);
            await uow.Trainers.SaveChangesAsync(ct);

            logger.LogInformation("Trainer created successfully with ID: {Id}", trainer.Id);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating trainer");
            return Result.Fail("An unexpected error occurred", "INTERNAL_ERROR");
        }
    }

    public async Task<Result<DetailsTrainerDTO>> GetDetailsAsync(int id, CancellationToken ct = default)
    {
        try
        {
            var trainer = await uow.Trainers.GetByIdAsync(id, ct);

            if (trainer is null)
            {
                logger.LogWarning("Trainer not found with ID: {Id}", id);
                return Result.Fail<DetailsTrainerDTO>("Trainer not found", "TRAINER_NOT_FOUND");
            }

            var dto = mapper.Map<DetailsTrainerDTO>(trainer);

            return Result.Ok(dto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting trainer details for ID: {Id}", id);
            return Result.Fail<DetailsTrainerDTO>("Failed to retrieve trainer details", "DATABASE_ERROR");
        }
    }

    public async Task<Result<EditTrainerDTO>> GetForEditAsync(int id, CancellationToken ct = default)
    {
        try
        {
            var trainer = await uow.Trainers.GetByIdAsync(id, ct);

            if (trainer is null)
            {
                logger.LogWarning("Trainer not found with ID: {Id}", id);
                return Result.Fail<EditTrainerDTO>("Trainer not found", "TRAINER_NOT_FOUND");
            }

            var dto = mapper.Map<EditTrainerDTO>(trainer);

            return Result.Ok(dto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting trainer for edit, ID: {Id}", id);
            return Result.Fail<EditTrainerDTO>("Failed to retrieve trainer data", "DATABASE_ERROR");
        }
    }

    public async Task<Result> UpdateAsync(EditTrainerDTO model, CancellationToken ct = default)
    {
        try
        {
            var trainer = await uow.Trainers.GetByIdAsync(model.Id, ct);

            if (trainer is null)
            {
                logger.LogWarning("Trainer not found with ID: {Id}", model.Id);
                return Result.Fail("Trainer not found", "TRAINER_NOT_FOUND");
            }

            // Only check email if it has changed
            if (trainer.Email != model.Email.Trim().ToLowerInvariant())
            {
                var emailCheck = await IsEmailTakenAsync(model.Email, ct);
                if (emailCheck.IsSuccess && emailCheck.Value)
                    return Result.Fail("Email is already taken", "EMAIL_TAKEN");
            }

            // Only check phone if it has changed
            if (trainer.Phone != model.Phone.Trim())
            {
                var phoneCheck = await IsPhoneTakenAsync(model.Phone, ct);
                if (phoneCheck.IsSuccess && phoneCheck.Value)
                    return Result.Fail("Phone number is already taken", "PHONE_TAKEN");
            }

            mapper.Map(model, trainer);

            uow.Trainers.Update(trainer, ct);
            await uow.Trainers.SaveChangesAsync(ct);

            logger.LogInformation("Trainer updated successfully");
            return Result.Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating trainer {Id}", model.Id);
            return Result.Fail("Failed to update trainer", "UPDATE_ERROR");
        }
    }

    public async Task<Result<DeleteTrainerDTO>> GetForDeleteAsync(int id, CancellationToken ct = default)
    {
        try
        {
            var trainer = await uow.Trainers.GetByIdAsync(id, ct);

            if (trainer is null)
            {
                logger.LogWarning("Trainer not found with ID: {Id}", id);
                return Result.Fail<DeleteTrainerDTO>("Trainer not found", "TRAINER_NOT_FOUND");
            }

            var dto = mapper.Map<DeleteTrainerDTO>(trainer);

            return Result.Ok(dto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting trainer for delete, ID: {Id}", id);
            return Result.Fail<DeleteTrainerDTO>("Failed to retrieve trainer data", "DATABASE_ERROR");
        }
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken ct = default)
    {
        try
        {
            var trainer = await uow.Trainers.GetByIdAsync(id, ct);

            if (trainer is null)
            {
                logger.LogWarning("Trainer not found with ID: {Id}", id);
                return Result.Fail("Trainer not found", "TRAINER_NOT_FOUND");
            }

            if (await uow.Sessions.HasUpcomingSessionsForTrainerAsync(id, DateTime.Now, ct))
            {
                logger.LogWarning("Cannot delete trainer with ID: {Id} because they have upcoming sessions", id);
                return Result.Fail("Cannot delete trainer with upcoming sessions", "UPCOMING_SESSIONS_EXIST");
            }

            await uow.Trainers.SoftDeleteAsync(trainer, ct);
            await uow.Trainers.SaveChangesAsync(ct);

            logger.LogInformation("TRAINER {Id} deleted successfully", id);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting trainer {Id}", id);
            return Result.Fail("Failed to delete trainer", "DELETE_ERROR");
        }
    }

    public async Task<Result<bool>> IsEmailTakenAsync(string email, CancellationToken ct = default)
    {
        try
        {
            var normalizedEmail = email.Trim().ToLowerInvariant();
            var isTaken = await uow.Trainers.IsEmailTakenAsync(normalizedEmail, null, ct);
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
            var isTaken = await uow.Trainers.IsPhoneTakenAsync(normalizedPhone, null, ct);
            return Result.Ok(isTaken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking phone: {Phone}", phone);
            return Result.Fail<bool>("Failed to check phone availability", "DATABASE_ERROR");
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
