namespace Rhombus.WinFormsMcp.Server.Tools;

public static partial class ToolDefinitions
{
    private static object[] GetProcessTools()
    {
        return
        [
            new
            {
                name = "launch_app",
                description = "Launch a WinForms application from an executable path",
                inputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        path = new { type = "string", description = "Path to the executable" },
                        arguments = new { type = "string", description = "Command-line arguments (optional)" },
                        workingDirectory = new { type = "string", description = "Working directory (optional)" }
                    },
                    required = new[] { "path" }
                }
            },
            new
            {
                name = "attach_to_process",
                description = "Attach to an existing process by PID or process name",
                inputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        pid = new { type = "integer", description = "Process ID" },
                        processName = new { type = "string", description = "Process name (alternative to PID)" }
                    }
                }
            },
            new
            {
                name = "close_app",
                description = "Close an application by process ID",
                inputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        pid = new { type = "integer", description = "Process ID" },
                        force = new { type = "boolean", description = "Force close without saving (default: false)" }
                    },
                    required = new[] { "pid" }
                }
            }
        ];
    }

    private static object[] GetWindowManagementTools()
    {
        return
        [
            new
            {
                name = "maximize_window",
                description = "Maximize the application window",
                inputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        pid = new { type = "integer", description = "Process ID" }
                    },
                    required = new[] { "pid" }
                }
            },
            new
            {
                name = "minimize_window",
                description = "Minimize the application window",
                inputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        pid = new { type = "integer", description = "Process ID" }
                    },
                    required = new[] { "pid" }
                }
            },
            new
            {
                name = "restore_window",
                description = "Restore the application window to normal state",
                inputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        pid = new { type = "integer", description = "Process ID" }
                    },
                    required = new[] { "pid" }
                }
            },
            new
            {
                name = "get_window_title",
                description = "Get the title of the application window",
                inputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        pid = new { type = "integer", description = "Process ID" }
                    },
                    required = new[] { "pid" }
                }
            },
            new
            {
                name = "get_window_state",
                description = "Get the current state of the window (maximized, minimized, normal)",
                inputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        pid = new { type = "integer", description = "Process ID" }
                    },
                    required = new[] { "pid" }
                }
            }
        ];
    }

    private static object[] GetValidationTools()
    {
        return
        [
            new
            {
                name = "take_screenshot",
                description = "Take a screenshot of the application window or a specific element",
                inputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        outputPath = new { type = "string", description = "Path to save the screenshot" },
                        elementId = new { type = "string", description = "Specific element to screenshot (optional)" }
                    },
                    required = new[] { "outputPath" }
                }
            },
            new
            {
                name = "element_exists",
                description = "Check if an element exists by AutomationId",
                inputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        automationId = new { type = "string", description = "AutomationId to check" }
                    },
                    required = new[] { "automationId" }
                }
            },
            new
            {
                name = "wait_for_element",
                description = "Wait for an element to appear by AutomationId",
                inputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        automationId = new { type = "string", description = "AutomationId to wait for" },
                        timeoutMs = new { type = "integer", description = "Timeout in milliseconds (default: 10000)" }
                    },
                    required = new[] { "automationId" }
                }
            },
            new
            {
                name = "wait_for_condition",
                description = "Wait for a specific condition on an element (exists, enabled, visible, has_text)",
                inputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        elementId = new { type = "string", description = "Element identifier" },
                        condition = new { type = "string", description = "Condition to wait for: exists, enabled, visible, has_text" },
                        expectedValue = new { type = "string", description = "Expected value (for has_text condition)" },
                        timeoutMs = new { type = "integer", description = "Timeout in milliseconds (default: uses session timeout)" }
                    },
                    required = new[] { "elementId", "condition" }
                }
            }
        ];
    }

    private static object[] GetInteractionTools()
    {
        return
        [
            new
            {
                name = "drag_drop",
                description = "Drag an element and drop it onto another element",
                inputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        sourceElementId = new { type = "string", description = "Source element identifier" },
                        targetElementId = new { type = "string", description = "Target element identifier" }
                    },
                    required = new[] { "sourceElementId", "targetElementId" }
                }
            },
            new
            {
                name = "send_keys",
                description = "Send arbitrary keyboard input to the active window",
                inputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        keys = new { type = "string", description = "Keys to send" }
                    },
                    required = new[] { "keys" }
                }
            },
            new
            {
                name = "press_key_combination",
                description = "Press a keyboard shortcut combination (e.g., 'Ctrl+C', 'Alt+F4', 'Ctrl+Shift+S')",
                inputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        keys = new { type = "string", description = "Key combination (e.g., 'Ctrl+C', 'Alt+Tab')" }
                    },
                    required = new[] { "keys" }
                }
            }
        ];
    }

    private static object[] GetScrollTools()
    {
        return
        [
            new
            {
                name = "scroll_to_element",
                description = "Scroll the view to make an element visible",
                inputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        elementId = new { type = "string", description = "Element identifier" }
                    },
                    required = new[] { "elementId" }
                }
            },
            new
            {
                name = "scroll_window",
                description = "Scroll a scrollable element in a specific direction",
                inputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        elementId = new { type = "string", description = "Scrollable element identifier" },
                        direction = new { type = "string", description = "Scroll direction: up, down, left, right (default: down)" },
                        amount = new { type = "integer", description = "Amount to scroll (default: 1)" }
                    },
                    required = new[] { "elementId" }
                }
            }
        ];
    }

    private static object[] GetSessionManagementTools()
    {
        return
        [
            new
            {
                name = "clear_cache",
                description = "Clear all cached element references",
                inputSchema = new
                {
                    type = "object",
                    properties = new { }
                }
            },
            new
            {
                name = "get_cached_elements",
                description = "Get a list of all cached element IDs",
                inputSchema = new
                {
                    type = "object",
                    properties = new { }
                }
            },
            new
            {
                name = "list_processes",
                description = "List all attached/launched process IDs",
                inputSchema = new
                {
                    type = "object",
                    properties = new { }
                }
            },
            new
            {
                name = "set_timeout",
                description = "Set the default timeout for wait operations",
                inputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        timeoutMs = new { type = "integer", description = "Timeout in milliseconds (default: 10000)" }
                    },
                    required = new[] { "timeoutMs" }
                }
            }
        ];
    }
}
