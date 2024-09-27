using System;
using System.Collections.Generic;
using SharpTest.Core;

namespace SharpTest.SampleTests;

[FixtureContainer]
public class MyFixtures : FixtureContainer
{
    [Fixture]
    public static IEnumerable<object> TestList()
    {
        var list = new List<int> { 1, 2, 3 };
        Console.WriteLine("Fixture: Initialized test list");
        yield return list;
        Console.WriteLine("Fixture: Clearing test list");
        list.Clear();
    }

    [Fixture]
    public static IEnumerable<object> TestDictionary()
    {
        var dict = new Dictionary<string, int> { { "a", 1 }, { "b", 2 }, { "c", 3 } };
        Console.WriteLine("Fixture: Initialized test dictionary");
        yield return dict;
        Console.WriteLine("Fixture: Clearing test dictionary");
        dict.Clear();
    }
}
