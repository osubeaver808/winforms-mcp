using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Rhombus.WinFormsMcp.Server.Testing;

/// <summary>
/// Manages test script storage and retrieval
/// </summary>
public class TestScriptManager
{
    private readonly string _scriptsDirectory;
    private readonly string _resultsDirectory;
    private readonly JsonSerializerOptions _jsonOptions;

    public TestScriptManager(string? baseDirectory = null)
    {
        baseDirectory ??= Path.Combine(Environment.CurrentDirectory, "test-scripts");
        _scriptsDirectory = Path.Combine(baseDirectory, "scripts");
        _resultsDirectory = Path.Combine(baseDirectory, "results");

        // Ensure directories exist
        Directory.CreateDirectory(_scriptsDirectory);
        Directory.CreateDirectory(_resultsDirectory);

        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    /// <summary>
    /// Save a test script to disk
    /// </summary>
    public async Task SaveScriptAsync(TestScript script)
    {
        if (string.IsNullOrEmpty(script.Name))
            throw new ArgumentException("Script name cannot be empty");

        script.Modified = DateTime.UtcNow;

        var fileName = SanitizeFileName(script.Name) + ".json";
        var filePath = Path.Combine(_scriptsDirectory, fileName);

        var json = JsonSerializer.Serialize(script, _jsonOptions);
        await File.WriteAllTextAsync(filePath, json);
    }

    /// <summary>
    /// Load a test script from disk
    /// </summary>
    public async Task<TestScript?> LoadScriptAsync(string scriptName)
    {
        var fileName = SanitizeFileName(scriptName) + ".json";
        var filePath = Path.Combine(_scriptsDirectory, fileName);

        if (!File.Exists(filePath))
            return null;

        var json = await File.ReadAllTextAsync(filePath);
        return JsonSerializer.Deserialize<TestScript>(json, _jsonOptions);
    }

    /// <summary>
    /// Delete a test script
    /// </summary>
    public bool DeleteScript(string scriptName)
    {
        var fileName = SanitizeFileName(scriptName) + ".json";
        var filePath = Path.Combine(_scriptsDirectory, fileName);

        if (!File.Exists(filePath))
            return false;

        File.Delete(filePath);
        return true;
    }

    /// <summary>
    /// List all available test scripts
    /// </summary>
    public async Task<List<TestScript>> ListScriptsAsync()
    {
        var scripts = new List<TestScript>();
        var files = Directory.GetFiles(_scriptsDirectory, "*.json");

        foreach (var file in files)
        {
            try
            {
                var json = await File.ReadAllTextAsync(file);
                var script = JsonSerializer.Deserialize<TestScript>(json, _jsonOptions);
                if (script != null)
                    scripts.Add(script);
            }
            catch
            {
                // Skip invalid files
            }
        }

        return scripts.OrderBy(s => s.Name).ToList();
    }

    /// <summary>
    /// Save test results to disk
    /// </summary>
    public async Task SaveResultAsync(TestResult result)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        var fileName = $"{SanitizeFileName(result.ScriptName)}_{timestamp}.json";
        var filePath = Path.Combine(_resultsDirectory, fileName);

        var json = JsonSerializer.Serialize(result, _jsonOptions);
        await File.WriteAllTextAsync(filePath, json);
    }

    /// <summary>
    /// Get test results for a specific script
    /// </summary>
    public async Task<List<TestResult>> GetResultsAsync(string scriptName, int maxResults = 10)
    {
        var results = new List<TestResult>();
        var pattern = $"{SanitizeFileName(scriptName)}_*.json";
        var files = Directory.GetFiles(_resultsDirectory, pattern)
            .OrderByDescending(f => File.GetCreationTimeUtc(f))
            .Take(maxResults);

        foreach (var file in files)
        {
            try
            {
                var json = await File.ReadAllTextAsync(file);
                var result = JsonSerializer.Deserialize<TestResult>(json, _jsonOptions);
                if (result != null)
                    results.Add(result);
            }
            catch
            {
                // Skip invalid files
            }
        }

        return results;
    }

