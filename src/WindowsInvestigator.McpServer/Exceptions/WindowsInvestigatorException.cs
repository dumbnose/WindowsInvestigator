namespace WindowsInvestigator.McpServer.Exceptions;

/// <summary>
/// Base exception for WindowsInvestigator errors.
/// </summary>
public class WindowsInvestigatorException : Exception
{
    public WindowsInvestigatorException(string message) : base(message) { }
    public WindowsInvestigatorException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Exception thrown when a requested resource is not found.
/// </summary>
public class ResourceNotFoundException : WindowsInvestigatorException
{
    public string ResourceType { get; }
    public string ResourceName { get; }

    public ResourceNotFoundException(string resourceType, string resourceName)
        : base($"{resourceType} '{resourceName}' not found")
    {
        ResourceType = resourceType;
        ResourceName = resourceName;
    }
}

/// <summary>
/// Exception thrown when access to a resource is denied.
/// </summary>
public class AccessDeniedException : WindowsInvestigatorException
{
    public string ResourcePath { get; }

    public AccessDeniedException(string resourcePath)
        : base($"Access denied to '{resourcePath}'")
    {
        ResourcePath = resourcePath;
    }

    public AccessDeniedException(string resourcePath, Exception innerException)
        : base($"Access denied to '{resourcePath}'", innerException)
    {
        ResourcePath = resourcePath;
    }
}

/// <summary>
/// Exception thrown when an invalid parameter is provided.
/// </summary>
public class InvalidParameterException : WindowsInvestigatorException
{
    public string ParameterName { get; }

    public InvalidParameterException(string parameterName, string message)
        : base($"Invalid parameter '{parameterName}': {message}")
    {
        ParameterName = parameterName;
    }
}

/// <summary>
/// Exception thrown when a Windows API call fails.
/// </summary>
public class WindowsApiException : WindowsInvestigatorException
{
    public string ApiName { get; }
    public int? ErrorCode { get; }

    public WindowsApiException(string apiName, string message)
        : base($"{apiName} failed: {message}")
    {
        ApiName = apiName;
    }

    public WindowsApiException(string apiName, int errorCode, Exception innerException)
        : base($"{apiName} failed with error code 0x{errorCode:X8}", innerException)
    {
        ApiName = apiName;
        ErrorCode = errorCode;
    }

    public WindowsApiException(string apiName, Exception innerException)
        : base($"{apiName} failed: {innerException.Message}", innerException)
    {
        ApiName = apiName;
    }
}
