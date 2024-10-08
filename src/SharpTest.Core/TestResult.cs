namespace SharpTest.Core;

using System;
using System.Collections.Generic;
using System.Linq;


public enum TestOutcome
{
    Passed,
    Failed,
    Error,
    TimedOut
}

public sealed class TestResult
{
    public string TestName { get; }
    public TestOutcome Outcome { get; }
    public TimeSpan Duration { get; }
    public string Message { get; }
    public IReadOnlyList<string> Tags { get; }

    public TestResult(string testName, TestOutcome outcome, TimeSpan duration, string message, IEnumerable<string> tags)
    {
        TestName = testName ?? throw new ArgumentNullException(nameof(testName));
        Outcome = outcome;
        Duration = duration;
        Message = message;
        Tags = tags?.ToList().AsReadOnly() ?? throw new ArgumentNullException(nameof(tags));
    }
}