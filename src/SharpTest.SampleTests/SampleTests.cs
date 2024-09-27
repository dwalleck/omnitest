using SharpTest.Core;


[TestClass]
public class ListTests
{
    [Test]
    [UseFixture("TestList")]
    public void TestListCount(List<int> testList)
    {
        Assert.AreEqual(3, testList.Count);
        testList.Add(4); // This modification won't affect other tests
    }

    [Test]
    [UseFixture("TestList")]
    public void TestListContains(List<int> testList)
    {
        Assert.AreEqual(true, testList.Contains(2));
        Assert.AreEqual(false, testList.Contains(4)); // This will pass, even if run after TestListCount
    }
}