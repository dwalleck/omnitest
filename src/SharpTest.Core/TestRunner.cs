namespace SharpTest.Core;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;


/// <summary>
/// Runs tests and manages test execution.
/// </summary>
public sealed class TestRunner
{
    private readonly ConcurrentDictionary<string, Func<IEnumerator<object>>> _fixtureCache = new();

    /// <summary>
    /// Runs all tests in the specified assembly.
    /// </summary>
    /// <param name="assembly">The assembly containing the tests.</param>
    /// <param name="includeTags">Tags to include in the test run. If null or empty, all tests are included.</param>
    /// <param name="excludeTags">Tags to exclude from the test run.</param>
    /// <returns>A list of test results.</returns>
    /// <exception cref="ArgumentNullException">Thrown when assembly is null.</exception>
    public async Task<IReadOnlyList<TestResult>> RunTestsAsync(Assembly assembly, IReadOnlySet<string> includeTags = null, IReadOnlySet<string> excludeTags = null)
    {
        if (assembly == null) throw new ArgumentNullException(nameof(assembly));

        var testClasses = assembly.GetTypes()
            .Where(t => t.GetCustomAttribute<TestClassAttribute>() != null);

        var fixtureMethods = assembly.GetTypes()
            .Where(t => t.Name == "<Program>$")
            .SelectMany(t => t.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            .Where(m => m.GetCustomAttribute<FixtureAttribute>() != null)
            .ToDictionary(m => m.Name, m => CreateFixtureDelegate(m));

        var allResults = new ConcurrentBag<TestResult>();

        await Task.WhenAll(testClasses.Select(testClass => RunTestClassAsync(testClass, fixtureMethods, allResults, includeTags, excludeTags)));

        return allResults.ToList().AsReadOnly();
    }

    /// <summary>
    /// Creates a delegate for a fixture method.
    /// </summary>
    /// <param name="fixtureMethod">The MethodInfo of the fixture method.</param>
    /// <returns>A delegate that returns an IEnumerator{object} for the fixture.</returns>
    private Func<IEnumerator<object>> CreateFixtureDelegate(MethodInfo fixtureMethod)
    {
        return () => ((IEnumerable<object>)fixtureMethod.Invoke(null, null)).GetEnumerator();
    }

    /// <summary>
    /// Runs all tests in a test class.
    /// </summary>
    /// <param name="testClass">The test class type.</param>
    /// <param name="fixtureMethods">Dictionary of available fixture methods.</param>
    /// <param name="results">Concurrent bag to store test results.</param>
    /// <param name="includeTags">Tags to include in the test run.</param>
    /// <param name="excludeTags">Tags to exclude from the test run.</param>
    private async Task RunTestClassAsync(Type testClass, Dictionary<string, Func<IEnumerator<object>>> fixtureMethods,
        ConcurrentBag<TestResult> results, IReadOnlySet<string> includeTags, IReadOnlySet<string> excludeTags)
    {
        var instance = Activator.CreateInstance(testClass);
        var testMethods = testClass.GetMethods()
            .Where(m => m.GetCustomAttribute<TestAttribute>() != null);

        await Task.WhenAll(testMethods.Select(testMethod =>
            RunTestMethodAsync(instance, testMethod, fixtureMethods, results, includeTags, excludeTags)));
    }

    /// <summary>
    /// Runs a single test method.
    /// </summary>
    /// <param name="instance">The instance of the test class.</param>
    /// <param name="testMethod">The test method to run.</param>
    /// <param name="fixtureMethods">Dictionary of available fixture methods.</param>
    /// <param name="results">Concurrent bag to store the test result.</param>
    /// <param name="includeTags">Tags to include in the test run.</param>
    /// <param name="excludeTags">Tags to exclude from the test run.</param>
    private async Task RunTestMethodAsync(object instance, MethodInfo testMethod,
        Dictionary<string, Func<IEnumerator<object>>> fixtureMethods, ConcurrentBag<TestResult> results,
        IReadOnlySet<string> includeTags, IReadOnlySet<string> excludeTags)
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
            await Task.Run(() => testMethod.Invoke(instance, fixtureValues.ToArray()));
            stopwatch.Stop();

            results.Add(new TestResult(testName, true, stopwatch.Elapsed, null, tags));
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            var errorMessage = ex.InnerException?.Message ?? ex.Message;
            results.Add(new TestResult(testName, false, stopwatch.Elapsed, errorMessage, tags));
        }
        finally
        {
            TeardownFixtures(fixtureEnumerators);
        }
    }

    /// <summary>
    /// Determines whether a test should be run based on its tags and the include/exclude tag sets.
    /// </summary>
    /// <param name="testTags">The tags of the test.</param>
    /// <param name="includeTags">Tags to include in the test run.</param>
    /// <param name="excludeTags">Tags to exclude from the test run.</param>
    /// <returns>True if the test should be run, false otherwise.</returns>
    private bool ShouldRunTest(IEnumerable<string> testTags, IReadOnlySet<string> includeTags, IReadOnlySet<string> excludeTags)
    {
        if (includeTags != null && includeTags.Any() && !testTags.Any(t => includeTags.Contains(t)))
            return false;
        if (excludeTags != null && excludeTags.Any() && testTags.Any(t => excludeTags.Contains(t)))
            return false;
        return true;
    }

    /// <summary>
    /// Sets up the fixtures for a test method.
    /// </summary>
    /// <param name="testMethod">The test method.</param>
    /// <param name="fixtureMethods">Dictionary of available fixture methods.</param>
    /// <param name="fixtureEnumerators">List to store the created fixture enumerators.</param>
    /// <returns>A list of fixture values to be passed to the test method.</returns>
    private List<object> SetupFixtures(MethodInfo testMethod, Dictionary<string, Func<IEnumerator<object>>> fixtureMethods, List<IEnumerator<object>> fixtureEnumerators)
    {
        var fixtureValues = new List<object>();
        var useFixtureAttrs = testMethod.GetCustomAttributes<UseFixtureAttribute>();

        foreach (var useFixtureAttr in useFixtureAttrs)
        {
            if (fixtureMethods.TryGetValue(useFixtureAttr.FixtureName, out var fixtureFunc))
            {
                var fixtureEnumerator = _fixtureCache.GetOrAdd(useFixtureAttr.FixtureName, _ => fixtureFunc)();
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

    /// <summary>
    /// Tears down the fixtures after a test method has run.
    /// </summary>
    /// <param name="fixtureEnumerators">The list of fixture enumerators to tear down.</param>
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
}

