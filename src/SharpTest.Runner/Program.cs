using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using SharpTest.Core;

namespace SharpTest.Runner
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length == 0)
            {
                PrintUsage();
                return;
            }

            var assemblyPath = args[0];
            Assembly testAssembly;

            try
            {
                testAssembly = Assembly.LoadFrom(assemblyPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading assembly: {ex.Message}");
                return;
            }

            var includeTags = GetTagsFromArgs(args, "--include-tags");
            var excludeTags = GetTagsFromArgs(args, "--exclude-tags");
            var maxParallelism = GetMaxParallelismFromArgs(args);
            var timeout = GetTimeoutFromArgs(args);

            var runner = new TestRunner();
            IReadOnlyList<TestResult> results;

            try
            {
                results = await runner.RunTestsAsync(testAssembly, includeTags, excludeTags, maxParallelism, timeout);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error running tests: {ex.Message}");
                return;
            }

            TestReporter.GenerateReport(results);

            // Set exit code based on test results
            Environment.ExitCode = results.All(r => r.Outcome == TestOutcome.Passed) ? 0 : 1;
        }

        static void PrintUsage()
        {
            Console.WriteLine("Usage: TestFrameworkCLI.exe <path-to-test-assembly> [options]");
            Console.WriteLine("Options:");
            Console.WriteLine("  --include-tags <tag1,tag2,...>  Run only tests with these tags");
            Console.WriteLine("  --exclude-tags <tag1,tag2,...>  Exclude tests with these tags");
            Console.WriteLine("  --parallel <number>             Number of tests to run in parallel");
            Console.WriteLine("                                  (default is the number of logical processors)");
            Console.WriteLine("  --timeout <seconds>             Timeout for each test in seconds");
            Console.WriteLine("                                  (default is 60 seconds)");
            Console.WriteLine("Example: TestFrameworkCLI.exe C:\\path\\to\\MyTests.dll --include-tags UnitTest,Fast --parallel 4 --timeout 30");
        }

        static HashSet<string> GetTagsFromArgs(string[] args, string option)
        {
            var index = Array.IndexOf(args, option);
            if (index >= 0 && index < args.Length - 1)
            {
                return new HashSet<string>(args[index + 1].Split(',', StringSplitOptions.RemoveEmptyEntries));
            }
            return null;
        }

        static int? GetMaxParallelismFromArgs(string[] args)
        {
            var index = Array.IndexOf(args, "--parallel");
            if (index >= 0 && index < args.Length - 1)
            {
                if (int.TryParse(args[index + 1], out int parallelism) && parallelism > 0)
                {
                    return parallelism;
                }
                else
                {
                    Console.WriteLine("Invalid parallelism value specified. Using default.");
                }
            }
            return null; // Default to null, which will use Environment.ProcessorCount
        }

        static TimeSpan? GetTimeoutFromArgs(string[] args)
        {
            var index = Array.IndexOf(args, "--timeout");
            if (index >= 0 && index < args.Length - 1)
            {
                if (int.TryParse(args[index + 1], out int seconds) && seconds > 0)
                {
                    return TimeSpan.FromSeconds(seconds);
                }
                else
                {
                    Console.WriteLine("Invalid timeout value specified. Using default.");
                }
            }
            return null; // Default to null, which will use the default timeout in TestRunner
        }
    }
}
