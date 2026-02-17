namespace Rhombus.WinFormsMcp.Server.Models;

/// <summary>
/// Error codes for structured error reporting
/// </summary>
public enum ErrorCode
{
    Success = 0,
    ElementNotFound = 1001,
    ElementStale = 1002,
    ElementNotInteractable = 1003,
    ProcessNotFound = 2001,
    ProcessAccessDenied = 2002,
    TimeoutExpired = 3001,
    InvalidArgument = 4001,
    OperationNotSupported = 4002,
    InternalError = 5001
}
