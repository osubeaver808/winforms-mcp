namespace Rhombus.WinFormsMcp.Server.Tools;

public static partial class ToolDefinitions
{
    private static object[] GetElementDiscoveryTools()
    {
        return
        [
            new
            {
                name = "find_element",
                description = "Find a UI element by AutomationId, Name, ClassName, or ControlType. Returns the first matching element. Example: {\"automationId\": \"btnSubmit\"}",
                inputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        automationId = new { type = "string", description = "AutomationId of the element" },
                        name = new { type = "string", description = "Name of the element" },
                        className = new { type = "string", description = "ClassName of the element" },
                        controlType = new { type = "string", description = "ControlType of the element" },
                        parent = new { type = "string", description = "Parent element path (optional)" }
                    }
                }
            },
            new
            {
                name = "find_all_elements",
                description = "Find all UI elements matching the criteria. Returns an array of all matching elements.",
                inputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        automationId = new { type = "string", description = "AutomationId of the elements" },
                        name = new { type = "string", description = "Name of the elements" },
                        className = new { type = "string", description = "ClassName of the elements" }
                    }
                }
            }
        ];
    }

    private static object[] GetElementInteractionTools()
    {
        return
        [
            new
            {
                name = "click_element",
                description = "Click on a UI element. Supports left-click, double-click, and right-click.",
                inputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        elementId = new { type = "string", description = "Element identifier from find_element" },
                        doubleClick = new { type = "boolean", description = "Double-click if true (default: false)" },
                        rightClick = new { type = "boolean", description = "Right-click if true (default: false)" }
                    },
                    required = new[] { "elementId" }
                }
            },
            new
            {
                name = "type_text",
                description = "Type text into a text field",
                inputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        elementId = new { type = "string", description = "Element identifier from find_element" },
                        text = new { type = "string", description = "Text to type" },
                        clearFirst = new { type = "boolean", description = "Clear field before typing (default: false)" }
                    },
                    required = new[] { "elementId", "text" }
                }
            },
            new
            {
                name = "set_value",
                description = "Set the value of an element directly",
                inputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        elementId = new { type = "string", description = "Element identifier" },
                        value = new { type = "string", description = "Value to set" }
                    },
                    required = new[] { "elementId", "value" }
                }
            },
            new
            {
                name = "focus_element",
                description = "Set keyboard focus to an element",
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
                name = "hover_element",
                description = "Move the mouse cursor over an element (useful for tooltips and hover effects)",
                inputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        elementId = new { type = "string", description = "Element identifier" }
                    },
                    required = new[] { "elementId" }
                }
            }
        ];
    }

    private static object[] GetElementInspectionTools()
    {
        return
        [
            new
            {
                name = "get_property",
                description = "Get a specific property value from an element",
                inputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        elementId = new { type = "string", description = "Element identifier" },
                        propertyName = new { type = "string", description = "Name of the property to retrieve" }
                    },
                    required = new[] { "elementId", "propertyName" }
                }
            },
            new
            {
                name = "get_element_text",
                description = "Get the text content of an element",
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
                name = "get_element_value",
                description = "Get the value of an element (for text boxes, combo boxes, etc.)",
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
                name = "get_element_bounds",
                description = "Get the position and size of an element (x, y, width, height)",
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
                name = "get_element_state",
                description = "Get the state of an element (isEnabled, isVisible, isOffscreen)",
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
                name = "get_child_elements",
                description = "Get all direct child elements of a specified element",
                inputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        elementId = new { type = "string", description = "Parent element identifier" }
                    },
                    required = new[] { "elementId" }
                }
            },
            new
            {
                name = "get_element_tree",
                description = "Get the hierarchical tree structure of an element and its descendants",
                inputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        elementId = new { type = "string", description = "Root element identifier" },
                        maxDepth = new { type = "integer", description = "Maximum depth to traverse (default: 3)" }
                    },
                    required = new[] { "elementId" }
                }
            }
        ];
    }
}
