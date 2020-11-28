using System;

namespace UAFEP
{
    /// <summary>
    /// Tool methods.
    /// </summary>
    public static class Tools
    {
        /// <summary>
        /// Random number generator.
        /// </summary>
        public static readonly Random  Random = new Random();

        /// <summary>
        /// Gets a random number following a normal distribution law.
        /// </summary>
        /// <param name="avg">Average.</param>
        /// <param name="stdDev">Standard deviation.</param>
        /// <returns>A random number.</returns>
        public static double GetRdmFromNormal(double avg, double stdDev)
        {
            return avg + stdDev * (Math.Pow(-2 * Math.Log(Random.NextDouble()), 0.5) * Math.Cos(2 * Math.PI * Random.NextDouble()));
        }
    }
}
