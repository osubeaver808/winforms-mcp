using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUI.Core.Definitions;
using FlaUI.UIA2;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using System.Threading.Tasks;

namespace Rhombus.WinFormsMcp.Server.Automation;

/// <summary>
/// Helper class for WinForms UI automation using FlaUI with UIA2 backend
/// </summary>
public class AutomationHelper : IAutomationHelper
{
    private UIA2Automation? _automation;
    private readonly Dictionary<string, Process> _launchedProcesses = new();
    private readonly object _lock = new object();

    public AutomationHelper()
    {
        _automation = new UIA2Automation();
    }

    /// <summary>
    /// Launch a WinForms application
    /// </summary>
    public Process LaunchApp(string path, string? arguments = null, string? workingDirectory = null)
    {
        var psi = new ProcessStartInfo
        {
            FileName = path,
            Arguments = arguments ?? string.Empty,
            WorkingDirectory = workingDirectory ?? string.Empty,
            UseShellExecute = false
        };

        var process = Process.Start(psi) ?? throw new InvalidOperationException($"Failed to launch {path}");
        process.WaitForInputIdle(5000);

        lock (_lock)
        {
            _launchedProcesses[process.Id.ToString()] = process;
        }

        return process;
    }

    /// <summary>
    /// Attach to a running process
    /// </summary>
    public Process AttachToProcess(int pid)
    {
        var process = Process.GetProcessById(pid);
        lock (_lock)
        {
            _launchedProcesses[pid.ToString()] = process;
        }
        return process;
    }

    /// <summary>
    /// Attach to a running process by name
    /// </summary>
    public Process AttachToProcessByName(string name)
    {
        var processes = Process.GetProcessesByName(name);
        if (processes.Length == 0)
            throw new InvalidOperationException($"No process found with name: {name}");

        var process = processes[0];
        lock (_lock)
        {
            _launchedProcesses[process.Id.ToString()] = process;
        }
        return process;
    }

