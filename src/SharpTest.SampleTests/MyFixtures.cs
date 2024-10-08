namespace SharpTest.SampleTests;

using SharpTest.Core;

[FixtureContainer]
public class TestFixtures
{
    [Fixture]
    public static IEnumerable<object> SimpleFixture()
    {
        Console.WriteLine("SimpleFixture: Setup");
        yield return 42;
        Console.WriteLine("SimpleFixture: Teardown");
    }

    [Fixture]
    public static IEnumerable<object> DisposableFixture()
    {
        Console.WriteLine("DisposableFixture: Setup");
        using var disposable = new DisposableResource();
        yield return disposable;
        Console.WriteLine("DisposableFixture: Teardown");
    }

    [Fixture]
    public static IEnumerable<object> AsyncFixture()
    {
        Console.WriteLine("AsyncFixture: Setup");
        yield return Task.FromResult("Async Result");
        Console.WriteLine("AsyncFixture: Teardown");
    }
}

public class DisposableResource : IDisposable
{
    public bool IsDisposed { get; private set; }

    public void Dispose()
    {
        IsDisposed = true;
    }
}
