namespace SharpTest.Core;

using System;
using System.Collections.Generic;
using System.Linq;

public static class TestReporter
{
    public static void GenerateReport(IEnumerable<TestResult> results)
    {
        if (results == null) throw new ArgumentNullException(nameof(results));

        var resultList = results.ToList();
        Console.WriteLine("Test Execution Report");
        Console.WriteLine("=====================");
        Console.WriteLine($"Total Tests: {resultList.Count}");
        Console.WriteLine($"Passed: {resultList.Count(r => r.Outcome == TestOutcome.Passed)}");
        Console.WriteLine($"Failed: {resultList.Count(r => r.Outcome == TestOutcome.Failed)}");
        Console.WriteLine($"Errors: {resultList.Count(r => r.Outcome == TestOutcome.Error)}");
        Console.WriteLine($"Timed Out: {resultList.Count(r => r.Outcome == TestOutcome.TimedOut)}");
        Console.WriteLine($"Total Duration: {resultList.Sum(r => r.Duration.TotalSeconds):F2} seconds");
        Console.WriteLine("\nDetailed Results:");
        Console.WriteLine("==================");

        foreach (var result in resultList)
        {
            Console.WriteLine($"Test: {result.TestName}");
            Console.WriteLine($"Tags: {string.Join(", ", result.Tags)}");
            Console.WriteLine($"Outcome: {result.Outcome}");
            Console.WriteLine($"Duration: {result.Duration.TotalMilliseconds:F2} ms");
            if (result.Outcome != TestOutcome.Passed)
            {
                Console.WriteLine($"Message: {result.Message}");
            }
            Console.WriteLine();
        }
    }
}