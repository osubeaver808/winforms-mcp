using Rhombus.WinFormsMcp.Server.Server;

namespace Rhombus.WinFormsMcp.Server;

/// <summary>
/// fnWindowsMCP - MCP Server for WinForms Automation
///
/// This server provides tools for automating WinForms applications in a headless manner.
/// It communicates via JSON-RPC over stdio (compatible with Claude Code).
/// </summary>
class Program
{
    private static AutomationServer? _server;

    static async Task Main(string[] args)
    {
        try
        {
            _server = new AutomationServer();
            await _server.RunAsync();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Fatal error: {ex.Message}");
            Console.Error.WriteLine(ex.StackTrace);
            Environment.Exit(1);
        }
    }
}
