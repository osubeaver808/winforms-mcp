using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FlaUI.Core.AutomationElements;
using Rhombus.WinFormsMcp.Server.Automation;

namespace Rhombus.WinFormsMcp.Server.Testing;

/// <summary>
/// Executes test scripts step by step
/// </summary>
public class TestRunner
{
    private readonly AutomationHelper _automation;
    private readonly Dictionary<string, AutomationElement> _elementCache = new();
    private readonly Dictionary<string, string> _variables = new();
    private TestResult? _currentResult;

    public TestRunner(AutomationHelper automation)
    {
        _automation = automation;
    }

    /// <summary>
    /// Execute a test script and return the results
    /// </summary>
    public async Task<TestResult> ExecuteAsync(TestScript script, Dictionary<string, string>? parameters = null)
    {
        _currentResult = new TestResult
        {
            ScriptName = script.Name,
            Status = TestStatus.Running,
            StartTime = DateTime.UtcNow,
            TotalSteps = script.Steps.Count
        };

        // Initialize variables
        _variables.Clear();
        foreach (var kvp in script.Variables)
        {
            _variables[kvp.Key] = kvp.Value;
        }

        // Override with runtime parameters
        if (parameters != null)
        {
            foreach (var kvp in parameters)
            {
                _variables[kvp.Key] = kvp.Value;
            }
        }

        // Execute each step
        for (int i = 0; i < script.Steps.Count; i++)
        {
            var step = script.Steps[i];
            var stepResult = await ExecuteStepAsync(step, i);
            _currentResult.StepResults.Add(stepResult);

            // Stop on failure unless continueOnFailure is set
            if (stepResult.Status == TestStepStatus.Failed && !step.ContinueOnFailure)
            {
                _currentResult.ErrorMessage = stepResult.ErrorMessage;
                break;
            }
        }

        // Determine final status
        _currentResult.EndTime = DateTime.UtcNow;
        _currentResult.Status = DetermineOverallStatus(_currentResult);

        return _currentResult;
    }

    /// <summary>
    /// Execute a single test step
    /// </summary>
    private async Task<TestStepResult> ExecuteStepAsync(TestStep step, int stepIndex)
    {
        var result = new TestStepResult
        {
            StepIndex = stepIndex,
            Step = step,
            Status = TestStepStatus.Running,
            StartTime = DateTime.UtcNow
        };

        try
        {
            // Resolve variables in params
            var resolvedParams = ResolveVariables(step.Params);

            switch (step.Type.ToLower())
            {
                case "action":
                    await ExecuteActionAsync(step.Command, resolvedParams, step.StoreResult);
                    result.Status = TestStepStatus.Passed;
                    break;

                case "assertion":
                    var assertionResult = await ExecuteAssertionAsync(step.Command, resolvedParams, step.Expected);
                    result.ActualValue = assertionResult.actualValue;
                    result.Status = assertionResult.passed ? TestStepStatus.Passed : TestStepStatus.Failed;
                    if (!assertionResult.passed)
                    {
                        result.ErrorMessage = step.Message ?? $"Assertion failed: expected '{step.Expected}', got '{assertionResult.actualValue}'";
                    }
                    break;

                case "wait":
                    await Task.Delay(GetIntParam(resolvedParams, "duration", 1000));
                    result.Status = TestStepStatus.Passed;
                    break;

                default:
                    throw new InvalidOperationException($"Unknown step type: {step.Type}");
            }
        }
        catch (Exception ex)
        {
            result.Status = TestStepStatus.Failed;
            result.ErrorMessage = ex.Message;
        }

        result.EndTime = DateTime.UtcNow;
        return result;
    }

    /// <summary>
    /// Execute an action command
    /// </summary>
    private async Task ExecuteActionAsync(string command, Dictionary<string, object> parameters, string? storeResult)
    {
        object? resultValue = null;

        switch (command.ToLower())
        {
            case "launch_app":
                var process = _automation.LaunchApp(
                    GetStringParam(parameters, "path"),
                    GetStringParam(parameters, "arguments"),
                    GetStringParam(parameters, "workingDirectory")
                );
                resultValue = process.Id.ToString();
                break;

            case "find_element":
                var element = FindElement(parameters);
                if (element != null)
                {
                    var elemId = $"elem_{_elementCache.Count}";
                    _elementCache[elemId] = element;
                    resultValue = elemId;
                }
                break;

            case "click_element":
                var clickElement = GetElementFromCache(GetStringParam(parameters, "elementId"));
                _automation.Click(clickElement,
                    GetBoolParam(parameters, "doubleClick"),
                    GetBoolParam(parameters, "rightClick"));
                break;

            case "type_text":
                var typeElement = GetElementFromCache(GetStringParam(parameters, "elementId"));
                _automation.TypeText(typeElement,
                    GetStringParam(parameters, "text"),
                    GetBoolParam(parameters, "clearFirst"));
                break;

            case "set_value":
                var valueElement = GetElementFromCache(GetStringParam(parameters, "elementId"));
                _automation.SetValue(valueElement, GetStringParam(parameters, "value"));
                break;

            case "wait_for_element":
                var found = await _automation.WaitForElementAsync(
                    GetStringParam(parameters, "automationId"),
                    null,
                    GetIntParam(parameters, "timeoutMs", 10000)
                );
                resultValue = found.ToString();
                break;

            case "take_screenshot":
                _automation.TakeScreenshot(
                    GetStringParam(parameters, "outputPath"),
                    parameters.ContainsKey("elementId") ? GetElementFromCache(GetStringParam(parameters, "elementId")) : null
                );
                break;

            case "close_app":
                _automation.CloseApp(
                    GetIntParam(parameters, "pid"),
                    GetBoolParam(parameters, "force")
                );
                break;

            default:
                throw new InvalidOperationException($"Unknown action command: {command}");
        }

        // Store result in variables if requested
        if (!string.IsNullOrEmpty(storeResult) && resultValue != null)
        {
            _variables[storeResult] = resultValue.ToString()!;
        }
    }

