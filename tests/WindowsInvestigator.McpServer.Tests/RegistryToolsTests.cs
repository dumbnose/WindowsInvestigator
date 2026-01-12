using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using WindowsInvestigator.McpServer.Exceptions;
using WindowsInvestigator.McpServer.Services;
using WindowsInvestigator.McpServer.Tools;

namespace WindowsInvestigator.McpServer.Tests;

public class RegistryToolsTests
{
    private readonly IRegistryService _registryService;
    private readonly RegistryTools _sut;

    public RegistryToolsTests()
    {
        _registryService = Substitute.For<IRegistryService>();
        _sut = new RegistryTools(_registryService, NullLogger<RegistryTools>.Instance);
    }

    [Fact]
    public void GetRegistryKey_ReturnsKeyFromService()
    {
        // Arrange
        var path = @"HKLM\SOFTWARE\Microsoft";
        var expectedKey = new RegistryKeyInfo
        {
            Path = path,
            SubKeyNames = new List<string> { "Windows", "Office" },
            Values = new List<RegistryValueInfo>
            {
                new() { Name = "Version", Type = "String", DisplayValue = "10.0" }
            }
        };
        _registryService.GetKey(path).Returns(expectedKey);

        // Act
        var result = _sut.GetRegistryKey(path);

        // Assert
        result.Should().BeEquivalentTo(expectedKey);
        _registryService.Received(1).GetKey(path);
    }

