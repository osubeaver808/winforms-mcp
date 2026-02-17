using System.Text.Json;
using Rhombus.WinFormsMcp.Server.Session;
using Rhombus.WinFormsMcp.Server.Testing;
using Rhombus.WinFormsMcp.Server.Tools;
using Rhombus.WinFormsMcp.Server.Tools.Handlers;

namespace Rhombus.WinFormsMcp.Server.Server;

/// <summary>
/// Core MCP server implementation handling JSON-RPC communication
/// </summary>
public class AutomationServer
{
    private readonly Dictionary<string, Func<JsonElement, Task<JsonElement>>> _tools;
    private int _nextId = 1;
    private readonly SessionManager _session = new();
    private readonly TestScriptManager _testManager = new();
    private readonly TestRecorder _recorder = new();

    // Handlers
    private readonly ElementDiscoveryHandlers _elementDiscovery;
    private readonly ElementInteractionHandlers _elementInteraction;
    private readonly ElementInspectionHandlers _elementInspection;
    private readonly ProcessHandlers _processHandlers;
    private readonly WindowHandlers _windowHandlers;
    private readonly TestScriptHandlers _testScriptHandlers;
    private readonly ValidationAndSessionHandlers _validationAndSession;

    public AutomationServer()
    {
        // Initialize handlers
        _elementDiscovery = new ElementDiscoveryHandlers(_session);
        _elementInteraction = new ElementInteractionHandlers(_session);
        _elementInspection = new ElementInspectionHandlers(_session);
        _processHandlers = new ProcessHandlers(_session);
        _windowHandlers = new WindowHandlers(_session);
        _testScriptHandlers = new TestScriptHandlers(_session, _testManager, _recorder);
        _validationAndSession = new ValidationAndSessionHandlers(_session);

        // Register all tools
        _tools = new Dictionary<string, Func<JsonElement, Task<JsonElement>>>
        {
            // Element Discovery
            { "find_element", _elementDiscovery.FindElement },
            { "find_all_elements", _elementDiscovery.FindAllElements },

            // Element Interaction
            { "click_element", _elementInteraction.ClickElement },
            { "type_text", _elementInteraction.TypeText },
            { "set_value", _elementInteraction.SetValue },
            { "focus_element", _elementInteraction.FocusElement },
            { "hover_element", _elementInteraction.HoverElement },
            { "drag_drop", _elementInteraction.DragDrop },
            { "scroll_to_element", _elementInteraction.ScrollToElement },
            { "scroll_window", _elementInteraction.ScrollWindow },

            // Element Inspection
            { "get_property", _elementInspection.GetProperty },
            { "get_element_text", _elementInspection.GetElementText },
            { "get_element_value", _elementInspection.GetElementValue },
            { "get_element_bounds", _elementInspection.GetElementBounds },
            { "get_element_state", _elementInspection.GetElementState },
            { "get_child_elements", _elementInspection.GetChildElements },
            { "get_element_tree", _elementInspection.GetElementTree },

            // Process Tools
            { "launch_app", _processHandlers.LaunchApp },
            { "attach_to_process", _processHandlers.AttachToProcess },
            { "close_app", _processHandlers.CloseApp },
            { "list_processes", _processHandlers.ListProcesses },

            // Window Management
            { "maximize_window", _windowHandlers.MaximizeWindow },
            { "minimize_window", _windowHandlers.MinimizeWindow },
            { "restore_window", _windowHandlers.RestoreWindow },
            { "get_window_title", _windowHandlers.GetWindowTitle },
            { "get_window_state", _windowHandlers.GetWindowState },

            // Validation and Session
            { "take_screenshot", _validationAndSession.TakeScreenshot },
            { "element_exists", _validationAndSession.ElementExists },
            { "wait_for_element", _validationAndSession.WaitForElement },
            { "wait_for_condition", _validationAndSession.WaitForCondition },
            { "send_keys", _validationAndSession.SendKeys },
            { "press_key_combination", _validationAndSession.PressKeyCombination },
            { "clear_cache", _validationAndSession.ClearCache },
            { "get_cached_elements", _validationAndSession.GetCachedElements },
            { "set_timeout", _validationAndSession.SetTimeout },
            { "raise_event", _validationAndSession.RaiseEvent },
            { "listen_for_event", _validationAndSession.ListenForEvent },

            // Test Script Management
            { "create_test_script", _testScriptHandlers.CreateTestScript },
            { "add_test_step", _testScriptHandlers.AddTestStep },
            { "save_test_script", _testScriptHandlers.SaveTestScript },
            { "load_test_script", _testScriptHandlers.LoadTestScript },
            { "list_test_scripts", _testScriptHandlers.ListTestScripts },
            { "delete_test_script", _testScriptHandlers.DeleteTestScript },
            { "run_test_script", _testScriptHandlers.RunTestScript },
            { "get_test_results", _testScriptHandlers.GetTestResults },
            { "export_test_results", _testScriptHandlers.ExportTestResults },

            // Test Recording
            { "start_recording", _testScriptHandlers.StartRecording },
            { "stop_recording", _testScriptHandlers.StopRecording },
            { "pause_recording", _testScriptHandlers.PauseRecording },
            { "resume_recording", _testScriptHandlers.ResumeRecording }
        };
    }

