using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UAFEP;

namespace UAFEPUnitTests
{
    [TestClass]
    public class MatchTests
    {
        [TestMethod]
        public void Match_Play_Nominal()
        {
            int drawsCount = 0;
            int totalGoals = 0;
            const int matchesCount = 1000000;

            for (int i = 0; i < matchesCount; i++)
            {
                var mu = new Match(new Team { Name = "A" }, new Team { Name = "B" });

                mu.Play();

                Assert.IsTrue(mu.Played);
                if (mu.GetWinner() == null)
                {
                    drawsCount++;
                }
                totalGoals += mu.Score1 + mu.Score2;
            }

            var goalAvg = Math.Round(totalGoals / (double)matchesCount, 1);
            Assert.IsTrue(goalAvg > 2.4 && goalAvg < 2.6);
            Assert.IsTrue(drawsCount > 230000 && drawsCount < 270000);
        }
    }
}
