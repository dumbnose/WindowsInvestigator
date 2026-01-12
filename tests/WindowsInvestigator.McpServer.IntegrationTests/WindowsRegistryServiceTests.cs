using FluentAssertions;
using WindowsInvestigator.McpServer.Services;

namespace WindowsInvestigator.McpServer.IntegrationTests;

public class WindowsRegistryServiceTests
{
    private readonly WindowsRegistryService _sut;

    public WindowsRegistryServiceTests()
    {
        _sut = new WindowsRegistryService();
    }

    [Fact]
    public void GetKey_WithValidHKLMPath_ReturnsKey()
    {
        // Arrange
        var path = @"HKLM\SOFTWARE\Microsoft";

        // Act
        var result = _sut.GetKey(path);

        // Assert
        result.Should().NotBeNull();
        result!.Path.Should().Be(path);
        result.SubKeyNames.Should().NotBeEmpty();
    }

    [Fact]
    public void GetKey_WithValidHKCUPath_ReturnsKey()
    {
        // Arrange
        var path = @"HKCU\Software";

        // Act
        var result = _sut.GetKey(path);

        // Assert
        result.Should().NotBeNull();
        result!.Path.Should().Be(path);
    }

    [Fact]
    public void GetKey_WithFullRootName_ReturnsKey()
    {
        // Arrange
        var path = @"HKEY_LOCAL_MACHINE\SOFTWARE";

        // Act
        var result = _sut.GetKey(path);

        // Assert
        result.Should().NotBeNull();
        result!.SubKeyNames.Should().NotBeEmpty();
    }

