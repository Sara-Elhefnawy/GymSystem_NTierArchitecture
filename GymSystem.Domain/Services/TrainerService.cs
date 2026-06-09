using GymSystem.Domain.DTOs.Trainer;
using GymSystem.Infrastructure.Entities;
using GymSystem.Infrastructure.Entities.Enums;
using GymSystem.Infrastructure.UnitOfWorks;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace GymSystem.Domain.Services;

public class TrainerService : ITrainerService
{
    private readonly IUnitOfWork _uow;
    private readonly ILogger<MemberService> _logger;

    public TrainerService(IUnitOfWork uow, ILogger<MemberService> logger)
    {
        _uow = uow;
        _logger = logger;
    }

    public async Task<IReadOnlyList<IndexTrainerDTO>> GetAllAsync(CancellationToken ct = default)
    {
        var trainers = await _uow.Trainers.GetAllAsync(ct);

        var dtoList = trainers.Select(trainers => {
            return new IndexTrainerDTO
            {
                Id = trainers.Id,
                Name = trainers.Name,
                Email = trainers.Email,
                Phone = trainers.Phone,
                Specialties = trainers.Specialty.ToString()
            };
        }).ToList();

        return dtoList;
    }

    public async Task<bool> CreateAsync(CreateTrainerDTO model, CancellationToken ct = default)
    {
        try
        {
            var email = model.Email.Trim().ToLowerInvariant();
            var phone = model.Phone.Trim().ToLowerInvariant();
            var name = model.Name.Trim();

            _logger.LogInformation("Creating member: {Name}, {Email}, {Phone}", name, email, phone);

            var age = CalculateAge(model.DateOfBirth);
            if (age < 12 || age > 120)
                return false;

            if (!Regex.IsMatch(model.Name, @"^[a-zA-Z\s\-']+$"))
                return false;

            if (await IsEmailTakenAsync(model.Email, ct))
                return false;

            if (await IsPhoneTakenAsync(model.Phone, ct))
                return false;

            if (!Enum.TryParse<Gender>(model.Gender, true, out var gender))
            {
                _logger.LogWarning("Invalid gender value: {Gender}", model.Gender);
                return false;
            }

            if (!Enum.TryParse<BloodType>(model.Specialties, true, out var speciality))
            {
                _logger.LogWarning("Invalid speciality value: {Specialities}", model.Specialties);
                return false;
            }

            _logger.LogInformation("Creating trainer object...");

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
                
            };

            _logger.LogInformation("Adding member to repository...");
            await _uow.Trainers.AddAsync(trainer, ct);

            _logger.LogInformation("Saving changes to database...");
            await _uow.Trainers.SaveChangesAsync(ct);

            _logger.LogInformation("Trainer created successfully!");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating member");
            return false;
        }
    }


    public async Task<bool> IsEmailTakenAsync(string email, CancellationToken ct = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        return await _uow.Trainers.IsEmailTakenAsync(normalizedEmail, null, ct);
    }

    public async Task<bool> IsPhoneTakenAsync(string phone, CancellationToken ct = default)
    {
        var normalizedPhone = phone.Trim();
        return await _uow.Trainers.IsPhoneTakenAsync(normalizedPhone, null, ct);
    }

    private int CalculateAge(DateOnly dateOfBirth)
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        var age = today.Year - dateOfBirth.Year;
        if (dateOfBirth > today.AddYears(-age)) age--;
        return age;
    }
}