    [Fact]
    public void GetRegistryKey_WithNonExistentPath_ReturnsNull()
    {
        // Arrange
        var path = @"HKLM\SOFTWARE\NonExistent";
        _registryService.GetKey(path).Returns((RegistryKeyInfo?)null);

        // Act
        var result = _sut.GetRegistryKey(path);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetRegistryKey_WithEmptyPath_ThrowsInvalidParameterException()
    {
        // Act
        Action act = () => _sut.GetRegistryKey("");

        // Assert
        act.Should().Throw<InvalidParameterException>()
            .WithMessage("*path*");
    }

    [Fact]
    public void GetRegistryValue_ReturnsValueFromService()
    {
        // Arrange
        var path = @"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion";
        var valueName = "ProgramFilesDir";
        var expectedValue = new RegistryValueInfo
        {
            Name = valueName,
            Type = "String",
            Value = @"C:\Program Files",
            DisplayValue = @"C:\Program Files"
        };
        _registryService.GetValue(path, valueName).Returns(expectedValue);

        // Act
        var result = _sut.GetRegistryValue(path, valueName);

        // Assert
        result.Should().BeEquivalentTo(expectedValue);
        _registryService.Received(1).GetValue(path, valueName);
    }

    [Fact]
    public void GetRegistryValue_WithNonExistentValue_ReturnsNull()
    {
        // Arrange
        var path = @"HKLM\SOFTWARE\Microsoft";
        var valueName = "NonExistent";
        _registryService.GetValue(path, valueName).Returns((RegistryValueInfo?)null);

        // Act
        var result = _sut.GetRegistryValue(path, valueName);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetRegistryValue_WithEmptyPath_ThrowsInvalidParameterException()
    {
        // Act
        Action act = () => _sut.GetRegistryValue("", "SomeValue");

        // Assert
        act.Should().Throw<InvalidParameterException>()
            .WithMessage("*path*");
    }

    [Fact]
    public void SearchRegistryKeys_ReturnsMatchingKeys()
    {
        // Arrange
        var basePath = @"HKLM\SOFTWARE";
        var pattern = "Microsoft";
        var expectedKeys = new[] { @"HKLM\SOFTWARE\Microsoft", @"HKLM\SOFTWARE\Microsoft\Windows" };
        _registryService.SearchKeys(basePath, pattern, 100).Returns(expectedKeys);

        // Act
        var result = _sut.SearchRegistryKeys(basePath, pattern);

        // Assert
        result.Should().BeEquivalentTo(expectedKeys);
        _registryService.Received(1).SearchKeys(basePath, pattern, 100);
    }

    [Fact]
    public void SearchRegistryKeys_WithEmptyBasePath_ThrowsInvalidParameterException()
    {
        // Act
        Action act = () => _sut.SearchRegistryKeys("", "pattern");

        // Assert
        act.Should().Throw<InvalidParameterException>()
            .WithMessage("*basePath*");
    }

    [Fact]
    public void SearchRegistryKeys_WithEmptyPattern_ThrowsInvalidParameterException()
    {
        // Act
        Action act = () => _sut.SearchRegistryKeys(@"HKLM\SOFTWARE", "");

        // Assert
        act.Should().Throw<InvalidParameterException>()
            .WithMessage("*pattern*");
    }

    [Fact]
    public void SearchRegistryKeys_WithInvalidRegex_ThrowsInvalidParameterException()
    {
        // Act
        Action act = () => _sut.SearchRegistryKeys(@"HKLM\SOFTWARE", "[invalid");

        // Assert
        act.Should().Throw<InvalidParameterException>()
            .WithMessage("*pattern*Invalid regex*");
    }

    [Fact]
    public void SearchRegistryKeys_WithInvalidMaxResults_ThrowsInvalidParameterException()
    {
        // Act
        Action act = () => _sut.SearchRegistryKeys(@"HKLM\SOFTWARE", "pattern", maxResults: 0);

        // Assert
        act.Should().Throw<InvalidParameterException>()
            .WithMessage("*maxResults*");
    }

    [Fact]
    public void SearchRegistryValues_ReturnsMatchingValues()
    {
        // Arrange
        var basePath = @"HKLM\SOFTWARE\Microsoft";
        var pattern = "Version";
        var expectedValues = new[]
        {
            new RegistryValueInfo { Name = "Version", Type = "String", DisplayValue = "10.0" },
            new RegistryValueInfo { Name = "BuildVersion", Type = "DWord", DisplayValue = "22000" }
        };
        _registryService.SearchValues(basePath, pattern, 100).Returns(expectedValues);

        // Act
        var result = _sut.SearchRegistryValues(basePath, pattern);

        // Assert
        result.Should().BeEquivalentTo(expectedValues);
        _registryService.Received(1).SearchValues(basePath, pattern, 100);
    }

    [Fact]
    public void SearchRegistryValues_WithEmptyBasePath_ThrowsInvalidParameterException()
    {
        // Act
        Action act = () => _sut.SearchRegistryValues("", "pattern");

        // Assert
        act.Should().Throw<InvalidParameterException>()
            .WithMessage("*basePath*");
    }

    [Fact]
    public void SearchRegistryValues_WithEmptyPattern_ThrowsInvalidParameterException()
    {
        // Act
        Action act = () => _sut.SearchRegistryValues(@"HKLM\SOFTWARE", "");

        // Assert
        act.Should().Throw<InvalidParameterException>()
            .WithMessage("*pattern*");
    }

    [Fact]
    public void SearchRegistryValues_WithInvalidRegex_ThrowsInvalidParameterException()
    {
        // Act
        Action act = () => _sut.SearchRegistryValues(@"HKLM\SOFTWARE", "[invalid");

        // Assert
        act.Should().Throw<InvalidParameterException>()
            .WithMessage("*pattern*Invalid regex*");
    }

    [Fact]
    public void SearchRegistryValues_WithInvalidMaxResults_ThrowsInvalidParameterException()
    {
        // Act
        Action act = () => _sut.SearchRegistryValues(@"HKLM\SOFTWARE", "pattern", maxResults: 0);

        // Assert
        act.Should().Throw<InvalidParameterException>()
            .WithMessage("*maxResults*");
    }

    [Theory]
    [InlineData(@"HKLM\SOFTWARE")]
    [InlineData(@"HKCU\Software")]
    [InlineData(@"HKEY_LOCAL_MACHINE\SYSTEM")]
    public void GetRegistryKey_WithVariousPaths_PassesCorrectPath(string path)
    {
        // Arrange
        _registryService.GetKey(path).Returns(new RegistryKeyInfo { Path = path });

        // Act
        var result = _sut.GetRegistryKey(path);

        // Assert
        result!.Path.Should().Be(path);
    }

    [Fact]
    public void GetRegistryKey_WhenServiceThrows_ThrowsWindowsApiException()
    {
        // Arrange
        _registryService.GetKey(Arg.Any<string>()).Returns(_ => throw new InvalidOperationException("Test error"));

        // Act
        Action act = () => _sut.GetRegistryKey(@"HKLM\SOFTWARE");

        // Assert
        act.Should().Throw<WindowsApiException>();
    }

    [Fact]
    public void SearchRegistryKeys_WhenServiceThrows_ThrowsWindowsApiException()
    {
        // Arrange
        _registryService.SearchKeys(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>())
            .Returns(_ => throw new InvalidOperationException("Test error"));

        // Act
        Action act = () => _sut.SearchRegistryKeys(@"HKLM\SOFTWARE", "test");

        // Assert
        act.Should().Throw<WindowsApiException>();
    }
}