    /// <summary>
    /// Execute an assertion command
    /// </summary>
    private async Task<(bool passed, string actualValue)> ExecuteAssertionAsync(string command, Dictionary<string, object> parameters, object? expected)
    {
        string actualValue = "";
        bool passed = false;

        switch (command.ToLower())
        {
            case "element_exists":
                var automationId = GetStringParam(parameters, "automationId");
                var exists = _automation.ElementExists(automationId);
                actualValue = exists.ToString();
                passed = exists == Convert.ToBoolean(expected);
                break;

            case "get_element_value":
                var element = GetElementFromCache(GetStringParam(parameters, "elementId"));
                var value = _automation.GetProperty(element, "Value") ?? _automation.GetProperty(element, "Text");
                actualValue = value?.ToString() ?? "";
                passed = actualValue == expected?.ToString();
                break;

            case "get_element_text":
                var textElement = GetElementFromCache(GetStringParam(parameters, "elementId"));
                actualValue = textElement.Name ?? "";
                passed = actualValue == expected?.ToString();
                break;

            case "get_element_state":
                var stateElement = GetElementFromCache(GetStringParam(parameters, "elementId"));
                var property = GetStringParam(parameters, "property", "IsEnabled");
                actualValue = property.ToLower() switch
                {
                    "isenabled" => stateElement.IsEnabled.ToString(),
                    "isoffscreen" => stateElement.IsOffscreen.ToString(),
                    _ => ""
                };
                passed = actualValue == expected?.ToString();
                break;

            default:
                throw new InvalidOperationException($"Unknown assertion command: {command}");
        }

        return (passed, actualValue);
    }

    /// <summary>
    /// Find element based on parameters
    /// </summary>
    private AutomationElement? FindElement(Dictionary<string, object> parameters)
    {
        if (parameters.ContainsKey("automationId"))
            return _automation.FindByAutomationId(GetStringParam(parameters, "automationId"));

        if (parameters.ContainsKey("name"))
            return _automation.FindByName(GetStringParam(parameters, "name"));

        if (parameters.ContainsKey("className"))
            return _automation.FindByClassName(GetStringParam(parameters, "className"));

        return null;
    }

    /// <summary>
    /// Get element from cache
    /// </summary>
    private AutomationElement GetElementFromCache(string? elementId)
    {
        if (string.IsNullOrEmpty(elementId) || !_elementCache.TryGetValue(elementId, out var element))
            throw new InvalidOperationException($"Element not found in cache: {elementId}");

        return element;
    }

    /// <summary>
    /// Resolve variable placeholders in parameters
    /// </summary>
    private Dictionary<string, object> ResolveVariables(Dictionary<string, object> parameters)
    {
        var resolved = new Dictionary<string, object>();

        foreach (var kvp in parameters)
        {
            var value = kvp.Value?.ToString() ?? "";
            
            // Replace {{variableName}} with actual value
            var matches = Regex.Matches(value, @"\{\{(\w+)\}\}");
            foreach (Match match in matches)
            {
                var varName = match.Groups[1].Value;
                if (_variables.TryGetValue(varName, out var varValue))
                {
                    value = value.Replace(match.Value, varValue);
                }
            }

            resolved[kvp.Key] = value;
        }

        return resolved;
    }

    /// <summary>
    /// Determine overall test status
    /// </summary>
    private TestStatus DetermineOverallStatus(TestResult result)
    {
        if (result.FailedSteps > 0 && result.PassedSteps > 0)
            return TestStatus.PartiallyPassed;

        if (result.FailedSteps > 0)
            return TestStatus.Failed;

        if (result.PassedSteps == result.TotalSteps)
            return TestStatus.Passed;

        return TestStatus.NotRun;
    }

    // Helper methods for parameter extraction
    private string GetStringParam(Dictionary<string, object> parameters, string key, string defaultValue = "")
    {
        return parameters.TryGetValue(key, out var value) ? value?.ToString() ?? defaultValue : defaultValue;
    }

    private int GetIntParam(Dictionary<string, object> parameters, string key, int defaultValue = 0)
    {
        if (parameters.TryGetValue(key, out var value))
        {
            if (value is int intVal) return intVal;
            if (int.TryParse(value?.ToString(), out var parsed)) return parsed;
        }
        return defaultValue;
    }

    private bool GetBoolParam(Dictionary<string, object> parameters, string key, bool defaultValue = false)
    {
        if (parameters.TryGetValue(key, out var value))
        {
            if (value is bool boolVal) return boolVal;
            if (bool.TryParse(value?.ToString(), out var parsed)) return parsed;
        }
        return defaultValue;
    }
}
