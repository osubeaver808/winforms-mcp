namespace Rhombus.WinFormsMcp.Server.Tools;

public static partial class ToolDefinitions
{
    private static object[] GetTestScriptTools()
    {
        return
        [
            new
            {
                name = "create_test_script",
                description = "Create a new test script",
                inputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        name = new { type = "string", description = "Test script name" },
                        description = new { type = "string", description = "Test script description (optional)" }
                    },
                    required = new[] { "name" }
                }
            },
            new
            {
                name = "add_test_step",
                description = "Add a step to an existing test script",
                inputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        scriptName = new { type = "string", description = "Name of the script to modify" },
                        stepType = new { type = "string", description = "Step type: action, assertion, wait (default: action)" },
                        command = new { type = "string", description = "Command to execute" },
                        @params = new { type = "object", description = "Parameters for the command" },
                        expected = new { type = "string", description = "Expected value for assertions" },
                        message = new { type = "string", description = "Assertion failure message" },
                        storeResult = new { type = "string", description = "Variable name to store result" },
                        description = new { type = "string", description = "Step description" }
                    },
                    required = new[] { "scriptName", "command" }
                }
            },
            new
            {
                name = "save_test_script",
                description = "Save a complete test script as JSON",
                inputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        script = new { type = "string", description = "Complete test script as JSON string" }
                    },
                    required = new[] { "script" }
                }
            },
            new
            {
                name = "load_test_script",
                description = "Load a test script by name",
                inputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        name = new { type = "string", description = "Name of the script to load" }
                    },
                    required = new[] { "name" }
                }
            },
            new
            {
                name = "list_test_scripts",
                description = "List all available test scripts",
                inputSchema = new
                {
                    type = "object",
                    properties = new { }
                }
            },
            new
            {
                name = "delete_test_script",
                description = "Delete a test script",
                inputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        name = new { type = "string", description = "Name of the script to delete" }
                    },
                    required = new[] { "name" }
                }
            },
            new
            {
                name = "run_test_script",
                description = "Execute a test script and return results",
                inputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        name = new { type = "string", description = "Name of the script to run" },
                        parameters = new { type = "object", description = "Runtime parameters to override variables" }
                    },
                    required = new[] { "name" }
                }
            },
            new
            {
                name = "get_test_results",
                description = "Get test execution results",
                inputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        scriptName = new { type = "string", description = "Script name (optional, returns last result if omitted)" },
                        maxResults = new { type = "integer", description = "Maximum number of results to return (default: 10)" }
                    }
                }
            },
            new
            {
                name = "export_test_results",
                description = "Export test results to HTML format",
                inputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        scriptName = new { type = "string", description = "Script name (optional, uses last result if omitted)" }
                    }
                }
            }
        ];
    }

    private static object[] GetTestRecordingTools()
    {
        return
        [
            new
            {
                name = "start_recording",
                description = "Start recording user actions as a test script",
                inputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        name = new { type = "string", description = "Name for the test script being recorded" },
                        description = new { type = "string", description = "Description of the test script (optional)" }
                    },
                    required = new[] { "name" }
                }
            },
            new
            {
                name = "stop_recording",
                description = "Stop recording and save the test script",
                inputSchema = new
                {
                    type = "object",
                    properties = new { }
                }
            },
            new
            {
                name = "pause_recording",
                description = "Temporarily pause recording",
                inputSchema = new
                {
                    type = "object",
                    properties = new { }
                }
            },
            new
            {
                name = "resume_recording",
                description = "Resume paused recording",
                inputSchema = new
                {
                    type = "object",
                    properties = new { }
                }
            }
        ];
    }
}
