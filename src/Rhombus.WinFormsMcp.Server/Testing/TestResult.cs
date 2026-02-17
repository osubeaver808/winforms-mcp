using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Rhombus.WinFormsMcp.Server.Testing;

/// <summary>
/// Overall test execution status
/// </summary>
public enum TestStatus
{
    NotRun,
    Running,
    Passed,
    Failed,
    PartiallyPassed
}

/// <summary>
/// Represents the overall result of a test script execution
/// </summary>
public class TestResult
{
    [JsonPropertyName("scriptName")]
    public string ScriptName { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public TestStatus Status { get; set; } = TestStatus.NotRun;

    [JsonPropertyName("startTime")]
    public DateTime StartTime { get; set; }

    [JsonPropertyName("endTime")]
    public DateTime EndTime { get; set; }

    [JsonPropertyName("duration")]
    public double DurationMs => (EndTime - StartTime).TotalMilliseconds;

    [JsonPropertyName("stepResults")]
    public List<TestStepResult> StepResults { get; set; } = new();

    [JsonPropertyName("totalSteps")]
    public int TotalSteps { get; set; }

    [JsonPropertyName("passedSteps")]
    public int PassedSteps => StepResults.Count(r => r.Status == TestStepStatus.Passed);

    [JsonPropertyName("failedSteps")]
    public int FailedSteps => StepResults.Count(r => r.Status == TestStepStatus.Failed);

    [JsonPropertyName("skippedSteps")]
    public int SkippedSteps => StepResults.Count(r => r.Status == TestStepStatus.Skipped);

    [JsonPropertyName("errorMessage")]
    public string? ErrorMessage { get; set; }

    [JsonPropertyName("screenshots")]
    public List<string> Screenshots { get; set; } = new();
}
