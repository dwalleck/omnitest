using System;

namespace SharpTest.Core;

using System;
using System.Collections.Generic;
using System.Linq;


/// <summary>
/// Represents the result of a single test execution.
/// </summary>
public sealed class TestResult
{
    /// <summary>
    /// Gets the name of the test.
    /// </summary>
    public string TestName { get; }

    /// <summary>
    /// Gets a value indicating whether the test passed.
    /// </summary>
    public bool Passed { get; }

    /// <summary>
    /// Gets the duration of the test execution.
    /// </summary>
    public TimeSpan Duration { get; }

    /// <summary>
    /// Gets the error message if the test failed, or null if it passed.
    /// </summary>
    public string ErrorMessage { get; }

    /// <summary>
    /// Gets the list of tags associated with the test.
    /// </summary>
    public IReadOnlyList<string> Tags { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TestResult"/> class.
    /// </summary>
    /// <param name="testName">The name of the test.</param>
    /// <param name="passed">Whether the test passed.</param>
    /// <param name="duration">The duration of the test execution.</param>
    /// <param name="errorMessage">The error message if the test failed, or null if it passed.</param>
    /// <param name="tags">The tags associated with the test.</param>
    /// <exception cref="ArgumentNullException">Thrown when testName or tags is null.</exception>
    public TestResult(string testName, bool passed, TimeSpan duration, string errorMessage, IEnumerable<string> tags)
    {
        TestName = testName ?? throw new ArgumentNullException(nameof(testName));
        Passed = passed;
        Duration = duration;
        ErrorMessage = errorMessage;
        Tags = tags?.ToList().AsReadOnly() ?? throw new ArgumentNullException(nameof(tags));
    }
}

