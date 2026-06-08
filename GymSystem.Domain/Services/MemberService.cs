using GymSystem.Domain.DTOs.HealthRecord;
using GymSystem.Domain.DTOs.Member;
using GymSystem.Infrastructure.Entities;
using GymSystem.Infrastructure.Entities.Enums;
using GymSystem.Infrastructure.UnitOfWorks;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace GymSystem.Domain.Services;

public class MemberService : IMemberService
{
    private readonly IUnitOfWork _uow;
    private readonly ILogger<MemberService> _logger;

    public MemberService(IUnitOfWork uow, ILogger<MemberService> logger)
    {
        _uow = uow;
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

            if (!Enum.TryParse<BloodType>(model.HealthRecord.BloodType, true, out var bloodType))
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
                JoinDate = DateOnly.FromDateTime(DateTime.UtcNow),
                Address = new Address
                {
                    BuildingNumber = model.BuildingNumber,
                    Street = model.Street.Trim(),
                    City = model.City.Trim()
                },
                HealthRecord = new HealthRecord
                {
                    BloodType = bloodType,
                    Weight = model.HealthRecord.Weight,
                    Height = model.HealthRecord.Height,
                    Note = model.HealthRecord.Note?.Trim(),
                    LastUpdate = DateTime.UtcNow
                }
            };

            _logger.LogInformation("Adding member to repository...");
            await _uow.Members.AddAsync(member, ct);

            _logger.LogInformation("Saving changes to database...");
            await _uow.Members.SaveChangesAsync(ct);

            _logger.LogInformation("Member created successfully!");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating member");
            return false;
        }
    }

    public async Task<IEnumerable<IndexMemberDTO>> GetAllAsync(CancellationToken ct = default)
    {
        var items = await _uow.Members.GetAllAsync(ct);
        return items.Select(m => new IndexMemberDTO
        {
            Id = m.Id,
            Name = m.Name,
            Email = m.Email,
            Phone = m.Phone,
            Photo = m.Photo,
            Gender = m.Gender.ToString()
        });
    }

    public async Task<bool> IsEmailTakenAsync(string email, CancellationToken ct = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        return await _uow.Members.IsEmailTakenAsync(normalizedEmail, null, ct);
    }

    public async Task<bool> IsPhoneTakenAsync(string phone, CancellationToken ct = default)
    {
        var normalizedPhone = phone.Trim();
        return await _uow.Members.IsPhoneTakenAsync(normalizedPhone, null, ct);
    }

    public async Task<DetailsMemberDTO?> GetDetailsAsync(int id, CancellationToken ct = default)
    {
        var member = await _uow.Members.GetWithDetailsAsync(id, ct);

        if (member is null)
        {
            _logger.LogWarning("Member not found with ID: {Id}", id);
            return null;
        }

        var activeMembership = member.Memberships
            .OrderByDescending(m => m.StartDate)
            .FirstOrDefault();

        return new DetailsMemberDTO
        {
            Id = member.Id,
            Name = member.Name,
            Photo = member.Photo,
            Email = member.Email,
            Phone = member.Phone,
            Gender = member.Gender.ToString(),
            DateOfBirth = member.DateOfBirth,
            Address = $"{member.Address.BuildingNumber} - {member.Address.Street} - {member.Address.City}",
            PlanName = activeMembership?.Plan?.Name ?? "—",
            MembershipStartDate = activeMembership?.StartDate,
            MembershipEndDate = activeMembership?.EndDate
        };

    }

    public async Task<DetailsHealthRecordDTO?> GetHealthRecordAsync(int id, CancellationToken ct = default)
    {
        var member = await _uow.Members.GetWithHealthRecordAsync(id, trackChanges: false, ct: ct);

        if (member?.HealthRecord is null)
            return null;

        var healthRecord = member.HealthRecord;
        return new DetailsHealthRecordDTO
        {
            Height = healthRecord.Height,
            Weight = healthRecord.Weight,
            BloodType = healthRecord.BloodType.ToString(),
            Notes = string.IsNullOrWhiteSpace(healthRecord.Note) ? "No notes available" : healthRecord.Note
        };
    }

    public async Task<EditMemberDTO?> GetForEditAsync(int id, CancellationToken ct = default)
    {
        var member = await _uow.Members.GetByIdAsync(id);
        if (member is null) return null;

        return new EditMemberDTO
        {
            Id = member.Id,
            Name = member.Name,
            Photo = member.Photo,
            Email = member.Email,
            Phone = member.Phone,
            BuildingNumber = member.Address.BuildingNumber,
            Street = member.Address.Street,
            City = member.Address.City
        };
    }

    public async Task<bool> UpdateAsync(EditMemberDTO dto, CancellationToken ct = default)
    {
        var member = await _uow.Members.GetByIdAsync(dto.Id, ct);
        if (member is null) return false;

        if (member.Email != dto.Email.Trim().ToLowerInvariant() && await IsEmailTakenAsync(dto.Email, ct))
        {
            throw new InvalidOperationException("Email is already taken.");
        }

        if (member.Phone != dto.Phone.Trim() && await IsPhoneTakenAsync(dto.Phone, ct))
        {
            throw new InvalidOperationException("Phone number is already taken.");
        }

        member.Email = dto.Email;
        member.Phone = dto.Phone;
        member.Address = new Address
        {
            BuildingNumber = dto.BuildingNumber,
            Street = dto.Street,
            City = dto.City
        };

        _uow.Members.Update(member, ct);
        await _uow.Members.SaveChangesAsync(ct);

        return true;
    }

    public async Task<DeleteMemberDTO?> GetForDeleteAsync(int id, CancellationToken ct = default)
    {
        var member = await _uow.Members.GetByIdAsync(id, ct);

        if (member is null) return null;

        return new DeleteMemberDTO 
        { 
            Id = member.Id, 
            Name = member.Name, 
            Photo = member.Photo 
        };
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var member = await _uow.Members.GetWithBookingsAsync(id, ct);
        if (member is null) return false;

        // Business rule: cannot delete if member has active bookings
        //if (member.Bookings.Any(b => b.IsActive)) return false;

        await _uow.Members.SoftDeleteAsync(member, ct);
        if (member.HealthRecord is not null)
            await _uow.HealthRecords.SoftDeleteAsync(member.HealthRecord, ct);


        await _uow.Members.SaveChangesAsync(ct);
        return true;
    }

    private int CalculateAge(DateOnly dateOfBirth)
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        var age = today.Year - dateOfBirth.Year;
        if (dateOfBirth > today.AddYears(-age)) age--;
        return age;
    }
}