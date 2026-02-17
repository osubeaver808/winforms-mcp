using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Rhombus.WinFormsMcp.Server.Testing;

/// <summary>
/// Represents a test script with a sequence of actions and assertions
/// </summary>
public class TestScript
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("version")]
    public string Version { get; set; } = "1.0";

    [JsonPropertyName("created")]
    public DateTime Created { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("modified")]
    public DateTime Modified { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("author")]
    public string Author { get; set; } = Environment.UserName;

    [JsonPropertyName("variables")]
    public Dictionary<string, string> Variables { get; set; } = new();

    [JsonPropertyName("steps")]
    public List<TestStep> Steps { get; set; } = new();

    [JsonPropertyName("tags")]
    public List<string> Tags { get; set; } = new();

    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;
}

/// <summary>
/// Represents a single step in a test script
/// </summary>
public class TestStep
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "action"; // action, assertion, wait

    [JsonPropertyName("command")]
    public string Command { get; set; } = string.Empty;

    [JsonPropertyName("params")]
    public Dictionary<string, object> Params { get; set; } = new();

    [JsonPropertyName("storeResult")]
    public string? StoreResult { get; set; }

    [JsonPropertyName("expected")]
    public object? Expected { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("continueOnFailure")]
    public bool ContinueOnFailure { get; set; } = false;

    [JsonPropertyName("description")]
    public string? Description { get; set; }
}

/// <summary>
/// Test step execution status
/// </summary>
public enum TestStepStatus
{
    Pending,
    Running,
    Passed,
    Failed,
    Skipped
}

/// <summary>
/// Represents the execution result of a test step
/// </summary>
public class TestStepResult
{
    public int StepIndex { get; set; }
    public TestStep Step { get; set; } = new();
    public TestStepStatus Status { get; set; }
    public string? ActualValue { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public TimeSpan Duration => EndTime - StartTime;
}
