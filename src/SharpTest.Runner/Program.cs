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

            var runner = new TestRunner();
            IReadOnlyList<TestResult> results;

            try
            {
                results = await runner.RunTestsAsync(testAssembly, includeTags, excludeTags);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error running tests: {ex.Message}");
                return;
            }

            TestReporter.GenerateReport(results);

            // Set exit code based on test results
            Environment.ExitCode = results.All(r => r.Passed) ? 0 : 1;
        }

        static void PrintUsage()
        {
            Console.WriteLine("Usage: TestFrameworkCLI.exe <path-to-test-assembly> [options]");
            Console.WriteLine("Options:");
            Console.WriteLine("  --include-tags <tag1,tag2,...>  Run only tests with these tags");
            Console.WriteLine("  --exclude-tags <tag1,tag2,...>  Exclude tests with these tags");
            Console.WriteLine("Example: TestFrameworkCLI.exe C:\\path\\to\\MyTests.dll --include-tags UnitTest,Fast");
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
    }
}
