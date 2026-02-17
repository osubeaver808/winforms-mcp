using FlaUI.Core.AutomationElements;
using Rhombus.WinFormsMcp.Server.Automation;

namespace Rhombus.WinFormsMcp.Server.Session;

/// <summary>
/// Session manager for tracking automation contexts and element references
/// </summary>
public class SessionManager : IDisposable
{
    private readonly Dictionary<string, AutomationElement> _elementCache = new();
    private readonly Dictionary<int, object> _processContext = new();
    private int _nextElementId = 1;
    private AutomationHelper? _automation;
    private int _defaultTimeout = 10000;

    public int DefaultTimeout
    {
        get => _defaultTimeout;
        set => _defaultTimeout = value > 0 ? value : 10000;
    }

    public AutomationHelper GetAutomation()
    {
        return _automation ??= new AutomationHelper();
    }

    public string CacheElement(AutomationElement element)
    {
        var id = $"elem_{_nextElementId++}";
        _elementCache[id] = element;
        return id;
    }

    public AutomationElement? GetElement(string elementId)
    {
        return _elementCache.TryGetValue(elementId, out var elem) ? elem : null;
    }

    public bool IsElementValid(AutomationElement element)
    {
        try
        {
            _ = element.Name;
            return true;
        }
        catch
        {
            return false;
        }
    }

    public void ClearElement(string elementId)
    {
        _elementCache.Remove(elementId);
    }

    public void ClearAllElements()
    {
        _elementCache.Clear();
    }

    public IEnumerable<string> GetCachedElementIds()
    {
        return _elementCache.Keys.ToList();
    }

    public void CacheProcess(int pid, object context)
    {
        _processContext[pid] = context;
    }

    public IEnumerable<int> GetCachedProcessIds()
    {
        return _processContext.Keys.ToList();
    }

    public void Dispose()
    {
        _automation?.Dispose();
    }
}