    /// <summary>
    /// Get the latest test result for a script
    /// </summary>
    public async Task<TestResult?> GetLatestResultAsync(string scriptName)
    {
        var results = await GetResultsAsync(scriptName, 1);
        return results.FirstOrDefault();
    }

    /// <summary>
    /// Export test result to HTML
    /// </summary>
    public async Task<string> ExportResultToHtmlAsync(TestResult result)
    {
        var html = $@"<!DOCTYPE html>
<html>
<head>
    <title>Test Result: {result.ScriptName}</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 20px; }}
        .header {{ background: #f0f0f0; padding: 20px; border-radius: 5px; }}
        .passed {{ color: green; font-weight: bold; }}
        .failed {{ color: red; font-weight: bold; }}
        .step {{ margin: 10px 0; padding: 10px; border: 1px solid #ddd; }}
        .step-passed {{ border-left: 4px solid green; }}
        .step-failed {{ border-left: 4px solid red; }}
        table {{ width: 100%; border-collapse: collapse; margin: 20px 0; }}
        th, td {{ padding: 10px; text-align: left; border-bottom: 1px solid #ddd; }}
        th {{ background: #f0f0f0; }}
    </style>
</head>
<body>
    <div class='header'>
        <h1>Test Result: {result.ScriptName}</h1>
        <p><strong>Status:</strong> <span class='{result.Status.ToString().ToLower()}'>{result.Status}</span></p>
        <p><strong>Duration:</strong> {result.DurationMs:F2} ms</p>
        <p><strong>Start Time:</strong> {result.StartTime:yyyy-MM-dd HH:mm:ss}</p>
        <p><strong>End Time:</strong> {result.EndTime:yyyy-MM-dd HH:mm:ss}</p>
    </div>
    
    <h2>Summary</h2>
    <table>
        <tr>
            <th>Total Steps</th>
            <th>Passed</th>
            <th>Failed</th>
            <th>Skipped</th>
        </tr>
        <tr>
            <td>{result.TotalSteps}</td>
            <td class='passed'>{result.PassedSteps}</td>
            <td class='failed'>{result.FailedSteps}</td>
            <td>{result.SkippedSteps}</td>
        </tr>
    </table>

    <h2>Steps</h2>";

        foreach (var step in result.StepResults)
        {
            var statusClass = step.Status == TestStepStatus.Passed ? "step-passed" : "step-failed";
            html += $@"
    <div class='step {statusClass}'>
        <strong>Step {step.StepIndex + 1}:</strong> {step.Step.Command} - <span class='{step.Status.ToString().ToLower()}'>{step.Status}</span><br>
        <strong>Duration:</strong> {step.Duration.TotalMilliseconds:F2} ms<br>";

            if (!string.IsNullOrEmpty(step.ErrorMessage))
                html += $"<strong>Error:</strong> {step.ErrorMessage}<br>";

            if (!string.IsNullOrEmpty(step.ActualValue))
                html += $"<strong>Actual Value:</strong> {step.ActualValue}<br>";

            html += "</div>";
        }

        html += @"
</body>
</html>";

        var fileName = $"{SanitizeFileName(result.ScriptName)}_{result.StartTime:yyyyMMdd_HHmmss}.html";
        var filePath = Path.Combine(_resultsDirectory, fileName);
        await File.WriteAllTextAsync(filePath, html);

        return filePath;
    }

    /// <summary>
    /// Sanitize a file name
    /// </summary>
    private string SanitizeFileName(string fileName)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return string.Join("_", fileName.Split(invalid, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
    }

    /// <summary>
    /// Get scripts directory path
    /// </summary>
    public string GetScriptsDirectory() => _scriptsDirectory;

    /// <summary>
    /// Get results directory path
    /// </summary>
    public string GetResultsDirectory() => _resultsDirectory;
}
