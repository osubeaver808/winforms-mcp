using System.Text.Json;
using Rhombus.WinFormsMcp.Server.Session;

namespace Rhombus.WinFormsMcp.Server.Tools.Handlers;

/// <summary>
/// Handles process-related operations (launch, attach, close)
/// </summary>
public class ProcessHandlers : ToolHandlerBase
{
    private readonly SessionManager _session;

    public ProcessHandlers(SessionManager session)
    {
        _session = session;
    }

    public Task<JsonElement> LaunchApp(JsonElement args)
    {
        try
        {
            var path = GetStringArg(args, "path") ?? throw new ArgumentException("path is required");
            var arguments = GetStringArg(args, "arguments");
            var workingDirectory = GetStringArg(args, "workingDirectory");

            var automation = _session.GetAutomation();
            var process = automation.LaunchApp(path, arguments, workingDirectory);

            _session.CacheProcess(process.Id, process);

            return Task.FromResult(JsonDocument.Parse($"{{\"success\": true, \"pid\": {process.Id}, \"processName\": \"{process.ProcessName}\"}}").RootElement);
        }
        catch (Exception ex)
        {
            return Task.FromResult(ResponseHelper.CreateErrorResponse(ex.Message));
        }
    }

    public Task<JsonElement> AttachToProcess(JsonElement args)
    {
        try
        {
            var pid = GetIntArg(args, "pid");
            var processName = GetStringArg(args, "processName");

            var automation = _session.GetAutomation();
            var process = !string.IsNullOrEmpty(processName)
                ? automation.AttachToProcessByName(processName)
                : automation.AttachToProcess(pid);

            _session.CacheProcess(process.Id, process);

            return Task.FromResult(JsonDocument.Parse($"{{\"success\": true, \"pid\": {process.Id}, \"processName\": \"{process.ProcessName}\"}}").RootElement);
        }
        catch (Exception ex)
        {
            return Task.FromResult(ResponseHelper.CreateErrorResponse(ex.Message));
        }
    }

    public Task<JsonElement> CloseApp(JsonElement args)
    {
        try
        {
            var pid = GetIntArg(args, "pid");
            var force = GetBoolArg(args, "force", false);

            var automation = _session.GetAutomation();
            automation.CloseApp(pid, force);

            return Task.FromResult(ResponseHelper.CreateSuccessResponse("Application closed"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(ResponseHelper.CreateErrorResponse(ex.Message));
        }
    }

    public Task<JsonElement> ListProcesses(JsonElement args)
    {
        try
        {
            var processIds = _session.GetCachedProcessIds().ToArray();
            var pidsJson = string.Join(",", processIds);

            return Task.FromResult(JsonDocument.Parse($"{{\"success\": true, \"count\": {processIds.Length}, \"processIds\": [{pidsJson}]}}").RootElement);
        }
        catch (Exception ex)
        {
            return Task.FromResult(ResponseHelper.CreateErrorResponse(ex.Message));
        }
    }
}
