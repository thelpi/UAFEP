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
        [DataRow(3)]
        [DataRow(4)]
        [DataRow(5)]
        [DataRow(6)]
        [DataRow(7)]
        [DataRow(16)]
        [DataRow(18)]
        [DataRow(19)]
        [DataRow(20)]
        [DataRow(21)]
        [DataRow(22)]
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
                var opponents = new List<Team>();
                foreach (var match in group.GetTeamMatches(team, excludeExempt: true))
                {
                    bool isAway = match.AwayTeam == team;
                    var newlastOpponent = isAway ? match.HomeTeam : match.AwayTeam;
                    if (i != 3 || teamsCount != 4)
                    {
                        Assert.IsFalse(newlastOpponent == lastOpponent);
                    }
                    lastOpponent = newlastOpponent;
                    if (isAway == away)
                    {
                        cumulative++;
                        if (teams.Count() % 2 == 0)
                        {
                            Assert.IsFalse(cumulative > 1);
                        }
                        else
                        {
                            Assert.IsFalse(cumulative > 3);
                        }
                    }
                    else
                    {
                        away = isAway;
                        cumulative = 0;
                    }
                    opponents.Add(isAway ? match.HomeTeam : match.AwayTeam);
                    i++;
                }
                var opponentsGroup = opponents.GroupBy(o => o);
                Assert.AreEqual(teams.Count() - 1, opponentsGroup.Count());
                Assert.IsTrue(opponentsGroup.All(og => og.Count() == 2));
            }
        }
    }
}
