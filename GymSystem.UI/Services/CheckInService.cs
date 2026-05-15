using GymSystem.Domain.DTOs;
using GymSystem.Domain.Interfaces;
using GymSystem.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace GymSystem.UI.Services;

public class CheckInService
{
    private readonly IAppDbContext _context;

    public CheckInService(IAppDbContext context)
    {
        _context = context;
    }

    // Record a member check-in
    public async Task<(bool Success, string Message)> RecordCheckInAsync(int memberId)
    {
        // Check if member exists and is not deleted
        var member = await _context.Members
            .FirstOrDefaultAsync(m => m.Id == memberId && !m.IsDeleted);

        if (member == null)
        {
            return (false, "Member not found");
        }

        // Check if member already checked in today
        var today = DateTime.Today;
        var alreadyCheckedIn = await _context.CheckIns
            .AnyAsync(c => c.MemberId == memberId && c.CheckInTime >= today);

        if (alreadyCheckedIn)
        {
            return (false, $"Member {member.Name} already checked in today");
        }

        // Record check-in
        var checkIn = new CheckIn
        {
            MemberId = memberId,
            CheckInTime = DateTime.Now
        };

        await _context.CheckIns.AddAsync(checkIn);
        await _context.SaveChangesAsync();

        return (true, $"Welcome {member.Name}! Check-in successful at {checkIn.CheckInTime:t}");
    }

    // Get today's all check-ins
    public async Task<List<CheckInDto>> GetTodayCheckInsAsync()
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);

        var checkIns = await _context.CheckIns
            .Include(c => c.Member)
            .Where(c => c.CheckInTime >= today && c.CheckInTime < tomorrow)
            .OrderByDescending(c => c.CheckInTime)
            .Select(c => new CheckInDto
            {
                Id = c.Id,
                MemberId = c.MemberId,
                MemberName = c.Member != null ? c.Member.Name : "Unknown",
                CheckInTime = c.CheckInTime
            })
            .ToListAsync();

        return checkIns;
    }

    // Get today's check-in count
    public async Task<int> GetTodayCheckInCountAsync()
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);

        return await _context.CheckIns
            .CountAsync(c => c.CheckInTime >= today && c.CheckInTime < tomorrow);
    }
}