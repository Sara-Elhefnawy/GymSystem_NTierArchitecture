using GymSystem.Domain.Common;
using GymSystem.Domain.DTOs.Session.Lookups;
using GymSystem.Domain.DTOs.Trainer;
using GymSystem.Domain.Services.Interfaces;
using GymSystem.Infrastructure.Entities;
using GymSystem.Infrastructure.Entities.Enums;
using GymSystem.Infrastructure.UnitOfWorks;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace GymSystem.Domain.Services;

public class TrainerService : ITrainerService
{
    private readonly IUnitOfWork _uow;
    private readonly ILogger<TrainerService> _logger;

    public TrainerService(IUnitOfWork uow, ILogger<TrainerService> logger)
    {
        _uow = uow;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<IndexTrainerDTO>>> GetAllAsync(CancellationToken ct = default)
    {
        try
        {
            var trainers = await _uow.Trainers.GetAllAsync(ct);
            var dtoList = trainers.Select(trainers => 
            {
                return new IndexTrainerDTO
                {
                    Id = trainers.Id,
                    Name = trainers.Name,
                    Email = trainers.Email,
                    Phone = trainers.Phone,
                    Specialties = trainers.Specialty.ToString()
                };
            }).ToList();

        return Result.Ok<IReadOnlyList<IndexTrainerDTO>>(dtoList);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all trainers");
            return Result.Fail<IReadOnlyList<IndexTrainerDTO>>("Failed to retrieve trainers", "DATABASE_ERROR");
        }
    }

    public async Task<Result> CreateAsync(CreateTrainerDTO model, CancellationToken ct = default)
    {
        try
        {
            var email = model.Email.Trim().ToLowerInvariant();
            var phone = model.Phone.Trim().ToLowerInvariant();
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
                return Result.Fail("Invalid blood type value", "INVALID_BLOOD_TYPE");

            var trainer = new Trainer
            {
                Name = name,
                Email = email,
                Phone = phone,
                DateOfBirth = model.DateOfBirth,
                Gender = gender,
                Address = new Address
                {
                    BuildingNumber = model.BuildingNumber,
                    Street = model.Street.Trim(),
                    City = model.City.Trim()
                },
                Specialty = speciality
            };

            await _uow.Trainers.AddAsync(trainer, ct);
            await _uow.Trainers.SaveChangesAsync(ct);

            _logger.LogInformation("Trainer created successfully with ID: {Id}", trainer.Id);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating trainer");
            return Result.Fail("An unexpected error occurred", "INTERNAL_ERROR");
        }
    }

    public async Task<Result<DetailsTrainerDTO>> GetDetailsAsync(int id, CancellationToken ct = default)
    {
        try
        {
            var trainer = await _uow.Trainers.GetByIdAsync(id, ct);

        if (trainer is null)
        {
            _logger.LogWarning("Trainer not found with ID: {Id}", id);
            return Result.Fail<DetailsTrainerDTO>("Trainer not found", "TRAINER_NOT_FOUND");
        }

        var dto = new DetailsTrainerDTO
        {
            Id = trainer.Id,
            Name = trainer.Name,
            Email = trainer.Email,
            Phone = trainer.Phone,
            Address = $"{trainer.Address.BuildingNumber} - {trainer.Address.Street} - {trainer.Address.City}",
            Specialty = trainer.Specialty.ToString()
        };

        return Result.Ok(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting trainer details for ID: {Id}", id);
            return Result.Fail<DetailsTrainerDTO>("Failed to retrieve trainer details", "DATABASE_ERROR");
        }
    }

    public async Task<Result<EditTrainerDTO>> GetForEditAsync(int id, CancellationToken ct = default)
    {
        try
        {
            var trainer = await _uow.Trainers.GetByIdAsync(id, ct);

        if (trainer is null)
        {
            _logger.LogWarning("Trainer not found with ID: {Id}", id);
            return Result.Fail<EditTrainerDTO>("Trainer not found", "TRAINER_NOT_FOUND");
        }

        var dto = new EditTrainerDTO
        {
            Id = trainer.Id,
            Email = trainer.Email,
            Phone = trainer.Phone,
            BuildingNumber = trainer.Address.BuildingNumber,
            City = trainer.Address.City,
            Street = trainer.Address.Street,
            Specialty = trainer.Specialty.ToString()
        };

        return Result.Ok(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting trainer for edit, ID: {Id}", id);
            return Result.Fail<EditTrainerDTO>("Failed to retrieve trainer data", "DATABASE_ERROR");
        }
    }

    public async Task<Result> UpdateAsync(EditTrainerDTO model, CancellationToken ct = default)
    {
        try
        {
            var trainer = await _uow.Trainers.GetByIdAsync(model.Id, ct);

        if (trainer is null)
        {
            _logger.LogWarning("Trainer not found with ID: {Id}", model.Id);
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

        trainer.Email = model.Email.Trim().ToLowerInvariant();
        trainer.Phone = model.Phone.Trim();
        trainer.Address = new Address
        {
            BuildingNumber = model.BuildingNumber,
            Street = model.Street,
            City = model.City
        };

        if (!Enum.TryParse<Specialty>(model.Specialty, true, out var speciality))
        {
            _logger.LogWarning("Invalid speciality value: {Speciality}", model.Specialty);
            return Result.Fail("Invalid speciality value: {Speciality}", model.Specialty);
        }
        trainer.Specialty = speciality;

        _uow.Trainers.Update(trainer, ct);
        await _uow.Trainers.SaveChangesAsync(ct);

        _logger.LogInformation("Trainer updated successfully");
        return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating trainer {Id}", model.Id);
            return Result.Fail("Failed to update trainer", "UPDATE_ERROR");
        }
    }

    public async Task<Result<DeleteTrainerDTO>> GetForDeleteAsync(int id, CancellationToken ct = default)
    {
        try
        {
            var trainer = await _uow.Trainers.GetByIdAsync(id, ct);

        if (trainer is null)
        {
            _logger.LogWarning("Trainer not found with ID: {Id}", id);
            return Result.Fail<DeleteTrainerDTO>("Trainer not found", "TRAINER_NOT_FOUND");
        }

        var dto = new DeleteTrainerDTO
        {
            Id = trainer.Id,
            Name = trainer.Name
        };

        return Result.Ok(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting trainer for delete, ID: {Id}", id);
            return Result.Fail<DeleteTrainerDTO>("Failed to retrieve trainer data", "DATABASE_ERROR");
        }
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken ct = default)
    {
        try
        {
            var trainer = await _uow.Trainers.GetByIdAsync(id, ct);

        if (trainer is null)
        {
            _logger.LogWarning("Trainer not found with ID: {Id}", id);
            return Result.Fail("Trainer not found", "TRAINER_NOT_FOUND");
        }

        if (await _uow.Sessions.HasUpcomingSessionsForTrainerAsync(id, DateTime.UtcNow, ct))
        {
            _logger.LogWarning("Cannot delete trainer with ID: {Id} because they have upcoming sessions", id);
            return Result.Fail("Cannot delete trainer with upcoming sessions", "UPCOMING_SESSIONS_EXIST");
        }

        await _uow.Trainers.SoftDeleteAsync(trainer, ct);
        await _uow.Trainers.SaveChangesAsync(ct);

        _logger.LogInformation("TRAINER {Id} deleted successfully", id);
        return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting trainer {Id}", id);
            return Result.Fail("Failed to delete trainer", "DELETE_ERROR");
        }
    }

    public async Task<Result<bool>> IsEmailTakenAsync(string email, CancellationToken ct = default)
    {
        try
        {
            var normalizedEmail = email.Trim().ToLowerInvariant();
            var isTaken = await _uow.Trainers.IsEmailTakenAsync(normalizedEmail, null, ct);
            return Result.Ok(isTaken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking email: {Email}", email);
            return Result.Fail<bool>("Failed to check email availability", "DATABASE_ERROR");
        }
    }

    public async Task<Result<bool>> IsPhoneTakenAsync(string phone, CancellationToken ct = default)
    {
        try
        {
            var normalizedPhone = phone.Trim();
            var isTaken = await _uow.Trainers.IsPhoneTakenAsync(normalizedPhone, null, ct);
            return Result.Ok(isTaken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking phone: {Phone}", phone);
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

    public async Task<IReadOnlyList<TrainerLookupDTO>> GetTrainerLookupAsync(CancellationToken ct = default)
    {
        var trainers = await _uow.Trainers.GetAllAsync(ct);

        return trainers.Select(t => new TrainerLookupDTO
        {
            Id = t.Id,
            Name = t.Name
        }).ToList();
    }
}
