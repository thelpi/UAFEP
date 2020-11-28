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
        [DataRow(3, false)]
        [DataRow(3, true)]
        [DataRow(4, false)]
        [DataRow(4, true)]
        [DataRow(5, false)]
        [DataRow(5, true)]
        [DataRow(6, false)]
        [DataRow(6, true)]
        [DataRow(7, false)]
        [DataRow(7, true)]
        [DataRow(16, false)]
        [DataRow(18, false)]
        [DataRow(19, false)]
        [DataRow(20, false)]
        [DataRow(21, false)]
        [DataRow(22, false)]
        public void Group_Ctor_Nominal_NoMoreThanTwoMatchesAwayOrHome_NeverTwiceTheSameMatchInARow(int teamsCount, bool oneLeg)
        {
            var teams = JsonConvert.DeserializeObject<List<Team>>(TestTools.GetFileContent("teams")).Take(teamsCount);

            var group = new Group(teams, oneLeg);

            foreach (var team in teams)
            {
                int cumulative = 0;
                bool? away = null;
                Team lastOpponent = null;
                int i = 0;
                var opponents = new List<Team>();
                foreach (var match in group.GetTeamMatches(team, excludeExempt: true))
                {
                    bool isAway = match.Team2 == team;
                    var newlastOpponent = isAway ? match.Team1 : match.Team2;
                    if (i != 3 || teamsCount != 4)
                    {
                        Assert.IsFalse(newlastOpponent == lastOpponent);
                    }
                    lastOpponent = newlastOpponent;
                    if (isAway == away)
                    {
                        cumulative++;
                        if (!oneLeg)
                        {
                            if (teams.Count() % 2 == 0)
                            {
                                Assert.IsFalse(cumulative > 1);
                            }
                            else
                            {
                                Assert.IsFalse(cumulative > 3);
                            }
                        }
                    }
                    else
                    {
                        away = isAway;
                        cumulative = 0;
                    }
                    opponents.Add(isAway ? match.Team1 : match.Team2);
                    i++;
                }
                var opponentsGroup = opponents.GroupBy(o => o);
                Assert.AreEqual(teams.Count() - 1, opponentsGroup.Count());
                Assert.IsTrue(opponentsGroup.All(og => og.Count() == (oneLeg ? 1 : 2)));
            }
        }
    }
}
