using System;
using System.Collections.Generic;
using FlaUI.Core.AutomationElements;

namespace Rhombus.WinFormsMcp.Server.Testing;

/// <summary>
/// Records user actions to create test scripts
/// </summary>
public class TestRecorder
{
    private TestScript? _currentScript;
    private bool _isRecording;
    private readonly Dictionary<AutomationElement, string> _elementIds = new();
    private int _nextElementId = 1;

    public bool IsRecording => _isRecording;
    public TestScript? CurrentScript => _currentScript;

    /// <summary>
    /// Start recording a new test script
    /// </summary>
    public void StartRecording(string name, string description = "")
    {
        if (_isRecording)
            throw new InvalidOperationException("Already recording");

        _currentScript = new TestScript
        {
            Name = name,
            Description = description,
            Created = DateTime.UtcNow,
            Modified = DateTime.UtcNow
        };

        _elementIds.Clear();
        _nextElementId = 1;
        _isRecording = true;
    }

    /// <summary>
    /// Stop recording and return the script
    /// </summary>
    public TestScript? StopRecording()
    {
        if (!_isRecording)
            return null;

        _isRecording = false;
        var script = _currentScript;
        _currentScript = null;
        
        return script;
    }

    /// <summary>
    /// Pause recording temporarily
    /// </summary>
    public void PauseRecording()
    {
        if (!_isRecording)
            throw new InvalidOperationException("Not recording");

        _isRecording = false;
    }

    /// <summary>
    /// Resume recording
    /// </summary>
    public void ResumeRecording()
    {
        if (_currentScript == null)
            throw new InvalidOperationException("No script to resume");

        _isRecording = true;
    }

    /// <summary>
    /// Record a find_element action
    /// </summary>
    public void RecordFindElement(AutomationElement element, string? automationId = null, string? name = null, string? className = null)
    {
        if (!_isRecording || _currentScript == null)
            return;

        var elementId = GetOrCreateElementId(element);
        var parameters = new Dictionary<string, object>();

        if (!string.IsNullOrEmpty(automationId))
            parameters["automationId"] = automationId;
        else if (!string.IsNullOrEmpty(name))
            parameters["name"] = name;
        else if (!string.IsNullOrEmpty(className))
            parameters["className"] = className;

        _currentScript.Steps.Add(new TestStep
        {
            Type = "action",
            Command = "find_element",
            Params = parameters,
            StoreResult = elementId,
            Description = $"Find element: {automationId ?? name ?? className}"
        });
    }

    /// <summary>
    /// Record a click action
    /// </summary>
    public void RecordClick(AutomationElement element, bool doubleClick = false, bool rightClick = false)
    {
        if (!_isRecording || _currentScript == null)
            return;

        var elementId = GetOrCreateElementId(element);
        var parameters = new Dictionary<string, object>
        {
            ["elementId"] = $"{{{{{elementId}}}}}"
        };

        if (doubleClick)
            parameters["doubleClick"] = true;
        if (rightClick)
            parameters["rightClick"] = true;

        _currentScript.Steps.Add(new TestStep
        {
            Type = "action",
            Command = "click_element",
            Params = parameters,
            Description = $"Click element ({(rightClick ? "right" : doubleClick ? "double" : "single")})"
        });
    }

    /// <summary>
    /// Record a type_text action
    /// </summary>
    public void RecordTypeText(AutomationElement element, string text, bool clearFirst = false)
    {
        if (!_isRecording || _currentScript == null)
            return;

        var elementId = GetOrCreateElementId(element);
        var parameters = new Dictionary<string, object>
        {
            ["elementId"] = $"{{{{{elementId}}}}}",
            ["text"] = text
        };

        if (clearFirst)
            parameters["clearFirst"] = true;

        _currentScript.Steps.Add(new TestStep
        {
            Type = "action",
            Command = "type_text",
            Params = parameters,
            Description = $"Type text: {text.Substring(0, Math.Min(20, text.Length))}"
        });
    }

    /// <summary>
    /// Record a wait action
    /// </summary>
    public void RecordWait(int durationMs)
    {
        if (!_isRecording || _currentScript == null)
            return;

        _currentScript.Steps.Add(new TestStep
        {
            Type = "wait",
            Command = "wait",
            Params = new Dictionary<string, object> { ["duration"] = durationMs },
            Description = $"Wait for {durationMs}ms"
        });
    }

    /// <summary>
    /// Record an assertion
    /// </summary>
    public void RecordAssertion(string command, Dictionary<string, object> parameters, object expected, string? message = null)
    {
        if (!_isRecording || _currentScript == null)
            return;

        _currentScript.Steps.Add(new TestStep
        {
            Type = "assertion",
            Command = command,
            Params = parameters,
            Expected = expected,
            Message = message ?? $"Assert {command}",
            Description = message
        });
    }

    /// <summary>
    /// Add a variable to the script
    /// </summary>
    public void AddVariable(string name, string value)
    {
        if (_currentScript == null)
            throw new InvalidOperationException("No active script");

        _currentScript.Variables[name] = value;
    }

    /// <summary>
    /// Add a tag to the script
    /// </summary>
    public void AddTag(string tag)
    {
        if (_currentScript == null)
            throw new InvalidOperationException("No active script");

        if (!_currentScript.Tags.Contains(tag))
            _currentScript.Tags.Add(tag);
    }

    /// <summary>
    /// Get or create an element ID for tracking
    /// </summary>
    private string GetOrCreateElementId(AutomationElement element)
    {
        if (_elementIds.TryGetValue(element, out var existingId))
            return existingId;

        var newId = $"element{_nextElementId++}";
        _elementIds[element] = newId;
        return newId;
    }
}
