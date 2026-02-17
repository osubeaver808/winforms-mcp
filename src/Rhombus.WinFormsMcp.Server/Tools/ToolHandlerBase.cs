using System.Text.Json;

namespace Rhombus.WinFormsMcp.Server.Tools;

/// <summary>
/// Base class for all tool handlers providing common utilities
/// </summary>
public abstract class ToolHandlerBase
{
    protected static string? GetStringArg(JsonElement args, string key)
    {
        if (args.ValueKind == JsonValueKind.Null)
            return null;

        return args.TryGetProperty(key, out var prop) && prop.ValueKind != JsonValueKind.Null
            ? prop.GetString()
            : null;
    }

    protected static int GetIntArg(JsonElement args, string key, int defaultValue = 0)
    {
        if (args.ValueKind == JsonValueKind.Null)
            return defaultValue;

        return args.TryGetProperty(key, out var prop) && prop.ValueKind == JsonValueKind.Number
            ? prop.GetInt32()
            : defaultValue;
    }

    protected static bool GetBoolArg(JsonElement args, string key, bool defaultValue = false)
    {
        if (args.ValueKind == JsonValueKind.Null)
            return defaultValue;

        return args.TryGetProperty(key, out var prop) && prop.ValueKind == JsonValueKind.True
            ? true
            : args.TryGetProperty(key, out var prop2) && prop2.ValueKind == JsonValueKind.False
                ? false
                : defaultValue;
    }

    public static string EscapeJson(string? value)
    {
        if (value == null)
            return "";
        return value.Replace("\\", "\\\\")
                    .Replace("\"", "\\\"")
                    .Replace("\n", "\\n")
                    .Replace("\r", "\\r");
    }
}
