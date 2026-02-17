using System.Text.Json;
using Rhombus.WinFormsMcp.Server.Models;

namespace Rhombus.WinFormsMcp.Server.Tools;

/// <summary>
/// Helper class for creating standardized JSON responses
/// </summary>
public static class ResponseHelper
{
    public static JsonElement CreateSuccessResponse(string message)
    {
        var json = $"{{\"success\": true, \"message\": \"{ToolHandlerBase.EscapeJson(message)}\"}}";
        return JsonDocument.Parse(json).RootElement;
    }

    public static JsonElement CreateErrorResponse(ErrorCode errorCode, string message)
    {
        var json = $"{{\"success\": false, \"errorCode\": {(int)errorCode}, \"error\": \"{ToolHandlerBase.EscapeJson(message)}\"}}";
        return JsonDocument.Parse(json).RootElement;
    }

    public static JsonElement CreateErrorResponse(string message)
    {
        var json = $"{{\"success\": false, \"error\": \"{ToolHandlerBase.EscapeJson(message)}\"}}";
        return JsonDocument.Parse(json).RootElement;
    }

    private static string EscapeJson(string? value)
    {
        if (value == null)
            return "";
        return value.Replace("\\", "\\\\")
                    .Replace("\"", "\\\"")
                    .Replace("\n", "\\n")
                    .Replace("\r", "\\r");
    }
}