    [Fact]
    public void GetKey_WithNonExistentPath_ReturnsNull()
    {
        // Arrange
        var path = @"HKLM\SOFTWARE\NonExistentKeyThatShouldNotExist12345";

        // Act
        var result = _sut.GetKey(path);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetKey_WithInvalidRoot_ReturnsNull()
    {
        // Arrange
        var path = @"INVALID_ROOT\SOFTWARE";

        // Act
        var result = _sut.GetKey(path);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetKey_ReturnsOrderedSubKeyNames()
    {
        // Arrange
        var path = @"HKLM\SOFTWARE";

        // Act
        var result = _sut.GetKey(path);

        // Assert
        result.Should().NotBeNull();
        result!.SubKeyNames.Should().BeInAscendingOrder(StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public void GetKey_ReturnsOrderedValues()
    {
        // Arrange
        var path = @"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion";

        // Act
        var result = _sut.GetKey(path);

        // Assert
        result.Should().NotBeNull();
        if (result!.Values.Count > 1)
        {
            result.Values.Select(v => v.Name).Should().BeInAscendingOrder(StringComparer.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void GetValue_WithValidValue_ReturnsValue()
    {
        // Arrange
        var path = @"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion";
        var valueName = "ProgramFilesDir";

        // Act
        var result = _sut.GetValue(path, valueName);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be(valueName);
        result.Type.Should().NotBeNullOrEmpty();
        result.DisplayValue.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GetValue_WithNonExistentValue_ReturnsNull()
    {
        // Arrange
        var path = @"HKLM\SOFTWARE\Microsoft";
        var valueName = "NonExistentValue12345";

        // Act
        var result = _sut.GetValue(path, valueName);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetValue_WithNonExistentKey_ReturnsNull()
    {
        // Arrange
        var path = @"HKLM\SOFTWARE\NonExistentKey12345";
        var valueName = "SomeValue";

        // Act
        var result = _sut.GetValue(path, valueName);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetValue_FormatsStringValue()
    {
        // Arrange
        var path = @"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion";
        var valueName = "ProgramFilesDir";

        // Act
        var result = _sut.GetValue(path, valueName);

        // Assert
        result.Should().NotBeNull();
        result!.Type.Should().Be("String");
        result.DisplayValue.Should().Contain("Program Files");
    }

    [Fact]
    public void SearchKeys_FindsMicrosoftKeys()
    {
        // Arrange
        var basePath = @"HKLM\SOFTWARE";
        var pattern = "^Microsoft$";

        // Act
        var result = _sut.SearchKeys(basePath, pattern, 10).ToList();

        // Assert
        result.Should().NotBeEmpty();
        result.Should().Contain(k => k.Contains("Microsoft", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void SearchKeys_RespectsMaxResults()
    {
        // Arrange
        var basePath = @"HKLM\SOFTWARE\Microsoft";
        var pattern = ".*";  // Match anything
        var maxResults = 5;

        // Act
        var result = _sut.SearchKeys(basePath, pattern, maxResults).ToList();

        // Assert
        result.Should().HaveCountLessThanOrEqualTo(maxResults);
    }

    [Fact]
    public void SearchKeys_WithNonExistentPath_ReturnsEmpty()
    {
        // Arrange
        var basePath = @"HKLM\SOFTWARE\NonExistentKey12345";
        var pattern = ".*";

        // Act
        var result = _sut.SearchKeys(basePath, pattern).ToList();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void SearchKeys_WithInvalidRegex_ReturnsEmpty()
    {
        // Arrange
        var basePath = @"HKLM\SOFTWARE";
        var pattern = "[invalid";

        // Act
        var result = _sut.SearchKeys(basePath, pattern).ToList();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void SearchValues_FindsValues()
    {
        // Arrange
        var basePath = @"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion";
        var pattern = "Program";

        // Act
        var result = _sut.SearchValues(basePath, pattern, 10).ToList();

        // Assert
        result.Should().NotBeEmpty();
    }

    [Fact]
    public void SearchValues_RespectsMaxResults()
    {
        // Arrange
        var basePath = @"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion";
        var pattern = ".*";  // Match anything
        var maxResults = 5;

        // Act
        var result = _sut.SearchValues(basePath, pattern, maxResults).ToList();

        // Assert
        result.Should().HaveCountLessThanOrEqualTo(maxResults);
    }

    [Fact]
    public void SearchValues_WithNonExistentPath_ReturnsEmpty()
    {
        // Arrange
        var basePath = @"HKLM\SOFTWARE\NonExistentKey12345";
        var pattern = ".*";

        // Act
        var result = _sut.SearchValues(basePath, pattern).ToList();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void SearchValues_WithInvalidRegex_ReturnsEmpty()
    {
        // Arrange
        var basePath = @"HKLM\SOFTWARE";
        var pattern = "[invalid";

        // Act
        var result = _sut.SearchValues(basePath, pattern).ToList();

        // Assert
        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData("HKLM")]
    [InlineData("HKEY_LOCAL_MACHINE")]
    [InlineData("HKCU")]
    [InlineData("HKEY_CURRENT_USER")]
    [InlineData("HKCR")]
    [InlineData("HKEY_CLASSES_ROOT")]
    public void GetKey_SupportsAllRootKeys(string rootKey)
    {
        // Arrange
        var path = rootKey;

        // Act
        var result = _sut.GetKey(path);

        // Assert
        result.Should().NotBeNull();
        result!.SubKeyNames.Should().NotBeEmpty();
    }

    [Fact]
    public void GetKey_WithWindowsCurrentVersion_HasExpectedValues()
    {
        // Arrange
        var path = @"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion";

        // Act
        var result = _sut.GetKey(path);

        // Assert
        result.Should().NotBeNull();
        result!.Values.Should().Contain(v => v.Name == "ProgramFilesDir" || v.Name == "CommonFilesDir");
    }

    [Fact]
    public void GetKey_ValuesHaveTypeInformation()
    {
        // Arrange
        var path = @"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion";

        // Act
        var result = _sut.GetKey(path);

        // Assert
        result.Should().NotBeNull();
        result!.Values.Should().NotBeEmpty();
        result.Values.Should().AllSatisfy(v =>
        {
            v.Type.Should().NotBeNullOrEmpty();
            v.Type.Should().BeOneOf("String", "ExpandString", "Binary", "DWord", "QWord", "MultiString", "Unknown");
        });
    }
}
