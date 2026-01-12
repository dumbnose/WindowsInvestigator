using FluentAssertions;
using NSubstitute;
using WindowsInvestigator.McpServer.Services;
using WindowsInvestigator.McpServer.Tools;

namespace WindowsInvestigator.McpServer.Tests;

public class ServiceToolsTests
{
    private readonly IServiceInfoService _serviceInfoService;
    private readonly ServiceTools _sut;

    public ServiceToolsTests()
    {
        _serviceInfoService = Substitute.For<IServiceInfoService>();
        _sut = new ServiceTools(_serviceInfoService);
    }

    [Fact]
    public void ListServices_ReturnsServicesFromService()
    {
        // Arrange
        var expectedServices = new[]
        {
            new ServiceInfo { Name = "Spooler", DisplayName = "Print Spooler", Status = "Running" },
            new ServiceInfo { Name = "wuauserv", DisplayName = "Windows Update", Status = "Stopped" }
        };
        _serviceInfoService.GetAllServices().Returns(expectedServices);

        // Act
        var result = _sut.ListServices();

        // Assert
        result.Should().BeEquivalentTo(expectedServices);
        _serviceInfoService.Received(1).GetAllServices();
    }

    [Fact]
    public void ListServices_WhenEmpty_ReturnsEmptyCollection()
    {
        // Arrange
        _serviceInfoService.GetAllServices().Returns(Enumerable.Empty<ServiceInfo>());

        // Act
        var result = _sut.ListServices();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetService_WithValidName_ReturnsService()
    {
        // Arrange
        var expectedService = new ServiceInfo
        {
            Name = "Spooler",
            DisplayName = "Print Spooler",
            Status = "Running",
            StartType = "Automatic",
            Description = "Manages print jobs",
            PathName = @"C:\Windows\System32\spoolsv.exe",
            ServiceAccount = "LocalSystem"
        };
        _serviceInfoService.GetService("Spooler").Returns(expectedService);

        // Act
        var result = _sut.GetService("Spooler");

        // Assert
        result.Should().BeEquivalentTo(expectedService);
        _serviceInfoService.Received(1).GetService("Spooler");
    }

    [Fact]
    public void GetService_WithNonExistentName_ReturnsNull()
    {
        // Arrange
        _serviceInfoService.GetService("NonExistentService").Returns((ServiceInfo?)null);

        // Act
        var result = _sut.GetService("NonExistentService");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetService_PassesServiceNameToService()
    {
        // Arrange
        var serviceName = "wuauserv";
        _serviceInfoService.GetService(serviceName).Returns(new ServiceInfo { Name = serviceName });

        // Act
        _ = _sut.GetService(serviceName);

        // Assert
        _serviceInfoService.Received(1).GetService(serviceName);
    }

    [Fact]
    public void SearchServices_WithPattern_ReturnsMatchingServices()
    {
        // Arrange
        var pattern = ".*Print.*";
        var expectedServices = new[]
        {
            new ServiceInfo { Name = "Spooler", DisplayName = "Print Spooler" },
            new ServiceInfo { Name = "PrintNotify", DisplayName = "Printer Extensions and Notifications" }
        };
        _serviceInfoService.SearchServices(pattern).Returns(expectedServices);

        // Act
        var result = _sut.SearchServices(pattern);

        // Assert
        result.Should().BeEquivalentTo(expectedServices);
        _serviceInfoService.Received(1).SearchServices(pattern);
    }

    [Fact]
    public void SearchServices_WithNoMatches_ReturnsEmpty()
    {
        // Arrange
        _serviceInfoService.SearchServices("NoMatchPattern").Returns(Enumerable.Empty<ServiceInfo>());

        // Act
        var result = _sut.SearchServices("NoMatchPattern");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void SearchServices_PassesPatternToService()
    {
        // Arrange
        var pattern = "wua.*";
        _serviceInfoService.SearchServices(pattern).Returns(Enumerable.Empty<ServiceInfo>());

        // Act
        _ = _sut.SearchServices(pattern);

        // Assert
        _serviceInfoService.Received(1).SearchServices(pattern);
    }

    [Theory]
    [InlineData("Spooler")]
    [InlineData("wuauserv")]
    [InlineData("BITS")]
    public void GetService_WithDifferentServiceNames_PassesCorrectName(string serviceName)
    {
        // Arrange
        _serviceInfoService.GetService(serviceName).Returns(new ServiceInfo { Name = serviceName });

        // Act
        var result = _sut.GetService(serviceName);

        // Assert
        result!.Name.Should().Be(serviceName);
    }
}
