namespace SharpTest.Core;

using System;


/// <summary>
/// Marks a method as a test method.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class TestAttribute : Attribute { }

/// <summary>
/// Marks a class as a test class.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class TestClassAttribute : Attribute { }

/// <summary>
/// Marks a method as a fixture method.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class FixtureAttribute : Attribute { }

/// <summary>
/// Marks a class as a fixture container.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class FixtureContainerAttribute : Attribute { }

/// <summary>
/// Specifies which fixture should be used for a test method.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class UseFixtureAttribute : Attribute
{
    /// <summary>
    /// Gets the name of the fixture to use.
    /// </summary>
    public string FixtureName { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="UseFixtureAttribute"/> class.
    /// </summary>
    /// <param name="fixtureName">The name of the fixture to use.</param>
    /// <exception cref="ArgumentException">Thrown when fixtureName is null or whitespace.</exception>
    public UseFixtureAttribute(string fixtureName)
    {
        if (string.IsNullOrWhiteSpace(fixtureName))
            throw new ArgumentException("Fixture name cannot be null or whitespace.", nameof(fixtureName));
        FixtureName = fixtureName;
    }
}

/// <summary>
/// Adds a tag to a test method.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class TagAttribute : Attribute
{
    /// <summary>
    /// Gets the name of the tag.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TagAttribute"/> class.
    /// </summary>
    /// <param name="name">The name of the tag.</param>
    /// <exception cref="ArgumentException">Thrown when name is null or whitespace.</exception>
    public TagAttribute(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tag name cannot be null or whitespace.", nameof(name));
        Name = name;
    }
}

