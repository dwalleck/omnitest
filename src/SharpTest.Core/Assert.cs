using System;

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
                throw new AssertionException($"Assert failed. Expected: {expected}, Actual: {actual}");
            }
        }

        // Add more assertion methods as needed
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

