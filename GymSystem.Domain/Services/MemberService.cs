using GymSystem.Domain.DTOs.Member;
using GymSystem.Infrastructure.Entities;
using GymSystem.Infrastructure.Entities.Enums;
using GymSystem.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;

namespace GymSystem.Domain.Services;

public class MemberService : IMemberService
{
    private readonly IMemberRepository _members;
    private readonly ILogger<MemberService> _logger;

    public MemberService(IMemberRepository members, ILogger<MemberService> logger)
    {
        _members = members;
        _logger = logger;
    }

    public async Task<bool> CreateAsync(CreateMemberDTO model, CancellationToken ct = default)
    {
        try
        {
            var email = model.Email.Trim().ToLowerInvariant();
            var phone = model.Phone.Trim().ToLowerInvariant();
            var name = model.Name.Trim();

            _logger.LogInformation("Creating member: {Name}, {Email}, {Phone}", name, email, phone);

            // Check for existing email
            if (await _members.ExistsAsync(m => m.Email == email, ct))
            {
                _logger.LogWarning("Email already exists: {Email}", email);
                return false;
            }

            // Check for existing phone
            if (await _members.ExistsAsync(m => m.Phone == phone, ct))
            {
                _logger.LogWarning("Phone already exists: {Phone}", phone);
                return false;
            }

            // Parse gender (string from UI to enum)
            if (!Enum.TryParse<Gender>(model.Gender, true, out var gender))
            {
                _logger.LogWarning("Invalid gender value: {Gender}", model.Gender);
                return false;
            }

            // Parse blood type (string from UI to enum)
            var bloodType = ParseBloodType(model.HealthRecord.BloodType);
            if (bloodType == null)
            {
                _logger.LogWarning("Invalid blood type value: {BloodType}", model.HealthRecord.BloodType);
                return false;
            }

            _logger.LogInformation("Creating member object...");

            var member = new Member
            {
                Name = name,
                Email = email,
                Phone = phone,
                DateOfBirth = model.DateOfBirth,
                Gender = gender,
                JoinDate = DateTime.UtcNow,
                Address = new Address
                {
                    BuildingNumber = model.BuildingNumber,
                    Street = model.Street.Trim(),
                    City = model.City.Trim()
                },
                HealthRecord = new HealthRecord
                {
                    BloodType = bloodType.Value,
                    Weight = model.HealthRecord.Weight,
                    Height = model.HealthRecord.Height,
                    Note = model.HealthRecord.Note?.Trim(),
                    LastUpdate = DateTime.UtcNow
                }
            };

            _logger.LogInformation("Adding member to repository...");
            await _members.AddAsync(member, ct);

            _logger.LogInformation("Saving changes to database...");
            await _members.SaveChangesAsync(ct);

            _logger.LogInformation("Member created successfully!");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating member");
            return false;
        }
    }

    public async Task<IEnumerable<MemberIndexDTO>> GetAllAsync(CancellationToken ct = default)
    {
        var items = await _members.GetAllAsync(ct);
        return items.Select(m => new MemberIndexDTO
        {
            Id = m.Id,
            Name = m.Name,
            Email = m.Email,
            Phone = m.Phone,
            Photo = m.Photo,
            Gender = m.Gender.ToString()
        });
    }

    private BloodType? ParseBloodType(string bloodTypeString)
    {
        // Try to parse as integer first (from dropdown)
        if (int.TryParse(bloodTypeString, out int bloodTypeInt))
        {
            return bloodTypeInt switch
            {
                1 => BloodType.A_Positive,
                2 => BloodType.A_Negative,
                3 => BloodType.B_Positive,
                4 => BloodType.B_Negative,
                5 => BloodType.AB_Positive,
                6 => BloodType.AB_Negative,
                7 => BloodType.O_Positive,
                8 => BloodType.O_Negative,
                _ => null
            };
        }

        // Then try string values
        return bloodTypeString switch
        {
            "A+" => BloodType.A_Positive,
            "A-" => BloodType.A_Negative,
            "B+" => BloodType.B_Positive,
            "B-" => BloodType.B_Negative,
            "AB+" => BloodType.AB_Positive,
            "AB-" => BloodType.AB_Negative,
            "O+" => BloodType.O_Positive,
            "O-" => BloodType.O_Negative,
            _ => null
        };
    }

}