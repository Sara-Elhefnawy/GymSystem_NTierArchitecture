using System.ComponentModel.DataAnnotations;

namespace GymSystem.Infrastructure.Entities.Enums;

public enum Specialty
{
    [Display(Name = "General Fitness")]
    GeneralFitness,
    Yoga,
    Boxing,
    CrossFit,
    Cardio,
    [Display(Name = "Personal Training")]
    PersonalTraining,
    Bodybuilding
}
