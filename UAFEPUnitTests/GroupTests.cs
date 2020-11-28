using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using UAFEP;

namespace UAFEPUnitTests
{
    [TestClass]
    public class GroupTests
    {
        [DataTestMethod]
        [DataRow(4)]
        [DataRow(6)]
        [DataRow(8)]
        [DataRow(10)]
        [DataRow(12)]
        [DataRow(14)]
        [DataRow(16)]
        [DataRow(18)]
        [DataRow(20)]
        [DataRow(22)]
        [DataRow(24)]
        public void Group_Ctor_Nominal_NoMoreThanTwoMatchesAwayOrHome_NeverTwiceTheSameMatchInARow(int teamsCount)
        {
            var teams = JsonConvert.DeserializeObject<List<Team>>(TestTools.GetFileContent("teams")).Take(teamsCount);

            var group = new Group(teams);

            foreach (var team in teams)
            {
                int cumulative = 0;
                bool? away = null;
                Team lastOpponent = null;
                int i = 0;
                foreach (var matchDay in group.MatchDays)
                {
                    var matchUp = matchDay.Matches.Single(m => m.IncludeTeam(team));
                    bool isAway = matchUp.AwayTeam == team;
                    var newlastOpponent = isAway ? matchUp.HomeTeam : matchUp.AwayTeam;
                    if (i != 3 || teamsCount != 4)
                    {
                        Assert.IsFalse(newlastOpponent == lastOpponent);
                    }
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
                    i++;
                }
            }
        }
    }
}
