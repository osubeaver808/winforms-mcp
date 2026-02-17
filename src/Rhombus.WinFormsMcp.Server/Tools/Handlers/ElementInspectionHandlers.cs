using System.Text.Json;
using FlaUI.Core.AutomationElements;
using Rhombus.WinFormsMcp.Server.Models;
using Rhombus.WinFormsMcp.Server.Session;

namespace Rhombus.WinFormsMcp.Server.Tools.Handlers;

/// <summary>
/// Handles element inspection operations (get properties, text, bounds, state, tree)
/// </summary>
public class ElementInspectionHandlers : ToolHandlerBase
{
    private readonly SessionManager _session;

    public ElementInspectionHandlers(SessionManager session)
    {
        _session = session;
    }

    public Task<JsonElement> GetProperty(JsonElement args)
    {
        try
        {
            var elementId = GetStringArg(args, "elementId") ?? throw new ArgumentException("elementId is required");
            var propertyName = GetStringArg(args, "propertyName") ?? "";

            var element = _session.GetElement(elementId);
            if (element == null)
                return Task.FromResult(ResponseHelper.CreateErrorResponse("Element not found in session"));

            var automation = _session.GetAutomation();
            var value = automation.GetProperty(element, propertyName);

            var valueJson = value == null ? "null" : $"\"{EscapeJson(value.ToString())}\"";
            var json = $"{{\"success\": true, \"propertyName\": \"{propertyName}\", \"value\": {valueJson}}}";
            return Task.FromResult(JsonDocument.Parse(json).RootElement);
        }
        catch (Exception ex)
        {
            return Task.FromResult(ResponseHelper.CreateErrorResponse(ex.Message));
        }
    }

    public Task<JsonElement> GetElementText(JsonElement args)
    {
        try
        {
            var elementId = GetStringArg(args, "elementId") ?? throw new ArgumentException("elementId is required");

            var element = _session.GetElement(elementId);
            if (element == null)
                return Task.FromResult(ResponseHelper.CreateErrorResponse(ErrorCode.ElementNotFound, "Element not found in session"));

            if (!_session.IsElementValid(element))
                return Task.FromResult(ResponseHelper.CreateErrorResponse(ErrorCode.ElementStale, "Element is stale"));

            var text = element.Name ?? "";
            return Task.FromResult(JsonDocument.Parse($"{{\"success\": true, \"text\": \"{EscapeJson(text)}\"}}").RootElement);
        }
        catch (Exception ex)
        {
            return Task.FromResult(ResponseHelper.CreateErrorResponse(ErrorCode.InternalError, ex.Message));
        }
    }

    public Task<JsonElement> GetElementValue(JsonElement args)
    {
        try
        {
            var elementId = GetStringArg(args, "elementId") ?? throw new ArgumentException("elementId is required");

            var element = _session.GetElement(elementId);
            if (element == null)
                return Task.FromResult(ResponseHelper.CreateErrorResponse(ErrorCode.ElementNotFound, "Element not found in session"));

            if (!_session.IsElementValid(element))
                return Task.FromResult(ResponseHelper.CreateErrorResponse(ErrorCode.ElementStale, "Element is stale"));

            var automation = _session.GetAutomation();
            var value = automation.GetProperty(element, "Value") ?? automation.GetProperty(element, "Text") ?? "";

            return Task.FromResult(JsonDocument.Parse($"{{\"success\": true, \"value\": \"{EscapeJson(value?.ToString())}\"}}").RootElement);
        }
        catch (Exception ex)
        {
            return Task.FromResult(ResponseHelper.CreateErrorResponse(ErrorCode.InternalError, ex.Message));
        }
    }

    public Task<JsonElement> GetElementBounds(JsonElement args)
    {
        try
        {
            var elementId = GetStringArg(args, "elementId") ?? throw new ArgumentException("elementId is required");

            var element = _session.GetElement(elementId);
            if (element == null)
                return Task.FromResult(ResponseHelper.CreateErrorResponse(ErrorCode.ElementNotFound, "Element not found in session"));

            if (!_session.IsElementValid(element))
                return Task.FromResult(ResponseHelper.CreateErrorResponse(ErrorCode.ElementStale, "Element is stale"));

            var bounds = element.BoundingRectangle;
            var json = $"{{\"success\": true, \"x\": {bounds.X}, \"y\": {bounds.Y}, \"width\": {bounds.Width}, \"height\": {bounds.Height}}}";
            return Task.FromResult(JsonDocument.Parse(json).RootElement);
        }
        catch (Exception ex)
        {
            return Task.FromResult(ResponseHelper.CreateErrorResponse(ErrorCode.InternalError, ex.Message));
        }
    }

