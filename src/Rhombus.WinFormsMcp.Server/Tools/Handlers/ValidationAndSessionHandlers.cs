using System.Text.Json;
using Rhombus.WinFormsMcp.Server.Models;
using Rhombus.WinFormsMcp.Server.Session;

namespace Rhombus.WinFormsMcp.Server.Tools.Handlers;

/// <summary>
/// Handles session management, validation, and keyboard/mouse operations
/// </summary>
public class ValidationAndSessionHandlers : ToolHandlerBase
{
    private readonly SessionManager _session;

    public ValidationAndSessionHandlers(SessionManager session)
    {
        _session = session;
    }

    public Task<JsonElement> TakeScreenshot(JsonElement args)
    {
        try
        {
            var outputPath = GetStringArg(args, "outputPath") ?? throw new ArgumentException("outputPath is required");
            var elementId = GetStringArg(args, "elementId");

            var automation = _session.GetAutomation();
            FlaUI.Core.AutomationElements.AutomationElement? element = null;

            if (!string.IsNullOrEmpty(elementId))
                element = _session.GetElement(elementId!);

            automation.TakeScreenshot(outputPath, element);

            return Task.FromResult(JsonDocument.Parse($"{{\"success\": true, \"message\": \"Screenshot saved to {EscapeJson(outputPath)}\"}}").RootElement);
        }
        catch (Exception ex)
        {
            return Task.FromResult(ResponseHelper.CreateErrorResponse(ex.Message));
        }
    }

    public Task<JsonElement> ElementExists(JsonElement args)
    {
        try
        {
            var automationId = GetStringArg(args, "automationId") ?? throw new ArgumentException("automationId is required");

            var automation = _session.GetAutomation();
            var exists = automation.ElementExists(automationId);

            return Task.FromResult(JsonDocument.Parse($"{{\"success\": true, \"exists\": {(exists ? "true" : "false")}}}").RootElement);
        }
        catch (Exception ex)
        {
            return Task.FromResult(ResponseHelper.CreateErrorResponse(ex.Message));
        }
    }

    public async Task<JsonElement> WaitForElement(JsonElement args)
    {
        try
        {
            var automationId = GetStringArg(args, "automationId") ?? throw new ArgumentException("automationId is required");
            var timeoutMs = GetIntArg(args, "timeoutMs", 10000);

            var automation = _session.GetAutomation();
            var found = await automation.WaitForElementAsync(automationId, null, timeoutMs);

            return JsonDocument.Parse($"{{\"success\": true, \"found\": {(found ? "true" : "false")}}}").RootElement;
        }
        catch (Exception ex)
        {
            return JsonDocument.Parse($"{{\"success\": false, \"error\": \"{EscapeJson(ex.Message)}\"}}").RootElement;
        }
    }

    public async Task<JsonElement> WaitForCondition(JsonElement args)
    {
        try
        {
            var elementId = GetStringArg(args, "elementId") ?? throw new ArgumentException("elementId is required");
            var condition = GetStringArg(args, "condition") ?? "exists";
            var timeoutMs = GetIntArg(args, "timeoutMs", _session.DefaultTimeout);
            var expectedValue = GetStringArg(args, "expectedValue");

            var element = _session.GetElement(elementId);
            if (element == null)
                return ResponseHelper.CreateErrorResponse(ErrorCode.ElementNotFound, "Element not found in session");

            var startTime = DateTime.Now;
            while ((DateTime.Now - startTime).TotalMilliseconds < timeoutMs)
            {
                try
                {
                    bool conditionMet = condition.ToLower() switch
                    {
                        "exists" => _session.IsElementValid(element),
                        "enabled" => element.IsEnabled,
                        "visible" => !element.IsOffscreen,
                        "has_text" => (element.Name ?? "").Contains(expectedValue ?? ""),
                        _ => false
                    };

                    if (conditionMet)
                        return JsonDocument.Parse("{\"success\": true, \"conditionMet\": true}").RootElement;
                }
                catch
                {
                    // Element might be temporarily unavailable
                }

                await Task.Delay(100);
            }

            return JsonDocument.Parse("{\"success\": true, \"conditionMet\": false, \"timedOut\": true}").RootElement;
        }
        catch (Exception ex)
        {
            return ResponseHelper.CreateErrorResponse(ErrorCode.InternalError, ex.Message);
        }
    }

    public Task<JsonElement> SendKeys(JsonElement args)
    {
        try
        {
            var keys = GetStringArg(args, "keys") ?? throw new ArgumentException("keys is required");

            var automation = _session.GetAutomation();
            automation.SendKeys(keys);

            return Task.FromResult(ResponseHelper.CreateSuccessResponse("Keys sent"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(ResponseHelper.CreateErrorResponse(ex.Message));
        }
    }

    public Task<JsonElement> PressKeyCombination(JsonElement args)
    {
        try
        {
            var keys = GetStringArg(args, "keys") ?? throw new ArgumentException("keys is required");

            var automation = _session.GetAutomation();
            automation.PressKeyCombination(keys);

            return Task.FromResult(JsonDocument.Parse($"{{\"success\": true, \"message\": \"Key combination '{EscapeJson(keys)}' pressed\"}}").RootElement);
        }
        catch (Exception ex)
        {
            return Task.FromResult(ResponseHelper.CreateErrorResponse(ErrorCode.InternalError, ex.Message));
        }
    }

    public Task<JsonElement> ClearCache(JsonElement args)
    {
        try
        {
            _session.ClearAllElements();
            return Task.FromResult(ResponseHelper.CreateSuccessResponse("Element cache cleared"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(ResponseHelper.CreateErrorResponse(ex.Message));
        }
    }

    public Task<JsonElement> GetCachedElements(JsonElement args)
    {
        try
        {
            var elementIds = _session.GetCachedElementIds().ToArray();
            var idsJson = string.Join(",", elementIds.Select(id => $"\"{id}\""));

            return Task.FromResult(JsonDocument.Parse($"{{\"success\": true, \"count\": {elementIds.Length}, \"elementIds\": [{idsJson}]}}").RootElement);
        }
        catch (Exception ex)
        {
            return Task.FromResult(ResponseHelper.CreateErrorResponse(ex.Message));
        }
    }

    public Task<JsonElement> SetTimeout(JsonElement args)
    {
        try
        {
            var timeoutMs = GetIntArg(args, "timeoutMs", 10000);
            _session.DefaultTimeout = timeoutMs;

            return Task.FromResult(JsonDocument.Parse($"{{\"success\": true, \"timeoutMs\": {_session.DefaultTimeout}}}").RootElement);
        }
        catch (Exception ex)
        {
            return Task.FromResult(ResponseHelper.CreateErrorResponse(ex.Message));
        }
    }

    public Task<JsonElement> RaiseEvent(JsonElement args)
    {
        return Task.FromResult(ResponseHelper.CreateErrorResponse("Event raising not yet implemented"));
    }

    public Task<JsonElement> ListenForEvent(JsonElement args)
    {
        return Task.FromResult(ResponseHelper.CreateErrorResponse("Event listening not yet implemented"));
    }
}
