namespace Modules.Acl.Internal.Utils
{
    using System;

    internal static class NumberUtils
    {
        #region Methods

        /// <summary>
        ///   Assert that a condition is met <c>true</c>; or not <c>false</c>
        /// </summary>
        ///
        /// <param name="actual">
        ///   The value to test against the given range.
        /// </param>
        /// <param name="min">
        ///   The minimum value to test for inclusively, may be <c>null</c>.
        /// </param>
        /// <param name="max">
        ///   The maximum value to test for inclusively, may be <c>null</c>.
        /// </param>
        /// 
        /// <returns>
        ///   <c>true</c> if a condition is met otherwise <c>false</c>.
        /// </returns>
        /// 
        /// <exception cref="System.ArgumentException">
        ///   Argument 'min' must have a value that is less than the value of 
        ///   argument 'max'.
        /// </exception>
        /// 
        /// <remarks>
        ///   <para>
        ///   If a call to the constructor is made with both <c>min</c> and
        ///   <c>max</c> specified as <c>null</c> then all calls to
        ///   this method will will return <c>true</c>.
        ///   </para>
        /// </remarks>
        public static bool InRange(decimal actual, decimal? min, decimal? max)
        {
            if (min != null && max != null)
            {
                if (min > max)
                {
                    throw new ArgumentException(
                        "Argument 'min' must have a value that is less than the value of argument 'max'.");
                }
            }

            var bmin = min != null && actual < min;
            var bmax = max != null && actual > max;

            var result = true;

            if (bmin || bmax)
            {
                result = false;
            }

            return result;
        }

        #endregion Methods
    }
}