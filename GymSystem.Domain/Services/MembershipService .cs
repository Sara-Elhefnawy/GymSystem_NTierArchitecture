using AutoMapper;
using GymSystem.Domain.DTOs.Membership;
using GymSystem.Domain.QRCode;
using GymSystem.Domain.Services.Interfaces;
using GymSystem.Infrastructure.Attachments;
using GymSystem.Infrastructure.Entities;
using GymSystem.Infrastructure.UnitOfWorks;
using GymSystem.Shared.Common;
using Microsoft.Extensions.Logging;

namespace GymSystem.Domain.Services;

public class MembershipService(
    IUnitOfWork uow,
    IQrService qrService,
    ILogger<MembershipService> logger,
    IMapper mapper) : IMembershipService
{
    public async Task<Result<IEnumerable<IndexMembershipDTO>>> GetActiveMembershipsAsync(CancellationToken ct = default)
    {
        try
        {
            var memberships = await uow.Memberships.GetActiveMembershipsAsync(ct);

            var dtos = mapper.Map<IEnumerable<IndexMembershipDTO>>(memberships);

            return Result.Ok(dtos);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting active memberships");
            return Result.Fail<IEnumerable<IndexMembershipDTO>>("DATABASE_ERROR", "Failed to get memberships");
        }
    }

    public async Task<Result> CreateMembershipAsync(CreateMembershipDTO model, CancellationToken ct = default)
    {
        try
        {
            logger.LogInformation("Creating membership for member {MemberId} with plan {PlanId}",
                model.MemberId, model.PlanId);

            // Validate member exists
            var member = await uow.Members.GetByIdAsync(model.MemberId, ct);
            if (member == null)
            {
                logger.LogWarning("Member {MemberId} not found", model.MemberId);
                return Result.Fail("Member not found", "MEMBER_NOT_FOUND");
            }

            // Validate plan exists
            var plan = await uow.Plans.GetByIdAsync(model.PlanId, ct);
            if (plan == null)
            {
                logger.LogWarning("Plan {PlanId} not found", model.PlanId);
                return Result.Fail("Plan not found", "PLAN_NOT_FOUND");
            }

            if (!plan.IsActive)
            {
                logger.LogWarning("Plan {PlanId} is deactivated", model.PlanId);
                return Result.Fail("Cannot create membership with a deactivated plan. Please choose an active plan.", "PLAN_INACTIVE");
            }

            // Check if member already has active membership
            if (await uow.Memberships.IsMemberAlreadyHasActivePlanAsync(model.MemberId, ct))
            {
                logger.LogWarning("Member {MemberId} already has an active membership", model.MemberId);
                return Result.Fail("Member already has an active membership", "ALREADY_ACTIVE");
            }

            var membership = mapper.Map<Membership>(model);

            membership.EndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(plan.DurationDays));

            await uow.Memberships.AddAsync(membership, ct);
            await uow.SaveChangesAsync(ct);

            await GenerateQrCodesForMemberAsync(model.MemberId, ct);

            logger.LogInformation("Membership created successfully for member {MemberId}", model.MemberId);

            return Result.Ok("Membership created successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating membership for member {MemberId}", model.MemberId);
            return Result.Fail("Failed to create membership", "DATABASE_ERROR");
        }
    }

    private async Task GenerateQrCodesForMemberAsync(int memberId, CancellationToken ct)
    {
        try
        {
            var bookings = await uow.Bookings.GetUpcomingBookingsByMemberIdAsync(memberId, ct);

            if (bookings == null || !bookings.Any())
            {
                logger.LogInformation("No upcoming bookings found for member {MemberId}", memberId);
                return;
            }

            logger.LogInformation("Generating QR codes for {Count} bookings for member {MemberId}",
                bookings.Count(), memberId);

            foreach (var booking in bookings)
            {
                try
                {
                    var qrResult = await qrService.GenerateMemberQrPngAsync(memberId, ct);

                    if (qrResult.IsSuccess)
                    {
                        logger.LogInformation("QR code generated for member {MemberId}, session {SessionId}",
                            memberId, booking.SessionId);
                    }
                    else
                    {
                        logger.LogWarning("Failed to generate QR for member {MemberId}, session {SessionId}: {Error}",
                            memberId, booking.SessionId, qrResult.Error);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error generating QR for member {MemberId}, session {SessionId}",
                        memberId, booking.SessionId);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error generating QR codes for member {MemberId}", memberId);
        }
    }

    public async Task<Result> CancelMembershipAsync(int membershipId, CancellationToken ct = default)
    {
        try
        {
            logger.LogInformation($"Attempting to cancel membership with ID: {membershipId}");

            var membership = await uow.Memberships.GetByIdWithIncludesAsync(membershipId, ct);

            if (membership == null)
            {
                logger.LogWarning($"Membership with ID {membershipId} not found");
                return Result.Fail("MEMBERSHIP_NOT_FOUND", "Membership not found");
            }

            if (membership.IsDeleted)
            {
                logger.LogWarning($"Membership with ID {membershipId} is already deleted");
                return Result.Fail("ALREADY_CANCELLED", "Membership is already cancelled");
            }

            var result = await uow.Memberships.CancelMembershipByIdAsync(membershipId, ct);

            if (!result)
            {
                return Result.Fail("CANCEL_FAILED", "Failed to cancel membership");
            }

            logger.LogInformation($"Membership {membershipId} cancelled successfully");

            try
            {
                var qrDeleteResult = await qrService.DeleteMemberQrCodeAsync(membership.MemberId, ct);

                if (qrDeleteResult.IsSuccess)
                {
                    logger.LogInformation($"QR code deleted successfully for member {membership.MemberId}");
                }
                else
                {
                    logger.LogWarning($"Failed to delete QR code for member {membership.MemberId}: {qrDeleteResult.Error}");
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, $"Exception deleting QR code for member {membership.MemberId}");
            }

            return Result.Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Error cancelling membership {membershipId}");
            return Result.Fail("DATABASE_ERROR", $"Failed to cancel membership: {ex.Message}");
        }
    }
}
