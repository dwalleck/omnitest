namespace SharpTest.Core;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;


public sealed class TestRunner
{
    private readonly TimeSpan _defaultTimeout = TimeSpan.FromSeconds(60);

    public async Task<IReadOnlyList<TestResult>> RunTestsAsync(
        Assembly assembly,
        IReadOnlySet<string> includeTags = null,
        IReadOnlySet<string> excludeTags = null,
        int? maxParallelism = null,
        TimeSpan? timeout = null)
    {
        if (assembly == null) throw new ArgumentNullException(nameof(assembly));

        var testClasses = assembly.GetTypes()
            .Where(t => t.GetCustomAttribute<TestClassAttribute>() != null);

        var fixtureMethods = assembly.GetTypes()
            .Where(t => t.GetCustomAttribute<FixtureContainerAttribute>() != null)
            .SelectMany(t => t.GetMethods(BindingFlags.Static | BindingFlags.Public))
            .Where(m => m.GetCustomAttribute<FixtureAttribute>() != null)
            .ToDictionary(m => m.Name, m => CreateFixtureDelegate(m));

        var allResults = new ConcurrentBag<TestResult>();
        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = maxParallelism ?? Environment.ProcessorCount
        };

        var allTestMethods = testClasses
            .SelectMany(testClass => testClass.GetMethods()
                .Where(m => m.GetCustomAttribute<TestAttribute>() != null)
                .Select(m => new { TestClass = testClass, TestMethod = m }));

        await Parallel.ForEachAsync(allTestMethods, parallelOptions, async (testMethod, ct) =>
        {
            var instance = Activator.CreateInstance(testMethod.TestClass);
            await RunTestMethodAsync(instance, testMethod.TestMethod, fixtureMethods, allResults, includeTags, excludeTags, timeout ?? _defaultTimeout);
        });

        return allResults.ToList().AsReadOnly();
    }

    private async Task RunTestMethodAsync(object instance, MethodInfo testMethod,
        Dictionary<string, Func<IEnumerator<object>>> fixtureMethods, ConcurrentBag<TestResult> results,
        IReadOnlySet<string> includeTags, IReadOnlySet<string> excludeTags, TimeSpan timeout)
    {
        var tags = testMethod.GetCustomAttributes<TagAttribute>().Select(t => t.Name).ToList();

        if (!ShouldRunTest(tags, includeTags, excludeTags))
            return;

        var testName = $"{instance.GetType().Name}.{testMethod.Name}";
        var stopwatch = Stopwatch.StartNew();
        var fixtureEnumerators = new List<IEnumerator<object>>();

        try
        {
            var fixtureValues = SetupFixtures(testMethod, fixtureMethods, fixtureEnumerators);

            using var cts = new CancellationTokenSource(timeout);
            var testTask = Task.Run(() => testMethod.Invoke(instance, fixtureValues.ToArray()));
            var completedTask = await Task.WhenAny(testTask, Task.Delay(timeout, cts.Token));

            if (completedTask == testTask)
            {
                await testTask; // Propagate any exceptions
                stopwatch.Stop();
                results.Add(new TestResult(testName, TestOutcome.Passed, stopwatch.Elapsed, null, tags));
            }
            else
            {
                stopwatch.Stop();
                results.Add(new TestResult(testName, TestOutcome.TimedOut, stopwatch.Elapsed, $"Test timed out after {timeout.TotalSeconds} seconds", tags));
            }
        }
        catch (TargetInvocationException ex) when (ex.InnerException is AssertionException)
        {
            stopwatch.Stop();
            results.Add(new TestResult(testName, TestOutcome.Failed, stopwatch.Elapsed, ex.InnerException.Message, tags));
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            var errorMessage = ex.InnerException?.Message ?? ex.Message;
            results.Add(new TestResult(testName, TestOutcome.Error, stopwatch.Elapsed, errorMessage, tags));
        }
        finally
        {
            TeardownFixtures(fixtureEnumerators);
        }
    }

    private bool ShouldRunTest(IEnumerable<string> testTags, IReadOnlySet<string> includeTags, IReadOnlySet<string> excludeTags)
    {
        if (includeTags != null && includeTags.Any() && !testTags.Any(t => includeTags.Contains(t)))
            return false;
        if (excludeTags != null && excludeTags.Any() && testTags.Any(t => excludeTags.Contains(t)))
            return false;
        return true;
    }

    private List<object> SetupFixtures(MethodInfo testMethod, Dictionary<string, Func<IEnumerator<object>>> fixtureMethods, List<IEnumerator<object>> fixtureEnumerators)
    {
        var fixtureValues = new List<object>();
        var useFixtureAttrs = testMethod.GetCustomAttributes<UseFixtureAttribute>();

        foreach (var useFixtureAttr in useFixtureAttrs)
        {
            if (fixtureMethods.TryGetValue(useFixtureAttr.FixtureName, out var fixtureFunc))
            {
                var fixtureEnumerator = fixtureFunc();
                fixtureEnumerators.Add(fixtureEnumerator);
                fixtureEnumerator.MoveNext();
                fixtureValues.Add(fixtureEnumerator.Current);
            }
            else
            {
                throw new InvalidOperationException($"Fixture '{useFixtureAttr.FixtureName}' not found.");
            }
        }

        return fixtureValues;
    }

    private void TeardownFixtures(List<IEnumerator<object>> fixtureEnumerators)
    {
        foreach (var enumerator in fixtureEnumerators)
        {
            try
            {
                enumerator.MoveNext(); // Run teardown
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as appropriate
                Console.WriteLine($"Error during fixture teardown: {ex.Message}");
            }
            finally
            {
                enumerator.Dispose();
            }
        }
    }

    private Func<IEnumerator<object>> CreateFixtureDelegate(MethodInfo fixtureMethod)
    {
        return () => ((IEnumerable<object>)fixtureMethod.Invoke(null, null)).GetEnumerator();
    }
}