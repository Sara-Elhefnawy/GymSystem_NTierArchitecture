using GymSystem.Domain.Abstractions.Services;
using GymSystem.Domain.Abstractions.UnitOfWorks;
using GymSystem.Domain.Common;
using GymSystem.Domain.DTOs.Plan;
using Mapster;
using Microsoft.Extensions.Logging;

namespace GymSystem.Domain.Services;

public class PlanService(
    IUnitOfWork uow,
    ILogger<PlanService> logger) : IPlanService
{
    public async Task<Result<IReadOnlyList<IndexPlanDTO>>> GetActivePlansAsync(CancellationToken ct = default)
    {
        try
        {
            var items = await uow.Plans.GetAllAsync(ct);

            var dtos = items.Adapt<IReadOnlyList<IndexPlanDTO>>();

            return Result.Ok(dtos);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting all plans");
            return Result.Fail<IReadOnlyList<IndexPlanDTO>>("Failed to retrieve plans", "DATABASE_ERROR");
        }
    }

    public async Task<Result<DetailsPlanDTO>> GetDetailsAsync(int id, CancellationToken ct = default)
    {
        try
        {
            var plan = await uow.Plans.GetByIdAsync(id, ct);

            if (plan is null)
            {
                logger.LogWarning("Plan not found with ID: {Id}", id);
                return Result.Fail<DetailsPlanDTO>("Plan not found", "PLAN_NOT_FOUND");
            }

            var dto = plan.Adapt<DetailsPlanDTO>();

            return Result.Ok(dto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting plan details for ID: {Id}", id);
            return Result.Fail<DetailsPlanDTO>("Failed to retrieve plan details", "DATABASE_ERROR");
        }
    }

    public async Task<Result<EditPlanDTO>> GetForEditAsync(int id, CancellationToken ct = default)
    {
        try
        {
            var plan = await uow.Plans.GetByIdAsync(id, ct);
            if (plan is null)
            {
                logger.LogWarning("Plan not found with ID: {Id}", id);
                return Result.Fail<EditPlanDTO>("Plan not found", "PLAN_NOT_FOUND");
            }

            var dto = plan.Adapt<EditPlanDTO>();

            return Result.Ok(dto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting plan for edit, ID: {Id}", id);
            return Result.Fail<EditPlanDTO>("Failed to retrieve plan data", "DATABASE_ERROR");
        }
    }

    public async Task<Result> UpdateAsync(EditPlanDTO dto, CancellationToken ct = default)
    {
        try
        {
            var plan = await uow.Plans.GetByIdAsync(dto.Id, ct);
            if (plan is null)
            {
                logger.LogWarning("Plan not found with ID: {Id}", dto.Id);
                return Result.Fail("Plan not found", "PLAN_NOT_FOUND");
            }

            // Business rule: Check if trying to deactivate plan with active memberships
            if (plan.IsActive && !dto.IsActive)
            {
                var hasActiveMemberships = await uow.Plans.HasActiveMembershipsAsync(dto.Id, ct);
                if (hasActiveMemberships)
                {
                    logger.LogWarning("Plan cannot be deactivated with active memberships");
                    return Result.Fail("Cannot deactivate plan with active memberships", "ACTIVE_MEMBERSHIPS_EXIST");
                }
            }

            TypeAdapter.Adapt(dto, plan);

            uow.Plans.Update(plan, ct);
            await uow.SaveChangesAsync(ct);

            logger.LogInformation("Plan {Id} updated successfully", dto.Id);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating plan {Id}", dto.Id);
            return Result.Fail("Failed to update plan", "UPDATE_ERROR");
        }
    }

    public async Task<Result> ToggleActivationAsync(int id, CancellationToken ct = default)
    {
        try
        {
            var plan = await uow.Plans.GetByIdAsync(id, ct);
            if (plan is null)
            {
                logger.LogWarning("Plan not found with ID: {Id}", id);
                return Result.Fail("Plan not found", "PLAN_NOT_FOUND");
            }

            if (plan.IsActive)
            {
                var hasActiveMemberships = await uow.Plans.HasActiveMembershipsAsync(id, ct);
                if (hasActiveMemberships)
                {
                    logger.LogWarning("Plan cannot be deactivated with active memberships");
                    return Result.Fail("Cannot deactivate plan with active memberships", "ACTIVE_MEMBERSHIPS_EXIST");
                }
            }

            plan.IsActive = !plan.IsActive;
            uow.Plans.Update(plan, ct);
            await uow.SaveChangesAsync(ct);

            logger.LogInformation("Plan {Id} activation toggled to {IsActive}", id, plan.IsActive);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error toggling activation for plan {Id}", id);
            return Result.Fail("Failed to toggle plan activation", "TOGGLE_ERROR");
        }
    }
}
