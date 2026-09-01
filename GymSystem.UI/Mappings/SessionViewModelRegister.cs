using GymSystem.Domain.DTOs.Category;
using GymSystem.Domain.DTOs.Session;
using GymSystem.UI.ViewModels.Session;
using Mapster;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GymSystem.UI.Mappings;

public class SessionViewModelRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // Map from IndexSessionDTO to IndexSessionViewModel
        config.NewConfig<IndexSessionDTO, IndexSessionViewModel>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.CategoryName, src => src.CategoryName)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.TrainerName, src => src.TrainerName)
            .Map(dest => dest.StartDate, src => src.StartDate)
            .Map(dest => dest.EndDate, src => src.EndDate)
            .Map(dest => dest.MaxCapacity, src => src.MaxCapacity)
            .Map(dest => dest.AvailableSlots, src => src.AvailableSlots)
            .Map(dest => dest.Duration, src => src.EndDate - src.StartDate)
            .Map(dest => dest.Status, src => src.Status);

        // Map from DetailsSessionDTO to DetailsSessionViewModel
        config.NewConfig<DetailsSessionDTO, DetailsSessionViewModel>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.CategoryName, src => src.CategoryName)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.TrainerName, src => src.TrainerName)
            .Map(dest => dest.StartDate, src => src.StartDate)
            .Map(dest => dest.EndDate, src => src.EndDate)
            .Map(dest => dest.Capacity, src => src.Capacity)
            .Map(dest => dest.AvailableSlots, src => src.AvailableSlots)
            .Map(dest => dest.Status, src => src.Status);

        // Map from CreateSessionViewModel to CreateSessionDTO
        config.NewConfig<CreateSessionViewModel, CreateSessionDTO>()
            .Map(dest => dest.CategoryId, src => src.CategoryId)
            .Map(dest => dest.TrainerId, src => src.TrainerId)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.Capacity, src => src.Capacity)
            .Map(dest => dest.StartDate, src => src.StartDate)
            .Map(dest => dest.EndDate, src => src.EndDate);

        // Map from EditSessionViewModel to EditSessionDTO
        config.NewConfig<EditSessionViewModel, EditSessionDTO>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.TrainerId, src => src.TrainerId)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.StartDate, src => src.StartDate)
            .Map(dest => dest.EndDate, src => src.EndDate);

        // Map from (Category, TrainerList) to CreateSessionViewModel
        config.NewConfig<(IndexCategoryDTO category, SelectList trainerList), CreateSessionViewModel>()
            .Map(dest => dest.CategoryId, src => src.category.Id)
            .Map(dest => dest.CategoryName, src => src.category.Name)
            .Map(dest => dest.Capacity, src => 25)
            .Map(dest => dest.StartDate, src => DateTime.Now)
            .Map(dest => dest.EndDate, src => DateTime.Now.AddHours(1))
            .Map(dest => dest.Description, src => string.Empty)
            .Ignore(dest => dest.TrainerList!);

        // Map from DeleteSessionDTO to DeleteSessionViewModel
        config.NewConfig<DeleteSessionDTO, DeleteSessionViewModel>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Specialty, src => src.Specialty)
            .Map(dest => dest.TrainerName, src => src.TrainerName)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.StartDate, src => src.StartDate)
            .Map(dest => dest.EndDate, src => src.EndDate)
            .Map(dest => dest.BookedCount, src => src.BookedCount)
            .Map(dest => dest.MaxCapacity, src => src.MaxCapacity)
            .Map(dest => dest.Status, src => src.Status)
            .Map(dest => dest.CanDelete, src => src.CanDelete);
    }
}