    public Task<JsonElement> GetElementState(JsonElement args)
    {
        try
        {
            var elementId = GetStringArg(args, "elementId") ?? throw new ArgumentException("elementId is required");

            var element = _session.GetElement(elementId);
            if (element == null)
                return Task.FromResult(ResponseHelper.CreateErrorResponse(ErrorCode.ElementNotFound, "Element not found in session"));

            if (!_session.IsElementValid(element))
                return Task.FromResult(ResponseHelper.CreateErrorResponse(ErrorCode.ElementStale, "Element is stale"));

            var isEnabled = element.IsEnabled;
            var isOffscreen = element.IsOffscreen;
            var isVisible = !isOffscreen;

            var json = $"{{\"success\": true, \"isEnabled\": {(isEnabled ? "true" : "false")}, \"isVisible\": {(isVisible ? "true" : "false")}, \"isOffscreen\": {(isOffscreen ? "true" : "false")}}}";
            return Task.FromResult(JsonDocument.Parse(json).RootElement);
        }
        catch (Exception ex)
        {
            return Task.FromResult(ResponseHelper.CreateErrorResponse(ErrorCode.InternalError, ex.Message));
        }
    }

    public Task<JsonElement> GetChildElements(JsonElement args)
    {
        try
        {
            var elementId = GetStringArg(args, "elementId") ?? throw new ArgumentException("elementId is required");

            var element = _session.GetElement(elementId);
            if (element == null)
                return Task.FromResult(ResponseHelper.CreateErrorResponse(ErrorCode.ElementNotFound, "Element not found in session"));

            if (!_session.IsElementValid(element))
                return Task.FromResult(ResponseHelper.CreateErrorResponse(ErrorCode.ElementStale, "Element is stale"));

            var children = element.FindAllChildren();
            var childInfos = children.Select(child =>
            {
                var id = _session.CacheElement(child);
                return $"{{\"elementId\": \"{id}\", \"name\": \"{EscapeJson(child.Name ?? "")}\", \"automationId\": \"{EscapeJson(child.AutomationId ?? "")}\", \"controlType\": \"{child.ControlType}\"}}";
            }).ToArray();

            var childrenJson = string.Join(",", childInfos);
            return Task.FromResult(JsonDocument.Parse($"{{\"success\": true, \"count\": {children.Length}, \"children\": [{childrenJson}]}}").RootElement);
        }
        catch (Exception ex)
        {
            return Task.FromResult(ResponseHelper.CreateErrorResponse(ErrorCode.InternalError, ex.Message));
        }
    }

    public Task<JsonElement> GetElementTree(JsonElement args)
    {
        try
        {
            var elementId = GetStringArg(args, "elementId") ?? throw new ArgumentException("elementId is required");
            var maxDepth = GetIntArg(args, "maxDepth", 3);

            var element = _session.GetElement(elementId);
            if (element == null)
                return Task.FromResult(ResponseHelper.CreateErrorResponse(ErrorCode.ElementNotFound, "Element not found in session"));

            if (!_session.IsElementValid(element))
                return Task.FromResult(ResponseHelper.CreateErrorResponse(ErrorCode.ElementStale, "Element is stale"));

            var tree = BuildElementTree(element, maxDepth, 0);
            return Task.FromResult(JsonDocument.Parse($"{{\"success\": true, \"tree\": {tree}}}").RootElement);
        }
        catch (Exception ex)
        {
            return Task.FromResult(ResponseHelper.CreateErrorResponse(ErrorCode.InternalError, ex.Message));
        }
    }

    private string BuildElementTree(AutomationElement element, int maxDepth, int currentDepth)
    {
        if (currentDepth >= maxDepth)
            return $"{{\"name\": \"{EscapeJson(element.Name ?? "")}\", \"controlType\": \"{element.ControlType}\"}}";

        var children = element.FindAllChildren();
        var childTrees = children.Select(child => BuildElementTree(child, maxDepth, currentDepth + 1)).ToArray();
        var childrenJson = string.Join(",", childTrees);

        return $"{{\"name\": \"{EscapeJson(element.Name ?? "")}\", \"controlType\": \"{element.ControlType}\", \"children\": [{childrenJson}]}}";
    }
}
