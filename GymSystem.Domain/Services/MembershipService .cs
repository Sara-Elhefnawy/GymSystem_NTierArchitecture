using GymSystem.Domain.DTOs.Memberships;
using GymSystem.Domain.Services.Interfaces;
using GymSystem.Infrastructure.Entities;
using GymSystem.Infrastructure.UnitOfWorks;
using GymSystem.Shared.Common;
using Microsoft.Extensions.Logging;

namespace GymSystem.Domain.Services;

public class MembershipService(IUnitOfWork uow, ILogger<MembershipService> logger) : IMembershipService
{
    private readonly IUnitOfWork _uow = uow;
    private readonly ILogger<MembershipService> _logger = logger;

    public async Task<Result<IEnumerable<IndexMembershipDTO>>> GetActiveMembershipsAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Getting all active memberships");

            var memberships = await uow.Memberships.GetActiveMembershipsAsync(ct);

            var viewModels = memberships.Select(m => new IndexMembershipDTO
            {
                MemberId = m.MemberId,
                MemberName = m.Member?.Name ?? "Unknown",
                Photo = m.Member?.Photo,
                PlanId = m.PlanId,
                PlanName = m.Plan?.Name ?? "Unknown",
                StartDate = m.StartDate,
                EndDate = m.EndDate
            });

            _logger.LogInformation("Retrieved {Count} active memberships", viewModels.Count());

            return Result<IEnumerable<IndexMembershipDTO>>.Ok(viewModels);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active memberships");
            return Result.Fail<IEnumerable<IndexMembershipDTO>>("Failed to retrieve memberships");
        }
    }

    public async Task<Result> CreateMembershipAsync(CreateMembershipDTO model, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Creating membership for member {MemberId} with plan {PlanId}",
                model.MemberId, model.PlanId);

            // Validate member exists
            var member = await uow.Members.GetByIdAsync(model.MemberId, ct);
            if (member == null)
            {
                _logger.LogWarning("Member {MemberId} not found", model.MemberId);
                return Result.Fail("Member not found");
            }

            // Validate plan exists
            var plan = await uow.Plans.GetByIdAsync(model.PlanId, ct);
            if (plan == null)
            {
                _logger.LogWarning("Plan {PlanId} not found", model.PlanId);
                return Result.Fail("Plan not found");
            }

            // Check if member already has active membership
            if (await uow.Memberships.IsMemberAlreadyHasActivePlanAsync(model.MemberId, ct))
            {
                _logger.LogWarning("Member {MemberId} already has an active membership", model.MemberId);
                return Result.Fail("Member already has an active membership");
            }

            // Create membership
            var membership = new Membership
            {
                MemberId = model.MemberId,
                PlanId = model.PlanId,
                StartDate = DateOnly.FromDateTime(DateTime.Now),
                EndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(plan.DurationDays)),
                //IsActive = true
            };

            await uow.Memberships.AddAsync(membership, ct);
            await uow.SaveChangesAsync(ct);

            _logger.LogInformation("Membership created successfully for member {MemberId}", model.MemberId);

            return Result.Ok("Membership created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating membership for member {MemberId}", model.MemberId);
            return Result.Fail("Failed to create membership");
        }
    }

    public async Task<Result> CancelMembershipAsync(int memberId, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Cancelling membership for member {MemberId}", memberId);

            var result = await uow.Memberships.CancelMembershipAsync(memberId, ct);

            if (!result)
            {
                _logger.LogWarning("No active membership found for member {MemberId}", memberId);
                return Result.Fail("No active membership found");
            }

            await uow.SaveChangesAsync(ct);

            _logger.LogInformation("Membership cancelled successfully for member {MemberId}", memberId);

            return Result.Ok("Membership cancelled successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling membership for member {MemberId}", memberId);
            return Result.Fail("Failed to cancel membership");
        }
    }

    public async Task<Result<bool>> IsMemberHasActivePlanAsync(int memberId, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Checking if member {MemberId} has active plan", memberId);

            var hasActivePlan = await uow.Memberships
                .IsMemberAlreadyHasActivePlanAsync(memberId, ct);

            return Result<bool>.Ok(hasActivePlan);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking active plan for member {MemberId}", memberId);
            return Result.Fail<bool>("Failed to check active plan");
        }
    }
}
