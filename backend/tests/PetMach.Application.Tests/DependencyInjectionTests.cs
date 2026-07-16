using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using PetMach.Application;

namespace PetMach.Application.Tests;

public sealed class DependencyInjectionTests
{
    [Fact]
    public void AddApplicationShouldRegisterTimeProvider()
    {
        ServiceProvider provider = new ServiceCollection()
            .AddApplication()
            .BuildServiceProvider();

        provider.GetRequiredService<TimeProvider>().Should().BeSameAs(TimeProvider.System);
    }
}
