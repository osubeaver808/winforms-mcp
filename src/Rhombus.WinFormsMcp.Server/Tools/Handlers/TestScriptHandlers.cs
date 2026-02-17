using System.Text.Json;
using Rhombus.WinFormsMcp.Server.Models;
using Rhombus.WinFormsMcp.Server.Session;
using Rhombus.WinFormsMcp.Server.Testing;

namespace Rhombus.WinFormsMcp.Server.Tools.Handlers;

/// <summary>
/// Handles test script management operations
/// </summary>
public class TestScriptHandlers : ToolHandlerBase
{
    private readonly SessionManager _session;
    private readonly TestScriptManager _testManager;
    private readonly TestRecorder _recorder;
    private TestResult? _lastTestResult;

    public TestScriptHandlers(SessionManager session, TestScriptManager testManager, TestRecorder recorder)
    {
        _session = session;
        _testManager = testManager;
        _recorder = recorder;
    }

    public void SetLastTestResult(TestResult? result)
    {
        _lastTestResult = result;
    }

    public async Task<JsonElement> CreateTestScript(JsonElement args)
    {
        try
        {
            var name = GetStringArg(args, "name") ?? throw new ArgumentException("name is required");
            var description = GetStringArg(args, "description") ?? "";

            var script = new TestScript
            {
                Name = name,
                Description = description
            };

            await _testManager.SaveScriptAsync(script);

            return JsonDocument.Parse($"{{\"success\": true, \"name\": \"{EscapeJson(name)}\", \"message\": \"Test script created\"}}").RootElement;
        }
        catch (Exception ex)
        {
            return ResponseHelper.CreateErrorResponse(ErrorCode.InternalError, ex.Message);
        }
    }

    public async Task<JsonElement> AddTestStep(JsonElement args)
    {
        try
        {
            var scriptName = GetStringArg(args, "scriptName") ?? throw new ArgumentException("scriptName is required");
            var stepType = GetStringArg(args, "stepType") ?? "action";
            var command = GetStringArg(args, "command") ?? throw new ArgumentException("command is required");

            var script = await _testManager.LoadScriptAsync(scriptName);
            if (script == null)
                return ResponseHelper.CreateErrorResponse(ErrorCode.InvalidArgument, $"Script not found: {scriptName}");

            var step = new TestStep
            {
                Type = stepType,
                Command = command,
                Description = GetStringArg(args, "description")
            };

            if (args.TryGetProperty("params", out var paramsElement) && paramsElement.ValueKind == JsonValueKind.Object)
            {
                foreach (var prop in paramsElement.EnumerateObject())
                {
                    step.Params[prop.Name] = prop.Value.ToString();
                }
            }

            if (args.TryGetProperty("expected", out var expectedElement))
                step.Expected = expectedElement.ToString();

            step.Message = GetStringArg(args, "message");
            step.StoreResult = GetStringArg(args, "storeResult");

            script.Steps.Add(step);
            await _testManager.SaveScriptAsync(script);

            return JsonDocument.Parse($"{{\"success\": true, \"stepIndex\": {script.Steps.Count - 1}, \"message\": \"Step added to script\"}}").RootElement;
        }
        catch (Exception ex)
        {
            return ResponseHelper.CreateErrorResponse(ErrorCode.InternalError, ex.Message);
        }
    }

    public async Task<JsonElement> SaveTestScript(JsonElement args)
    {
        try
        {
            var scriptJson = GetStringArg(args, "script") ?? throw new ArgumentException("script is required");

            var script = JsonSerializer.Deserialize<TestScript>(scriptJson);
            if (script == null)
                return ResponseHelper.CreateErrorResponse(ErrorCode.InvalidArgument, "Invalid script JSON");

            await _testManager.SaveScriptAsync(script);

            return JsonDocument.Parse($"{{\"success\": true, \"name\": \"{EscapeJson(script.Name)}\", \"message\": \"Script saved\"}}").RootElement;
        }
        catch (Exception ex)
        {
            return ResponseHelper.CreateErrorResponse(ErrorCode.InternalError, ex.Message);
        }
    }

    public async Task<JsonElement> LoadTestScript(JsonElement args)
    {
        try
        {
            var name = GetStringArg(args, "name") ?? throw new ArgumentException("name is required");

            var script = await _testManager.LoadScriptAsync(name);
            if (script == null)
                return ResponseHelper.CreateErrorResponse(ErrorCode.InvalidArgument, $"Script not found: {name}");

            var scriptJson = JsonSerializer.Serialize(script);
            return JsonDocument.Parse($"{{\"success\": true, \"script\": {scriptJson}}}").RootElement;
        }
        catch (Exception ex)
        {
            return ResponseHelper.CreateErrorResponse(ErrorCode.InternalError, ex.Message);
        }
    }

    public async Task<JsonElement> ListTestScripts(JsonElement args)
    {
        try
        {
            var scripts = await _testManager.ListScriptsAsync();
            var scriptsInfo = scripts.Select(s => new
            {
                name = s.Name,
                description = s.Description,
                stepCount = s.Steps.Count,
                created = s.Created,
                modified = s.Modified,
                tags = s.Tags
            });

            var json = JsonSerializer.Serialize(new { success = true, count = scripts.Count, scripts = scriptsInfo });
            return JsonDocument.Parse(json).RootElement;
        }
        catch (Exception ex)
        {
            return ResponseHelper.CreateErrorResponse(ErrorCode.InternalError, ex.Message);
        }
    }

