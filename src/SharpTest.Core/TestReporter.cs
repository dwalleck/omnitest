using System;

namespace SharpTest.Core;

using System;
using System.Collections.Generic;
using System.Linq;



    /// <summary>
    /// Provides methods for reporting test results.
    /// </summary>
    public static class TestReporter
{
    /// <summary>
    /// Generates and prints a report of the test results.
    /// </summary>
    /// <param name="results">The collection of test results to report.</param>
    /// <exception cref="ArgumentNullException">Thrown when results is null.</exception>
    public static void GenerateReport(IEnumerable<TestResult> results)
    {
        if (results == null) throw new ArgumentNullException(nameof(results));

        var resultList = results.ToList();
        Console.WriteLine("Test Execution Report");
        Console.WriteLine("=====================");
        Console.WriteLine($"Total Tests: {resultList.Count}");
        Console.WriteLine($"Passed: {resultList.Count(r => r.Passed)}");
        Console.WriteLine($"Failed: {resultList.Count(r => !r.Passed)}");
        Console.WriteLine($"Total Duration: {resultList.Sum(r => r.Duration.TotalSeconds):F2} seconds");
        Console.WriteLine("\nDetailed Results:");
        Console.WriteLine("==================");

        foreach (var result in resultList)
        {
            Console.WriteLine($"Test: {result.TestName}");
            Console.WriteLine($"Tags: {string.Join(", ", result.Tags)}");
            Console.WriteLine($"Status: {(result.Passed ? "Passed" : "Failed")}");
            Console.WriteLine($"Duration: {result.Duration.TotalMilliseconds:F2} ms");
            if (!result.Passed)
            {
                Console.WriteLine($"Error: {result.ErrorMessage}");
            }
            Console.WriteLine();
        }
    }
}
