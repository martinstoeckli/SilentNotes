using System;

namespace SilentNotesTest
{
    /// <summary>
    /// Collection of helper functions for unit tests.
    /// </summary>
    public static class TestExtensions
    {
        /// <summary>
        /// Determines whether two <see cref="DateTime"/> values are within a specified tolerance of each other.
        /// </summary>
        /// <param name="expected">The expected DateTime value to compare against.</param>
        /// <param name="actual">The actual DateTime value to compare.</param>
        /// <param name="tolerance">The maximum allowed difference between the two dates.</param>
        /// <returns>Returns true if the absolute difference is inside the tolerance, otherwise false.</returns>
        public static bool AreNearlyEqual(DateTime expected, DateTime actual, TimeSpan tolerance)
        {
            TimeSpan difference = expected - actual;
            return difference.Duration() <= tolerance.Duration();
        }

        /// <summary>
        /// Determines whether the specified date and time is within 0.2s of the current UTC time.
        /// </summary>
        /// <param name="actual">The value to test.</param>
        /// <returns>Returns true if the specified date and time is nearly DateTime.UtcNow, otherwise false.</returns>
        public static bool IsNearlyUtcNow(DateTime actual)
        {
            return AreNearlyEqual(DateTime.UtcNow, actual, TimeSpan.FromMilliseconds(200));
        }
    }
}
