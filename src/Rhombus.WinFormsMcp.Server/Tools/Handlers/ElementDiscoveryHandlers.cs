using System.Text.Json;
using FlaUI.Core.AutomationElements;
using Rhombus.WinFormsMcp.Server.Models;
using Rhombus.WinFormsMcp.Server.Session;

namespace Rhombus.WinFormsMcp.Server.Tools.Handlers;

/// <summary>
/// Handles element discovery and search operations
/// </summary>
public class ElementDiscoveryHandlers : ToolHandlerBase
{
    private readonly SessionManager _session;

    public ElementDiscoveryHandlers(SessionManager session)
    {
        _session = session;
    }

    public Task<JsonElement> FindElement(JsonElement args)
    {
        try
        {
            var automation = _session.GetAutomation();
            var automationId = GetStringArg(args, "automationId");
            var name = GetStringArg(args, "name");
            var className = GetStringArg(args, "className");

            AutomationElement? element = null;

            if (!string.IsNullOrEmpty(automationId))
            {
                element = automation.FindByAutomationId(automationId);
            }
            else if (!string.IsNullOrEmpty(name))
            {
                element = automation.FindByName(name);
            }
            else if (!string.IsNullOrEmpty(className))
            {
                element = automation.FindByClassName(className);
            }

            if (element == null)
                return Task.FromResult(ResponseHelper.CreateErrorResponse("Element not found"));

            var elementId = _session.CacheElement(element);
            var json = $"{{\"success\": true, \"elementId\": \"{elementId}\", \"name\": \"{EscapeJson(element.Name ?? "")}\", \"automationId\": \"{EscapeJson(element.AutomationId ?? "")}\", \"controlType\": \"{element.ControlType}\"}}";
            return Task.FromResult(JsonDocument.Parse(json).RootElement);
        }
        catch (Exception ex)
        {
            return Task.FromResult(ResponseHelper.CreateErrorResponse(ex.Message));
        }
    }

    public Task<JsonElement> FindAllElements(JsonElement args)
    {
        try
        {
            var automation = _session.GetAutomation();
            var automationId = GetStringArg(args, "automationId");
            var name = GetStringArg(args, "name");
            var className = GetStringArg(args, "className");

            var elements = new List<AutomationElement>();

            if (!string.IsNullOrEmpty(automationId))
            {
                elements = automation.FindAllByAutomationId(automationId).ToList();
            }
            else if (!string.IsNullOrEmpty(name))
            {
                elements = automation.FindAllByName(name).ToList();
            }
            else if (!string.IsNullOrEmpty(className))
            {
                elements = automation.FindAllByClassName(className).ToList();
            }

            var elementInfos = elements.Select(elem =>
            {
                var id = _session.CacheElement(elem);
                return $"{{\"elementId\": \"{id}\", \"name\": \"{EscapeJson(elem.Name ?? "")}\", \"automationId\": \"{EscapeJson(elem.AutomationId ?? "")}\", \"controlType\": \"{elem.ControlType}\"}}";
            }).ToArray();

            var elementsJson = string.Join(",", elementInfos);
            return Task.FromResult(JsonDocument.Parse($"{{\"success\": true, \"count\": {elements.Count}, \"elements\": [{elementsJson}]}}").RootElement);
        }
        catch (Exception ex)
        {
            return Task.FromResult(ResponseHelper.CreateErrorResponse(ErrorCode.InternalError, ex.Message));
        }
    }
}
