using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UAFEP;

namespace UAFEPUnitTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void Group_Ctor_Nominal_NoMoreThanTwoMatchesAwayOrHome_NeverTwiceTheSameMatchInARow()
        {
            var teams = new List<Team>
            {
                new Team { Name = "PSG" },
                new Team { Name = "Bayern" },
                new Team { Name = "Liverpool" },
                new Team { Name = "Barcelone" },
                new Team { Name = "Juventus" },
                new Team { Name = "Real Madrid" }
            };

            var group = new Group(teams);

            foreach (var team in teams)
            {
                int cumulative = 0;
                bool? away = null;
                Team lastOpponent = null;
                foreach (var matchDay in group.MatchDays)
                {
                    var matchUp = matchDay.Matches.Single(m => m.IncludeTeam(team));
                    bool isAway = matchUp.AwayTeam == team;
                    var newlastOpponent = isAway ? matchUp.HomeTeam : matchUp.AwayTeam;
                    Assert.IsFalse(newlastOpponent == lastOpponent);
                    lastOpponent = newlastOpponent;
                    if (isAway == away)
                    {
                        cumulative++;
                        Assert.IsFalse(cumulative > 1);
                    }
                    else
                    {
                        away = isAway;
                        cumulative = 0;
                    }
                }
            }
        }
    }
}
