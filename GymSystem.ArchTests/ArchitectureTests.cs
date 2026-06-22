using Microsoft.AspNetCore.Mvc;
using NetArchTest.Rules;
using System.Reflection;
using FluentAssertions;
using Xunit;
using GymSystem.Domain.Abstractions.Services;

namespace GymSystem.ArchTests;

public class ArchitectureTests
{
    private static readonly Assembly BllAssembly = typeof(GymSystem.Domain.Services.BookingService).Assembly;
    private static readonly Assembly DalAssembly = typeof(GymSystem.Infrastructure.Repositories.BookingRepository).Assembly;
    private static readonly Assembly PresentationAssembly = typeof(GymSystem.UI.Controllers.BookingController).Assembly;

    [Fact]
    public void BLL_Must_Not_Reference_DAL()
    {
        var result = Types.InAssembly(BllAssembly)
            .ShouldNot()
            .HaveDependencyOn("GymSystem.Infrastructure")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Controllers_Must_Not_Import_EF_Directly()
    {
        var result = Types.InAssembly(PresentationAssembly)
            .That()
            .Inherit(typeof(Controller))
            .ShouldNot()
            .HaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void All_Services_Must_Implement_An_Interface()
    {
        var serviceClasses = Types.InAssembly(BllAssembly)
        .That()
        .HaveNameEndingWith("Service")
        .And()
        .AreClasses()
        .GetTypes()
        .ToList();

        var violations = serviceClasses
            .Where(t => !t.GetInterfaces().Any(i => i.Name == $"I{t.Name}"))
            .Select(t => $"{t.Name} (implements: {string.Join(", ", t.GetInterfaces().Select(i => i.Name))})")
            .ToList();

        violations.Should().BeEmpty(
            $"All services must implement their corresponding interface (e.g., BookingService -> IBookingService). Violations: {string.Join("; ", violations)}");
    }

    [Fact]
    public void All_Repositories_Must_Live_In_DataAccess()
    {
        var result = Types.InAssembly(DalAssembly)
            .That()
            .HaveNameEndingWith("Repository")
            .Should()
            .ResideInNamespace("GymSystem.Infrastructure.Repositories")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }
}
