namespace SharpTest.Core;

using System;

/// <summary>
/// Provides assertion methods for tests.
/// </summary>
public static class Assert
{
    /// <summary>
    /// Verifies that two objects are equal.
    /// </summary>
    /// <param name="expected">The expected value.</param>
    /// <param name="actual">The actual value.</param>
    /// <exception cref="AssertionException">Thrown when the assertion fails.</exception>
    public static void AreEqual(object expected, object actual)
    {
        if (!Equals(expected, actual))
        {
            throw new AssertionException($"Assert.AreEqual failed. Expected: <{expected}>. Actual: <{actual}>.");
        }
    }

    /// <summary>
    /// Verifies that two objects are not equal.
    /// </summary>
    /// <param name="notExpected">The value that should not match actual.</param>
    /// <param name="actual">The actual value.</param>
    /// <exception cref="AssertionException">Thrown when the assertion fails.</exception>
    public static void AreNotEqual(object notExpected, object actual)
    {
        if (Equals(notExpected, actual))
        {
            throw new AssertionException($"Assert.AreNotEqual failed. Value was <{actual}>, but it should not have been.");
        }
    }

    /// <summary>
    /// Verifies that the specified condition is true.
    /// </summary>
    /// <param name="condition">The condition to verify.</param>
    /// <exception cref="AssertionException">Thrown when the condition is false.</exception>
    public static void IsTrue(bool condition)
    {
        if (!condition)
        {
            throw new AssertionException("Assert.IsTrue failed.");
        }
    }

    /// <summary>
    /// Verifies that the specified condition is false.
    /// </summary>
    /// <param name="condition">The condition to verify.</param>
    /// <exception cref="AssertionException">Thrown when the condition is true.</exception>
    public static void IsFalse(bool condition)
    {
        if (condition)
        {
            throw new AssertionException("Assert.IsFalse failed.");
        }
    }

    /// <summary>
    /// Verifies that the specified object is null.
    /// </summary>
    /// <param name="value">The object to verify.</param>
    /// <exception cref="AssertionException">Thrown when the object is not null.</exception>
    public static void IsNull(object value)
    {
        if (value != null)
        {
            throw new AssertionException($"Assert.IsNull failed. Object was not null.");
        }
    }

    /// <summary>
    /// Verifies that the specified object is not null.
    /// </summary>
    /// <param name="value">The object to verify.</param>
    /// <exception cref="AssertionException">Thrown when the object is null.</exception>
    public static void IsNotNull(object value)
    {
        if (value == null)
        {
            throw new AssertionException("Assert.IsNotNull failed. Object was null.");
        }
    }

    /// <summary>
    /// Verifies that the specified action throws an exception of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of exception expected to be thrown.</typeparam>
    /// <param name="action">The action expected to throw an exception.</param>
    /// <exception cref="AssertionException">Thrown when the action does not throw the expected exception.</exception>
    public static void Throws<T>(Action action) where T : Exception
    {
        try
        {
            action();
        }
        catch (T)
        {
            return;
        }
        catch (Exception ex)
        {
            throw new AssertionException($"Assert.Throws failed. Expected exception of type {typeof(T).Name}, but {ex.GetType().Name} was thrown.");
        }

        throw new AssertionException($"Assert.Throws failed. Expected exception of type {typeof(T).Name}, but no exception was thrown.");
    }
}

/// <summary>
/// Exception thrown when an assertion fails.
/// </summary>
public sealed class AssertionException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AssertionException"/> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public AssertionException(string message) : base(message) { }
}
