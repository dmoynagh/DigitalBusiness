using DigitalBusiness.DependencyInjectionExtensions.ServiceConfig;
using Microsoft.Extensions.DependencyInjection;

namespace DigitalBusiness.DependencyInjectionExtensions.Tests.ServiceConfig;

public class ServiceConfigExtensionsTests
{
    private sealed class FirstConfig
    {
        public int Value { get; set; }
    }

    private sealed class SecondConfig
    {
        public string Value { get; set; } = string.Empty;
    }

    [Fact]
    public void GetOrAddConfig_FirstCall_CreatesAndReturnsFactoryValue()
    {
        var services = new ServiceCollection();

        var result = services.GetOrAddConfig(() => new FirstConfig { Value = 42 });

        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void GetOrAddConfig_RepeatCall_ReturnsSameInstanceWithoutReinvokingFactory()
    {
        var services = new ServiceCollection();
        var callCount = 0;

        var first = services.GetOrAddConfig(() =>
        {
            callCount++;
            return new FirstConfig { Value = 1 };
        });
        var second = services.GetOrAddConfig<FirstConfig>(() =>
        {
            callCount++;
            throw new InvalidOperationException("Factory should not be invoked on repeat call.");
        });

        Assert.Same(first, second);
        Assert.Equal(1, callCount);
    }

    [Fact]
    public void GetConfig_WhenAbsent_ReturnsNull()
    {
        var services = new ServiceCollection();

        var result = services.GetConfig<FirstConfig>();

        Assert.Null(result);
    }

    [Fact]
    public void GetConfig_WhenPresent_ReturnsAttachedValue()
    {
        var services = new ServiceCollection();
        var added = services.GetOrAddConfig(() => new FirstConfig { Value = 7 });

        var result = services.GetConfig<FirstConfig>();

        Assert.Same(added, result);
    }

    [Fact]
    public void GetConfig_DoesNotCreate()
    {
        var services = new ServiceCollection();

        services.GetConfig<FirstConfig>();

        Assert.False(services.HasConfig<FirstConfig>());
    }

    [Fact]
    public void HasConfig_WhenAbsent_ReturnsFalse()
    {
        var services = new ServiceCollection();

        Assert.False(services.HasConfig<FirstConfig>());
    }

    [Fact]
    public void HasConfig_WhenPresent_ReturnsTrue()
    {
        var services = new ServiceCollection();
        services.GetOrAddConfig(() => new FirstConfig());

        Assert.True(services.HasConfig<FirstConfig>());
    }

    [Fact]
    public void GetOrAddConfig_NullServices_Throws()
    {
        IServiceCollection services = null!;

        Assert.Throws<ArgumentNullException>(() => services.GetOrAddConfig(() => new FirstConfig()));
    }

    [Fact]
    public void GetOrAddConfig_NullFactory_Throws()
    {
        var services = new ServiceCollection();

        Assert.Throws<ArgumentNullException>(() => services.GetOrAddConfig<FirstConfig>(null!));
    }

    [Fact]
    public void GetOrAddConfig_FactoryReturnsNull_Throws()
    {
        var services = new ServiceCollection();

        Assert.Throws<ArgumentNullException>(() => services.GetOrAddConfig<FirstConfig>(() => null!));
    }

    [Fact]
    public void GetConfig_NullServices_Throws()
    {
        IServiceCollection services = null!;

        Assert.Throws<ArgumentNullException>(() => services.GetConfig<FirstConfig>());
    }

    [Fact]
    public void HasConfig_NullServices_Throws()
    {
        IServiceCollection services = null!;

        Assert.Throws<ArgumentNullException>(() => services.HasConfig<FirstConfig>());
    }

    [Fact]
    public void DistinctConfigTypes_DoNotCollide()
    {
        var services = new ServiceCollection();

        services.GetOrAddConfig(() => new FirstConfig { Value = 1 });

        Assert.True(services.HasConfig<FirstConfig>());
        Assert.False(services.HasConfig<SecondConfig>());

        services.GetOrAddConfig(() => new SecondConfig { Value = "hello" });

        Assert.True(services.HasConfig<FirstConfig>());
        Assert.True(services.HasConfig<SecondConfig>());
        Assert.Equal(1, services.GetConfig<FirstConfig>()!.Value);
        Assert.Equal("hello", services.GetConfig<SecondConfig>()!.Value);
    }
}