    public Task<JsonElement> DeleteTestScript(JsonElement args)
    {
        try
        {
            var name = GetStringArg(args, "name") ?? throw new ArgumentException("name is required");

            var deleted = _testManager.DeleteScript(name);
            if (!deleted)
                return Task.FromResult(ResponseHelper.CreateErrorResponse(ErrorCode.InvalidArgument, $"Script not found: {name}"));

            return Task.FromResult(ResponseHelper.CreateSuccessResponse("Script deleted"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(ResponseHelper.CreateErrorResponse(ErrorCode.InternalError, ex.Message));
        }
    }

    public async Task<JsonElement> RunTestScript(JsonElement args)
    {
        try
        {
            var name = GetStringArg(args, "name") ?? throw new ArgumentException("name is required");

            var script = await _testManager.LoadScriptAsync(name);
            if (script == null)
                return ResponseHelper.CreateErrorResponse(ErrorCode.InvalidArgument, $"Script not found: {name}");

            Dictionary<string, string>? parameters = null;
            if (args.TryGetProperty("parameters", out var paramsElement) && paramsElement.ValueKind == JsonValueKind.Object)
            {
                parameters = new Dictionary<string, string>();
                foreach (var prop in paramsElement.EnumerateObject())
                {
                    parameters[prop.Name] = prop.Value.GetString() ?? "";
                }
            }

            var runner = new TestRunner(_session.GetAutomation());
            var result = await runner.ExecuteAsync(script, parameters);

            _lastTestResult = result;
            await _testManager.SaveResultAsync(result);

            var resultJson = JsonSerializer.Serialize(new
            {
                success = true,
                result = new
                {
                    status = result.Status.ToString(),
                    durationMs = result.DurationMs,
                    totalSteps = result.TotalSteps,
                    passedSteps = result.PassedSteps,
                    failedSteps = result.FailedSteps,
                    skippedSteps = result.SkippedSteps,
                    errorMessage = result.ErrorMessage
                }
            });

            return JsonDocument.Parse(resultJson).RootElement;
        }
        catch (Exception ex)
        {
            return ResponseHelper.CreateErrorResponse(ErrorCode.InternalError, ex.Message);
        }
    }

    public async Task<JsonElement> GetTestResults(JsonElement args)
    {
        try
        {
            var scriptName = GetStringArg(args, "scriptName");

            if (string.IsNullOrEmpty(scriptName))
            {
                if (_lastTestResult == null)
                    return ResponseHelper.CreateErrorResponse(ErrorCode.InvalidArgument, "No test results available");

                var lastResultJson = JsonSerializer.Serialize(_lastTestResult);
                return JsonDocument.Parse($"{{\"success\": true, \"result\": {lastResultJson}}}").RootElement;
            }
            else
            {
                var maxResults = GetIntArg(args, "maxResults", 10);
                var results = await _testManager.GetResultsAsync(scriptName, maxResults);

                var resultsJson = JsonSerializer.Serialize(new { success = true, count = results.Count, results });
                return JsonDocument.Parse(resultsJson).RootElement;
            }
        }
        catch (Exception ex)
        {
            return ResponseHelper.CreateErrorResponse(ErrorCode.InternalError, ex.Message);
        }
    }

    public async Task<JsonElement> ExportTestResults(JsonElement args)
    {
        try
        {
            var scriptName = GetStringArg(args, "scriptName");

            TestResult? result = null;
            if (string.IsNullOrEmpty(scriptName))
            {
                result = _lastTestResult;
            }
            else
            {
                result = await _testManager.GetLatestResultAsync(scriptName);
            }

            if (result == null)
                return ResponseHelper.CreateErrorResponse(ErrorCode.InvalidArgument, "No test results found");

            var htmlPath = await _testManager.ExportResultToHtmlAsync(result);

            return JsonDocument.Parse($"{{\"success\": true, \"htmlPath\": \"{EscapeJson(htmlPath)}\", \"message\": \"Results exported to HTML\"}}").RootElement;
        }
        catch (Exception ex)
        {
            return ResponseHelper.CreateErrorResponse(ErrorCode.InternalError, ex.Message);
        }
    }

    public Task<JsonElement> StartRecording(JsonElement args)
    {
        try
        {
            var name = GetStringArg(args, "name") ?? throw new ArgumentException("name is required");
            var description = GetStringArg(args, "description") ?? "";

            _recorder.StartRecording(name, description);

            return Task.FromResult(JsonDocument.Parse($"{{\"success\": true, \"message\": \"Recording started for '{EscapeJson(name)}'\"}}").RootElement);
        }
        catch (Exception ex)
        {
            return Task.FromResult(ResponseHelper.CreateErrorResponse(ErrorCode.InternalError, ex.Message));
        }
    }

    public async Task<JsonElement> StopRecording(JsonElement args)
    {
        try
        {
            var script = _recorder.StopRecording();
            if (script == null)
                return ResponseHelper.CreateErrorResponse(ErrorCode.InvalidArgument, "No active recording");

            await _testManager.SaveScriptAsync(script);

            return JsonDocument.Parse($"{{\"success\": true, \"name\": \"{EscapeJson(script.Name)}\", \"stepCount\": {script.Steps.Count}, \"message\": \"Recording stopped and saved\"}}").RootElement;
        }
        catch (Exception ex)
        {
            return ResponseHelper.CreateErrorResponse(ErrorCode.InternalError, ex.Message);
        }
    }

    public Task<JsonElement> PauseRecording(JsonElement args)
    {
        try
        {
            _recorder.PauseRecording();
            return Task.FromResult(ResponseHelper.CreateSuccessResponse("Recording paused"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(ResponseHelper.CreateErrorResponse(ErrorCode.InternalError, ex.Message));
        }
    }

    public Task<JsonElement> ResumeRecording(JsonElement args)
    {
        try
        {
            _recorder.ResumeRecording();
            return Task.FromResult(ResponseHelper.CreateSuccessResponse("Recording resumed"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(ResponseHelper.CreateErrorResponse(ErrorCode.InternalError, ex.Message));
        }
    }
}
