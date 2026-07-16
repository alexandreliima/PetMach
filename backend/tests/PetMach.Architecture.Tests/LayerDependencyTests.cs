using FluentAssertions;
using NetArchTest.Rules;
using PetMach.Application;
using PetMach.Domain.SharedKernel;
using PetMach.Infrastructure;

namespace PetMach.Architecture.Tests;

public sealed class LayerDependencyTests
{
    [Fact]
    public void DomainShouldNotDependOnOuterLayers()
    {
        TestResult result = Types.InAssembly(typeof(Result).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny("PetMach.Application", "PetMach.Infrastructure", "PetMach.Api")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void ApplicationShouldNotDependOnInfrastructure()
    {
        TestResult result = Types.InAssembly(typeof(PetMach.Application.DependencyInjection).Assembly)
            .ShouldNot()
            .HaveDependencyOn(typeof(Infrastructure.DependencyInjection).Namespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }
}