    public async Task RunAsync()
    {
        var reader = Console.In;
        var writer = Console.Out;

        // Send initialization
        var initMessage = new
        {
            jsonrpc = "2.0",
            result = new
            {
                protocolVersion = "2024-11-05",
                capabilities = new
                {
                    tools = ToolDefinitions.GetAllDefinitions()
                },
                serverInfo = new
                {
                    name = "fnWindowsMCP",
                    version = "1.0.0"
                }
            }
        };

        await writer.WriteLineAsync(JsonSerializer.Serialize(initMessage));
        await writer.FlushAsync();

        // Process incoming messages
        while (true)
        {
            var line = await reader.ReadLineAsync();
            if (string.IsNullOrEmpty(line))
                break;

            try
            {
                var request = JsonDocument.Parse(line).RootElement;
                var response = await ProcessRequest(request);
                await writer.WriteLineAsync(JsonSerializer.Serialize(response));
                await writer.FlushAsync();
            }
            catch (Exception ex)
            {
                var error = new
                {
                    jsonrpc = "2.0",
                    error = new
                    {
                        code = -32603,
                        message = "Internal error",
                        data = new { details = ex.Message }
                    }
                };
                await writer.WriteLineAsync(JsonSerializer.Serialize(error));
                await writer.FlushAsync();
            }
        }
    }

    private async Task<object> ProcessRequest(JsonElement request)
    {
        if (!request.TryGetProperty("method", out var methodElement))
            throw new InvalidOperationException("Missing method");

        var method = methodElement.GetString();
        if (method == "initialize")
        {
            return new
            {
                jsonrpc = "2.0",
                id = request.TryGetProperty("id", out var id) ? id.GetInt32() : _nextId++,
                result = new
                {
                    protocolVersion = "2024-11-05",
                    capabilities = new
                    {
                        tools = ToolDefinitions.GetAllDefinitions()
                    },
                    serverInfo = new
                    {
                        name = "fnWindowsMCP",
                        version = "1.0.0"
                    }
                }
            };
        }

        if (method == "tools/list")
        {
            return new
            {
                jsonrpc = "2.0",
                id = request.TryGetProperty("id", out var id) ? id.GetInt32() : _nextId++,
                result = new
                {
                    tools = ToolDefinitions.GetAllDefinitions()
                }
            };
        }

        if (method == "tools/call")
        {
            if (!request.TryGetProperty("params", out var paramsElement))
                throw new InvalidOperationException("Missing params");

            if (!paramsElement.TryGetProperty("name", out var nameElement))
                throw new InvalidOperationException("Missing tool name");

            var toolName = nameElement.GetString() ?? throw new InvalidOperationException("Tool name is empty");
            var toolArgs = paramsElement.TryGetProperty("arguments", out var args) ? args : default;

            if (!_tools.ContainsKey(toolName))
                throw new InvalidOperationException($"Unknown tool: {toolName}");

            var result = await _tools[toolName](toolArgs);

            return new
            {
                jsonrpc = "2.0",
                id = request.TryGetProperty("id", out var id) ? id.GetInt32() : _nextId++,
                result = new
                {
                    content = new[]
                    {
                        new
                        {
                            type = "text",
                            text = result.ToString()
                        }
                    }
                }
            };
        }

        throw new InvalidOperationException($"Unknown method: {method}");
    }
}
