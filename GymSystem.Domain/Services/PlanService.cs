using GymSystem.Domain.DTOs.Plan;
using GymSystem.Domain.Services.Interfaces;
using GymSystem.Infrastructure.UnitOfWorks;
using GymSystem.Shared.Common;
using Microsoft.Extensions.Logging;

namespace GymSystem.Domain.Services;

public class PlanService : IPlanService
{
    private readonly IUnitOfWork _uow;
    private readonly ILogger<PlanService> _logger;

    public PlanService(IUnitOfWork uow, ILogger<PlanService> logger)
    {
        _uow = uow;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<IndexPlanDTO>>> GetAllAsync(CancellationToken ct = default)
    {
        try
        {
            var items = await _uow.Plans.GetAllAsync(ct);
            var dtos = items.Select(m => new IndexPlanDTO
            {
                Id = m.Id,
                Name = m.Name,
                Description = m.Description,
                DurationDays = m.DurationDays,
                IsActive = m.IsActive,
                Price = m.Price,
            }).ToList();

            return Result.Ok<IReadOnlyList<IndexPlanDTO>>(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all plans");
            return Result.Fail<IReadOnlyList<IndexPlanDTO>>("Failed to retrieve plans", "DATABASE_ERROR");
        }
    }

    public async Task<Result<DetailsPlanDTO>> GetDetailsAsync(int id, CancellationToken ct = default)
    {
        try
        {
            var plan = await _uow.Plans.GetByIdAsync(id, ct);

            if (plan is null)
            {
                _logger.LogWarning("Plan not found with ID: {Id}", id);
                return Result.Fail<DetailsPlanDTO>("Plan not found", "PLAN_NOT_FOUND");
            }

            var dto = new DetailsPlanDTO
            {
                Id = plan.Id,
                Name = plan.Name,
                Description = plan.Description,
                DurationDays = plan.DurationDays,
                IsActive = plan.IsActive,
                Price = plan.Price,
            };

            return Result.Ok(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting plan details for ID: {Id}", id);
            return Result.Fail<DetailsPlanDTO>("Failed to retrieve plan details", "DATABASE_ERROR");
        }
    }

    public async Task<Result<EditPlanDTO>> GetForEditAsync(int id, CancellationToken ct = default)
    {
        try
        {
            var plan = await _uow.Plans.GetByIdAsync(id, ct);
            if (plan is null)
            {
                _logger.LogWarning("Plan not found with ID: {Id}", id);
                return Result.Fail<EditPlanDTO>("Plan not found", "PLAN_NOT_FOUND");
            }

            var dto = new EditPlanDTO
            {
                Id = plan.Id,
                Name = plan.Name,
                Description = plan.Description,
                DurationDays = plan.DurationDays,
                Price = plan.Price
            };

            return Result.Ok(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting plan for edit, ID: {Id}", id);
            return Result.Fail<EditPlanDTO>("Failed to retrieve plan data", "DATABASE_ERROR");
        }
    }

    public async Task<Result> UpdateAsync(EditPlanDTO dto, CancellationToken ct = default)
    {
        try
        {
            var plan = await _uow.Plans.GetByIdAsync(dto.Id, ct);
            if (plan is null)
            {
                _logger.LogWarning("Plan not found with ID: {Id}", dto.Id);
                return Result.Fail("Plan not found", "PLAN_NOT_FOUND");
            }

            // Business rule: Check if trying to deactivate plan with active memberships
            if (plan.IsActive && !dto.IsActive)
            {
                var hasActiveMemberships = await _uow.Plans.HasActiveMembershipsAsync(dto.Id, ct);
                if (hasActiveMemberships)
                {
                    _logger.LogWarning("Plan cannot be deactivated with active memberships");
                    return Result.Fail("Cannot deactivate plan with active memberships", "ACTIVE_MEMBERSHIPS_EXIST");
                }
            }

            plan.Description = dto.Description;
            plan.DurationDays = dto.DurationDays;
            plan.Price = dto.Price;
            plan.IsActive = dto.IsActive;

            _uow.Plans.Update(plan, ct);
            await _uow.SaveChangesAsync(ct);

            _logger.LogInformation("Plan {Id} updated successfully", dto.Id);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating plan {Id}", dto.Id);
            return Result.Fail("Failed to update plan", "UPDATE_ERROR");
        }
    }

    public async Task<Result> ToggleActivationAsync(int id, CancellationToken ct = default)
    {
        try
        {
            var plan = await _uow.Plans.GetByIdAsync(id, ct);
            if (plan is null)
            {
                _logger.LogWarning("Plan not found with ID: {Id}", id);
                return Result.Fail("Plan not found", "PLAN_NOT_FOUND");
            }

            if (plan.IsActive)
            {
                var hasActiveMemberships = await _uow.Plans.HasActiveMembershipsAsync(id, ct);
                if (hasActiveMemberships)
                {
                    _logger.LogWarning("Plan cannot be deactivated with active memberships");
                    return Result.Fail("Cannot deactivate plan with active memberships", "ACTIVE_MEMBERSHIPS_EXIST");
                }
            }

            plan.IsActive = !plan.IsActive;
            _uow.Plans.Update(plan, ct);
            await _uow.SaveChangesAsync(ct);

            _logger.LogInformation("Plan {Id} activation toggled to {IsActive}", id, plan.IsActive);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling activation for plan {Id}", id);
            return Result.Fail("Failed to toggle plan activation", "TOGGLE_ERROR");
        }
    }
}