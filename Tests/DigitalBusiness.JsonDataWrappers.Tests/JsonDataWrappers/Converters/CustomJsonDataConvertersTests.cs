using System;
using System.Diagnostics.CodeAnalysis;
using DigitalBusiness.JsonDataWrappers;
using DigitalBusiness.JsonDataWrappers.Converters;

namespace DigitalBusiness.JsonDataWrappers.Tests;

public class CustomJsonDataConvertersTests
{
    // -- Helper types ------------------------------------------------------------

    /// <summary>Marker type that has a direct converter registered via the test assembly.</summary>
    public struct TestDirectMarker { }

    /// <summary>Direct converter auto-discovered from this assembly by CustomJsonDataConverters.</summary>
    public sealed class TestDirectMarkerConverter : IJsonDataConverter<TestDirectMarker>
    {
        public bool TryGet(in JsonData jsonData, [MaybeNullWhen(false)] out TestDirectMarker value)
        {
            value = new TestDirectMarker();
            return true;
        }

        public JsonData Create(TestDirectMarker value) => new JsonData();
    }

    /// <summary>Marker type whose converter is produced by a factory.</summary>
    public struct TestFactoryMarker { }

    /// <summary>Converter returned by <see cref="TestFactoryMarkerFactory"/>.</summary>
    public sealed class TestFactoryMarkerConverter : IJsonDataConverter<TestFactoryMarker>
    {
        public bool TryGet(in JsonData jsonData, [MaybeNullWhen(false)] out TestFactoryMarker value)
        {
            value = new TestFactoryMarker();
            return true;
        }

        public JsonData Create(TestFactoryMarker value) => new JsonData();
    }

    /// <summary>Factory auto-discovered from this assembly that handles <see cref="TestFactoryMarker"/>.</summary>
    public sealed class TestFactoryMarkerFactory : IJsonDataConverterFactory
    {
        public bool CanConvert(Type type) => type == typeof(TestFactoryMarker);

        public IJsonDataConverter? CreateConverter(Type typeToConvert) =>
            typeToConvert == typeof(TestFactoryMarker) ? new TestFactoryMarkerConverter() : null;
    }

    /// <summary>A type that intentionally has no converter or factory registered.</summary>
    public struct UnregisteredMarker { }

    // -- Tests -------------------------------------------------------------------

    [Fact]
    public void GetConverter_DirectlyRegisteredType_ReturnsConverter()
    {
        // Act
        var converter = CustomJsonDataConverters.GetConverter<TestDirectMarker>();

        // Assert
        Assert.NotNull(converter);
        Assert.IsAssignableFrom<IJsonDataConverter<TestDirectMarker>>(converter);
    }

    [Fact]
    public void GetConverter_FactoryHandledType_ReturnsConverter()
    {
        // Act
        var converter = CustomJsonDataConverters.GetConverter<TestFactoryMarker>();

        // Assert
        Assert.NotNull(converter);
        Assert.IsAssignableFrom<IJsonDataConverter<TestFactoryMarker>>(converter);
    }

    [Fact]
    public void GetConverter_UnregisteredTypeWithNoFactory_ReturnsNull()
    {
        // Act
        var converter = CustomJsonDataConverters.GetConverter<UnregisteredMarker>();

        // Assert
        Assert.Null(converter);
    }

    [Fact]
    public void GetConverter_DirectlyRegisteredType_ConverterIsCorrectInstance()
    {
        // Act
        var converter = CustomJsonDataConverters.GetConverter<TestDirectMarker>();

        // Assert
        Assert.IsType<TestDirectMarkerConverter>(converter);
    }

    [Fact]
    public void GetConverter_FactoryHandledType_ConverterIsCorrectInstance()
    {
        // Act
        var converter = CustomJsonDataConverters.GetConverter<TestFactoryMarker>();

        // Assert
        Assert.IsType<TestFactoryMarkerConverter>(converter);
    }
}
