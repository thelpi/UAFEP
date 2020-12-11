using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using UAFEP;

namespace UAFEPUnitTests
{
    [TestClass]
    public class GroupStageTests
    {
        [TestMethod]
        public void GroupStage_Ctor_Nominal()
        {
            const int groupCount = 9;

            var teams = JsonConvert.DeserializeObject<List<Team>>(TestTools.GetFileContent("teams"));

            var seed1 = teams.Take(groupCount).ToList();
            var seed2 = teams.Skip(groupCount).Take(groupCount * 2).ToList();
            var rest = teams.Skip(groupCount * 3).ToList();

            var gs = new GroupStage(groupCount, false, 16, seed1, seed2, rest);

            Assert.AreEqual(groupCount, gs.Groups.Count);
            Assert.IsTrue(gs.Groups.All(g => g.Teams.Count(t => seed1.Contains(t)) == 1));
            Assert.IsTrue(gs.Groups.All(g => g.Teams.Count(t => seed2.Contains(t)) == 2));
            Assert.IsTrue(teams.All(t => gs.Groups.SelectMany(g => g.Teams).Contains(t)));
        }

        [TestMethod]
        public void GroupStage_GetQualified_Nominal()
        {
            const int qualifiedCount = 16;

            var teams = JsonConvert.DeserializeObject<List<Team>>(TestTools.GetFileContent("teams"));

            var gs = new GroupStage(9, false, qualifiedCount, teams);

            Assert.AreEqual(0, gs.QualifiedTeams.Count);

            while (!gs.IsComplete)
            {
                gs.Play();
            }

            Assert.AreEqual(qualifiedCount, gs.QualifiedTeams.Count);
        }
    }
}
