namespace Rhombus.WinFormsMcp.Server.Tools;

/// <summary>
/// Provides MCP tool definitions for all available automation tools
/// </summary>
public static partial class ToolDefinitions
{
    public static object[] GetAllDefinitions()
    {
        return
        [
            ..GetElementDiscoveryTools(),
            ..GetElementInteractionTools(),
            ..GetElementInspectionTools(),
            ..GetProcessTools(),
            ..GetValidationTools(),
            ..GetInteractionTools(),
            ..GetScrollTools(),
            ..GetWindowManagementTools(),
            ..GetSessionManagementTools(),
            ..GetTestScriptTools(),
            ..GetTestRecordingTools()
        ];
    }
}
