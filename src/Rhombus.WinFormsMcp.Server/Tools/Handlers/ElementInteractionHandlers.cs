using System.Text.Json;
using Rhombus.WinFormsMcp.Server.Models;
using Rhombus.WinFormsMcp.Server.Session;

namespace Rhombus.WinFormsMcp.Server.Tools.Handlers;

/// <summary>
/// Handles element interaction operations (click, type, focus, hover, etc.)
/// </summary>
public class ElementInteractionHandlers : ToolHandlerBase
{
    private readonly SessionManager _session;

    public ElementInteractionHandlers(SessionManager session)
    {
        _session = session;
    }

    public Task<JsonElement> ClickElement(JsonElement args)
    {
        try
        {
            var elementId = GetStringArg(args, "elementId") ?? throw new ArgumentException("elementId is required");
            var doubleClick = GetBoolArg(args, "doubleClick", false);
            var rightClick = GetBoolArg(args, "rightClick", false);

            var element = _session.GetElement(elementId);
            if (element == null)
                return Task.FromResult(ResponseHelper.CreateErrorResponse(ErrorCode.ElementNotFound, "Element not found in session"));

            if (!_session.IsElementValid(element))
                return Task.FromResult(ResponseHelper.CreateErrorResponse(ErrorCode.ElementStale, "Element is stale"));

            var automation = _session.GetAutomation();
            automation.Click(element, doubleClick, rightClick);

            return Task.FromResult(ResponseHelper.CreateSuccessResponse("Element clicked"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(ResponseHelper.CreateErrorResponse(ErrorCode.InternalError, ex.Message));
        }
    }

    public Task<JsonElement> TypeText(JsonElement args)
    {
        try
        {
            var elementId = GetStringArg(args, "elementId") ?? throw new ArgumentException("elementId is required");
            var text = GetStringArg(args, "text") ?? "";
            var clearFirst = GetBoolArg(args, "clearFirst", false);

            var element = _session.GetElement(elementId);
            if (element == null)
                return Task.FromResult(ResponseHelper.CreateErrorResponse("Element not found in session"));

            var automation = _session.GetAutomation();
            automation.TypeText(element, text, clearFirst);

            return Task.FromResult(ResponseHelper.CreateSuccessResponse("Text typed"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(ResponseHelper.CreateErrorResponse(ex.Message));
        }
    }

    public Task<JsonElement> SetValue(JsonElement args)
    {
        try
        {
            var elementId = GetStringArg(args, "elementId") ?? throw new ArgumentException("elementId is required");
            var value = GetStringArg(args, "value") ?? "";

            var element = _session.GetElement(elementId);
            if (element == null)
                return Task.FromResult(ResponseHelper.CreateErrorResponse("Element not found in session"));

            var automation = _session.GetAutomation();
            automation.SetValue(element, value);

            return Task.FromResult(ResponseHelper.CreateSuccessResponse("Value set"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(ResponseHelper.CreateErrorResponse(ex.Message));
        }
    }

    public Task<JsonElement> FocusElement(JsonElement args)
    {
        try
        {
            var elementId = GetStringArg(args, "elementId") ?? throw new ArgumentException("elementId is required");

            var element = _session.GetElement(elementId);
            if (element == null)
                return Task.FromResult(ResponseHelper.CreateErrorResponse(ErrorCode.ElementNotFound, "Element not found in session"));

            if (!_session.IsElementValid(element))
                return Task.FromResult(ResponseHelper.CreateErrorResponse(ErrorCode.ElementStale, "Element is stale"));

            element.Focus();
            return Task.FromResult(ResponseHelper.CreateSuccessResponse("Element focused"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(ResponseHelper.CreateErrorResponse(ErrorCode.InternalError, ex.Message));
        }
    }

    public Task<JsonElement> HoverElement(JsonElement args)
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
            automation.Hover(element);

            return Task.FromResult(ResponseHelper.CreateSuccessResponse("Hover action completed"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(ResponseHelper.CreateErrorResponse(ErrorCode.InternalError, ex.Message));
        }
    }

    public Task<JsonElement> DragDrop(JsonElement args)
    {
        try
        {
            var sourceElementId = GetStringArg(args, "sourceElementId") ?? throw new ArgumentException("sourceElementId is required");
            var targetElementId = GetStringArg(args, "targetElementId") ?? throw new ArgumentException("targetElementId is required");

            var sourceElement = _session.GetElement(sourceElementId);
            var targetElement = _session.GetElement(targetElementId);

            if (sourceElement == null || targetElement == null)
                return Task.FromResult(ResponseHelper.CreateErrorResponse("Source or target element not found in session"));

            var automation = _session.GetAutomation();
            automation.DragDrop(sourceElement, targetElement);

            return Task.FromResult(ResponseHelper.CreateSuccessResponse("Drag and drop completed"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(ResponseHelper.CreateErrorResponse(ex.Message));
        }
    }

    public Task<JsonElement> ScrollToElement(JsonElement args)
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
            automation.ScrollToElement(element);

            return Task.FromResult(ResponseHelper.CreateSuccessResponse("Scrolled to element"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(ResponseHelper.CreateErrorResponse(ErrorCode.InternalError, ex.Message));
        }
    }

    public Task<JsonElement> ScrollWindow(JsonElement args)
    {
        try
        {
            var elementId = GetStringArg(args, "elementId") ?? throw new ArgumentException("elementId is required");
            var direction = GetStringArg(args, "direction") ?? "down";
            var amount = GetIntArg(args, "amount", 1);

            var element = _session.GetElement(elementId);
            if (element == null)
                return Task.FromResult(ResponseHelper.CreateErrorResponse(ErrorCode.ElementNotFound, "Element not found in session"));

            if (!_session.IsElementValid(element))
                return Task.FromResult(ResponseHelper.CreateErrorResponse(ErrorCode.ElementStale, "Element is stale"));

            var automation = _session.GetAutomation();
            automation.ScrollWindow(element, direction, amount);

            return Task.FromResult(ResponseHelper.CreateSuccessResponse("Scroll completed"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(ResponseHelper.CreateErrorResponse(ErrorCode.InternalError, ex.Message));
        }
    }
}
