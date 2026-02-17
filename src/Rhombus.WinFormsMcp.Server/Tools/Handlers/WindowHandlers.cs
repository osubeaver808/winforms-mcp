using System.Text.Json;
using Rhombus.WinFormsMcp.Server.Session;

namespace Rhombus.WinFormsMcp.Server.Tools.Handlers;

/// <summary>
/// Handles window management operations (maximize, minimize, restore, title, state)
/// </summary>
public class WindowHandlers : ToolHandlerBase
{
    private readonly SessionManager _session;

    public WindowHandlers(SessionManager session)
    {
        _session = session;
    }

    public Task<JsonElement> MaximizeWindow(JsonElement args)
    {
        try
        {
            var pid = GetIntArg(args, "pid");
            var automation = _session.GetAutomation();
            automation.MaximizeWindow(pid);

            return Task.FromResult(ResponseHelper.CreateSuccessResponse("Window maximized"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(ResponseHelper.CreateErrorResponse(ex.Message));
        }
    }

    public Task<JsonElement> MinimizeWindow(JsonElement args)
    {
        try
        {
            var pid = GetIntArg(args, "pid");
            var automation = _session.GetAutomation();
            automation.MinimizeWindow(pid);

            return Task.FromResult(ResponseHelper.CreateSuccessResponse("Window minimized"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(ResponseHelper.CreateErrorResponse(ex.Message));
        }
    }

    public Task<JsonElement> RestoreWindow(JsonElement args)
    {
        try
        {
            var pid = GetIntArg(args, "pid");
            var automation = _session.GetAutomation();
            automation.RestoreWindow(pid);

            return Task.FromResult(ResponseHelper.CreateSuccessResponse("Window restored"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(ResponseHelper.CreateErrorResponse(ex.Message));
        }
    }

    public Task<JsonElement> GetWindowTitle(JsonElement args)
    {
        try
        {
            var pid = GetIntArg(args, "pid");
            var automation = _session.GetAutomation();
            var title = automation.GetWindowTitle(pid);

            return Task.FromResult(JsonDocument.Parse($"{{\"success\": true, \"title\": \"{EscapeJson(title)}\"}}").RootElement);
        }
        catch (Exception ex)
        {
            return Task.FromResult(ResponseHelper.CreateErrorResponse(ex.Message));
        }
    }

    public Task<JsonElement> GetWindowState(JsonElement args)
    {
        try
        {
            var pid = GetIntArg(args, "pid");
            var automation = _session.GetAutomation();
            var state = automation.GetWindowState(pid);

            return Task.FromResult(JsonDocument.Parse($"{{\"success\": true, \"state\": \"{state}\"}}").RootElement);
        }
        catch (Exception ex)
        {
            return Task.FromResult(ResponseHelper.CreateErrorResponse(ex.Message));
        }
    }
}
