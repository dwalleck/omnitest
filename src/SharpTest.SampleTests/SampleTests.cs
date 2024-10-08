namespace EnhancedTestFramework.Tests;

using SharpTest.Core;
using SharpTest.SampleTests;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


[TestClass]
public class FixtureTests
{
    [Test]
    [UseFixture("SimpleFixture")]
    public void TestWithSimpleFixture(int value)
    {
        Assert.AreEqual(42, value);
    }

    [Test]
    [UseFixture("DisposableFixture")]
    public void TestWithDisposableFixture(DisposableResource resource)
    {
        Assert.IsNotNull(resource);
        Assert.IsFalse(resource.IsDisposed);
    }

    [Test]
    [UseFixture("AsyncFixture")]
    public void TestWithAsyncFixture(Task<string> asyncResult)
    {
        Assert.AreEqual("Async Result", asyncResult.Result);
    }

    [Test]
    [UseFixture("SimpleFixture")]
    [UseFixture("DisposableFixture")]
    public void TestWithMultipleFixtures(int value, DisposableResource resource)
    {
        Assert.AreEqual(42, value);
        Assert.IsNotNull(resource);
    }
}

[TestClass]
public class TaggedTests
{
    [Test]
    [Tag("Fast")]
    public void FastTest()
    {
        Assert.IsTrue(true);
    }

    [Test]
    [Tag("Slow")]
    public void SlowTest()
    {
        System.Threading.Thread.Sleep(100);
        Assert.IsTrue(true);
    }
}

[TestClass]
public class TimeoutTests
{
    [Test]
    public void TestThatPasses()
    {
        Assert.IsTrue(true);
    }

    [Test]
    public void TestThatTimesOut()
    {
        System.Threading.Thread.Sleep(TimeSpan.FromSeconds(61));
    }
}

