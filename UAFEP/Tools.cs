using System;

namespace UAFEP
{
    public static class Tools
    {
        public static readonly Random  Random = new Random();

        public static double GetRdmFromNormal(double avg, double stdDev)
        {
            return avg + stdDev * (Math.Pow(-2 * Math.Log(Random.NextDouble()), 0.5) * Math.Cos(2 * Math.PI * Random.NextDouble()));
        }
    }
}