    /// <summary>
    /// Get main window element of a process
    /// </summary>
    public AutomationElement? GetMainWindow(int pid)
    {
        if (_automation == null)
            throw new ObjectDisposedException(nameof(AutomationHelper));

        try
        {
            var process = Process.GetProcessById(pid);
            return _automation.FromHandle(process.MainWindowHandle);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Find element by AutomationId
    /// </summary>
    public AutomationElement? FindByAutomationId(string automationId, AutomationElement? parent = null, int timeoutMs = 5000)
    {
        if (_automation == null)
            throw new ObjectDisposedException(nameof(AutomationHelper));

        var condition = new PropertyCondition(_automation.PropertyLibrary.Element.AutomationId, automationId);
        return FindElement(condition, parent, timeoutMs);
    }

    /// <summary>
    /// Find element by Name
    /// </summary>
    public AutomationElement? FindByName(string name, AutomationElement? parent = null, int timeoutMs = 5000)
    {
        if (_automation == null)
            throw new ObjectDisposedException(nameof(AutomationHelper));

        var condition = new PropertyCondition(_automation.PropertyLibrary.Element.Name, name);
        return FindElement(condition, parent, timeoutMs);
    }

    /// <summary>
    /// Find element by ClassName
    /// </summary>
    public AutomationElement? FindByClassName(string className, AutomationElement? parent = null, int timeoutMs = 5000)
    {
        if (_automation == null)
            throw new ObjectDisposedException(nameof(AutomationHelper));

        var condition = new PropertyCondition(_automation.PropertyLibrary.Element.ClassName, className);
        return FindElement(condition, parent, timeoutMs);
    }

    /// <summary>
    /// Find element by ControlType
    /// </summary>
    public AutomationElement? FindByControlType(ControlType controlType, AutomationElement? parent = null, int timeoutMs = 5000)
    {
        if (_automation == null)
            throw new ObjectDisposedException(nameof(AutomationHelper));

        var condition = new PropertyCondition(_automation.PropertyLibrary.Element.ControlType, controlType);
        return FindElement(condition, parent, timeoutMs);
    }

    /// <summary>
    /// Find multiple elements matching condition
    /// </summary>
    public AutomationElement[]? FindAll(ConditionBase condition, AutomationElement? parent = null, int timeoutMs = 5000)
    {
        if (_automation == null)
            throw new ObjectDisposedException(nameof(AutomationHelper));

        var root = parent ?? _automation.GetDesktop();
        var stopwatch = Stopwatch.StartNew();

        while (stopwatch.ElapsedMilliseconds < timeoutMs)
        {
            try
            {
                var elements = root.FindAllChildren(condition);
                if (elements.Length > 0)
                    return elements;
            }
            catch { }

            Thread.Sleep(100);
        }

        return null;
    }

    /// <summary>
    /// Find element with retry/timeout
    /// </summary>
    private AutomationElement? FindElement(ConditionBase condition, AutomationElement? parent, int timeoutMs)
    {
        if (_automation == null)
            throw new ObjectDisposedException(nameof(AutomationHelper));

        var root = parent ?? _automation.GetDesktop();
        var stopwatch = Stopwatch.StartNew();

        while (stopwatch.ElapsedMilliseconds < timeoutMs)
        {
            try
            {
                var element = root.FindFirstChild(condition);
                if (element != null)
                    return element;
            }
            catch { }

            Thread.Sleep(100);
        }

        return null;
    }

    /// <summary>
    /// Check if element exists
    /// </summary>
    public bool ElementExists(string automationId, AutomationElement? parent = null)
    {
        return FindByAutomationId(automationId, parent, 1000) != null;
    }

    /// <summary>
    /// Click element with support for right-click
    /// </summary>
    public void Click(AutomationElement element, bool doubleClick = false, bool rightClick = false)
    {
        if (rightClick)
        {
            element.RightClick();
        }
        else if (doubleClick)
        {
            element.DoubleClick();
        }
        else
        {
            element.Click();
        }
    }

    /// <summary>
    /// Type text into element
    /// </summary>
    public void TypeText(AutomationElement element, string text, bool clearFirst = false)
    {
        element.Focus();

        if (clearFirst)
        {
            System.Windows.Forms.SendKeys.SendWait("^a");
            Thread.Sleep(100);
        }

        System.Windows.Forms.SendKeys.SendWait(text);
    }

    /// <summary>
    /// Set value on element
    /// </summary>
    public void SetValue(AutomationElement element, string value)
    {
        element.Focus();
        System.Windows.Forms.SendKeys.SendWait("^a");
        Thread.Sleep(50);
        System.Windows.Forms.SendKeys.SendWait(value);
    }

    /// <summary>
    /// Get element property
    /// </summary>
    public object? GetProperty(AutomationElement element, string propertyName)
    {
        return propertyName.ToLower() switch
        {
            "name" => element.Name,
            "automationid" => element.AutomationId,
            "classname" => element.ClassName,
            "controltype" => element.ControlType.ToString(),
            "isoffscreen" => element.IsOffscreen,
            "isenabled" => element.IsEnabled,
            _ => null
        };
    }

    /// <summary>
    /// Take screenshot of element or full desktop
    /// </summary>
    public void TakeScreenshot(string outputPath, AutomationElement? element = null)
    {
        try
        {
            Bitmap? bitmap = null;

            if (element != null)
            {
                bitmap = element.Capture();
            }
            else if (_automation != null)
            {
                var desktop = _automation.GetDesktop();
                bitmap = desktop.Capture();
            }

            if (bitmap != null)
            {
                bitmap.Save(outputPath, ImageFormat.Png);
                bitmap.Dispose();
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to take screenshot: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Drag and drop
    /// </summary>
    public void DragDrop(AutomationElement source, AutomationElement target)
    {
        var sourceBounds = source.BoundingRectangle;
        var targetBounds = target.BoundingRectangle;

        if (sourceBounds.Width == 0 || targetBounds.Width == 0)
            throw new InvalidOperationException("Source or target element has invalid bounding rectangle");

        // Simulate drag-drop using mouse movements
        var sourceCenter = new Point(
            (int)(sourceBounds.X + sourceBounds.Width / 2),
            (int)(sourceBounds.Y + sourceBounds.Height / 2)
        );

        var targetCenter = new Point(
            (int)(targetBounds.X + targetBounds.Width / 2),
            (int)(targetBounds.Y + targetBounds.Height / 2)
        );

        source.Focus();
        System.Windows.Forms.Cursor.Position = sourceCenter;
        Thread.Sleep(100);

        // Simulate mouse down, move, mouse up
        System.Windows.Forms.SendKeys.SendWait("{LDown}");
        System.Windows.Forms.Cursor.Position = targetCenter;
        Thread.Sleep(200);
        System.Windows.Forms.SendKeys.SendWait("{LUp}");
    }

    /// <summary>
    /// Send keyboard keys
    /// </summary>
    public void SendKeys(string keys)
    {
        System.Windows.Forms.SendKeys.SendWait(keys);
    }

    /// <summary>
    /// Close application
    /// </summary>
    public void CloseApp(int pid, bool force = false)
    {
        lock (_lock)
        {
            if (_launchedProcesses.TryGetValue(pid.ToString(), out var process))
            {
                try
                {
                    if (force)
                    {
                        process.Kill();
                    }
                    else
                    {
                        process.CloseMainWindow();
                        process.WaitForExit(5000);
                        if (!process.HasExited)
                            process.Kill();
                    }
                }
                catch { }
                finally
                {
                    _launchedProcesses.Remove(pid.ToString());
                }
            }
        }
    }

    /// <summary>
    /// Wait for element to appear
    /// </summary>
    public async Task<bool> WaitForElementAsync(string automationId, AutomationElement? parent = null, int timeoutMs = 10000)
    {
        var stopwatch = Stopwatch.StartNew();
        while (stopwatch.ElapsedMilliseconds < timeoutMs)
        {
            if (FindByAutomationId(automationId, parent, 500) != null)
                return true;

            await Task.Delay(100);
        }

        return false;
    }

    /// <summary>
    /// Get all child elements
    /// </summary>
    public AutomationElement[]? GetAllChildren(AutomationElement element)
    {
        try
        {
            return element.FindAllChildren();
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Find all elements by AutomationId
    /// </summary>
    public IEnumerable<AutomationElement> FindAllByAutomationId(string automationId, AutomationElement? parent = null, int timeoutMs = 5000)
    {
        if (_automation == null)
            throw new ObjectDisposedException(nameof(AutomationHelper));

        var condition = new PropertyCondition(_automation.PropertyLibrary.Element.AutomationId, automationId);
        var elements = FindAll(condition, parent, timeoutMs);
        return elements ?? Array.Empty<AutomationElement>();
    }

    /// <summary>
    /// Find all elements by Name
    /// </summary>
    public IEnumerable<AutomationElement> FindAllByName(string name, AutomationElement? parent = null, int timeoutMs = 5000)
    {
        if (_automation == null)
            throw new ObjectDisposedException(nameof(AutomationHelper));

        var condition = new PropertyCondition(_automation.PropertyLibrary.Element.Name, name);
        var elements = FindAll(condition, parent, timeoutMs);
        return elements ?? Array.Empty<AutomationElement>();
    }

    /// <summary>
    /// Find all elements by ClassName
    /// </summary>
    public IEnumerable<AutomationElement> FindAllByClassName(string className, AutomationElement? parent = null, int timeoutMs = 5000)
    {
        if (_automation == null)
            throw new ObjectDisposedException(nameof(AutomationHelper));

        var condition = new PropertyCondition(_automation.PropertyLibrary.Element.ClassName, className);
        var elements = FindAll(condition, parent, timeoutMs);
        return elements ?? Array.Empty<AutomationElement>();
    }

    /// <summary>
    /// Hover over an element
    /// </summary>
    public void Hover(AutomationElement element)
    {
        var bounds = element.BoundingRectangle;
        var centerPoint = new Point(
            (int)(bounds.X + bounds.Width / 2),
            (int)(bounds.Y + bounds.Height / 2)
        );

        System.Windows.Forms.Cursor.Position = centerPoint;
        Thread.Sleep(100);
    }

    /// <summary>
    /// Maximize window
    /// </summary>
    public void MaximizeWindow(int pid)
    {
        var window = GetMainWindow(pid);
        if (window != null)
        {
            var windowPattern = window.Patterns.Window.PatternOrDefault;
            windowPattern?.SetWindowVisualState(WindowVisualState.Maximized);
        }
    }

    /// <summary>
    /// Minimize window
    /// </summary>
    public void MinimizeWindow(int pid)
    {
        var window = GetMainWindow(pid);
        if (window != null)
        {
            var windowPattern = window.Patterns.Window.PatternOrDefault;
            windowPattern?.SetWindowVisualState(WindowVisualState.Minimized);
        }
    }

    /// <summary>
    /// Restore window
    /// </summary>
    public void RestoreWindow(int pid)
    {
        var window = GetMainWindow(pid);
        if (window != null)
        {
            var windowPattern = window.Patterns.Window.PatternOrDefault;
            windowPattern?.SetWindowVisualState(WindowVisualState.Normal);
        }
    }

    /// <summary>
    /// Get window title
    /// </summary>
    public string GetWindowTitle(int pid)
    {
        var window = GetMainWindow(pid);
        return window?.Name ?? "";
    }

    /// <summary>
    /// Get window state
    /// </summary>
    public string GetWindowState(int pid)
    {
        var window = GetMainWindow(pid);
        if (window != null)
        {
            var windowPattern = window.Patterns.Window.PatternOrDefault;
            if (windowPattern != null)
            {
                return windowPattern.WindowVisualState.ToString();
            }
        }
        return "Unknown";
    }

    /// <summary>
    /// Scroll to element
    /// </summary>
    public void ScrollToElement(AutomationElement element)
    {
        try
        {
            var scrollItemPattern = element.Patterns.ScrollItem.PatternOrDefault;
            scrollItemPattern?.ScrollIntoView();
        }
        catch
        {
            // If ScrollItem pattern not supported, try to bring into view
            element.Focus();
        }
    }

    /// <summary>
    /// Scroll window in a direction
    /// </summary>
    public void ScrollWindow(AutomationElement element, string direction, int amount)
    {
        var scrollPattern = element.Patterns.Scroll.PatternOrDefault;
        if (scrollPattern == null)
            throw new InvalidOperationException("Element does not support scrolling");

        var scrollAmount = amount switch
        {
            1 => FlaUI.Core.Definitions.ScrollAmount.SmallIncrement,
            _ => FlaUI.Core.Definitions.ScrollAmount.LargeIncrement
        };

        switch (direction.ToLower())
        {
            case "up":
                scrollPattern.Scroll(FlaUI.Core.Definitions.ScrollAmount.NoAmount, FlaUI.Core.Definitions.ScrollAmount.SmallDecrement);
                break;
            case "down":
                scrollPattern.Scroll(FlaUI.Core.Definitions.ScrollAmount.NoAmount, scrollAmount);
                break;
            case "left":
                scrollPattern.Scroll(FlaUI.Core.Definitions.ScrollAmount.SmallDecrement, FlaUI.Core.Definitions.ScrollAmount.NoAmount);
                break;
            case "right":
                scrollPattern.Scroll(scrollAmount, FlaUI.Core.Definitions.ScrollAmount.NoAmount);
                break;
            default:
                throw new ArgumentException($"Invalid scroll direction: {direction}");
        }
    }

    /// <summary>
    /// Press key combination (e.g., Ctrl+C, Alt+F4)
    /// </summary>
    public void PressKeyCombination(string keys)
    {
        // Parse and send key combination
        // FlaUI doesn't have built-in key combo support, so we use SendKeys
        var parsedKeys = ParseKeyCombination(keys);
        System.Windows.Forms.SendKeys.SendWait(parsedKeys);
    }

    private string ParseKeyCombination(string keys)
    {
        // Convert readable format to SendKeys format
        // E.g., "Ctrl+C" -> "^c", "Alt+F4" -> "%{F4}"
        var result = keys
            .Replace("Ctrl+", "^")
            .Replace("Alt+", "%")
            .Replace("Shift+", "+")
            .Replace("Win+", "^{ESC}"); // Windows key approximation

        return result;
    }

    public void Dispose()
    {
        lock (_lock)
        {
            foreach (var process in _launchedProcesses.Values)
            {
                try
                {
                    if (!process.HasExited)
                        process.Kill();
                }
                catch { }
            }

            _launchedProcesses.Clear();
        }

        _automation?.Dispose();
        _automation = null;
    }
}
